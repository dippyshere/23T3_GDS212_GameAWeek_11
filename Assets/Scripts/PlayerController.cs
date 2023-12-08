using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Player movement speed multiplier on X/Z axis")] private float moveSpeed = 5f;
    [SerializeField, Tooltip("Player sprint speed multiplier on X/Z axis")] private float sprintSpeed = 1.5f;
    [SerializeField, Tooltip("Multiplier for how fast movement rotations are slerped")] private float turnSpeed = 14f;
    [SerializeField, Tooltip("Multiplier on velocity lerping")] private float velocityDampeningSpeed = 15f;
    [SerializeField, Tooltip("Range around the player to check for pickup objects as a 'fallback' to trigger based pending objects")] private float pickupFallbackRange = 0.25f;
    [SerializeField, Tooltip("Multiplier on run animation speed")] private float animationWalkMultiplier = 0.4f;
    [Header("References")]
    [SerializeField, Tooltip("Reference to the player's animator controller for setting run + run speed")] private Animator animator;
    [SerializeField, Tooltip("Reference to the level manager to trigger win condition checks unpon dropping an object")] private LevelManager levelManager;
    [SerializeField, Tooltip("Reference to the pickup target transforms for each size of object")]
    public Transform pickupTargetSmall;
    public Transform pickupTargetMedium;
    public Transform pickupTargetLarge;
    public Transform pickupTargetVeryLarge;

    private Rigidbody rigidBody => GetComponent<Rigidbody>();
    private List<GameObject> pendingObjects = new List<GameObject>();
    private GameObject heldObject;

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        bool isWalking = (horizontalInput != 0f || verticalInput != 0f); // true if the player is intending to move

        // rotate the player to face the direction of movement
        if (isWalking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(horizontalInput, 0f, verticalInput));
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, Time.deltaTime * turnSpeed));
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        // set the run speed based on the player's velocity
        animator.SetFloat("WalkMultiplier", rigidBody.velocity.magnitude * animationWalkMultiplier);

        // move the player
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, new Vector3(movement.x, 0, movement.z).normalized * moveSpeed * sprintSpeed, Time.deltaTime * velocityDampeningSpeed);
        }
        else
        {
            rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, new Vector3(movement.x, 0, movement.z).normalized * moveSpeed, Time.deltaTime * velocityDampeningSpeed);
        }

        // pickup/drop objects
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pendingObjects.Count > 0)
            {
                // find the object closest to the pickup small transform
                GameObject closestObject = null;
                float closestDistance = float.MaxValue;
                Collider[] colliders = Physics.OverlapSphere(pickupTargetSmall.position, pickupFallbackRange);
                foreach (Collider collider in colliders)
                {
                    if ((collider.CompareTag("Atom") || collider.CompareTag("Connector")) && !pendingObjects.Contains(collider.gameObject))
                    {
                        pendingObjects.Add(collider.gameObject);
                    }
                }
                foreach (GameObject obj in pendingObjects)
                {
                    float distance = Vector3.Distance(obj.transform.position, pickupTargetSmall.position);
                    if (distance < closestDistance)
                    {
                        closestObject = obj;
                        closestDistance = distance;
                    }
                }
                if (closestObject != null)
                {
                    bool insideCollider = false;
                    colliders = Physics.OverlapSphere(pickupTargetSmall.position, pickupFallbackRange);
                    foreach (Collider collider in colliders)
                    {
                        if (collider.gameObject == closestObject)
                        {
                            PickupObject(closestObject);
                            insideCollider = true;
                            break;
                        }
                    }
                    if (!insideCollider)
                    {
                        insideCollider = false;
                        colliders = Physics.OverlapSphere(transform.position, pickupFallbackRange);
                        foreach (Collider collider in colliders)
                        {
                            if (collider.gameObject == closestObject)
                            {
                                PickupObject(closestObject);
                                insideCollider = true;
                                break;
                            }
                        }
                        if (!insideCollider)
                        {
                            Debug.Log("Not inside collider so clearing pendingObjects");
                            pendingObjects.Clear();
                        }
                    }
                }
            }
            else if (heldObject != null)
            {
                // drop the object
                DropObject(heldObject);
            }
            else
            {
                // check for atoms in range
                bool insideCollider = false;
                Collider[] colliders = Physics.OverlapSphere(pickupTargetSmall.position, pickupFallbackRange);
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Atom") || collider.CompareTag("Connector"))
                    {
                        PickupObject(collider.gameObject);
                        insideCollider = true;
                        break;
                    }
                }
                if (!insideCollider)
                {
                    colliders = Physics.OverlapSphere(transform.position, pickupFallbackRange);
                    foreach (Collider collider in colliders)
                    {
                        if (collider.CompareTag("Atom") || collider.CompareTag("Connector"))
                        {
                            PickupObject(collider.gameObject);
                            insideCollider = true;
                            break;
                        }
                    }

                }
            }
        }

        // rotate the held object
        if (Input.GetKeyDown(KeyCode.R) && heldObject != null)
        {
            RotateObject(heldObject);
        }
    }

    /// <summary>
    /// Picks up an atom/connector object
    /// </summary>
    /// <param name="obj">The atom/connector object to pickup</param>
    private void PickupObject(GameObject obj)
    {
        Debug.Log("Picking up " + obj.name);
        obj.GetComponent<AtomManager>().PickupAtom();
        heldObject = obj;
        pendingObjects.Clear();
    }

    /// <summary>
    /// Drops an atom/connector object
    /// </summary>
    /// <param name="obj">The atom/conncetor to drop</param>
    private void DropObject(GameObject obj)
    {
        Debug.Log("Dropping " + obj.name);
        obj.GetComponent<AtomManager>().DropAtom();
        heldObject = null;
        pendingObjects.Clear();
        if (levelManager != null)
        {
            levelManager.CheckWinCondition();
        }
    }

    /// <summary>
    /// Rotates an atom/connector object 90 degrees
    /// </summary>
    /// <param name="obj">The atom/connector to rotate</param>
    private void RotateObject(GameObject obj)
    {
        obj.GetComponent<AtomManager>().RotateAtom();
    }

    /// <summary>
    /// Handles entering atoms/connectors to append them to pending pickup objects and entering loading zones (on explanation scene)
    /// </summary>
    /// <param name="other">The collider of the object entering the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Atom") || other.CompareTag("Connector")) && heldObject == null)
        {
            pendingObjects.Add(other.gameObject);
            other.gameObject.GetComponent<AtomManager>().debugActive = true;
            Debug.Log("Entering " + other.gameObject.name);
        }
        else if (other.CompareTag("LoadingZone"))
        {
            levelManager.NextLevel();
            Destroy(other);
        }
    }

    /// <summary>
    /// Handles exiting atoms/connectors to remove them from pending pickup objects
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Atom") || other.CompareTag("Connector"))
        {
            pendingObjects.Remove(other.gameObject);
            other.gameObject.GetComponent<AtomManager>().debugActive = false;
        }
    }

    /// <summary>
    /// Resets the player's variables (pending objects and held object)
    /// </summary>
    public void ResetVariables()
    {
        pendingObjects.Clear();
        heldObject = null;
    }
}

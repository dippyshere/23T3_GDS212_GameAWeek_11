using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Player movement speed multiplier on X/Z axis")] private float moveSpeed = 5f;
    [SerializeField, Tooltip("Player sprint speed multiplier on X/Z axis")] private float sprintSpeed = 1.5f;
    [SerializeField, Tooltip("Multiplier for how fast movement rotations are slerped")] private float turnSpeed = 14f;
    [SerializeField, Tooltip("Multiplier on velocity lerping")] private float velocityDampeningSpeed = 15f;
    [SerializeField, Tooltip("Range around the player to check for pickup objects as a 'fallback' to trigger based pending objects")] private float pickupFallbackRange = 0.25f;
    [SerializeField, Tooltip("Multiplier on run animation speed")] private float animationWalkMultiplier = 0.4f;
    [SerializeField, Tooltip("How long the player needs to push an atom for before it will move")] private float atomPushDuration = 0.4f;
    [SerializeField, Tooltip("The levels which the player needs to complete in order")] private List<Compound.CompoundType> atomLevelsList = new List<Compound.CompoundType>();
    [Header("References")]
    [SerializeField, Tooltip("Reference to the player's animator controller for setting run + run speed")] private Animator animator;
    [SerializeField, Tooltip("Reference to the level manager to trigger win condition checks unpon dropping an object")] private LevelManager levelManager;
    [SerializeField, Tooltip("Effect to spawn when picking up an object")] private GameObject pickupEffect;
    [SerializeField, Tooltip("Effect to spawn when placing an object")] public GameObject dropEffect;
    [SerializeField, Tooltip("Effect to spawn when placing an object and having a successful connection")] public GameObject successfulDropEffect;
    public Animator transitionAnimator;
    public Animator transitionIconAnimator;
    public GameObject lightningArea1;
    public GameObject lightningArea2;
    [SerializeField, Tooltip("Reference to the pickup target transforms for each size of object")]
    public Transform pickupTargetSmall;
    public Transform pickupTargetMedium;
    public Transform pickupTargetLarge;
    public Transform pickupTargetVeryLarge;
    public Transform pickupTargetOverhead;

    private Rigidbody rigidBody => GetComponent<Rigidbody>();
    private List<GameObject> pendingObjects = new List<GameObject>();
    private List<GameObject> pendingFallbackObjects = new List<GameObject>();
    private GameObject heldObject;
    private DialogueManager dialogueManager;

    private float pushTimer = 0f;
    private int currentAtomLevel = 0;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

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

        // check for atoms to highlight
        if (heldObject == null)
        {
            if (pendingObjects.Count > 0)
            {
                foreach (GameObject obj in pendingObjects)
                {
                    if (obj == null)
                    {
                        pendingObjects.Remove(obj);
                        continue;
                    }
                    obj.GetComponent<AtomManager>().UnhighlightAtom();
                }
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
                            closestObject.GetComponent<AtomManager>().HighlightAtom();
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
                                closestObject.GetComponent<AtomManager>().HighlightAtom();
                                insideCollider = true;
                                break;
                            }
                        }
                        if (!insideCollider)
                        {
                            Debug.Log("Not inside collider so clearing pendingObjects");
                            foreach (GameObject obj in pendingObjects)
                            {
                                obj.GetComponent<AtomManager>().UnhighlightAtom();
                            }
                            pendingObjects.Clear();
                        }
                    }
                }
            }
            else
            {
                //// check for atoms in range
                //bool insideCollider = false;
                //Collider[] colliders = Physics.OverlapSphere(pickupTargetSmall.position, pickupFallbackRange);
                //if (pendingFallbackObjects.Count > 0)
                //{
                //    foreach (Collider collider in colliders)
                //    {
                //        if (collider.CompareTag("Atom") || collider.CompareTag("Connector") && pendingFallbackObjects.Contains(collider.gameObject))
                //        {
                //            collider.gameObject.GetComponent<AtomManager>().UnhighlightAtom();
                //            pendingFallbackObjects.Remove(collider.gameObject);
                //            insideCollider = true;
                //            break;
                //        }
                //    }
                //    if (!insideCollider)
                //    {
                //        colliders = Physics.OverlapSphere(transform.position, pickupFallbackRange);
                //        foreach (Collider collider in colliders)
                //        {
                //            if (collider.CompareTag("Atom") || collider.CompareTag("Connector") && pendingFallbackObjects.Contains(collider.gameObject))
                //            {
                //                collider.gameObject.GetComponent<AtomManager>().UnhighlightAtom();
                //                pendingFallbackObjects.Remove(collider.gameObject);
                //                insideCollider = true;
                //                break;
                //            }
                //        }
                //    }
                //}
                //insideCollider = false;
                //foreach (Collider collider in colliders)
                //{
                //    if (collider.CompareTag("Atom") || collider.CompareTag("Connector"))
                //    {
                //        collider.gameObject.GetComponent<AtomManager>().HighlightAtom();
                //        pendingFallbackObjects.Add(collider.gameObject);
                //        insideCollider = true;
                //        break;
                //    }
                //}
                //if (!insideCollider)
                //{
                //    colliders = Physics.OverlapSphere(transform.position, pickupFallbackRange);
                //    foreach (Collider collider in colliders)
                //    {
                //        if (collider.CompareTag("Atom") || collider.CompareTag("Connector"))
                //        {
                //            collider.gameObject.GetComponent<AtomManager>().HighlightAtom();
                //            pendingFallbackObjects.Add(collider.gameObject);
                //            insideCollider = true;
                //            break;
                //        }
                //    }
                //}
            }

            if (movement != Vector3.zero && rigidBody.velocity.magnitude < 5f)
            {
                pushTimer += Time.deltaTime;
                if (pushTimer >= atomPushDuration)
                {
                    Debug.Log("Pushing atom");
                    pushTimer = 0f;
                    // push the closest atom
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
                                closestObject.GetComponent<AtomManager>().PushAtom(movement);
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
                                    closestObject.GetComponent<AtomManager>().PushAtom(movement);
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
            }
            else
            {
                pushTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Picks up an atom/connector object
    /// </summary>
    /// <param name="obj">The atom/connector object to pickup</param>
    private void PickupObject(GameObject obj)
    {
        Debug.Log("Picking up " + obj.name);
        AtomManager objAtomManager = obj.GetComponent<AtomManager>();
        objAtomManager.PickupAtom();
        heldObject = obj;
        pendingObjects.Clear();

        GameObject spawnedPickupEffect = Instantiate(pickupEffect, pickupTargetOverhead.transform);
        switch (objAtomManager.atomSize)
        {
            case AtomManager.AtomSize.Small:
                spawnedPickupEffect.transform.localScale = new Vector3(4, 4, 4);
                break;
            case AtomManager.AtomSize.Medium:
                spawnedPickupEffect.transform.localScale = new Vector3(4.5f, 4.5f, 4.5f);
                break;
            case AtomManager.AtomSize.Large:
                spawnedPickupEffect.transform.localScale = new Vector3(5.3f, 5.3f, 5.3f);
                break;
            case AtomManager.AtomSize.VeryLarge:
                spawnedPickupEffect.transform.localScale = new Vector3(6.4f, 6.4f, 6.4f);
                break;
        }
        dialogueManager.AdvancePickup();
    }

    /// <summary>
    /// Drops an atom/connector object
    /// </summary>
    /// <param name="obj">The atom/conncetor to drop</param>
    private void DropObject(GameObject obj)
    {
        levelManager = FindObjectOfType<LevelManager>();
        Debug.Log("Dropping " + obj.name);
        AtomManager objAtomManager = obj.GetComponent<AtomManager>();
        objAtomManager.DropAtom();
        heldObject = null;
        pendingObjects.Clear();
        if (levelManager != null)
        {
            levelManager.CheckWinCondition();
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Atom") || collider.CompareTag("Connector"))
            {
                pendingObjects.Add(collider.gameObject);
            }
        }
        dialogueManager.AdvancePlacement();
    }

    /// <summary>
    /// Rotates an atom/connector object 90 degrees
    /// </summary>
    /// <param name="obj">The atom/connector to rotate</param>
    private void RotateObject(GameObject obj)
    {
        obj.GetComponent<AtomManager>().RotateAtom();
        dialogueManager.AdvanceRotate();
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

    public void LoadNextLevel()
    {
        currentAtomLevel++;
        if (currentAtomLevel >= atomLevelsList.Count)
        {
            StartCoroutine(levelManager.LoadLevel("Menu"));
            return;
        }
        StartCoroutine(UnloadCurrentScene());
        if (currentAtomLevel == 1)
        {
            LoadNextLevelByTrigger();
        }
        lightningArea1.SetActive(false);
        GameObject spawnedDropEffect = Instantiate(successfulDropEffect, new Vector3(-221.02f, 2.44f, -31.13f), Quaternion.identity);
        spawnedDropEffect.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
        lightningArea2.SetActive(false);
        spawnedDropEffect = Instantiate(successfulDropEffect, new Vector3(-107.72f, 2.44f, 17.37f), Quaternion.identity);
        spawnedDropEffect.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
    }

    public void LoadNextLevelByTrigger()
    {
        StartCoroutine(LoadNextScene());
        lightningArea1.SetActive(true);
        lightningArea2.SetActive(true);
    }

    private IEnumerator UnloadCurrentScene()
    {
        levelManager.transitionAnimator.SetTrigger("Start");
        levelManager.transitionIconAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1);
        transitionAnimator.SetTrigger("End");
        transitionIconAnimator.SetTrigger("End");
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(atomLevelsList[currentAtomLevel - 1].ToString() + " Scene");
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadNextScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(atomLevelsList[currentAtomLevel].ToString() + " Scene", LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        levelManager = FindObjectOfType<LevelManager>();
        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Tooltip("The Y Position the atom is constrained to")] private float yPosition = 5f;
    [SerializeField, Tooltip("The size of the grid the atom will snap to")] private float gridSize = 3f;
    [SerializeField, Tooltip("The hitbox size of the atom for registering player events")] public AtomSize atomSize = AtomSize.Small;
    [SerializeField, Tooltip("The thickness of the outline when the player is going to pickup this atom")] private float outlineHighlightThickness = 6f;
    [SerializeField, Tooltip("The thickness of the outline when the player is holding this atom")] private float outlinePickedupThickness = 4f;
    [SerializeField, Tooltip("The prefab to use to preview placement location")] private GameObject previewPrefab;
    [SerializeField, Tooltip("The collider that interacts with the player")] private Collider collider1;
    public Compound.AtomType atomType;
    public List<Transform> connectionPoints = new List<Transform>(4);
    private Rigidbody rigidBody => GetComponent<Rigidbody>();
    private Outline outline => GetComponent<Outline>();
    private PlayerController playerController => GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    private HingeJoint hingeJointToPlayer;
    private float outlineDefaultThickness;
    private bool held = false;
    private float rotationOffset;
    private GameObject preview;

    //public List<HingeJoint> hingeJoints;
    public List<GameObject> connections;
    public List<Compound.AtomType> connectedAtomTypes;

    public bool debugActive;

    void Awake()
    {
        SnapToGrid();
        foreach (Transform connectionPoint in connectionPoints)
        {
            //hingeJoints.Add(null);
            connections.Add(null);
        }
    }

    private void Start()
    {
        SnapToGrid();
        Invoke("SnapToGrid", 0.01f);
        outlineDefaultThickness = outline.OutlineWidth;
        preview = Instantiate(previewPrefab, transform.position, Quaternion.identity);
        preview.SetActive(false);
    }

    public void PickupAtom()
    {
        held = true;
        outline.OutlineWidth = outlinePickedupThickness;
        //switch (atomSize)
        //{
        //    case AtomSize.Small:
        //        transform.position = playerController.pickupTargetSmall.position;
        //        break;
        //    case AtomSize.Medium:
        //        transform.position = playerController.pickupTargetMedium.position;
        //        break;
        //    case AtomSize.Large:
        //        transform.position = playerController.pickupTargetLarge.position;
        //        break;
        //    case AtomSize.VeryLarge:
        //        transform.position = playerController.pickupTargetVeryLarge.position;
        //        break;
        //}
        transform.position = playerController.pickupTargetOverhead.position;
        rotationOffset = Mathf.Round(playerController.transform.rotation.eulerAngles.y / 90) * 90 - playerController.transform.rotation.eulerAngles.y;
        // Debug.Log(rotationOffset);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y - rotationOffset, 0);
        // CancelInvoke("SetKinematicTrue");
        // SetKinematic(false);
        //if (hingeJointToPlayer != null)
        //{
        //    Destroy(hingeJointToPlayer);
        //    hingeJointToPlayer = null;
        //}
        if (gameObject.CompareTag("Atom"))
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i] != null)
                {
                    connections[i].SetActive(true);
                    // Destroy(hingeJoints[i]);
                    // hingeJoints[i] = null;
                    connections[i] = null;
                    connectionPoints[i].gameObject.SetActive(true);
                }
            }
        }
        collider1.enabled = false;
        //hingeJointToPlayer = gameObject.AddComponent<HingeJoint>();
        //hingeJointToPlayer.connectedBody = playerController.GetComponent<Rigidbody>();
        //hingeJointToPlayer.axis = new Vector3(0, 0, 1);
        //hingeJointToPlayer.useSpring = true;
        //hingeJointToPlayer.spring = new JointSpring() { spring = 10, damper = 1 };
        //hingeJointToPlayer.useLimits = true;
        //hingeJointToPlayer.limits = new JointLimits() { min = -10, max = 10 };
        preview.SetActive(true);
    }

    public void DropAtom()
    {
        held = false;
        outline.OutlineWidth = outlineDefaultThickness;
        //Destroy(hingeJointToPlayer);
        //hingeJointToPlayer = null;
        //rigidBody.isKinematic = true;
        //Invoke("SetKinematicTrue", 0.3f);
        switch (atomSize)
        {
            case AtomSize.Small:
                transform.position = playerController.pickupTargetSmall.position;
                break;
            case AtomSize.Medium:
                transform.position = playerController.pickupTargetMedium.position;
                break;
            case AtomSize.Large:
                transform.position = playerController.pickupTargetLarge.position;
                break;
            case AtomSize.VeryLarge:
                transform.position = playerController.pickupTargetVeryLarge.position;
                break;
        }
        SnapToGrid();
        ConnectToConnector();
        collider1.enabled = true;
        Invoke("DropAtomTask", 0.02f);
        preview.SetActive(false);
    }

    public void DropAtomTask()
    {
        switch (atomSize)
        {
            case AtomSize.Small:
                transform.position = playerController.pickupTargetSmall.position;
                break;
            case AtomSize.Medium:
                transform.position = playerController.pickupTargetMedium.position;
                break;
            case AtomSize.Large:
                transform.position = playerController.pickupTargetLarge.position;
                break;
            case AtomSize.VeryLarge:
                transform.position = playerController.pickupTargetVeryLarge.position;
                break;
        }
        SnapToGrid();
        ConnectToConnector();
        bool successful = false;
        foreach (Transform connectionPoint in connectionPoints)
        {
            if (!connectionPoint.gameObject.activeSelf)
            {
                successful = true;
            }
        }
        if (successful)
        {
            switch (atomSize)
            {
                case AtomManager.AtomSize.Small:
                    GameObject spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
                    break;
                case AtomManager.AtomSize.Medium:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.15f, 3.15f, 3.15f);
                    break;
                case AtomManager.AtomSize.Large:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.75f, 3.75f, 3.75f);
                    break;
                case AtomManager.AtomSize.VeryLarge:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(4.25f, 4.25f, 4.25f);
                    break;
            }
        }
        else
        {
            switch (atomSize)
            {
                case AtomManager.AtomSize.Small:
                    GameObject spawnedDropEffect = Instantiate(playerController.dropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
                    break;
                case AtomManager.AtomSize.Medium:
                    spawnedDropEffect = Instantiate(playerController.dropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.15f, 3.15f, 3.15f);
                    break;
                case AtomManager.AtomSize.Large:
                    spawnedDropEffect = Instantiate(playerController.dropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.75f, 3.75f, 3.75f);
                    break;
                case AtomManager.AtomSize.VeryLarge:
                    spawnedDropEffect = Instantiate(playerController.dropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(4.25f, 4.25f, 4.25f);
                    break;
            }
        }
    }

    public void PushAtom(Vector3 direction)
    {
        StartCoroutine(pushAtomTask(direction));
    }

    private IEnumerator pushAtomTask(Vector3 direction)
    {
        StopCoroutine("PushAtom");
        Debug.Log("Push atom");
        Vector3 newPosition = transform.position + direction * gridSize;
        float time = 0;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPosition, time / 0.2f);
            yield return null;
        }
        SnapToGrid();
        ConnectToConnector();
        bool successful = false;
        foreach (Transform connectionPoint in connectionPoints)
        {
            if (!connectionPoint.gameObject.activeSelf)
            {
                successful = true;
            }
        }
        if (successful)
        {
            switch (atomSize)
            {
                case AtomManager.AtomSize.Small:
                    GameObject spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
                    break;
                case AtomManager.AtomSize.Medium:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.15f, 3.15f, 3.15f);
                    break;
                case AtomManager.AtomSize.Large:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(3.75f, 3.75f, 3.75f);
                    break;
                case AtomManager.AtomSize.VeryLarge:
                    spawnedDropEffect = Instantiate(playerController.successfulDropEffect, transform);
                    spawnedDropEffect.transform.localScale = new Vector3(4.25f, 4.25f, 4.25f);
                    break;
            }
        }
    }

    private void Update()
    {
        if (held)
        {
            transform.position = playerController.pickupTargetOverhead.position;
            transform.rotation = Quaternion.Euler(0, playerController.transform.rotation.eulerAngles.y - 90, 0);
            switch (atomSize)
            {
                case AtomSize.Small:
                    float x = Mathf.Round(playerController.pickupTargetSmall.position.x / gridSize) * gridSize;
                    float z = Mathf.Round(playerController.pickupTargetSmall.position.z / gridSize) * gridSize;
                    preview.transform.position = new Vector3(x, yPosition, z);
                    break;
                case AtomSize.Medium:
                    x = Mathf.Round(playerController.pickupTargetMedium.position.x / gridSize) * gridSize;
                    z = Mathf.Round(playerController.pickupTargetMedium.position.z / gridSize) * gridSize;
                    preview.transform.position = new Vector3(x, yPosition, z);
                    break;
                case AtomSize.Large:
                    x = Mathf.Round(playerController.pickupTargetLarge.position.x / gridSize) * gridSize;
                    z = Mathf.Round(playerController.pickupTargetLarge.position.z / gridSize) * gridSize;
                    preview.transform.position = new Vector3(x, yPosition, z);
                    break;
                case AtomSize.VeryLarge:
                    x = Mathf.Round(playerController.pickupTargetVeryLarge.position.x / gridSize) * gridSize;
                    z = Mathf.Round(playerController.pickupTargetVeryLarge.position.z / gridSize) * gridSize;
                    preview.transform.position = new Vector3(x, yPosition, z);
                    break;
            }
            preview.transform.rotation = Quaternion.Euler(0, Mathf.Round(transform.rotation.eulerAngles.y / 90) * 90, 0);
        }
    }

    public void HighlightAtom()
    {
        outline.OutlineWidth = outlineHighlightThickness;
    }

    public void UnhighlightAtom()
    {
        outline.OutlineWidth = outlineDefaultThickness;
    }

    private void SetKinematicTrue()
    {
        SetKinematic(true);
    }

    private void SnapToGrid()
    {
        float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float z = Mathf.Round(transform.position.z / gridSize) * gridSize;
        transform.position = new Vector3(x, yPosition, z);
        float y = Mathf.Round(transform.rotation.eulerAngles.y / 90) * 90;
        transform.rotation = Quaternion.Euler(0, y, 0);
    }

    public void RotateAtom()
    {
        //Destroy(hingeJointToPlayer);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 90, 0);
        //hingeJointToPlayer = gameObject.AddComponent<HingeJoint>();
        //hingeJointToPlayer.connectedBody = playerController.GetComponent<Rigidbody>();
        //hingeJointToPlayer.axis = new Vector3(0, 0, 1);
        //hingeJointToPlayer.useSpring = true;
        //hingeJointToPlayer.spring = new JointSpring() { spring = 10, damper = 1 };
        //hingeJointToPlayer.useLimits = true;
        //hingeJointToPlayer.limits = new JointLimits() { min = -10, max = 10 };
    }

    public void ConnectToConnector()
    {
        if (gameObject.CompareTag("Connector"))
        {
            ConnectToAtom();
            return;
        }

        foreach (Transform connectionPoint in connectionPoints)
        {
            if (connectionPoint.gameObject.activeSelf)
            {
                Collider[] colliders = Physics.OverlapSphere(connectionPoint.position, 0.25f);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.CompareTag("Connector"))
                    {
                        // find the closest connection point on the connector
                        Transform closestConnectionPoint = null;
                        float closestDistance = float.MaxValue;
                        AtomManager atomManager = collider.gameObject.GetComponent<AtomManager>();
                        foreach (Transform connectorConnectionPoint in atomManager.connectionPoints)
                        {
                            float distance = Vector3.Distance(connectionPoint.position, connectorConnectionPoint.position);
                            if (distance < closestDistance)
                            {
                                closestConnectionPoint = connectorConnectionPoint;
                                closestDistance = distance;
                            }
                        }
                        if (closestConnectionPoint != null && closestDistance <= 0.1)
                        {
                            if (connectionPoints.IndexOf(connectionPoint) < 0)
                            {
                                continue;
                            }
                            if (Mathf.Abs(connectionPoint.rotation.eulerAngles.y - closestConnectionPoint.rotation.eulerAngles.y) > 90)
                            {
                                Debug.Log("Bad angle (" + (connectionPoint.rotation.eulerAngles.y - closestConnectionPoint.rotation.eulerAngles.y) + ")");
                                continue;
                            }
                            // disable both connection points
                            connectionPoint.gameObject.SetActive(false);
                            closestConnectionPoint.gameObject.SetActive(false);

                            //if (hingeJoints[connectionPoints.IndexOf(connectionPoint)] != null)
                            //{
                            //    Destroy(hingeJoints[connectionPoints.IndexOf(connectionPoint)]);
                            //    hingeJoints[connectionPoints.IndexOf(connectionPoint)] = null;
                            //    atomManager.hingeJoints[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = null;
                            //}

                            // create a hinge joint between the two connection points
                            //HingeJoint hingeJoint = gameObject.AddComponent<HingeJoint>();
                            //hingeJoint.connectedBody = collider.gameObject.GetComponent<Rigidbody>();
                            //hingeJoint.useLimits = true;
                            //hingeJoint.limits = new JointLimits() { min = -10, max = 10 };

                            //hingeJoints[connectionPoints.IndexOf(connectionPoint)] = hingeJoint;
                            connections[connectionPoints.IndexOf(connectionPoint)] = closestConnectionPoint.gameObject;
                            //atomManager.hingeJoints[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = hingeJoint;
                            atomManager.connections[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = connectionPoint.gameObject;
                            //rigidBody.isKinematic = false;
                            atomManager.UpdateConnectedAtoms();
                        }
                    }
                }
            }
        }
    }

    public void ConnectToAtom()
    {
        if (gameObject.CompareTag("Atom"))
        {
            ConnectToConnector();
            return;
        }

        foreach (Transform connectionPoint in connectionPoints)
        {
            if (connectionPoint.gameObject.activeSelf)
            {
                Collider[] colliders = Physics.OverlapSphere(connectionPoint.position, 0.25f);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.CompareTag("Atom"))
                    {
                        // find the closest connection point on the atom
                        Transform closestConnectionPoint = null;
                        float closestDistance = float.MaxValue;
                        AtomManager atomManager = collider.gameObject.GetComponent<AtomManager>();
                        foreach (Transform atomConnectionPoint in atomManager.connectionPoints)
                        {
                            float distance = Vector3.Distance(connectionPoint.position, atomConnectionPoint.position);
                            if (distance < closestDistance)
                            {
                                closestConnectionPoint = atomConnectionPoint;
                                closestDistance = distance;
                            }
                        }
                        if (closestConnectionPoint != null && closestDistance <= 0.1)
                        {
                            if (connectionPoints.IndexOf(connectionPoint) < 0)
                            {
                                continue;
                            }
                            // disable both connection points
                            connectionPoint.gameObject.SetActive(false);
                            closestConnectionPoint.gameObject.SetActive(false);

                            //if (hingeJoints[connectionPoints.IndexOf(connectionPoint)] != null)
                            //{
                            //    Destroy(hingeJoints[connectionPoints.IndexOf(connectionPoint)]);
                            //    hingeJoints[connectionPoints.IndexOf(connectionPoint)] = null;
                            //    atomManager.hingeJoints[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = null;
                            //}

                            // create a hinge joint between the two connection points
                            //HingeJoint hingeJoint = collider.gameObject.AddComponent<HingeJoint>();
                            //hingeJoint.connectedBody = gameObject.GetComponent<Rigidbody>();
                            //hingeJoint.useLimits = true;
                            //hingeJoint.limits = new JointLimits() { min = -10, max = 10 };

                            //hingeJoints[connectionPoints.IndexOf(connectionPoint)] = hingeJoint;
                            connections[connectionPoints.IndexOf(connectionPoint)] = closestConnectionPoint.gameObject;
                            //atomManager.hingeJoints[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = hingeJoint;
                            atomManager.connections[atomManager.connectionPoints.IndexOf(closestConnectionPoint)] = connectionPoint.gameObject;
                            //collider.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                        }
                    }
                }
            }
        }
        UpdateConnectedAtoms();
    }

    public void UpdateConnectedAtoms()
    {
        connectedAtomTypes.Clear();
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i] != null)
            {
                Compound.AtomType atomType = connections[i].transform.parent.GetComponent<AtomManager>().atomType;
                if (atomType != Compound.AtomType._Connector)
                {
                    connectedAtomTypes.Add(atomType);
                }
            }
        }
    }

    private void SetKinematic(bool isKinematic)
    {
        rigidBody.isKinematic = isKinematic;
        // messy but im tired
        foreach (GameObject connection in connections)
        {
            if (connection != null)
            {
                connection.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                foreach (GameObject connection2 in connection.transform.parent.GetComponent<AtomManager>().connections)
                {
                    if (connection2 != null)
                    {
                        connection2.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                        foreach (GameObject connection3 in connection2.transform.parent.GetComponent<AtomManager>().connections)
                        {
                            if (connection3 != null)
                            {
                                connection3.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                foreach (GameObject connection4 in connection3.transform.parent.GetComponent<AtomManager>().connections)
                                {
                                    if (connection4 != null)
                                    {
                                        connection4.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                        foreach (GameObject connection5 in connection4.transform.parent.GetComponent<AtomManager>().connections)
                                        {
                                            if (connection5 != null)
                                            {
                                                connection5.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                                foreach (GameObject connection6 in connection5.transform.parent.GetComponent<AtomManager>().connections)
                                                {
                                                    if (connection6 != null)
                                                    {
                                                        connection6.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                                        foreach (GameObject connection7 in connection6.transform.parent.GetComponent<AtomManager>().connections)
                                                        {
                                                            if (connection7 != null)
                                                            {
                                                                connection7.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                                                foreach (GameObject connection8 in connection7.transform.parent.GetComponent<AtomManager>().connections)
                                                                {
                                                                    if (connection8 != null)
                                                                    {
                                                                        connection8.transform.parent.GetComponent<AtomManager>().rigidBody.isKinematic = isKinematic;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (isKinematic)
        {
            SnapToGrid();
        }
    }

    public enum AtomSize
    {
        Small,
        Medium,
        Large,
        VeryLarge
    }

    private void OnDrawGizmos()
    {
        if (debugActive)
        {
            Gizmos.color = new Color(0, 1, 0, 0.35f);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 0.35f);
        }
        switch (atomSize)
        {
            case AtomSize.Small:
                Gizmos.DrawSphere(new Vector3(transform.position.x, 1, transform.position.z), 5.5f);
                break;
            case AtomSize.Medium:
                Gizmos.DrawSphere(new Vector3(transform.position.x, 1, transform.position.z), 6f);
                break;
            case AtomSize.Large:
                Gizmos.DrawSphere(new Vector3(transform.position.x, 1, transform.position.z), 7f);
                break;
            case AtomSize.VeryLarge:
                Gizmos.DrawSphere(new Vector3(transform.position.x, 1, transform.position.z), 8.25f);
                break;
        }
    }
}
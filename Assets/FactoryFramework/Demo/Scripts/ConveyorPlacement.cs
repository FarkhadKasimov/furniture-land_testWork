using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FactoryFramework;
using Sirenix.OdinInspector;

public class ConveyorPlacement : MonoBehaviour
{
    [SerializeField] private bool calculateInUpdate = false;     
    [SerializeField] private Conveyor conveyorPrefab;
    [SerializeField] private FabriqueConveyor fabriqueConveyor;
    [SerializeField] private Material originalFrameMat;
    [SerializeField] private Material originalBeltMat;
    [SerializeField] private Material greenGhostMat;
    [SerializeField] private Material redGhostMat;
    [SerializeField] private State state;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    
    private Conveyor current;
    private FabriqueConveyor currentFC;

    private Vector3 startPos;
    private float startHeight;
    private Vector3 endPos;

    private Socket startSocket;
    private Socket endSocket;

    private enum State
    {
        None,
        Start,
        End
    }

    [Button]
    private void CreateConveyorEditor(Socket input, Socket output)
    {
        CreateConveyor(input, output);
    }
    
    public void Update()
    {
        if (calculateInUpdate == false)
        {
            return;
        }
        
        if (Input.GetKeyDown(cancelKey))
        {
            CancelConveyorPlacement();
        }
        
        switch (state)
        {
            case State.None:
                HandleNoneState();
                break;
            case State.Start:
                HandleStartState();
                break;
            case State.End:
                HandleEndState();
                break;
        }
    }
    
    public void StartPlacingConveyor() 
    { 
        current = Instantiate(conveyorPrefab); 
        state = State.Start; 
        startSocket = null; 
        endSocket = null; 
    }

    public void CreateConveyor(Socket inputSocket, Socket outputSocket)
    {
        CancelConveyorPlacement();
        
        currentFC = Instantiate(fabriqueConveyor); 
        state = State.Start;
        
        var inputSocketTransform = inputSocket.transform;
        var outputSocketTransform = outputSocket.transform;
        
        var inputDir = inputSocketTransform.forward;
        var outputDir = outputSocketTransform.forward;
        
        startSocket = inputSocket;
        startPos = inputSocketTransform.position;
        
        endSocket = outputSocket;
        endPos = outputSocketTransform.position;
        
        currentFC.start = startPos;
        currentFC.startDir = inputDir;
        currentFC.end = endPos;
        currentFC.endDir = outputDir;

        currentFC.Init();
        currentFC.DisableBridge();

        if (currentFC.ValidMesh == false)
        {
            return;
        }

        startSocket.Connect(currentFC);
        endSocket.Connect(currentFC);
            
        currentFC.UpdateMesh(true);
        currentFC.SetMaterials(originalFrameMat, originalBeltMat);
        currentFC.AddCollider();

        currentFC = null;
        startSocket = null;
        endSocket = null;
    }
    
    private bool TryChangeState(State desiredState)
    {
        state = desiredState;
        return true;
    }

    private bool ValidLocation()
    {
        if (current == null) return false;
        
            foreach (Collider c in Physics.OverlapSphere(startPos, 1f))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name}");
                    //ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
            }
            foreach (Collider c in Physics.OverlapSphere(endPos, 1f))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name}");
                    //ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
            }

        //ChangeMatrerial(greenPlacementMaterial);
        return true;
    }
   
    private void HandleStartState()
    {
        Debug.Assert(current != null, "Not currently placing a conveyor.");
        Vector3 worldPos = Vector3.zero;
        Vector3 worldDir = Vector3.forward;
        Ray mousedownRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(mousedownRay, 100f))
        {
            if (hit.collider.gameObject.TryGetComponent<Socket>(out Socket socket))
            {
                if (!socket.IsOpen())
                {
                    // Socket already Occupied
                    break;
                }
                worldPos = hit.collider.transform.position;
                worldDir = hit.collider.transform.forward;
                startSocket = socket;
                break;
            }
            
            if (hit.collider.gameObject.TryGetComponent<Terrain>(out Terrain t))
            {
                startSocket = null;
                worldPos = hit.point;
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                worldDir = camForward;
            }
        }
        startPos = worldPos;

        current.start = worldPos;
        current.startDir = worldDir;
        current.end = worldPos + worldDir;
        current.endDir = worldDir;
        List<Collider> collidersToIgnore = new List<Collider>();
        // add colliders associated with the connected start socket
        if (startSocket != null)
            collidersToIgnore.AddRange(startSocket.transform.parent.GetComponentsInChildren<Collider>());
        
        if (collidersToIgnore.Count > 0)
            current.UpdateMesh(ignored: collidersToIgnore.ToArray());
        else
            current.UpdateMesh();

        // startSocket != null prevents belt from starting disconnected
        if (current.ValidMesh && startSocket != null)
        {
            current.SetMaterials(greenGhostMat, greenGhostMat);
            if (Input.GetMouseButtonDown(0))
            {
                TryChangeState(State.End);
            }
        }
        else 
            current.SetMaterials(redGhostMat, redGhostMat);

        
    }

    private void HandleEndState()
    {
        Debug.Assert(current != null, "Not currently placing a conveyor.");
        Vector3 worldPos = Vector3.zero;
        Vector3 worldDir = Vector3.forward;
        Ray mousedownRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(mousedownRay, 100f))
        {
            if (hit.collider.transform.root == current.transform) continue;
            // want to specifically connect to a conveyor socket, not a belt bridge
            if (hit.collider.gameObject.TryGetComponent<ConveyorSocket>(out ConveyorSocket socket))
            {
                if (!socket.IsOpen())
                {
                    // Socket already Occupied
                    break;
                }
                worldPos = hit.collider.transform.position;
                worldDir = hit.collider.transform.forward;
                endSocket = socket;

                current.DisableBridge();

                break;
            }
            if (hit.collider.gameObject.TryGetComponent<Terrain>(out Terrain t))
            {
                worldPos = hit.point;
                // stay same level if this is the terrain
                //if (Vector3.Distance(worldPos, startPos) < 10f)
                //    worldPos.y = startPos.y;
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                worldDir = camForward;
                // reset socket
                endSocket = null;

                // enable the belt bridge connection
                current.EnableBridge();

            }
            
        }
        endPos = worldPos;
        current.end = worldPos;
        current.endDir = worldDir;
        List<Collider> collidersToIgnore = new List<Collider>();
        //add colliders associated with the connected start and end sockets
        //THIS IS NOT A GREAT WAY TO DO THIS - CONSIDER USING LAYERMASKS
        collidersToIgnore.AddRange(FindObjectsOfType<TerrainCollider>());
        if (startSocket != null)
            collidersToIgnore.AddRange(startSocket.GetComponentsInChildren<Collider>());
        if (endSocket != null)
            collidersToIgnore.AddRange(endSocket.GetComponentsInChildren<Collider>());
        ConveyorBridge bridge = current.GetComponentInChildren<ConveyorBridge>();
        if (bridge != null)
            collidersToIgnore.Add(bridge.GetComponent<Collider>());

        current.UpdateMesh(
            startskip: startSocket != null ? 2 : 0, 
            endskip: 1, 
            ignored: collidersToIgnore.Count > 0 ? collidersToIgnore.ToArray() : null
        );

        if (current.ValidMesh)
            current.SetMaterials(greenGhostMat, greenGhostMat);
        else
            current.SetMaterials(redGhostMat, redGhostMat);

        if (Input.GetMouseButtonDown(0) && current.ValidMesh)
        {

            // change the sockets!
            if (startSocket != null)
            {
                startSocket.Connect(current);
            }
            if (endSocket != null)
            {
                endSocket.Connect(current);
            }
            // finalize the conveyor
            current.UpdateMesh(true);
            current.SetMaterials(originalFrameMat, originalBeltMat);
            current.AddCollider();

            // stop placing conveyor
            current = null;
            startSocket = null;
            endSocket = null;

            TryChangeState(State.None);
        }
    }

    private void HandleNoneState()
    {
        // right click to delete
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
            {
                if (hit.collider.transform.root.TryGetComponent<Conveyor>(out Conveyor conveyor))
                {
                    //foreach (Socket socket in building.gameObject.GetComponentsInChildren<Socket>())
                    //{
                    //    // do nothing
                    //}
                    Destroy(conveyor.gameObject);
                    return;
                }
            }
        }
        return;
        
    }

    private void CancelConveyorPlacement()
    {
        if (current != null)
            Destroy(current.gameObject);
        current = null;
        startSocket = null;
        endSocket = null;
        state = State.None;
    }
}

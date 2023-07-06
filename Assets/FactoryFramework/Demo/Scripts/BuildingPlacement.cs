using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FactoryFramework;

public class BuildingPlacement : MonoBehaviour
{
    private GameObject current;
    public GameObject Miner;
    public GameObject Processor;
    public GameObject Factory;
    public GameObject Storage;
    public GameObject Splitter;
    public GameObject Merger;
    public GameObject Assembler;

    public Material originalMaterial;
    public Material greenPlacementMaterial;
    public Material redPlacementMaterial;
    private Material[] validMaterials { get { return new Material[3] { originalMaterial, greenPlacementMaterial, redPlacementMaterial }; } }

    public KeyCode CancelKey = KeyCode.Escape;
    private enum State
    {
        None,
        PlaceBuilding,
        RotateBuilding
    }
    [SerializeField] private State state;
    private bool RequiresResourceDepoist = false;


    private Vector3 mouseDownPos;
    private float mouseHeldTime = 0f;

    public void PlaceMiner() => StartPlacingItem(Miner, true);
    public void PlaceProcessor() => StartPlacingItem(Processor);
    public void PlaceFactory() => StartPlacingItem(Factory);
    public void PlaceAssembler() => StartPlacingItem(Assembler);
    public void PlaceStorage() => StartPlacingItem(Storage);
    public void PlaceSplitter() => StartPlacingItem(Splitter);
    public void PlaceMerger() => StartPlacingItem(Merger);

    public void StartPlacingItem(GameObject prefab, bool requireDeposit=false)
    {
        RequiresResourceDepoist = requireDeposit;
        state = State.PlaceBuilding;
        current = Instantiate(prefab);
        current.name = prefab.name;
        Building b = current.GetComponent<Building>();
        b.enabled = false;
        ChangeMatrerial(greenPlacementMaterial);
        mouseHeldTime = 0f;
    }

    private void ChangeMatrerial(Material mat)
    {
        foreach (MeshRenderer mr in current?.GetComponentsInChildren<MeshRenderer>())
        {
            // dont change materials that shouldn't be changed!
            if (validMaterials.Contains(mr.sharedMaterial))
                mr.sharedMaterial = mat;
        }
    }

    private void HandleIdleState()
    {
        // right click to delete
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
            {
                if (hit.collider.gameObject.TryGetComponent<Building>(out Building building))
                {
                    foreach(Socket socket in building.gameObject.GetComponentsInChildren<Socket>())
                    {
                        
                    }
                    Destroy(building.gameObject);
                    return;
                }
            }
        }
        return;
    }
    private void HandlePlaceBuildingState()
    {
        
        // move building with mouse pos
        Vector3 groundPos = transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
        {
            if (hit.collider.tag == "Terrain")
                groundPos = hit.point;
        }


        current.transform.position = groundPos;
        bool valid = ValidLocation();
        if (Input.GetMouseButtonDown(0) && valid)
        {
            mouseDownPos = groundPos;
            TryChangeState(State.RotateBuilding);
        }

    }
    private void HandleRotateBuildingState()
    {
        // wait for mouse to be held for X seconds until building rotation is allowed
        // this prevents quick clicks resulting in seemingly random building rotations
        mouseHeldTime += Time.deltaTime;
        if (mouseHeldTime > .333f)
        {
            bool valid = ValidLocation();
            // get new ground position to rotate towards
            Vector3 dir = current.transform.forward;
            // rotate the building!
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(mouseRay, 100f))
            {
                if (hit.collider.tag == "Terrain")
                    current.transform.forward = (mouseDownPos - hit.point).normalized;
            }
            current.transform.position = mouseDownPos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            TryChangeState(State.None);
        }
    }

    private bool ValidLocation()
    {
        if (current == null) return false;
        if (current.TryGetComponent<BoxCollider>(out BoxCollider col))
        {
            bool onResourceDeposit = false;
            foreach (Collider c in Physics.OverlapBox(col.transform.TransformPoint(col.center), col.size/2f, col.transform.rotation))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                        Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name}");
                    ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
                // check for resources
                if (c.tag == "Resources")
                {
                    onResourceDeposit = true;
                }
            }
            if (RequiresResourceDepoist && !onResourceDeposit)
            {
                if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                    Debug.LogWarning($"Invalid placement: {current.gameObject.name} requries placement near Resource Deposit");
                ChangeMatrerial(redPlacementMaterial);
                return false;
            }
        }
        ChangeMatrerial(greenPlacementMaterial);
        return true;
    }

    private bool TryChangeState(State desiredState)
    {
        if (desiredState == State.PlaceBuilding)
        {
            Debug.Assert(current != null, "No building set to place!");
            Debug.Assert(state == State.None, "Cannot interrupt building placement");
            mouseHeldTime = 0f;
            this.state = desiredState;
            return true;
        }
        if (desiredState == State.RotateBuilding)
        {
            this.state = desiredState;
            return true;
        }
        if (desiredState == State.None)
        {   
            // if we weren't placing a building, ignore
            if (current == null)
            {
                this.state = desiredState;
                return true;
            }

            // make sure building placement and rotation is valid
            if (ValidLocation())
            {
                // finish placing building and enable it
                this.state = desiredState;
                ChangeMatrerial(originalMaterial);
                Building b = current.GetComponent<Building>();
                b.enabled = true;
                current = null;
                return true;
            }
            else
            {
                this.state = State.PlaceBuilding;
                return false;
            }
        }
        return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(CancelKey))
        {
            if (current != null)
                Destroy(current.gameObject);
            current = null;
            state = State.None;
        }

        switch (state)
        {
            case State.RotateBuilding:
                HandleRotateBuildingState();
                break;
            case State.None:
                HandleIdleState();
                break;
            case State.PlaceBuilding:
                HandlePlaceBuildingState();
                break;
        }

    }

}

using System.Collections.Generic;
using FactoryFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FabriqueConveyor : LogisticComponent
    { 
        public bool ValidMesh => _validMesh;

        [SerializeField] private BeltMeshSO frameBM;
        [SerializeField] private BeltMeshSO beltBM;
        
        [SerializeField] private MeshFilter frameFilter;
        [SerializeField] private MeshFilter beltFilter;
        
        [OnValueChanged(nameof(ChangeSpeedMaterial))]
        [SerializeField] private float speed = 1f;
        
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.forward;
        public Vector3 startDir = Vector3.forward;
        public Vector3 endDir = Vector3.forward;
        
        private IPath _path;
        private bool _validMesh = true;
        private float Length => _path?.GetTotalLength() ?? 0f;
        
        private ConveyorBridge _bridge;
        private MeshRenderer _beltMeshRenderer;
        private int _capacity;

        private void Awake()
        {
            Init();
        }
        
        public void Init()
        {
            CalculateCapacity();
            ChangeSpeedMaterial();
            
            _path = PathFactory.GeneratePathOfType(start, startDir, end, endDir, settings.PATHTYPE);
            
            _bridge = GetComponentInChildren<ConveyorBridge>();
            _bridge.gameObject.SetActive(false);
        }

        public void EnableBridge() => _bridge.gameObject.SetActive(true);
        
        public void DisableBridge() => _bridge.gameObject.SetActive(false);

        public bool MoveItem(Transform resource, float travelledDistance)
        {
            float cumulativeMaxPos = Length;
            
            travelledDistance = math.clamp(travelledDistance, 0f, cumulativeMaxPos);
            
            float percent = travelledDistance / _path.GetTotalLength();
            Vector3 worldPos = _path.GetWorldPointFromPathSpace(percent);
            Quaternion worldRotation = _path.GetRotationAtPoint(percent);
            resource.SetPositionAndRotation(worldPos, worldRotation);

            return travelledDistance >= cumulativeMaxPos;
        }

        public void UpdateMesh(bool finalize = false, Collider[] ignored = null, int startskip = 0, int endskip = 0)
        {
            _validMesh = true;
            _path = PathFactory.GeneratePathOfType(start, startDir, end, endDir, settings.PATHTYPE);

            if (!_path.IsValid) _validMesh = false;
            int length = Mathf.Max(1,(int)(_path.GetTotalLength() * settings.BELT_SEGMENTS_PER_UNIT));

            bool collision = PathFactory.CollisionAlongPath(_path, 0.5f, 0.25f, ~0, ignored, startskip: startskip, endskip: endskip); //only collide belt collideable layer
            if (collision)
            {
                _validMesh = false;
            }

            frameFilter.mesh = BeltMeshGenerator.Generate(_path, frameBM, length, .35f);
            beltFilter.mesh = BeltMeshGenerator.Generate(_path, beltBM, length, .35f, 1f, true);

            _beltMeshRenderer = beltFilter.gameObject.GetComponent<MeshRenderer>();

            if (finalize)
                CalculateCapacity();
        }

        public void SetMaterials(Material frameMat, Material beltMat)
        {
            frameFilter.gameObject.GetComponent<MeshRenderer>().material = frameMat;
            beltFilter.gameObject.GetComponent<MeshRenderer>().material = beltMat;
        }
        
        public void AddCollider()
        {
            frameFilter.gameObject.AddComponent(typeof(MeshCollider));
        }
        
        private void CalculateCapacity()
        {
            _capacity = Mathf.FloorToInt(Length / settings.BELT_SPACING);
        }

        private void ChangeSpeedMaterial()
        {
            _beltMeshRenderer?.material.SetFloat("_Speed", speed);
        }
    }

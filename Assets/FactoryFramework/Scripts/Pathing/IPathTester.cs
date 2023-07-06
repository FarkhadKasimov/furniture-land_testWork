using UnityEngine;

namespace FactoryFramework
{

    public class IPathTester : MonoBehaviour
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.forward;
        public Vector3 startdir = Vector3.forward;
        public Vector3 enddir = Vector3.forward;
        public IPath p;
        public GlobalLogisticsSettings.PathSolveType pt = GlobalLogisticsSettings.PathSolveType.SPLINE;
        public float scaleFactor = 0.5f;

        public BeltMeshSO frameBM;
        public BeltMeshSO beltBM;

        public MeshFilter frameFilter;
        public MeshFilter beltFilter;
        public void Regen()
        {
            p = PathFactory.GeneratePathOfType(start, startdir, end, enddir, pt);
        }

        public void GenerateMesh()
        {
            frameFilter.mesh = BeltMeshGenerator.Generate(p, frameBM, ConveyorLogisticsUtils.settings.BELT_SEGMENTS_PER_UNIT * p.GetTotalLength(), scaleFactor + 0.01f);
            beltFilter.mesh = BeltMeshGenerator.Generate(p, beltBM, ConveyorLogisticsUtils.settings.BELT_SEGMENTS_PER_UNIT * p.GetTotalLength(), scaleFactor, 1f, true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public static class BeltMeshGenerator
    {
        public static Mesh Generate(IPath path, BeltMeshSO model, float segments, float scaleFactor, float uvScaleFactor = 1f, bool generateBeltUVS = false)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();
            float len = path.GetTotalLength();
            float perSegment = 1f / segments;
            SerializableMesh current = model.startCap;

            int triOffset = 0;

            float unitperuv = uvScaleFactor;
            float uvperpath = len / unitperuv;

            for (int i = 0; i < segments; i++)
            {
                if (i >= segments - 1)
                {
                    current = model.endCap;
                }
                else if (i > 0)
                {
                    current = model.midSegment;
                }
                float segStart = i * perSegment;
                float segEnd = (i + 1) * perSegment;
                float meshStart = current.GetMesh().bounds.min.z;
                float meshEnd = current.GetMesh().bounds.max.z;

                int index = 0;
                foreach (Vector3 v in current.GetMesh().vertices)
                {
                    float pathSpaceZ = Remap(v.z, meshStart, meshEnd, segStart, segEnd);
                    Vector3 offsetPos = path.GetWorldPointFromPathSpace(pathSpaceZ);
                    Quaternion rot = path.GetRotationAtPoint(pathSpaceZ);
                    Vector3 rotatedMeshPos = rot * new Vector3(v.x * scaleFactor, v.y * scaleFactor, 0);
                    verts.Add(offsetPos + rotatedMeshPos);
                    normals.Add(current.GetMesh().normals[index]);
                    if (generateBeltUVS)
                        uvs.Add(new Vector2(pathSpaceZ * uvperpath, current.GetMesh().uv[index].y));
                    else
                        uvs.Add(current.GetMesh().uv[index]);
                    index++;
                }
                foreach (int t in current.GetMesh().triangles)
                {
                    tris.Add(t + triOffset);
                }
                triOffset = verts.Count;
            }

            Mesh m = new Mesh();
            m.vertices = verts.ToArray();
            m.triangles = tris.ToArray();
            m.normals = normals.ToArray();
            m.uv = uvs.ToArray();
            return m;
        }
        //shamelessly stolen
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}

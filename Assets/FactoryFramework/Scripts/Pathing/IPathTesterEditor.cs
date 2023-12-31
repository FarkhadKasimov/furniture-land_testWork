using FactoryFramework;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FactoryFramework
{
#if UNITY_EDITOR
    [CustomEditor(typeof(IPathTester))]
    public class IPathTesterEditor : UnityEditor.Editor
    {
        IPathTester holder;
        void OnEnable()
        {
            holder = (IPathTester)target;
            if (holder.p == null)
            {
                holder.Regen();
            }
        }
        void OnSceneGUI()
        {
            //Input();
            Draw();
        }

        public float pathP = 0.5f;
        public Vector3 closestPosTarget = new Vector3(0, 1, 0);

        void Draw()
        {
            bool changed = false;

            //draw anchors
            Handles.color = Color.white;
            var fmh_40_72_638228734247020230 = Quaternion.identity; Vector3 newStartPos = Handles.FreeMoveHandle(holder.start, 0.1f, Vector3.zero, Handles.CircleHandleCap);
            if (newStartPos != holder.start)
            {
                holder.start = newStartPos;
                changed = true;
            }

            Handles.color = Color.black;
            var fmh_48_68_638228734247046110 = Quaternion.identity; Vector3 newEndPos = Handles.FreeMoveHandle(holder.end, 0.1f, Vector3.zero, Handles.CircleHandleCap);
            if (newEndPos != holder.end)
            {
                holder.end = newEndPos;
                changed = true;
            }

            Handles.color = Color.yellow;
            Handles.DrawLine(holder.start, holder.start + holder.startdir * 0.2f);
            Quaternion newStartAng = Handles.Disc(Quaternion.LookRotation(holder.startdir, Vector3.up), holder.start, Vector3.up, 0.3f, false, 5);
            Vector3 newForward = newStartAng * Vector3.forward;
            if (newForward != holder.startdir)
            {
                holder.startdir = newForward;
                changed = true;
            }

            Handles.DrawLine(holder.end, holder.end + holder.enddir * 0.2f);
            Quaternion newEndAng = Handles.Disc(Quaternion.LookRotation(holder.enddir, Vector3.up), holder.end, Vector3.up, 0.3f, false, 5);
            Vector3 newEndForward = newEndAng * Vector3.forward;
            if (newEndForward != holder.enddir)
            {
                holder.enddir = newEndForward;
                changed = true;
            }

            Vector3 pMid = holder.p.GetWorldPointFromPathSpace(pathP);
            Handles.color = Color.red; //right
            Handles.DrawLine(pMid, pMid + holder.p.GetRightAtPoint(pathP) * 0.2f, 4f);

            Handles.color = Color.cyan; // up
            Handles.DrawLine(pMid, pMid + holder.p.GetUpAtPoint(pathP) * 0.2f, 4f);

            Handles.color = Color.green; // forward
            Handles.DrawLine(pMid, pMid + holder.p.GetDirectionAtPoint(pathP) * 0.2f, 4f);

            if (holder.p as SmartPath != null)
            {
                SmartPath bs = holder.p as SmartPath;
                var (a, b) = bs.subPaths[0];
                foreach (var (subP, _) in bs.subPaths)
                {
                    if (subP.GetType() == typeof(ArcPath))
                    {
                        ArcPath arc = subP as ArcPath;
                        Handles.color = Color.blue;
                        Handles.DrawWireArc(arc.center, arc.normal, arc.GetFrom(), arc.angle * Mathf.Rad2Deg, arc.radius, 2);
                    }
                    else if (subP.GetType() == typeof(SegmentPath))
                    {
                        Handles.color = Color.green;
                        SegmentPath segment = subP as SegmentPath;
                        Handles.DrawLine(segment.GetStart(), segment.GetEnd(), 2);
                    }
                }
            }
            else if (holder.p as CubicBezierPath != null)
            {
                CubicBezierPath cbp = holder.p as CubicBezierPath;
                Handles.DrawBezier(cbp.start, cbp.end, cbp.controlPointA, cbp.controlPointB, Color.blue, null, 2f);
            }
            else if (holder.p as SegmentPath != null)
            {
                SegmentPath sp = holder.p as SegmentPath;
                Handles.color = Color.blue;
                Handles.DrawLine(sp.start, sp.end, 2f);
            }

            //Draw closestPoint holder
            Handles.color = Color.yellow;
            var fmh_118_73_638228734247049000 = Quaternion.identity; closestPosTarget = Handles.FreeMoveHandle(closestPosTarget, 0.1f, Vector3.zero, Handles.CircleHandleCap);
            var closest = holder.p.GetClosestPoint(closestPosTarget);
            var fmh_120_51_638228734247051850 = Quaternion.identity; Handles.FreeMoveHandle(closest.Item1, 0.05f, Vector3.zero, Handles.CircleHandleCap);

            if (changed)
                holder.Regen();

        }


        public override void OnInspectorGUI()
        {
            bool changed = false;

            float pathPercent = EditorGUILayout.Slider(pathP, 0, 1);
            if (pathPercent != pathP)
            {
                pathP = pathPercent;
                changed = true;
            }

            GlobalLogisticsSettings.PathSolveType newType = (GlobalLogisticsSettings.PathSolveType)EditorGUILayout.EnumPopup(holder.pt);
            if (newType != holder.pt)
            {
                holder.pt = newType;
                changed = true;
            }

            EditorGUILayout.LabelField("Length: " + holder.p?.GetTotalLength());

            if (changed)
            {
                holder.Regen();
                HandleUtility.Repaint();
                SceneView.RepaintAll();
            }

            holder.frameBM = (BeltMeshSO)EditorGUILayout.ObjectField(holder.frameBM, typeof(BeltMeshSO), true);
            holder.beltBM = (BeltMeshSO)EditorGUILayout.ObjectField(holder.beltBM, typeof(BeltMeshSO), true);
            holder.frameFilter = (MeshFilter)EditorGUILayout.ObjectField(holder.frameFilter, typeof(MeshFilter), true);
            holder.beltFilter = (MeshFilter)EditorGUILayout.ObjectField(holder.beltFilter, typeof(MeshFilter), true);
            holder.scaleFactor = EditorGUILayout.FloatField(holder.scaleFactor);

            if (GUILayout.Button("Generate mesh"))
            {
                holder.GenerateMesh();
            }
        }
    }
#endif
}

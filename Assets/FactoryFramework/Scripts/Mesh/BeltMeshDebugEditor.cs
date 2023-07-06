using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FactoryFramework
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BeltMeshDebug))]
    public class BeltMeshDebugEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Slice"))
            {
                ((BeltMeshDebug)target).Slice();
            }

        }
    }
    #endif
}


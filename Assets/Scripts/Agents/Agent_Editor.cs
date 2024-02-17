#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Agents
{
    [CustomEditor(typeof(Agent))]
    public class Agent_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Agent myTarget = (Agent)target;
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Damage [1]"))
            {
                if (Application.isPlaying) myTarget.Damage(1);
                else Debug.LogWarning("Application not in play mode.");
            }
            if (GUILayout.Button("Damage [3]"))
            {
                if (Application.isPlaying) myTarget.Damage(3);
                else Debug.LogWarning("Application not in play mode.");
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif
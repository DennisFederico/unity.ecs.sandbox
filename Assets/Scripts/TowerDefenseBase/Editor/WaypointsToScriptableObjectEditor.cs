using TowerDefenseBase.Helpers;
using UnityEditor;
using UnityEngine;

namespace TowerDefenseBase.Editor {
    [CustomEditor(typeof(WaypointsToScriptableObject))]
    public class WaypointsToScriptableObjectEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {

            serializedObject.UpdateIfRequiredOrScript();
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            
            // Draw the waypointsScriptableObject property Transform
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waypointsScriptableObject"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waypointsReferences"));

            // Draw the button action to parse and store into a scriptable object
            WaypointsToScriptableObject myScript = (WaypointsToScriptableObject)target;
            if (GUILayout.Button("Store Positions")) {
                myScript.StoreWaypoints();
            }

            // Draw the positions property as read-only
            SerializedProperty positionsProperty = serializedObject.FindProperty("positions");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(positionsProperty);
            EditorGUI.EndDisabledGroup();

            // Draw the rest of the inspector
            DrawPropertiesExcluding(serializedObject,
                "waypointsScriptableObject",
                "waypointsReferences", 
                "positions", "m_Script");
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
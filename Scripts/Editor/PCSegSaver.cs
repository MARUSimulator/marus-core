using UnityEditor;
using UnityEngine;

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// Custom editpr for PointCloudSemanticSegmentationSaver component.
    /// Show all defined classes and belonging indices in current scene.
    /// </summary>
    [CustomEditor(typeof(PointCloudSemanticSegmentationSaver))]
    public class PCSegSaver : Editor
    {
        PointCloudClassDefinition [] classes;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var PCCD = target as PointCloudClassDefinition;
            classes = FindObjectsOfType<PointCloudClassDefinition>();
            EditorGUILayout.LabelField("Classes defined in this scene:");
            EditorGUI.BeginDisabledGroup(true);
            foreach(var c in classes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"Name: ");
                EditorGUILayout.TextField(c.ClassName);
                EditorGUILayout.PrefixLabel($"Index: ");
                EditorGUILayout.IntField(c.Index);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel($"Name: ");
            EditorGUILayout.TextField("Other");
            EditorGUILayout.PrefixLabel($"Index: ");
            EditorGUILayout.IntField(0);
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }
    }
}
using UnityEditor;

namespace Marus.ObjectAnnotation
{
	[CustomEditor(typeof(PointCloudClassDefinition))]
    public class PointCloudClassDefinitionEditor : Editor
    {
        PointCloudClassDefinition [] classes;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var PCCD = target as PointCloudClassDefinition;
            classes = FindObjectsOfType<PointCloudClassDefinition>();
            foreach (var c in classes)
            {
                if (c.GetInstanceID() != target.GetInstanceID())
                {

                    if (c.ClassName == PCCD.ClassName)
                    {
                        EditorGUILayout.HelpBox($"Class name {PCCD.ClassName} already used.", MessageType.Error);
                    }
                    else if (c.Index == PCCD.Index)
                    {
                        EditorGUILayout.HelpBox($"Class index {PCCD.Index} already used. Please set it to unused value.", MessageType.Error);
                    }
                }
                else if (PCCD.Index == 0)
                {
                    EditorGUILayout.HelpBox($"Class index 0 is reserved for unlabeled objects (class Other).", MessageType.Error);
                }
            }
        }
    }
}
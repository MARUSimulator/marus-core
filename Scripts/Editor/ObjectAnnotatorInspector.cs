using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectAnnotator))]
public class ObjectAnnotatorInspector : Editor
{
    ObjectAnnotator script;
    void OnEnable()
    {
        script = (ObjectAnnotator) target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (script.SplitTrainValTest)
        {
            if ((script.TrainSize + script.TestSize) == 100)
            {
                EditorGUILayout.HelpBox("Train and test size sum up to 100. This means validation subset will be empty.", MessageType.Warning);
            }
            if ((script.TrainSize + script.TestSize) > 100 || (script.TrainSize + script.TestSize) < 0)
            {
                EditorGUILayout.HelpBox("Sum of train and test subsets must be between 0 and 100.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("Validation Size", (100 - (script.TrainSize + script.TestSize)).ToString());
            }
        }
    }
}

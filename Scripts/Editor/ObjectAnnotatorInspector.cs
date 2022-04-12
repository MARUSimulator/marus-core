// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEditor;

namespace Marus.ObjectAnnotation
{
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
}
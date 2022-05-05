// Copyright 2021 Laboratory for Underwater Systems and Technologies (LABUST)
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
using UnityEngine;
using Marus.Utils;

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// Custom editor for ObjectAnnotator component
    /// </summary>
    [CustomEditor(typeof(CameraObjectDetectionSaver))]
    public class CameraObjectDetectionSaverEditor : Editor
    {
        CameraObjectDetectionSaver script;
        void OnEnable()
        {
            script = (CameraObjectDetectionSaver) target;
        }

        /// <summary>
        /// Sanity checks for test/val/test sizes.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var t = target as CameraObjectDetectionSaver;
            if (t.RaycastCheck)
            {
                EditorGUILayout.HelpBox("Objects must have (Mesh) Collider.", MessageType.Info);
            }

            serializedObject.DrawInspectorExcept(new string[]{ "TestSize"});
            if (script.GenerateTestSubset)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TestSize"), new GUIContent("Test Size"));

                script.TestSize = 100 - script.TrainSize - script.ValSize;
                serializedObject.ApplyModifiedProperties();

                if ((script.TrainSize + script.ValSize) == 100)
                {
                    EditorGUILayout.HelpBox("Train and val size sum up to 100. This means test subset will be empty.", MessageType.Warning);
                }
                if ((script.TrainSize + script.ValSize) > 100 || (script.TrainSize + script.TestSize) < 0)
                {
                    script.ValSize = 100 - script.TrainSize;
                    script.TestSize = 0;
                }
            }
            else
            {
                if ((script.TrainSize + script.ValSize) != 100)
                {
                    script.ValSize = 100 - script.TrainSize;
                }
            }
        }
    }
}

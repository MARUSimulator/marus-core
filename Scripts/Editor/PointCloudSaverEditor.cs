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

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// Custom editpr for PointCloudSemanticSegmentationSaver component.
    /// Show all defined classes and belonging indices in current scene.
    /// </summary>
    [CustomEditor(typeof(PointCloudSemanticSegmentationSaver))]
    public class PointCloudSaverEditor : Editor
    {
        AnnotationClassDefinition [] classes;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var PCCD = target as AnnotationClassDefinition;
            classes = FindObjectsOfType<AnnotationClassDefinition>();
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

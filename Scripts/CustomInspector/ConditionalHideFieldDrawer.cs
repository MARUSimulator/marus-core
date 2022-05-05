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

using UnityEngine;
using UnityEditor;

namespace Marus.CustomInspector
{
    [CustomPropertyDrawer(typeof(HideInRuntimeInspectorAttribute))]
    [CustomPropertyDrawer(typeof(ConditionalHideInInspectorAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label )
        {
            var enabled = GetConditionalHideAttributeResult(attribute, property);

            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label )
        {
            var enabled = GetConditionalHideAttributeResult(attribute, property);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                //The property is not being drawn
                //We want to undo the spacing added before and after the property
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(PropertyAttribute propertyAttr, SerializedProperty property )
        {
            var enabled = true;

            if (propertyAttr is HideInRuntimeInspectorAttribute hideInRuntime)
            {
                return !EditorApplication.isPlaying;
            }

            if (propertyAttr is ConditionalHideInInspectorAttribute condHAtt)
            {
                var propertyPath =
                    property.propertyPath; //returns the property path of the property we want to apply the attribute to
                string conditionPath;

                if (!string.IsNullOrEmpty(condHAtt.ConditionalSourceField))
                {
                    //Get the full relative property path of the sourcefield so we can have nested hiding
                    conditionPath =
                        propertyPath.Replace(property.name,
                            condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
                    var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

                    if (sourcePropertyValue != null)
                    {
                        enabled = CheckPropertyType(sourcePropertyValue, condHAtt.Value);
                    }
                    else
                    {
                        //Debug.LogWarning("Attempting to use a ConditionalHideInInspectorAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
                    }
                }

                if (condHAtt.Inverse) enabled = !enabled;

                return enabled;

            }
            return true;
        }

        private bool CheckPropertyType(SerializedProperty sourcePropertyValue, object value)
        {
            switch ( sourcePropertyValue.propertyType )
            {
                case SerializedPropertyType.Boolean:
                    return sourcePropertyValue.boolValue;
                case SerializedPropertyType.Enum:
                    return sourcePropertyValue.enumValueIndex != (int)value;
                case SerializedPropertyType.ObjectReference:
                    return sourcePropertyValue.objectReferenceValue != null;
                default:
                    Debug.LogError("Data type of the property used for conditional hiding [" +
                                    sourcePropertyValue.propertyType + "] is currently not supported");
                    return true;
            }
        }
    }
}

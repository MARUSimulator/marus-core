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
using System;

namespace Marus.CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ConditionalHideInInspectorAttribute : PropertyAttribute
    {
        public readonly string ConditionalSourceField;
        public bool HideInInspector = false;
        public bool Inverse = false;
        public object Value;

        // Use this for initialization
        public ConditionalHideInInspectorAttribute(string conditionalSourceField, bool inverse=false)
        {
            ConditionalSourceField = conditionalSourceField;
            Inverse = inverse;
        }

        public ConditionalHideInInspectorAttribute(string conditionalSourceField, bool inverse=false, object value=null)
        {
            ConditionalSourceField = conditionalSourceField;
            Inverse = inverse;
            Value = value;
        }
    }
}

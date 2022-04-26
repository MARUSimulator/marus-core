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
using Marus.Utils;
using System.Collections.Generic;

namespace Marus.ObjectAnnotation
{

    /// <summary>
    /// Singleton class that holds all object instance ids for object segmentation.
    /// </summary>
    public class AnnotationHandler : Singleton<AnnotationHandler>
    {
        Dictionary<int, int> ObjectInstanceIds;
        int currentIndex;
        protected override void Initialize()
        {
            ObjectInstanceIds = new Dictionary<int, int>();
            currentIndex = 1;
        }

        /// <summary>
        /// Creates and sets unique id for gameObject.
        /// </summary>
        /// <param name="obj">game object</param>
        /// <returns>Instance id</returns>
        public int SetUniqueId(GameObject obj)
        {
            var id = currentIndex;
            var ObjectInstanceId = obj.GetInstanceID();
            if (!ObjectInstanceIds.ContainsKey(ObjectInstanceId))
            {
                ObjectInstanceIds.Add(ObjectInstanceId, currentIndex++);
            }
            else
            {
                id = ObjectInstanceIds[ObjectInstanceId];
            }
            return id;
        }
    }
}

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Marus.Utils;
#if CREST_OCEAN
using Crest;
#endif

namespace Marus.Ocean
{
    public class WaterHeightSampler : Singleton<WaterHeightSampler>
    {
        bool crestInScene = false;

        void Awake()
        {
    #if CREST_OCEAN
            if (Object.FindObjectOfType<OceanRenderer>() != null)
            {
                crestInScene = true;
            }
    #endif
        }


        /// <summary>
        /// Returns the water level at given location.
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="z">z-coordinate</param>
        /// <returns>Water level</returns>
        public float GetWaterLevel(Vector3 position, float  minSpatialLength = 0.5f)
        {
    #if CREST_OCEAN
            Crest.SampleHeightHelper _sampleHeightHelper = new Crest.SampleHeightHelper();
            _sampleHeightHelper.Init(position, minSpatialLength, true);
                        
            if (_sampleHeightHelper.Sample(out var height))
            {
                return height;
            }
    #endif
            return 0.0f;
        }

        public void GetWaterLevel(Vector3[] i_points, float[] o_heights, int i_array_size, float i_minSpatialLength = 0.5f)
        {
#if CREST_OCEAN
            if (crestInScene)
            {    var collProvider = OceanRenderer.Instance?.CollisionProvider;
                if (collProvider == null)
                {
                    for (int i = 0; i < i_array_size; i++)
                    {
                        o_heights[i] = 0.0f;
                    }
                }

                var status = collProvider.Query(GetHashCode(), i_minSpatialLength, i_points, o_heights, null, null);

                if (!collProvider.RetrieveSucceeded(status))
                {
                    for (int i = 0; i < i_array_size; i++)
                    {
                        o_heights[i] = OceanRenderer.Instance.SeaLevel;
                    }
                }
            }
            else
            {
                for (int i = 0; i < i_array_size; i++)
                {
                    o_heights[i] = 0.0f;
                }
            }
#else
            for (int i = 0; i < i_array_size; i++)
            {
                o_heights[i] = 0.0f;
            }
#endif
        }

        public float[] GetWaterLevel(List<Vector3> points, float  minSpatialLength = 0.5f)
        {
            float[] heights = new float[points.Count];
#if CREST_OCEAN
            Vector3[] _queryPos = points.ToArray();
            Vector3[] _queryResult = new Vector3[points.Count];

            var collProvider = OceanRenderer.Instance?.CollisionProvider;
            if (collProvider == null)
            {
                return heights;
            }

            var status = collProvider.Query(GetHashCode(), minSpatialLength, _queryPos, _queryResult, null, null);

            if (!collProvider.RetrieveSucceeded(status))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    heights[i] = OceanRenderer.Instance.SeaLevel;
                }
                return heights;
            }

            for (int i = 0; i < points.Count; i++)
            {
                heights[i] = _queryResult[i].y + OceanRenderer.Instance.SeaLevel;
            }
#endif
            return heights;
        }
    }
}
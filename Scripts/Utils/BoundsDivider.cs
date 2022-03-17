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

namespace UniColliderInterpolator
{
    public static class BoundsDivider
    {
        public static void Divide(Bounds bounds, float edgeLength, out Vector3[] positions, out int xDivisionCount,
            out int yDivisionCount,
            out int zDivisionCount)
        {
            Vector3 min, max;
            GetMinMax(bounds, out min, out max);
            var center = (min + max) / 2f;

            xDivisionCount = 1;
            var minX = center.x;
            while (minX > min.x)
            {
                minX -= edgeLength;
                xDivisionCount++;
            }

            var maxX = center.x;
            while (maxX < max.x)
            {
                maxX += edgeLength;
                xDivisionCount++;
            }

            yDivisionCount = 1;
            var minY = center.y;
            while (minY > min.y)
            {
                minY -= edgeLength;
                yDivisionCount++;
            }

            var maxY = center.y;
            while (maxY < max.y)
            {
                maxY += edgeLength;
                yDivisionCount++;
            }

            zDivisionCount = 1;
            var minZ = center.z;
            while (minZ > min.z)
            {
                minZ -= edgeLength;
                zDivisionCount++;
            }

            var maxZ = center.z;
            while (maxZ < max.z)
            {
                maxZ += edgeLength;
                zDivisionCount++;
            }

            positions = new Vector3[xDivisionCount * yDivisionCount * zDivisionCount];
            var index = 0;
            for (var x = minX; x <= maxX; x += edgeLength)
            {
                for (var y = minY; y <= maxY; y += edgeLength)
                {
                    for (var z = minZ; z <= maxZ; z += edgeLength)
                    {
                        positions[index] = new Vector3(x, y, z);
                        index++;
                    }
                }
            }
        }

        static void GetMinMax(Bounds bounds, out Vector3 min, out Vector3 max)
        {
            var tmpMin = bounds.min;
            var tmpMax = bounds.max;
            var minZ = Mathf.Min(tmpMin.z, tmpMax.z);
            var maxZ = Mathf.Max(tmpMin.z, tmpMax.z);
            tmpMin.z = minZ;
            tmpMax.z = maxZ;
            min = tmpMin;
            max = tmpMax;
        }
    }
}

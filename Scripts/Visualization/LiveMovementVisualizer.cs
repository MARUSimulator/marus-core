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

using Labust.Visualization.Primitives;
using UnityEngine;

namespace Labust.Visualization
{
    /// <summary>
    /// This class implements visualization of past movement.
    /// Attach script to an object and it will visualize it's trajectory over time.
    /// Path is visualized with points and lines between those points.
    /// </summary>
    public class LiveMovementVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Point color
        /// </summary>
        public Color PointColor = Color.blue;

        /// <summary>
        /// Point size
        /// </summary>
        [Range(0, 1)]
        public float PointSize = 0.1f;

        /// <summary>
        /// Line color
        /// </summary>
        public Color LineColor = Color.yellow;

        /// <summary>
        /// Line thickness
        /// </summary>
        [Range(0, 0.5f)]
        public float LineThickness = 0.05f;

        /// <summary>
        /// Draw point frequency in Hz
        /// </summary>
        public float RefreshRateHz = 2;

        /// <summary>
        /// Track in 3D space if true, otherwise track in 2D plane
        /// with y coordinate set to 0
        /// </summary>
        public bool TrackIn3D = false;

        /// <summary>
        /// Delete points older than FadeOutAfterSecs seconds if enabled.
        /// </summary>
        public bool EnableFadeout = true;

        /// <summary>
        /// Point lifetime in seconds if EnableFadeout is set to true.
        /// </summary>
        public float FadeOutAfterSecs = 3;

        /// <summary>
        /// Minimum distance difference to save a point in path.
        /// This means that new points will not be saved and drawn if object is not moving.
        /// </summary>
        public float MinimumDistanceDelta = 0.1f;

        private float elapsedTime = 0;
        private Path path;
        private Vector3 lastPosition;
        private float lastPointSize;
        private Color lastPointColor;

        private float lastLineThickness;
        private Color lastLineColor;

        void Awake()
        {
            lastPosition = transform.position;
            path = new Path(LineThickness, LineColor);
            lastPointSize = PointSize;
            lastPointColor = PointColor;
            lastLineThickness = LineThickness;
            lastLineColor = LineColor;
        }

        void Update()
        {
            // delete old points if fadeout is enabled
            if (EnableFadeout)
            {
                path.RefreshAndFade(FadeOutAfterSecs);
            }

            elapsedTime += Time.deltaTime;

            /// add point to path depending on frequency
            if (elapsedTime > 1 / RefreshRateHz)
            {
                elapsedTime = 0;
                if (Vector3.Distance(lastPosition, transform.position) < MinimumDistanceDelta)
                {
                    return;
                }

                var pos = transform.position;
                if (!TrackIn3D)
                {
                    pos.y = 0;
                }
                path.AddPointToPath(pos, PointSize, PointColor);

                lastPosition = transform.position;
            }

            // change path properties in real time
            if (lastPointSize != PointSize)
            {
                path.SetPointSize(PointSize);
                lastPointSize = PointSize;
            }

            if (lastPointColor != PointColor)
            {
                path.SetPointColor(PointColor);
                lastPointColor = PointColor;
            }

            if (lastLineThickness != LineThickness)
            {
                path.SetLineThickness(LineThickness);
                lastLineThickness = LineThickness;
            }

            if (lastLineColor != LineColor)
            {
                path.SetLineColor(LineColor);
                lastLineColor = LineColor;
            }
            // draw path every frame
            path.Draw();
        }
    }
}
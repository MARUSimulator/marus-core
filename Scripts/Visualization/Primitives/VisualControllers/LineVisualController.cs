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
using Labust.Visualization.Primitives;

namespace Labust.Visualization
{
	/// <summary>
	/// Used for line appearance customization
	/// </summary>
	public class LineVisualController : MonoBehaviour {

		public Line MyLine;

		public Color LineColor = Color.red;

		[Range(0f, 4f)]
		public float LineThickness = 0.01f;

		private Color _lastLineColor;
		private float _lastLineThickness;

		private void Start() {
			LineThickness = MyLine.Thickness;
			LineColor = MyLine.LineColor;
		}
		void Update() {
			if (_lastLineThickness != LineThickness)
			{
				MyLine.SetThickness(LineThickness);
				_lastLineThickness = LineThickness;
			}

			if (_lastLineColor != LineColor)
			{
				MyLine.SetColor(LineColor);
				_lastLineColor = LineColor;
			}
		}

		void OnDestroy() {
			MyLine.Destroy();
		}
	}
}

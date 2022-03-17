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
	/// Used for point appearance customization
	/// </summary>
	public class PointVisualController : MonoBehaviour {

		public Point MyPoint;

		public Color PointColor = Color.white;
		[Range(0.1f, 4f)]
		public float PointSize = 0.7f;

		private Color _lastPointColor;
		private float _lastPointSize;

		void Start()
		{
			PointSize = MyPoint.PointSize;
			PointColor = MyPoint.PointColor;
		}

		void Update() {
			if (_lastPointSize != PointSize)
			{
				MyPoint.SetSize(PointSize);
				_lastPointSize = PointSize;
			}

			if (_lastPointColor != PointColor)
			{
				MyPoint.SetColor(PointColor);
				_lastPointColor = PointColor;
			}
		}

		void OnDestroy() {
			MyPoint.Destroy();
		}
	}
}

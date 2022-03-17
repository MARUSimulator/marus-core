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
	/// Used for path appearance customization
	/// </summary>
	public class PathVisualController : MonoBehaviour {

		public Path MyPath;

		public Color PointColor = Color.white;
		[Range(0.1f, 4f)]
		public float PointSize = 0.7f;
		public Color LineColor = Color.red;
		[Range(0f, 2f)]
		public float LineThickness = 0.01f;

		private Color _lastPointColor;
		private Color _lastLineColor;
		private float _lastPointSize;
		private float _lastLineThickness;

		private void Start() {
			PointColor = MyPath.PointColor;
			PointSize = MyPath.PointSize;
			LineColor = MyPath.LineColor;
			LineThickness = MyPath.LineThickness;
		}

		void Update() {
			if (_lastPointSize != PointSize)
			{
				MyPath.SetPointSize(PointSize);
				_lastPointSize = PointSize;
			}

			if (_lastPointColor != PointColor)
			{
				MyPath.SetPointColor(PointColor);
				_lastPointColor = PointColor;
			}

			if (_lastLineThickness != LineThickness)
			{
				MyPath.SetLineThickness(LineThickness);
				_lastLineThickness = LineThickness;
			}

			if (_lastLineColor != LineColor)
			{
				MyPath.SetLineColor(LineColor);
				_lastLineColor = LineColor;
			}
		}

		public void SetPointColor(Color c)
		{
			PointColor = c;
			MyPath.SetPointColor(c);
		}

		public void SetLineColor(Color c)
		{
			LineColor = c;
			MyPath.SetLineColor(c);
		}

		public void SetPointSize(float s)
		{
			PointSize = s;
			MyPath.SetPointSize(s);
		}

		public void SetLineThickness(float s)
		{
			LineThickness = s;
			MyPath.SetLineThickness(s);
		}

		void OnDestroy() {
			MyPath.Destroy();
		}
	}
}

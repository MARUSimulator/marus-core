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
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Marus.StatisticsUI
{
	/// <summary>
	/// This class disables scrollview when dragging.
	/// </summary>
	public class ScrollViewController : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
		public void OnBeginDrag(PointerEventData data)
		{
			GetComponent<ScrollRect>().enabled = false;
		}

		public void OnEndDrag(PointerEventData data)
		{
			GetComponent<ScrollRect>().enabled = true;
		}
	}
}

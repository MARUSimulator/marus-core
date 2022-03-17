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

namespace Labust.Sensors.AIS
{
	public enum NavigationStatus
	{
		UnderWayUsingEngine = 0,
		AtAnchor = 1,
		NotUnderCommand = 2,
		RestrictedManeuverability = 3,
		ConstrainedByHerDraught = 4,
		Moored = 5,
		Aground = 6,
		EngagedInFishing = 7,
		UnderWaySailing = 8,
		ReservedForFutureAmendmentOfNavigationalStatusForHsc = 9,
		ReservedForFutureAmendmentOfNavigationalStatusForWig = 10,
		ReservedForFutureUse1 = 11,
		ReservedForFutureUse2 = 12,
		ReservedForFutureUse3 = 13,
		AisSartIsActive = 14,
		NotDefined = 15
	}
}

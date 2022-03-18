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

namespace Marus.Sensors.AIS
{
	public enum AISMessageType
	{
		PositionReportClassA = 1,
		PositionReportClassAAssignedSchedule = 2,
		PositionReportClassAResponseToInterrogation = 3,
		BaseStationReport = 4,
		StaticAndVoyageRelatedData = 5,
		BinaryAddressedMessage = 6,
		BinaryAcknowledge = 7,
		BinaryBroadcastMessage = 8,
		StandardSarAircraftPositionReport = 9,
		UtcAndDateInquiry = 10,
		UtcAndDateResponse = 11,
		AddressedSafetyRelatedMessage = 12,
		SafetyRelatedAcknowledgement = 13,
		SafetyRelatedBroadcastMessage = 14,
		Interrogation = 15,
		AssignmentModeCommand = 16,
		DgnssBinaryBroadcastMessage = 17,
		StandardClassBCsPositionReport = 18,
		ExtendedClassBCsPositionReport = 19,
		DataLinkManagement = 20,
		AidToNavigationReport = 21,
		ChannelManagement = 22,
		GroupAssignmentCommand = 23,
		StaticDataReport = 24,
		SingleSlotBinaryMessage = 25,
		MultipleSlotBinaryMessageWithCommunicationsState = 26,
		PositionReportForLongRangeApplications = 27
	}
}

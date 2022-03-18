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
    /// <summary>
    /// This class serves as a base for AIS message types.
    /// For more reference see <see cref="!:https://www.navcen.uscg.gov/?pageName=AISMessages">here.</see>
    /// </summary>
    public abstract class AisMessage
    {
        /// <summary>
        /// Message type. Is always 1, 2 or 3.
        /// </summary>
        public AISMessageType MessageType { get; set; }

        public AisDevice sender { get; set; }

        /// <summary>
        /// Maritime Mobile Service Identity
        /// Unique 9 digit number assigned to radio or AIS unit.
        /// </summary>
        public int MMSI { get; set; }
    }
}

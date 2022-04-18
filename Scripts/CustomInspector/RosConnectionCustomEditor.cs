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
using UnityEditor;
using Marus.Networking;

[CustomEditor(typeof(RosConnection))]
public class RosConnectionCustomEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var conn = (RosConnection)target;
        if (EditorApplication.isPlaying &&
            !conn.IsConnected && !conn.IsConnecting)
        {
            if (GUILayout.Button("Try reconnect"))
            {
                conn.Connect();
            }
        }
    }
}
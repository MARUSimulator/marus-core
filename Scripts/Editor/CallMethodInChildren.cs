using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

#if UNITY_EDITOR
public class CallMethodInChildren : MonoBehaviour
{
    public String callbackName;
}

[CustomEditor(typeof(CallMethodInChildren))]
public class CallMethodInChildrenInspector : Editor
{
    // Start is called before the first frame update

    public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Call method in children"))
            {
                UpdateChildren();
            }
        }
    // Update is called once per frame
    void UpdateChildren()
    {
        var conn = (CallMethodInChildren) target;
        if(!String.IsNullOrEmpty(conn.callbackName))
        {
            conn.gameObject.BroadcastMessage(conn.callbackName, null, SendMessageOptions.DontRequireReceiver);
        }
    }
}
#endif
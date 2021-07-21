using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(BoxVolume)), CanEditMultipleObjects]
public class BoundsEditor : Editor
{
    private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

    const float SIZE = 5;
    // the OnSceneGUI callback uses the Scene view camera for drawing handles by default
    protected virtual void OnSceneGUI()
    {
        BoxVolume boxVolume = (BoxVolume)target;

        // copy the target object's data to the handle
        if (boxVolume.Type == BoxVolume.BoxType.Box)
        {
            DrawBoxVolumeEditor(boxVolume);
        }
        else if (boxVolume.Type == BoxVolume.BoxType.HalfSpace)
        {
            DrawHalfSpaceEditor(boxVolume);
        }
    }

    private void DrawHalfSpaceEditor(BoxVolume boxVolume)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, boxVolume.rotate, Vector3.one);
        var center = boxVolume.bounds.center;
        using(new Handles.DrawingScope(matrix))
        {
            var p1 = TransformPoint(matrix, boxVolume.transform, 
                center + SIZE * new Vector3(1, 0, 0));
            var p2 = TransformPoint(matrix, boxVolume.transform, 
                center + SIZE * new Vector3(-1, 0, 0));
            var p3 =  TransformPoint(matrix, boxVolume.transform, 
                center + SIZE * new Vector3(0, 0, 1));
            var p4 =  TransformPoint(matrix, boxVolume.transform, 
                center + SIZE * new Vector3(0, 0, -1));
            Handles.DrawSolidRectangleWithOutline(new Vector3[] {p1, p3, p2, p4}, Color.white, Color.green);
        }
    }

    private void DrawBoxVolumeEditor(BoxVolume boxVolume)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, boxVolume.rotate, Vector3.one);
        using(new Handles.DrawingScope(matrix))
        {
            m_BoundsHandle.center = TransformPoint(
                matrix, boxVolume.transform, boxVolume.bounds.center);
            m_BoundsHandle.size = boxVolume.bounds.size;        
            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(boxVolume, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = InverseTransformPoint(
                    matrix, boxVolume.transform, m_BoundsHandle.center);
                newBounds.size = m_BoundsHandle.size;
                boxVolume.bounds = newBounds;
            }
        }
    }

    Vector3 TransformPoint(Matrix4x4 matrix, Transform transform, Vector3 point)
    {
        return 
            matrix.inverse.MultiplyPoint3x4(
                transform.TransformPoint(
                    point
            ));
    }

    Vector3 InverseTransformPoint(Matrix4x4 matrix, Transform transform, Vector3 point)
    {
        return 
            matrix.MultiplyPoint3x4(
                transform.InverseTransformPoint(
                    point
            ));

    }
}
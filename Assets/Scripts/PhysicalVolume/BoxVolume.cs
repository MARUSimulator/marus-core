using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BoxVolume : MonoBehaviour
{
    public enum BoxType { Box, HalfSpace, World }

    public BoxType Type;
    public Quaternion rotate = Quaternion.identity;
    public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
}
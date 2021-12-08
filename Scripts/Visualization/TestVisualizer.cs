using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Labust.Visualization;
using UnityEngine;

public class TestVisualizer : MonoBehaviour
{

    GameObject _sphere;
    void Start()
    {
        var vis = Visualizer.Instance;
        List<Vector3> path = new List<Vector3> {new Vector3(0, 1, 0), new Vector3(14, 1, 0), new Vector3(25, 1, 0)};
        vis.AddPath(path, "test path");
        vis.AddPoint(new Vector3(0, 2, 10), "test point", 0.5f);
        _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject.Destroy(_sphere.GetComponent<Collider>());
        _sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        _sphere.transform.position = new Vector3(0f, 0f, 1f);
        _sphere.name = "test transform";

        var transform = vis.AddTransform(_sphere.transform, "test transform");
        vis.AddLine(new Vector3(0, 1, 0), new Vector3(0, 10, 0), "test line");
        var tf = TfHandler.Instance;
    }

    void FixedUpdate()
    {
        var angVel = 1.3f;
        var linVel = angVel;
        float newYAngle = _sphere.transform.eulerAngles.y + Time.fixedDeltaTime * angVel * Mathf.Rad2Deg;
        _sphere.transform.eulerAngles = new Vector3(_sphere.transform.eulerAngles.x, newYAngle, _sphere.transform.eulerAngles.z);

        var sinRot = Mathf.Sin(newYAngle * Mathf.Deg2Rad);
        var cosRot = Mathf.Cos(newYAngle * Mathf.Deg2Rad);
        _sphere.transform.position += new Vector3(sinRot, 0, cosRot) * Time.fixedDeltaTime * linVel;
    }
}

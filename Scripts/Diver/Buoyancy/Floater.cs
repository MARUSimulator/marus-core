using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour {
	public float waterLevel, floatHeight;
	public Vector3 buoyancyCentreOffset;
	public float bounceDamp;

    public float forceFactor;
    public bool buoyUp = true;
	

	void FixedUpdate () {
		Vector3 actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
		forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);
		
		if (forceFactor > 0f && buoyUp) {
			Vector3 uplift = -Physics.gravity * (forceFactor - GetComponent<Rigidbody>().velocity.y * bounceDamp);
			GetComponent<Rigidbody>().AddForceAtPosition(uplift, actionPoint);
		}
	}
}

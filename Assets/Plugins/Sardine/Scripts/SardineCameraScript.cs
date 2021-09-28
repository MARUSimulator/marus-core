using UnityEngine;
using System.Collections;

public class SardineCameraScript : MonoBehaviour {
	public GameObject target;
	public float turnSpeed=.2f;
	
	void FixedUpdate(){
		transform.position = Vector3.Lerp (transform.position,target.transform.position,Time.deltaTime);
		transform.rotation = Quaternion.Lerp (transform.rotation,target.transform.rotation,Time.deltaTime*turnSpeed);
	}
}

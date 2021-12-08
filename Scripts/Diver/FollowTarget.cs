using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {

    public Transform target;
	
	// Update is called once per frame
	void LateUpdate () {

        transform.position = target.position;
	
	}
}

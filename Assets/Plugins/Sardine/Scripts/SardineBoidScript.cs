using UnityEngine;
using System.Collections;

public class SardineBoidScript : MonoBehaviour {
	public Transform maneru;
	public float speed=1.5f;
	public GameObject[] eyes;
	Rigidbody iwasirigid;
	public SardineBoidsController sardineBoidsController;
	public float maneruSpeed=1f;
	
	void Start () {
		iwasirigid = GetComponent<Rigidbody> ();
		GetComponent<Animator> ().SetFloat ("Forward",speed);
	}

	void Update () {
		Miru ();
		Maneru ();
		iwasirigid.velocity= transform.forward * speed;
	}


	void Miru(){
		RaycastHit hitInfo;
		foreach (GameObject eye in eyes) {
			if (Physics.Raycast (eye.transform.position, eye.transform.up, out hitInfo, 100f)) {
				if(hitInfo.collider.name=="SardineCol"){
				maneru = hitInfo.transform;
				}
			}
		}
	}

	void Maneru(){
		if (maneru != null) {
			transform.rotation =Quaternion.Slerp(transform.rotation, maneru.rotation, Time.deltaTime * maneruSpeed);
		}
	}
}

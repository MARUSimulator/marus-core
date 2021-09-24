using UnityEngine;
using System.Collections;

public class IwasiOcean : MonoBehaviour {
	public int iwasicount=0;
	public GameObject zaru;
	public bool isCleared=false;
	public int maxIwasicount=10;
	
	void OnTriggerEnter(Collider other){
		Rigidbody iwasirigid = other.GetComponentInParent<Rigidbody> ();
		other.transform.root.rotation = Quaternion.identity;
		IwasiMove aniwasimove = other.GetComponentInParent<IwasiMove> ();
		if (aniwasimove.enabled == false){
			AudioSource iwasiaudio=other.GetComponentInParent<AudioSource>();
			iwasiaudio.Play();
			aniwasimove.enabled = true;
			iwasicount++;
			if(iwasicount>maxIwasicount-1 & isCleared==false){
				Destroy(zaru);
				isCleared=true;
			}
		}
		iwasirigid.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
	}
}

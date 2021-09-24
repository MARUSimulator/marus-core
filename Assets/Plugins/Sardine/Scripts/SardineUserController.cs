using UnityEngine;
using System.Collections;

public class SardineUserController : MonoBehaviour {
	SardineCharacter sardineCharacter;

	void Start () {
		sardineCharacter = GetComponent<SardineCharacter> ();
	}
	

	void Update () {

		if (Input.GetKeyDown(KeyCode.K)) {
			sardineCharacter.TurnRight();
		}
		if (Input.GetKeyDown(KeyCode.H)) {
			sardineCharacter.TurnLeft();
		}
		if (Input.GetKeyDown(KeyCode.M)) {
			sardineCharacter.TurnDown();			
		}

		if (Input.GetKeyDown(KeyCode.J)) {
			sardineCharacter.MoveForward();		
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			sardineCharacter.TurnUp();
		}


		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		sardineCharacter.Move (v,h);


	}
}

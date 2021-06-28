using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labust.Controllers
{
	/// <summary>
	/// Vessel controller that directly controls velocity and orientation to move and rotate towards the Target position
	/// </summary>
	public class VesselVelocityController : MonoBehaviour
	{
		public Transform Target;
		private Boolean stop;
		public float Speed = 0.5f;
		public float RotationSpeed = 1.0f;
		public float StoppingDistance = 1f;
		public float RestartDistance = 5f;

		public void Awake()
		{
			stop = false;
		}

		public void FixedUpdate()
		{  
			RotateTowards();
			MoveTowards();
		}

		public void MoveTowards()
		{
			
			float dist = Vector3.Distance(new Vector3(Target.position.x, 0, Target.position.z), new Vector3(transform.position.x, 0, transform.position.z));
			
			//Stop the vessel when close enough to target
			if (dist < StoppingDistance)
			{
				stop = true;
			}
				
			
			//start the vessel when target changed
			if (stop && dist > RestartDistance)
			{
				stop = false;
			}

			if (!stop)
			{
				//use distance to slow down when approaching target position
				transform.position += transform.forward * Time.deltaTime * Mathf.Sqrt(dist) * Speed;
			}
		}

		public void RotateTowards() 
		{
			
			Vector3 relativePos = Target.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
			Vector3 targetAngles = targetRotation.eulerAngles;
			Quaternion target = Quaternion.Euler(0, targetAngles.y, 0);
			float error = Vector3.Angle(relativePos, transform.forward);

			if (!stop)
			{
				//use error in interpolation ratio to minimize rotation when approaching optimal course
				transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, target, Time.deltaTime * RotationSpeed);
			}
		}
	}
}

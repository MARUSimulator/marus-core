using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labust.Controllers
{
	/// <summary>
	/// Vessel controller that applies force to move and rotate towards the Target position
	/// </summary>
	public class VesselForceController : MonoBehaviour
	{
		public Transform Motor;
		public float SteerPower = 500f;
		public float Power = 5f;
		public float MaxSpeed = 10f;
		protected Rigidbody vessel;
		public Transform Target;
		private Boolean stop;
		public float StoppingDistance = 1f;
		public float MaxAngleDelta = 5f;
		public float RestartDistance = 5f;

		public void Awake()
		{
			vessel = GetComponent<Rigidbody>();
			stop = false;
		}

		public void FixedUpdate()
		{
			MoveTowards();
			RotateTowards();
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

			var forward = Vector3.Scale(new Vector3(1,0,1), transform.forward);
			if (!stop)
			{
				ApplyForceToReachVelocity(vessel, forward * MaxSpeed, Power * Mathf.Sqrt(dist) * Time.fixedDeltaTime);
			}
		}

		public void RotateTowards() {
			Vector3 relativePos = Target.position - transform.position;
			float angle = Vector3.SignedAngle(relativePos, transform.forward, Vector3.up);
			if (!stop && Mathf.Abs(angle) > MaxAngleDelta)
			{
				vessel.AddForceAtPosition(Mathf.Sign(angle) * transform.right * SteerPower, Motor.position);
			}
		}

		public static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force = 1, ForceMode mode = ForceMode.Force)
		{
			if (force == 0 || velocity.magnitude == 0)
				return;

			velocity = velocity + velocity.normalized * rigidbody.drag;

			//force = 1 => need 1 s to reach velocity (if mass is 1) => force can be max 1 / Time.fixedDeltaTime
			force = Mathf.Clamp(force, -rigidbody.mass / Time.fixedDeltaTime, rigidbody.mass / Time.fixedDeltaTime);

			//dot product is a projection from rhs to lhs with a length of result / lhs.magnitude https://www.youtube.com/watch?v=h0NJK4mEIJU
			if (rigidbody.velocity.magnitude == 0)
			{
				rigidbody.AddForce(velocity * force, mode);
			}
			else
			{
				var velocityProjectedToTarget = (velocity.normalized * Vector3.Dot(velocity, rigidbody.velocity) / velocity.magnitude);
				rigidbody.AddForce((velocity - velocityProjectedToTarget) * force, mode);
			}
		}
	}
}

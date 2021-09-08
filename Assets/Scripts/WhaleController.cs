using UnityEngine;

public class WhaleController : MonoBehaviour
{
		/// <summary>
		/// This class implements controller for whale object.
		/// </summary>

		public float speed = 0.05f;
		public bool InvertMovement = true;
		private float lastSpeed;
		private Vector3 lastPosition;
		public float rotationSpeed = 2;
		private Vector3 currentEulerAngles;
		private float timer = 0;
		private int mod = 1;
		private float period;
		private float distanceDelta;
		RaycastHit hit;
		RaycastHit hit1;
		RaycastHit hit2;

		void Start()
		{
			currentEulerAngles = transform.eulerAngles;
			period = Random.Range(3.0f, 15.0f);
			speed = Random.Range(0.5f, 1.5f);
			lastSpeed = speed;
			rotationSpeed = Random.Range(0.0f, 1.5f);

			float x = Random.Range(220, 580);
			float z = Random.Range(220, 580);
			transform.position = new Vector3(x, 45, z);

			float x1 = transform.eulerAngles.x;
			float y1 = Random.Range(0, 180);
			float z1 = transform.eulerAngles.z;
			transform.eulerAngles = new Vector3(x1, y1, z1);
			lastPosition = transform.position;

		}

		void Update()
		{	
			Ray ray;
			
			if (InvertMovement)
			{
				ray = new Ray(transform.position, -transform.forward);
			}
			else
			{
				ray = new Ray(transform.position, transform.forward);
			}
			
			Ray ray1 = new Ray(transform.position, transform.right);
			Ray ray2 = new Ray(transform.position, -transform.right);
			Physics.Raycast(ray, out hit);
			Physics.Raycast(ray1, out hit1);
			Physics.Raycast(ray2, out hit2);

			if (hit.distance < 100 || hit1.distance < 100 || hit2.distance < 100)
			{
				speed = 2;
				currentEulerAngles += new Vector3(0, 1, 0) * Time.deltaTime * 20;
				transform.eulerAngles = currentEulerAngles;
				return;
			}
			else
			{
				speed = lastSpeed;
			}

			timer += Time.deltaTime;
			if (InvertMovement)
			{
				transform.position -= transform.forward * Time.deltaTime * speed;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * speed;
			}
			if (timer >= period)
			{
				timer = 0;
				period = Random.Range(3.0f, 15.0f);
				speed = Random.Range(0.5f, 1.5f);
				rotationSpeed = Random.Range(0.0f, 1.5f);
			}
			currentEulerAngles += new Vector3(0, 1, 0) * Time.deltaTime * rotationSpeed;
			transform.eulerAngles = currentEulerAngles;

		}
}

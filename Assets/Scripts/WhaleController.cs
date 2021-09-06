using UnityEngine;

public class WhaleController : MonoBehaviour
{
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

		void Start()
		{
			currentEulerAngles = transform.eulerAngles;
			period = Random.Range(3.0f, 15.0f);
			speed = Random.Range(0.5f, 1.5f);
			lastSpeed = speed;
			rotationSpeed = Random.Range(0.0f, 20.0f);

			float x = Random.Range(0, 250);
			float z = Random.Range(100, 600);
			transform.position = new Vector3(x, 20, z);

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
			
			Ray ray1 = new Ray(transform.position, -transform.right);
			Ray ray2 = new Ray(transform.position, transform.right);
			if (Physics.Raycast(ray, out hit) || Physics.Raycast(ray1, out hit) || Physics.Raycast(ray2, out hit))
			{
				if (hit.distance < 20 && speed > 0)
				{
					lastSpeed = speed;
					speed = 0;
					return;
				}
				else if (hit.distance < 10 && speed == 0)
				{
					currentEulerAngles += new Vector3(0, 1, 0) * Time.deltaTime * rotationSpeed * 40;
					transform.eulerAngles = currentEulerAngles;
					return;
				}
				else
				{
					speed = lastSpeed;
				}
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
				mod = mod * (-1);
				timer = 0;
				period = Random.Range(3.0f, 15.0f);
				speed = Random.Range(0.5f, 3.0f);
				rotationSpeed = Random.Range(0.0f, 20.0f);
			}
			currentEulerAngles += new Vector3(0, 1, 0) * Time.deltaTime * rotationSpeed * mod;
			transform.eulerAngles = currentEulerAngles;

		}
}

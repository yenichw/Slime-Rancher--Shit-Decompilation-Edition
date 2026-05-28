using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldShootBall : MonoBehaviour
	{
		public Rigidbody bullet;

		public Transform origshoot;

		public float speed = 1000f;

		private float distance = 10f;

		private void Update()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
				position = Camera.main.ScreenToWorldPoint(position);
				Rigidbody rigidbody = Object.Instantiate(bullet, base.transform.position, Quaternion.identity);
				rigidbody.transform.LookAt(position);
				rigidbody.AddForce(rigidbody.transform.forward * speed);
			}
		}
	}
}

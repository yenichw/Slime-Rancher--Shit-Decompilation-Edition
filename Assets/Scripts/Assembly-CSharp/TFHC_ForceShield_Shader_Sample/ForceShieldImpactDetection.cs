using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldImpactDetection : MonoBehaviour
	{
		private float hitTime;

		private Material mat;

		private void Start()
		{
			mat = GetComponent<Renderer>().material;
		}

		private void Update()
		{
			if (hitTime > 0f)
			{
				hitTime -= Time.deltaTime * 1000f;
				if (hitTime < 0f)
				{
					hitTime = 0f;
				}
				mat.SetFloat("_HitTime", hitTime);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			ContactPoint[] contacts = collision.contacts;
			foreach (ContactPoint contactPoint in contacts)
			{
				mat.SetVector("_HitPosition", base.transform.InverseTransformPoint(contactPoint.point));
				hitTime = 500f;
				mat.SetFloat("_HitTime", hitTime);
			}
		}
	}
}

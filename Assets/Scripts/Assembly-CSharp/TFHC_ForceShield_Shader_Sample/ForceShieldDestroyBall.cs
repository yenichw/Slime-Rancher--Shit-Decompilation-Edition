using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldDestroyBall : MonoBehaviour
	{
		public float lifetime = 5f;

		private void Start()
		{
			Object.Destroy(base.gameObject, lifetime);
		}
	}
}

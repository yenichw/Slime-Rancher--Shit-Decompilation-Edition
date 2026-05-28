using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
	private vp_FPCamera fpCamera;

	public void Awake()
	{
		fpCamera = GetComponentInChildren<vp_FPCamera>();
	}

	public void ShakeDamage(float intensity)
	{
		fpCamera.AddForce2(new Vector3(Randoms.SHARED.GetInRange(-1f, 1f), -1f, 0f) * intensity);
	}

	public void ShakeFrontImpact(float intensity)
	{
		fpCamera.AddForce2(new Vector3(0f, 0f, -1f) * intensity);
	}
}

using UnityEngine;

public class CloudController : MonoBehaviour, AmbianceDirector.DaynessListener
{
	private Material mat;

	public void Awake()
	{
		mat = GetComponent<Renderer>().material;
	}

	public void Start()
	{
		SRSingleton<SceneContext>.Instance.AmbianceDirector.RegisterDaynessListener(this);
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.AmbianceDirector.UnregisterDaynessListener(this);
		}
		Destroyer.Destroy(mat, "CloudController.OnDestroy");
	}

	public void SetDayness(float dayness)
	{
		mat.SetFloat("_Dayness", dayness);
	}
}

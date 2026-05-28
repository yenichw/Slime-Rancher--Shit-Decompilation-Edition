using UnityEngine;

public class DuskedLight : MonoBehaviour
{
	public void Start()
	{
		SRSingleton<SceneContext>.Instance.AmbianceDirector.RegisterDuskedLight(GetComponent<Light>());
	}
}

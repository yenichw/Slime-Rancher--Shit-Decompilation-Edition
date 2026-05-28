using UnityEngine;

public class RequiresDebugMode : MonoBehaviour
{
	public void Awake()
	{
		Destroyer.Destroy(base.gameObject, "RequiresDebugMode.Awake");
	}
}

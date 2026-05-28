using UnityEngine;

public class EnableForceStencilClear : MonoBehaviour
{
	private ForceStencilClear instance;

	public void Awake()
	{
		instance = Camera.main.GetComponent<ForceStencilClear>();
	}

	public void OnEnable()
	{
		if (instance != null)
		{
			instance.RegisterEnabler(base.gameObject);
		}
	}

	public void OnDisable()
	{
		if (instance != null)
		{
			instance.DeregisterEnabler(base.gameObject);
		}
	}
}

using UnityEngine;

public class GlitchBreadcrumbNetworkNode : PathingNetworkNode
{
	[Tooltip("GameObject that is enabled while this breadcrumb is active. (optional)")]
	public GameObject onActiveFX;

	public void Awake()
	{
		if (onActiveFX != null)
		{
			onActiveFX.SetActive(value: false);
		}
	}

	public void Activate(Vector3 nextPoint)
	{
		if (onActiveFX != null)
		{
			onActiveFX.SetActive(value: true);
			onActiveFX.transform.rotation = Quaternion.LookRotation(nextPoint - base.position);
		}
	}

	public void Deactivate()
	{
		if (onActiveFX != null)
		{
			onActiveFX.SetActive(value: false);
		}
	}
}

using UnityEngine;

public class ResetLayerChanges : MonoBehaviour
{
	private int layer;

	public void Awake()
	{
		layer = base.gameObject.layer;
	}

	public void LateUpdate()
	{
		if (base.gameObject.layer != layer)
		{
			base.gameObject.layer = layer;
		}
	}
}

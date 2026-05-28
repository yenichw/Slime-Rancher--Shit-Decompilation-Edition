using Assets.Script.Util.Extensions;
using UnityEngine;

public class CameraDepthMode : MonoBehaviour
{
	public DepthTextureMode depth;

	private void Start()
	{
		this.GetRequiredComponent<Camera>().depthTextureMode = depth;
	}
}

using System.Collections.Generic;
using UnityEngine;

public class CameraFOVAdjuster : SRBehaviour
{
	public static List<CameraFOVAdjuster> Instances = new List<CameraFOVAdjuster>();

	private Camera ownCamera;

	public void Awake()
	{
		Instances.Add(this);
		ownCamera = GetRequiredComponent<Camera>();
	}

	public void OnDestroy()
	{
		Instances.Remove(this);
	}

	public void SetFOV(float fov)
	{
		ownCamera.fieldOfView = fov;
	}
}

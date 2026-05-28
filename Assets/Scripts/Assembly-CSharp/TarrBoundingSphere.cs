using System;
using UnityEngine;

public class TarrBoundingSphere : RegisteredActorBehaviour, RegistryLateUpdateable
{
	public const int TARR_BOUNDING_SPHERE_START_COUNT = 100;

	private const int ARRAY_RESIZE_STEP = 100;

	public static BoundingSphere[] allSpheres = new BoundingSphere[100];

	public static int sphereCount = 0;

	public static int nearbyTarr = 0;

	public static void ResetTarrData()
	{
		allSpheres = new BoundingSphere[100];
		sphereCount = 0;
	}

	public void RegistryLateUpdate()
	{
		if (sphereCount == allSpheres.Length)
		{
			Array.Resize(ref allSpheres, allSpheres.Length + 100);
		}
		allSpheres[sphereCount].position = base.transform.position;
		allSpheres[sphereCount].radius = 2f;
		sphereCount++;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereOverlapTrigger : MonoBehaviour
{
	public delegate void OnSphereOverlap(IEnumerable<Collider> colliders);

	public OnSphereOverlap onSphereOverlap;

	private List<Collider> colliders = new List<Collider>();

	private bool hasDoneOneFixedUpdate;

	public void OnTriggerEnter(Collider col)
	{
		colliders.Add(col);
	}

	public void FixedUpdate()
	{
		hasDoneOneFixedUpdate = true;
	}

	public void LateUpdate()
	{
		if (!hasDoneOneFixedUpdate)
		{
			return;
		}
		try
		{
			if (onSphereOverlap != null)
			{
				onSphereOverlap(colliders.Where((Collider c) => c != null));
			}
		}
		finally
		{
			onSphereOverlap = null;
			Destroyer.Destroy(base.gameObject, "SphereOverlapTrigger.LateUpdate");
		}
	}

	public static GameObject CreateGameObject(Vector3 center, float radius, OnSphereOverlap onOverlap, int layer = 0)
	{
		GameObject obj = new GameObject("SphereOverlapTrigger");
		obj.transform.position = center;
		obj.layer = layer;
		SphereOverlapTrigger sphereOverlapTrigger = obj.AddComponent<SphereOverlapTrigger>();
		sphereOverlapTrigger.onSphereOverlap = (OnSphereOverlap)Delegate.Combine(sphereOverlapTrigger.onSphereOverlap, onOverlap);
		SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
		sphereCollider.radius = radius;
		sphereCollider.isTrigger = true;
		obj.AddComponent<Rigidbody>().isKinematic = true;
		return obj;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsUtil
{
	private const float PLAYER_FORCE_FACTOR = 0.001f;

	public static void Explode(GameObject source, float radius, float power, float minPlayerDamage, float maxPlayerDamage, bool ignites = false)
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Vector3 position = source.transform.position;
		LayerMask layerMask = -65537;
		Collider[] array = Physics.OverlapSphere(position, radius, layerMask);
		foreach (Collider collider in array)
		{
			if (!collider || collider.isTrigger || !(collider.GetComponent<Rigidbody>() != null) || !(collider.gameObject != source) || hashSet.Contains(collider.gameObject))
			{
				continue;
			}
			vp_FPController component = collider.gameObject.GetComponent<vp_FPController>();
			if (component != null)
			{
				Vector3 vector = SlimeSubbehaviour.GetGotoPos(component.gameObject) - position;
				float magnitude = vector.magnitude;
				vector.Normalize();
				float num = (1f - magnitude / radius) * 0.001f;
				component.AddForce(vector * (power * num));
				Damageable interfaceComponent = collider.gameObject.GetInterfaceComponent<Damageable>();
				if (interfaceComponent != null)
				{
					int healthLoss = Mathf.RoundToInt(Mathf.Lerp(minPlayerDamage, maxPlayerDamage, 1f - magnitude / radius));
					if (interfaceComponent.Damage(healthLoss, source))
					{
						DeathHandler.Kill(collider.gameObject, DeathHandler.Source.SLIME_EXPLODE, source, "PhysicsUtil.Explode");
					}
				}
				if (ignites)
				{
					Ignitable[] componentsInChildren = collider.gameObject.GetComponentsInChildren<Ignitable>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].Ignite(source);
					}
				}
			}
			else
			{
				SoftExplosionForce(power, position, radius, collider.GetComponent<Rigidbody>());
			}
			hashSet.Add(collider.gameObject);
		}
	}

	public static void SoftExplosionForce(float power, Vector3 pos, float radius, Rigidbody body)
	{
		Vector3 vector = body.position - pos;
		float magnitude = vector.magnitude;
		vector.Normalize();
		float num = 1f - Mathf.Max(2f, magnitude) / radius;
		body.AddForce(vector * (power * num * num));
	}

	public static float RadiusOfObject(GameObject obj)
	{
		float num = 0f;
		Collider[] components = obj.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger)
			{
				if (obj.activeInHierarchy)
				{
					Bounds bounds = collider.bounds;
					num = Mathf.Max(num, 0.5f * Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));
				}
				else
				{
					num = Mathf.Max(num, CalcRad(collider));
				}
			}
		}
		return num;
	}

	private static float CalcRad(Collider col)
	{
		Vector3 lossyScale = col.transform.lossyScale;
		if (col is SphereCollider)
		{
			return ((SphereCollider)col).radius * Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
		}
		if (col is BoxCollider)
		{
			BoxCollider boxCollider = (BoxCollider)col;
			return Mathf.Max(boxCollider.size.x * lossyScale.x, boxCollider.size.y * lossyScale.y, boxCollider.size.z * lossyScale.z) * 0.5f;
		}
		if (col is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = (CapsuleCollider)col;
			float num = ((capsuleCollider.direction == 0) ? (capsuleCollider.height * 0.5f) : capsuleCollider.radius);
			float num2 = ((capsuleCollider.direction == 1) ? (capsuleCollider.height * 0.5f) : capsuleCollider.radius);
			float num3 = ((capsuleCollider.direction == 2) ? (capsuleCollider.height * 0.5f) : capsuleCollider.radius);
			return Mathf.Max(num * lossyScale.x, num2 * lossyScale.y, num3 * lossyScale.z);
		}
		return 0f;
	}

	public static bool IsPlayerMainCollider(Collider collider)
	{
		if (collider.gameObject == SRSingleton<SceneContext>.Instance.Player)
		{
			return collider is CharacterController;
		}
		return false;
	}

	public static void RestoreFreezeRotationConstraints(GameObject gameObject)
	{
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		if (component != null && component.constraints != 0)
		{
			Vector3 eulerAngles = component.transform.rotation.eulerAngles;
			if ((component.constraints & RigidbodyConstraints.FreezeRotationX) != 0)
			{
				eulerAngles.x = 0f;
			}
			if ((component.constraints & RigidbodyConstraints.FreezeRotationY) != 0)
			{
				eulerAngles.y = 0f;
			}
			if ((component.constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
			{
				eulerAngles.z = 0f;
			}
			component.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	public static void IgnoreCollision(GameObject a, GameObject b, bool ignored = true)
	{
		Collider[] componentsInChildren = a.GetComponentsInChildren<Collider>();
		Collider[] componentsInChildren2 = b.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			Collider[] array2 = componentsInChildren2;
			foreach (Collider collider2 in array2)
			{
				Physics.IgnoreCollision(collider, collider2, ignored);
			}
		}
	}

	public static void IgnoreCollision(GameObject a, GameObject b, float enableAfter)
	{
		IgnoreCollision(a, b);
		SRSingleton<GameContext>.Instance.StartCoroutine(RestoreCollision(a, b, enableAfter));
	}

	private static IEnumerator RestoreCollision(GameObject a, GameObject b, float enableAfter)
	{
		yield return new WaitForSeconds(enableAfter);
		if (a != null && b != null)
		{
			IgnoreCollision(a, b, ignored: false);
		}
	}
}

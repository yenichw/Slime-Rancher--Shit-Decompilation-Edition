using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAndShockOnTouching : SRBehaviour
{
	[Tooltip("When we poof, how large an area is shocked.")]
	public float shockRadius;

	[Tooltip("The effect to play when we poof.")]
	public GameObject destroyFX;

	private bool destroying;

	private Identifiable.Id id;

	public void Awake()
	{
		id = GetComponent<Identifiable>().id;
	}

	public void NoteDestroying()
	{
		destroying = true;
	}

	private void DestroyAndShock()
	{
		if (destroying)
		{
			return;
		}
		destroying = true;
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
		}
		if (shockRadius > 0f)
		{
			SphereOverlapTrigger.CreateGameObject(base.transform.position, shockRadius, delegate(IEnumerable<Collider> colliders)
			{
				HashSet<ReactToShock> hashSet = new HashSet<ReactToShock>();
				foreach (Collider collider in colliders)
				{
					ReactToShock[] componentsInParent = collider.gameObject.GetComponentsInParent<ReactToShock>();
					foreach (ReactToShock item in componentsInParent)
					{
						hashSet.Add(item);
					}
				}
				foreach (ReactToShock item2 in hashSet)
				{
					item2.DoShock(id);
				}
			}, 15);
		}
		Destroyer.DestroyActor(base.gameObject, "DestroyAndShockOnTouching.DestroyAndShock");
	}

	public void OnCollisionEnter(Collision col)
	{
		StartCoroutine(DestroyAndShockAtEndOfFrame());
	}

	private IEnumerator DestroyAndShockAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		DestroyAndShock();
	}
}

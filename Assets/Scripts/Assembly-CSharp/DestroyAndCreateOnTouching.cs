using System.Collections;
using UnityEngine;

public class DestroyAndCreateOnTouching : SRBehaviour
{
	[Tooltip("Prefab to instantiate.")]
	public GameObject prefab;

	private bool hasCollided;

	public void OnCollisionEnter(Collision col)
	{
		if (!hasCollided)
		{
			SRBehaviour.InstantiateDynamic(prefab, base.transform.position, base.transform.rotation);
			StartCoroutine(DestroyAfterFrame());
			hasCollided = true;
		}
	}

	private IEnumerator DestroyAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		Destroyer.DestroyActor(base.gameObject, "DestroyAndCreateOnTouching.DestroyAfterFrame");
	}
}

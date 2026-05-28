using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class TransformChanceOnReproduce : SRBehaviour
{
	[Tooltip("Probability we will transform on any given opportunity.")]
	public float transformChance = 0.05f;

	[Tooltip("What do we transform into.")]
	public GameObject targetPrefab;

	[Tooltip("Extra particle effect to play on transform.")]
	public GameObject transformFX;

	private RegionMember regionMember;

	public void Awake()
	{
		regionMember = GetComponent<RegionMember>();
	}

	public void MaybeTransform()
	{
		if (Randoms.SHARED.GetProbability(transformChance))
		{
			SRBehaviour.SpawnAndPlayFX(transformFX, base.transform.position, base.transform.rotation);
			Destroyer.DestroyActor(base.gameObject, "TransformChanceOnReproduce.MaybeTransform");
			SRBehaviour.InstantiateActor(targetPrefab, regionMember.setId, base.transform.position, base.transform.rotation);
		}
	}
}

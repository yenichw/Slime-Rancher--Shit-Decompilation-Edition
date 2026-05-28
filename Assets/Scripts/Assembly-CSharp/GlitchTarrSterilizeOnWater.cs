using Assets.Script.Util.Extensions;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchTarrSterilizeOnWater : TarrSterilizeOnWater, DestroyAfterTimeListener
{
	private RegionMember regionMember;

	private float multiplyChance;

	public override void Awake()
	{
		base.Awake();
		regionMember = GetComponent<RegionMember>();
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		multiplyChance = glitch.tarrBaseMultiplyChance;
	}

	public override void Start()
	{
		base.Start();
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		destroyer.SetDeathTime(timeDir.HoursFromNow(glitch.tarrLifetime.GetRandom() * (1f / 60f)));
	}

	public void WillDestroyAfterTime()
	{
		if (!sterilized && Randoms.SHARED.GetProbability(multiplyChance))
		{
			LookupDirector lookupDirector = SRSingleton<GameContext>.Instance.LookupDirector;
			GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
			GameObject prefab = lookupDirector.GetPrefab(Identifiable.Id.GLITCH_TARR_SLIME);
			for (int i = 0; i < Mathf.RoundToInt(glitch.tarrMultiplyCount.GetRandom()); i++)
			{
				GameObject gameObject = SRBehaviour.InstantiateActor(prefab, regionMember.setId, base.gameObject.transform.position + Random.insideUnitSphere * 2f, Quaternion.identity);
				gameObject.GetComponent<GlitchTarrSterilizeOnWater>().multiplyChance = multiplyChance * (1f - glitch.tarrMultiplyChanceDegradation);
				gameObject.GetComponent<Rigidbody>().velocity = (Quaternion.Euler(new Vector2(-45f, 30f).GetRandom(), new Vector2(0f, 360f).GetRandom(), 0f) * gameObject.transform.forward).normalized * 15f;
				float fromValue = gameObject.transform.localScale.x * 0.2f;
				gameObject.transform.DOScale(gameObject.transform.localScale, 0.2f).From(fromValue).SetEase(Ease.Linear);
			}
		}
	}
}

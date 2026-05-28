using System;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PollenCloudController : SRBehaviour
{
	public float pctGrowthPerGameHour = 1f;

	public float startGrowthAgitation = 0.75f;

	public float maxCloudScale = 5f;

	public GameObject cloudActorPrefab;

	private PollenCloudMarker cloud;

	private SlimeEmotions emotions;

	private RegionMember regionMember;

	private TimeDirector timeDir;

	private float growthFactor;

	private float pctGrowthPerGameSec;

	private const float CLOUD_SPEED = 1f;

	private const float RELEASE_CUTOFF = 0.95f;

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		regionMember = GetComponent<RegionMember>();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		growthFactor = 1f / (1f - startGrowthAgitation);
		pctGrowthPerGameSec = pctGrowthPerGameHour * 0.00027777778f;
	}

	public void Start()
	{
		cloud = GetComponentInChildren<PollenCloudMarker>(includeInactive: true);
	}

	public void Update()
	{
		float num = ScaleForAgitation(emotions.GetCurr(SlimeEmotions.Emotion.AGITATION));
		float num2 = (cloud.gameObject.activeSelf ? (cloud.transform.localScale.x / maxCloudScale) : 0f);
		if (num2 > num)
		{
			num2 = Mathf.Max(num, num2 - (float)(timeDir.DeltaWorldTime() * (double)pctGrowthPerGameSec));
		}
		else if (num2 < num)
		{
			num2 = Mathf.Min(num, num2 + (float)(timeDir.DeltaWorldTime() * (double)pctGrowthPerGameSec));
		}
		if (num2 >= 0.95f)
		{
			SRBehaviour.InstantiateActor(cloudActorPrefab, regionMember.setId, base.transform.position, base.transform.rotation).GetComponent<Rigidbody>().velocity = base.transform.forward * 1f;
			num2 = 0f;
		}
		cloud.transform.localScale = Vector3.one * (maxCloudScale * num2);
		cloud.gameObject.SetActive(num2 > 0f);
	}

	private float ScaleForAgitation(float agitation)
	{
		return Math.Max(0f, (agitation - startGrowthAgitation) * growthFactor);
	}
}

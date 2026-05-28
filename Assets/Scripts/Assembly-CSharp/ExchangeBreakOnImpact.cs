using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ExchangeBreakOnImpact : SRBehaviour
{
	public GameObject breakFX;

	public GameObject coinPrefab;

	[Tooltip("Prefab spawned when a time extension is granted. (optional)")]
	public GameObject timeExtensionFX;

	[HideInInspector]
	public bool breakOpenOnStart = true;

	private const float COLLISION_THRESHOLD = 0f;

	private Rigidbody body;

	private ExchangeDirector exchangeDir;

	private LookupDirector lookupDir;

	private bool breaking;

	private const int COINS_PER_ITEM = 50;

	private const float BREAK_SPAWN_RADIUS = 1f;

	private const float PRODUCE_SCALE_UP_TIME = 0.2f;

	public void Awake()
	{
		body = GetComponent<Rigidbody>();
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
	}

	public void Start()
	{
		if (breakOpenOnStart)
		{
			BreakOpen();
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (!col.collider.isTrigger && !body.isKinematic)
		{
			float num = 0f;
			ContactPoint[] contacts = col.contacts;
			foreach (ContactPoint contactPoint in contacts)
			{
				num = Mathf.Max(num, Vector3.Dot(contactPoint.normal, col.relativeVelocity));
			}
			if (num > 0f)
			{
				BreakOpen();
			}
		}
	}

	private void BreakOpen()
	{
		if (breaking)
		{
			return;
		}
		breaking = true;
		SRBehaviour.SpawnAndPlayFX(breakFX, base.gameObject.transform.position, base.gameObject.transform.rotation);
		Destroyer.DestroyActor(base.gameObject, "ExchangeBreakOnImpact.BreakOpen");
		List<ExchangeDirector.ItemEntry> offerRewards = exchangeDir.GetOfferRewards(ExchangeDirector.OfferType.GENERAL);
		RegionRegistry.RegionSetId setId = GetComponent<RegionMember>().setId;
		if (offerRewards != null)
		{
			foreach (ExchangeDirector.ItemEntry item in offerRewards)
			{
				if (item.specReward != 0)
				{
					SpawnSpecReward(item.specReward);
					continue;
				}
				GameObject prefab = lookupDir.GetPrefab(item.id);
				for (int i = 0; i < item.count; i++)
				{
					Vector3 position = base.transform.position + Random.insideUnitSphere * 1f;
					GameObject gameObject = SRBehaviour.InstantiateActor(prefab, setId, position, Quaternion.identity);
					gameObject.transform.DOScale(gameObject.transform.localScale, 0.2f).From(0.01f).SetEase(Ease.Linear);
				}
			}
		}
		exchangeDir.RewardsDidSpawn(ExchangeDirector.OfferType.GENERAL);
	}

	private void SpawnSpecReward(ExchangeDirector.NonIdentReward reward)
	{
		switch (reward)
		{
		case ExchangeDirector.NonIdentReward.NEWBUCKS_SMALL:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MEDIUM:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_LARGE:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_HUGE:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MOCHI:
		{
			int num2 = GetNewbucksRewardValue(reward) / 50;
			for (int i = 0; i < num2; i++)
			{
				Vector3 position = base.transform.position + Random.insideUnitSphere;
				SRBehaviour.SpawnAndPlayFX(coinPrefab, position, Quaternion.identity);
			}
			break;
		}
		case ExchangeDirector.NonIdentReward.TIME_EXTENSION_12H:
		{
			PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
			double num = (float)GetTimeExtensionRewardValue(reward) * 3600f;
			playerState.SetEndGameTime(playerState.GetEndGameTime().Value + num);
			if (timeExtensionFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(timeExtensionFX, base.transform.position, Quaternion.identity);
			}
			break;
		}
		}
	}

	public static int GetNewbucksRewardValue(ExchangeDirector.NonIdentReward reward)
	{
		switch (reward)
		{
		case ExchangeDirector.NonIdentReward.NEWBUCKS_SMALL:
			return 250;
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MEDIUM:
			return 500;
		case ExchangeDirector.NonIdentReward.NEWBUCKS_LARGE:
			return 750;
		case ExchangeDirector.NonIdentReward.NEWBUCKS_HUGE:
			return 1000;
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MOCHI:
			return 200;
		default:
			return 0;
		}
	}

	public static int GetTimeExtensionRewardValue(ExchangeDirector.NonIdentReward reward)
	{
		if (reward == ExchangeDirector.NonIdentReward.TIME_EXTENSION_12H)
		{
			return 12;
		}
		return 0;
	}
}

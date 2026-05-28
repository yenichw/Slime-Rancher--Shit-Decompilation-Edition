using System.Collections.Generic;
using UnityEngine;

public class ScorePlort : SRBehaviour, VacShootAccelerator
{
	public class Deposit_Response
	{
		public int deposits;

		public int currency;

		public static bool operator true(Deposit_Response response)
		{
			return response.deposits > 0;
		}

		public static bool operator false(Deposit_Response response)
		{
			return response.deposits <= 0;
		}
	}

	public GameObject ExplosionFX;

	private PlayerState player;

	private SECTR_AudioSource scoreAudio;

	private EconomyDirector econDir;

	private TutorialDirector tutDir;

	private ModDirector modDir;

	private AchievementsDirector achieveDir;

	private int soldCount;

	private float lastUpdateTime;

	private ProgressDirector progressDir;

	private DroneNetwork droneNetwork;

	private VacAccelerationHelper accelerationInput = VacAccelerationHelper.CreateInput();

	private const float MOCHI_CHANCE = 0.05f;

	private const float MOCHI_FACTOR = 2f;

	private const float TIME_BETWEEN_UPDATES = 300f;

	public void Awake()
	{
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		econDir = SRSingleton<SceneContext>.Instance.EconomyDirector;
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		modDir = SRSingleton<SceneContext>.Instance.ModDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		scoreAudio = GetRequiredComponent<SECTR_AudioSource>();
		droneNetwork = GetComponentInParent<DroneNetwork>();
		if (droneNetwork != null)
		{
			droneNetwork.Register(this);
		}
	}

	public void OnDestroy()
	{
		UpdateSoldPlorts(isDestroyed: true);
		if (droneNetwork != null)
		{
			droneNetwork.Deregister(this);
			droneNetwork = null;
		}
	}

	public bool CanDeposit(Identifiable.Id id, bool ignoreMarketShutdown = false)
	{
		return GetMarketValue(id, ignoreMarketShutdown).HasValue;
	}

	public Deposit_Response Deposit(Identifiable.Id id, int count = 1, PlayerState.CoinsType? coinsTypeOverride = null, bool ignoreMarketShutdown = false)
	{
		if (count <= 0)
		{
			return new Deposit_Response();
		}
		int? marketValue = GetMarketValue(id, ignoreMarketShutdown);
		if (!marketValue.HasValue)
		{
			return new Deposit_Response();
		}
		Deposit_Response deposit_Response = new Deposit_Response
		{
			deposits = count
		};
		PlayerState.CoinsType coinsType = coinsTypeOverride ?? PlayerState.CoinsType.NORM;
		marketValue = Mathf.RoundToInt((float)marketValue.Value * modDir.PlortPriceFactor(id));
		for (int i = 0; i < count; i++)
		{
			if (progressDir.GetProgress(ProgressDirector.ProgressType.MOCHI_REWARDS) >= 1 && Randoms.SHARED.GetProbability(0.05f))
			{
				coinsType = coinsTypeOverride ?? PlayerState.CoinsType.MOCHI;
				deposit_Response.currency += Mathf.RoundToInt((float)marketValue.Value * 2f);
			}
			else
			{
				deposit_Response.currency += marketValue.Value;
			}
		}
		player.AddCurrency(deposit_Response.currency, coinsType);
		achieveDir.AddToStat(AchievementsDirector.IntStat.PLORTS_SOLD, count);
		achieveDir.AddToStat(AchievementsDirector.GameIdDictStat.PLORT_TYPES_SOLD, id, count);
		scoreAudio.Play();
		econDir.RegisterSold(id, count);
		tutDir.OnPlortSold();
		soldCount += count;
		accelerationInput.OnTriggered();
		return deposit_Response;
	}

	private void OnTriggerEnter(Collider col)
	{
		if (Deposit(Identifiable.GetId(col.gameObject)))
		{
			SRBehaviour.SpawnAndPlayFX(ExplosionFX, col.gameObject.transform.position, col.gameObject.transform.rotation);
			Destroyer.DestroyActor(col.gameObject, "ScorePlort.OnTriggerEnter");
			DestroyOnTouching component = col.gameObject.GetComponent<DestroyOnTouching>();
			if (component != null)
			{
				component.NoteDestroying();
			}
		}
	}

	public void Update()
	{
		if (lastUpdateTime + 300f < Time.time)
		{
			UpdateSoldPlorts(isDestroyed: false);
			lastUpdateTime = Time.time;
		}
	}

	private void UpdateSoldPlorts(bool isDestroyed)
	{
		if (soldCount > 0)
		{
			AnalyticsUtil.CustomEvent("PlortsSold", new Dictionary<string, object> { { "count", soldCount } }, !isDestroyed);
			soldCount = 0;
		}
	}

	public float GetVacShootSpeedFactor()
	{
		return accelerationInput.Factor;
	}

	private int? GetMarketValue(Identifiable.Id id, bool ignoreMarketShutdown)
	{
		if (!ignoreMarketShutdown && econDir.IsMarketShutdown())
		{
			return null;
		}
		return econDir.GetCurrValue(id);
	}
}

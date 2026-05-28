using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class TreasurePod : IdHandler, TreasurePodModel.Participant
{
	public enum State
	{
		LOCKED = 0,
		OPEN = 1
	}

	private class BlueprintPopupCreator : PopupDirector.PopupCreator
	{
		private Gadget.Id id;

		public BlueprintPopupCreator(Gadget.Id id)
		{
			this.id = id;
		}

		public override void Create()
		{
			BlueprintPopupUI.CreateBlueprintPopup(SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id));
		}

		public override bool Equals(object other)
		{
			if (other is BlueprintPopupCreator)
			{
				return ((BlueprintPopupCreator)other).id == id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override bool ShouldClear()
		{
			return false;
		}
	}

	public bool needsUpgrade = true;

	public PlayerState.Upgrade requiredUpgrade;

	public SECTR_AudioCue openCue;

	[Tooltip("The FX that will be played as soon as the treasure pod is unlocked.")]
	public GameObject openFX;

	[Tooltip("The FX that will be played after the treasure pod opening animation has finished.")]
	public GameObject afterOpenFX;

	public Gadget.Id blueprint;

	public GameObject[] spawnObjs;

	public SECTR_AudioCue spawnObjCue;

	[Tooltip("The SlimeDefinition for the appearance that will be unlocked.")]
	public SlimeDefinition unlockedSlimeAppearanceDefinition;

	[Tooltip("The SlimeAppearance that will be unlocked.")]
	public SlimeAppearance unlockedSlimeAppearance;

	[Tooltip("Coins prefab awarded by the treasure pod. (TIME_LIMIT_V2)")]
	public GameObject coins;

	[Tooltip("FX played when coins are awarded. (TIME_LIMIT_V2, optional)")]
	public GameObject onCoinsFX;

	[Tooltip("SFX played when coins are awarded. (TIME_LIMIT_V2, optional)")]
	public SECTR_AudioCue onCoinsCue;

	private CellDirector cellDirector;

	private bool nextUpdateImmediate;

	private bool forceUpdate;

	private int animOpenId;

	private int animOpenImmediateId;

	private PlayerState playerState;

	private GadgetDirector gadgetDir;

	private SlimeAppearanceDirector slimeAppearanceDirector;

	private TreasurePodModel model;

	public static float ITEM_GAP_DELAY = 0.8f;

	public static float OPEN_DELAY = 3.3f;

	private static Vector3 EJECT_VEL = new Vector3(0f, 5f, 2.5f);

	private static Vector3 EJECT_OFF = new Vector3(0f, 1.75f, 1f);

	public State CurrState
	{
		get
		{
			return model.state;
		}
		set
		{
			model.state = value;
			forceUpdate = true;
		}
	}

	public void Awake()
	{
		cellDirector = GetComponentInParent<CellDirector>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPod(base.id, base.gameObject);
		animOpenId = Animator.StringToHash("Open");
		animOpenImmediateId = Animator.StringToHash("OpenImmediate");
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
		slimeAppearanceDirector = SRSingleton<SceneContext>.Instance.SlimeAppearanceDirector;
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterPod(base.id);
		}
	}

	public void OnEnable()
	{
		forceUpdate = true;
		nextUpdateImmediate = true;
		StartCoroutine(OnEnableCoroutine());
	}

	private IEnumerator OnEnableCoroutine()
	{
		yield return new WaitForSeconds(OPEN_DELAY);
		yield return StartCoroutine(SpawnQueuedPrizeObjs());
	}

	public void InitModel(TreasurePodModel model)
	{
	}

	public void SetModel(TreasurePodModel model)
	{
		this.model = model;
		UpdateImmediate(model.state);
	}

	public ZoneDirector.Zone GetZoneId()
	{
		if (cellDirector != null)
		{
			return cellDirector.GetZoneId();
		}
		return ZoneDirector.Zone.NONE;
	}

	private void UpdateImmediate(State state)
	{
		nextUpdateImmediate = true;
		CurrState = state;
	}

	public void Update()
	{
		if (!forceUpdate)
		{
			return;
		}
		forceUpdate = false;
		if (nextUpdateImmediate)
		{
			if (model.state == State.OPEN && model.spawnQueue.Count == 0)
			{
				GetComponentInChildren<Animator>().SetTrigger(animOpenImmediateId);
			}
			nextUpdateImmediate = false;
		}
		GetComponentInChildren<Animator>().SetBool(animOpenId, model.state == State.OPEN);
	}

	protected override string IdPrefix()
	{
		return "pod";
	}

	public bool HasKey()
	{
		if (needsUpgrade)
		{
			return playerState.HasUpgrade(requiredUpgrade);
		}
		return true;
	}

	public bool HasAnyKey()
	{
		if (!playerState.HasUpgrade(PlayerState.Upgrade.TREASURE_CRACKER_1) && !playerState.HasUpgrade(PlayerState.Upgrade.TREASURE_CRACKER_2) && !playerState.HasUpgrade(PlayerState.Upgrade.TREASURE_CRACKER_3))
		{
			return playerState.HasUpgrade(PlayerState.Upgrade.TREASURE_CRACKER_4);
		}
		return true;
	}

	public void Activate()
	{
		if (HasKey())
		{
			CurrState = State.OPEN;
			if (openCue != null)
			{
				SECTR_AudioSystem.Play(openCue, base.transform.position, loop: false);
			}
			if (openFX != null)
			{
				SRBehaviour.InstantiateDynamic(openFX, base.transform.position, base.transform.rotation);
			}
			StartCoroutine((SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2) ? AwardPrizesRushMode() : AwardPrizesDefault());
			AnalyticsUtil.CustomEvent("PodOpened", new Dictionary<string, object> { { "id", base.id } });
		}
	}

	private IEnumerator AwardPrizesDefault()
	{
		if (blueprint != 0)
		{
			gadgetDir.AddBlueprint(blueprint);
		}
		if (unlockedSlimeAppearance != null)
		{
			slimeAppearanceDirector.UnlockAppearance(unlockedSlimeAppearanceDefinition, unlockedSlimeAppearance);
		}
		if (spawnObjs != null && spawnObjs.Length != 0)
		{
			GameObject[] array = spawnObjs;
			for (int i = 0; i < array.Length; i++)
			{
				Identifiable.Id item = Identifiable.GetId(array[i]);
				model.spawnQueue.Enqueue(item);
			}
		}
		yield return new WaitForSeconds(OPEN_DELAY);
		if (afterOpenFX != null)
		{
			SRBehaviour.InstantiateDynamic(afterOpenFX, base.transform.position, base.transform.rotation);
		}
		if (blueprint != 0)
		{
			SRSingleton<SceneContext>.Instance.PopupDirector.QueueForPopup(new BlueprintPopupCreator(blueprint));
			SRSingleton<SceneContext>.Instance.PopupDirector.MaybePopupNext();
			yield return new WaitForSeconds(ITEM_GAP_DELAY);
		}
		if (unlockedSlimeAppearance != null)
		{
			slimeAppearanceDirector.UpdateChosenSlimeAppearance(unlockedSlimeAppearanceDefinition, unlockedSlimeAppearance);
			unlockedSlimeAppearance.MaybeShowPopupUI();
		}
		yield return StartCoroutine(SpawnQueuedPrizeObjs());
	}

	private IEnumerator AwardPrizesRushMode()
	{
		yield return new WaitForSeconds(OPEN_DELAY);
		if (onCoinsFX != null)
		{
			SRBehaviour.InstantiateDynamic(onCoinsFX).transform.position = base.transform.TransformPoint(EJECT_OFF);
		}
		SECTR_AudioSystem.Play(onCoinsCue, base.transform.position, loop: false);
		SRBehaviour.InstantiateDynamic(coins, base.transform.position, Quaternion.identity);
	}

	private IEnumerator SpawnQueuedPrizeObjs()
	{
		yield return new WaitForSeconds(ITEM_GAP_DELAY);
		Vector3 ejectPos = base.transform.TransformPoint(EJECT_OFF);
		while (model.spawnQueue.Count > 0)
		{
			Rigidbody component = SRBehaviour.InstantiateActor(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(model.spawnQueue.Dequeue()), cellDirector.region.setId, ejectPos, base.transform.rotation).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.AddForce(base.transform.TransformDirection(EJECT_VEL) + Randoms.SHARED.GetInRange(-0.1f, 0.1f) * base.transform.right, ForceMode.VelocityChange);
			}
			if (spawnObjCue != null)
			{
				SECTR_AudioSystem.Play(spawnObjCue, ejectPos, loop: false);
			}
			yield return new WaitForSeconds(ITEM_GAP_DELAY);
		}
	}
}

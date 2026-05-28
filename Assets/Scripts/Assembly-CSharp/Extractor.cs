using System;
using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;
using UnityEngine.UI;

public class Extractor : SRBehaviour, GadgetInteractor, GadgetModel.Participant
{
	[Serializable]
	public class ProduceEntry
	{
		public Identifiable.Id id;

		public float weight;

		public bool restrictZone;

		public ZoneDirector.Zone zone;

		public GameObject spawnFX;
	}

	[Tooltip("All the items the extractor can produce and their weights")]
	public ProduceEntry[] produces;

	public int cycles = 1;

	public bool infiniteCycles;

	public int produceMin = 4;

	public int produceMax = 8;

	public float hoursPerCycle = 22f;

	public Transform spawnPoint;

	public Text countdownText;

	private TimeDirector timeDir;

	private LookupDirector lookupDir;

	private ZoneDirector zoneDirector;

	private Dictionary<Identifiable.Id, float> spawnWeights = new Dictionary<Identifiable.Id, float>(Identifiable.idComparer);

	private Dictionary<Identifiable.Id, GameObject> spawnFX = new Dictionary<Identifiable.Id, GameObject>(Identifiable.idComparer);

	private Animator anim;

	private int extractedAnimId;

	private int ejectAnimId;

	private int readyToDespawnAnimId;

	private int skipDeployAnimId;

	private bool isPendingDestroy;

	private MessageBundle uiBundle;

	private ExtractorModel model;

	private const float SPAWN_EJECT_FORCE = 50f;

	private const float PRODUCE_GAP = 0.25f;

	private const float DESTROY_DELAY = 1.5f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		anim = GetComponent<Animator>();
		zoneDirector = GetComponentInParent<ZoneDirector>();
		extractedAnimId = Animator.StringToHash("extracted");
		ejectAnimId = Animator.StringToHash("eject");
		readyToDespawnAnimId = Animator.StringToHash("readyToDespawn");
		skipDeployAnimId = Animator.StringToHash("skipDeploy");
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
	}

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	public void InitModel(GadgetModel model)
	{
	}

	public void SetModel(GadgetModel model)
	{
		this.model = (ExtractorModel)model;
		if (this.model.cyclesRemaining == 0 && this.model.cycleEndTime == 0.0)
		{
			this.model.cyclesRemaining = cycles;
			StartNewCycleOrDestroy();
		}
		else
		{
			anim.SetTrigger(skipDeployAnimId);
		}
	}

	public void OnBundlesAvailable(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
	}

	public void Start()
	{
		ProduceEntry[] array = produces;
		foreach (ProduceEntry produceEntry in array)
		{
			if (!produceEntry.restrictZone || IsInZone(produceEntry.zone))
			{
				spawnWeights[produceEntry.id] = produceEntry.weight;
				spawnFX[produceEntry.id] = produceEntry.spawnFX;
			}
		}
	}

	public void OnDisable()
	{
		if (isPendingDestroy)
		{
			DestroyFromGadgetSite(includeInactive: true);
		}
	}

	public void Update()
	{
		if (model.queuedToProduce > 0)
		{
			if (timeDir.HasReached(model.nextProduceTime))
			{
				anim.SetTrigger(ejectAnimId);
				SpawnItem();
				model.queuedToProduce--;
				if (model.queuedToProduce <= 0)
				{
					model.nextProduceTime = 0.0;
					StartNewCycleOrDestroy();
				}
				else
				{
					model.nextProduceTime = timeDir.HoursFromNow(0.004166667f);
				}
			}
		}
		else if (timeDir.HasReached(model.cycleEndTime) && !anim.GetBool(extractedAnimId))
		{
			anim.SetBool(extractedAnimId, value: true);
		}
		if (model.cycleEndTime == double.PositiveInfinity || timeDir.HasReached(model.cycleEndTime))
		{
			countdownText.text = uiBundle.Get("m.ready");
		}
		else
		{
			countdownText.text = timeDir.FormatTime((int)(timeDir.HoursUntil(model.cycleEndTime) * 60.0));
		}
	}

	public void OnInteract()
	{
		if (timeDir.HasReached(model.cycleEndTime))
		{
			model.queuedToProduce = Randoms.SHARED.GetInRange(produceMin, produceMax + 1);
			model.nextProduceTime = timeDir.WorldTime();
			model.cycleEndTime = double.PositiveInfinity;
		}
	}

	public bool CanInteract()
	{
		return timeDir.HasReached(model.cycleEndTime);
	}

	private void StartNewCycleOrDestroy()
	{
		anim.SetBool(extractedAnimId, value: false);
		if (model.cyclesRemaining <= 0)
		{
			StartCoroutine(AnimAndDestroy());
			return;
		}
		if (!infiniteCycles)
		{
			model.cyclesRemaining--;
		}
		model.cycleEndTime = timeDir.HoursFromNow(hoursPerCycle);
	}

	private IEnumerator AnimAndDestroy()
	{
		isPendingDestroy = true;
		anim.SetBool(readyToDespawnAnimId, value: true);
		yield return new WaitForSeconds(1.5f);
		isPendingDestroy = false;
		DestroyFromGadgetSite(includeInactive: false);
	}

	private void DestroyFromGadgetSite(bool includeInactive)
	{
		GadgetSite componentInParent = GetComponentInParent<GadgetSite>(includeInactive);
		if (componentInParent != null)
		{
			componentInParent.DestroyAttached();
		}
	}

	private void SpawnItem()
	{
		Identifiable.Id id = Randoms.SHARED.Pick(spawnWeights, Identifiable.Id.NONE);
		if (id != 0)
		{
			Rigidbody component = SRBehaviour.InstantiateActor(lookupDir.GetPrefab(id), zoneDirector.regionSetId, spawnPoint.position, spawnPoint.rotation).GetComponent<Rigidbody>();
			if (component != null)
			{
				float num = (Identifiable.IsEcho(id) ? 20f : 1f);
				component.AddForce(spawnPoint.forward * 50f + new Vector3(Randoms.SHARED.GetInRange(0f - num, num), Randoms.SHARED.GetInRange(0f - num, num), Randoms.SHARED.GetInRange(0f - num, num)));
			}
			GameObject gameObject = spawnFX.Get(id);
			if (gameObject != null)
			{
				SRBehaviour.SpawnAndPlayFX(gameObject, spawnPoint.position, spawnPoint.rotation);
			}
		}
	}

	private bool IsInZone(ZoneDirector.Zone zone)
	{
		return zoneDirector.zone == zone;
	}
}

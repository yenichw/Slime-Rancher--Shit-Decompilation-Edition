using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Util.Extensions;
using UnityEngine;

public class GlintController : RegisteredActorBehaviour, RegistryUpdateable, LiquidConsumer
{
	private enum Phase
	{
		SUSPENDED = 0,
		READY = 1,
		FREE = 2
	}

	private class Glint
	{
		public GameObject gameObject;

		public Dictionary<Phase, double> phaseTimes;

		public Phase phase;
	}

	private static readonly Vector2 SPAWN_RADIUS = new Vector2(7.5f, 30f);

	private const float UPDATE_PERIOD = 1f / 6f;

	private const float TIME_SPAWN = 0.5f;

	private const float TIME_PHASE_BASE = 1f;

	private const float TIME_PHASE_READY = 0.5f;

	private const float TIME_PHASE_FREE = 0.5f;

	private const float FAST_FORWARD_MIN_SECONDS = 7200f;

	private GameObject suspendedGlintPrefab;

	private GameObject readyGlintPrefab;

	private GameObject freeGlintPrefab;

	private double nextUpdateTime;

	private double nextSpawnTime;

	private double previousUpdateTime;

	private TimeDirector timeDirector;

	private SlimeEmotions emotions;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private List<Glint> glints = new List<Glint>();

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += UpdateGlintAppearance;
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateGlintAppearance(slimeAppearanceApplicator.Appearance);
		}
	}

	public override void Start()
	{
		base.Start();
		float curr = emotions.GetCurr(SlimeEmotions.Emotion.AGITATION);
		nextSpawnTime = timeDirector.HoursFromNow(AdjustTime(0.5f, curr));
		Initialize(Phase.SUSPENDED, AdjustTime(1f, curr), curr);
		Initialize(Phase.READY, AdjustTime(0.5f, curr), curr);
		previousUpdateTime = timeDirector.WorldTime();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		foreach (Glint glint in glints)
		{
			if (glint.gameObject != null)
			{
				glint.gameObject.SetActive(value: true);
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		foreach (Glint glint in glints)
		{
			if (glint.gameObject != null)
			{
				glint.gameObject.SetActive(value: false);
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			DestroyAllGlints();
		}
	}

	public void RegistryUpdate()
	{
		if (timeDirector.HasReached(nextUpdateTime))
		{
			nextUpdateTime = timeDirector.HoursFromNow(1f / 6f);
			UpdateToTime(timeDirector.WorldTime());
		}
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			DestroyAllGlints();
		}
	}

	private static float AdjustTime(float hours, float agitation)
	{
		return hours * (1f - 0.8f * agitation);
	}

	private void UpdateGlintAppearance(SlimeAppearance appearance)
	{
		suspendedGlintPrefab = appearance.GlintAppearance.suspendedGlintPrefab;
		readyGlintPrefab = appearance.GlintAppearance.readyGlintPrefab;
		freeGlintPrefab = appearance.GlintAppearance.freeGlintPrefab;
	}

	private void DestroyAllGlints()
	{
		foreach (Glint glint in glints)
		{
			if (glint.gameObject != null)
			{
				SRBehaviour.RequestDestroy(glint.gameObject, "GlintController.DestroyAllGlints");
			}
		}
		glints.Clear();
	}

	private void UpdateToTime(double time)
	{
		float curr = emotions.GetCurr(SlimeEmotions.Emotion.AGITATION);
		if (time - previousUpdateTime >= 7200.0)
		{
			DestroyAllGlints();
			Initialize(Phase.SUSPENDED, AdjustTime(1f, curr), curr);
			Initialize(Phase.READY, AdjustTime(0.5f, curr), curr);
			Initialize(Phase.FREE, AdjustTime(0.5f, curr), curr);
		}
		else
		{
			glints.RemoveAll((Glint g) => g.gameObject == null);
			foreach (Glint glint in glints)
			{
				if (TimeUtil.HasReached(time, glint.phaseTimes[Phase.FREE]))
				{
					SRBehaviour.RequestDestroy(glint.gameObject, "GlintDestroyer.UpdateToTime");
					glint.gameObject = null;
				}
				else if (glint.phase < Phase.FREE && TimeUtil.HasReached(time, glint.phaseTimes[Phase.READY]))
				{
					ChangePhase(glint, Phase.FREE, time, curr);
				}
				else if (glint.phase < Phase.READY && TimeUtil.HasReached(time, glint.phaseTimes[Phase.SUSPENDED]))
				{
					ChangePhase(glint, Phase.READY, time, curr);
				}
			}
			if (TimeUtil.HasReached(time, nextSpawnTime))
			{
				nextSpawnTime = TimeDirector.HoursFromTime(AdjustTime(0.5f, curr), time);
				glints.Add(ChangePhase(new Glint(), Phase.SUSPENDED, time, curr));
			}
		}
		previousUpdateTime = time;
	}

	private Glint ChangePhase(Glint instance, Phase phase, double currentTime, float agitation)
	{
		instance.phase = phase;
		if (instance.gameObject != null)
		{
			GameObject gameObject = Instantiate(phase, instance.gameObject.transform.position, instance.gameObject.transform.rotation);
			SRBehaviour.RequestDestroy(instance.gameObject, "GlintDestroyer.ChangePhase");
			instance.gameObject = gameObject;
		}
		else
		{
			Vector3 vector = UnityEngine.Random.insideUnitSphere * SPAWN_RADIUS.Lerp(agitation);
			vector.y = Mathf.Abs(vector.y);
			instance.gameObject = Instantiate(phase, base.transform.position + vector, Quaternion.identity);
		}
		instance.phaseTimes = new Dictionary<Phase, double>();
		for (Phase phase2 = phase; phase2 <= Phase.FREE; phase2++)
		{
			Dictionary<Phase, double> phaseTimes = instance.phaseTimes;
			Phase key = phase2;
			float hours;
			switch (phase2)
			{
			default:
				hours = 0.5f;
				break;
			case Phase.READY:
				hours = AdjustTime(0.5f, agitation);
				break;
			case Phase.SUSPENDED:
				hours = AdjustTime(1f, agitation);
				break;
			}
			double num2 = (phaseTimes[key] = TimeDirector.HoursFromTime(hours, currentTime));
			currentTime = num2;
		}
		return instance;
	}

	private void Initialize(Phase phase, float phaseHours, float agitation)
	{
		int num = Mathf.RoundToInt(phaseHours / AdjustTime(0.5f, agitation));
		for (int i = 0; i < num; i++)
		{
			glints.Add(ChangePhase(new Glint(), phase, timeDirector.WorldTime() - (double)(Randoms.SHARED.GetFloat(phaseHours) * 3600f), agitation));
		}
	}

	private GameObject Instantiate(Phase phase, Vector3 position, Quaternion rotation)
	{
		object original;
		switch (phase)
		{
		default:
			original = null;
			break;
		case Phase.FREE:
			original = freeGlintPrefab;
			break;
		case Phase.READY:
			original = readyGlintPrefab;
			break;
		case Phase.SUSPENDED:
			original = suspendedGlintPrefab;
			break;
		}
		GameObject obj = SRBehaviour.InstantiatePooledDynamic((GameObject)original, position, rotation);
		Recycler component = obj.GetComponent<Recycler>();
		component.pool = SRSingleton<SceneContext>.Instance.fxPool;
		component.OnBeforeRecycle = (Recycler.RecycleEvent)Delegate.Combine(component.OnBeforeRecycle, new Recycler.RecycleEvent(OnBeforeRecycle));
		return obj;
	}

	private void OnBeforeRecycle(GameObject gameObject)
	{
		Recycler component = gameObject.GetComponent<Recycler>();
		component.OnBeforeRecycle = (Recycler.RecycleEvent)Delegate.Remove(component.OnBeforeRecycle, new Recycler.RecycleEvent(OnBeforeRecycle));
		glints.First((Glint g) => g.gameObject == gameObject).gameObject = null;
	}
}

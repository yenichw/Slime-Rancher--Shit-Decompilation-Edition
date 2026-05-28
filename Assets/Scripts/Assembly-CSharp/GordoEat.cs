using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using Noise;
using UnityEngine;

public class GordoEat : IdHandler, GadgetModel.Participant, GordoModel.Participant
{
	public SlimeDefinition slimeDefinition;

	public int targetCount = 100;

	public GameObject eatFX;

	public SECTR_AudioCue eatCue;

	public GameObject destroyFX;

	public float growthFactor = 1.5f;

	public float vibrationFactor;

	public GameObject slimePrefab;

	public GameObject slimeSpawnFXPrefab;

	public SECTR_AudioCue strainCue;

	public SECTR_AudioCue burstCue;

	public GameObject EatFavoriteFX;

	public Transform toVibrate;

	private List<Identifiable.Id> allEats = new List<Identifiable.Id>();

	protected SnareModel snareModel;

	protected GordoModel gordoModel;

	private GordoRewardsBase rewards;

	private HashSet<GameObject> eating = new HashSet<GameObject>();

	private CellDirector cellDirector;

	private float initScale;

	private float vibrateInitScale;

	protected const float EXPLODE_DELAY = 2f;

	public const int ALREADY_BURST_FLAG = -1;

	public void Awake()
	{
		rewards = GetComponent<GordoRewardsBase>();
		initScale = base.transform.localScale.x;
		vibrateInitScale = toVibrate.localScale.x;
		cellDirector = GetComponentInParent<CellDirector>();
		if (!string.IsNullOrEmpty(base.id))
		{
			SRSingleton<SceneContext>.Instance.GameModel.RegisterGordo(base.id, base.gameObject);
		}
	}

	public void Start()
	{
		allEats.AddRange(slimeDefinition.Diet.GetDietIdentifiableIds());
		int eatenCount = GetEatenCount();
		if (eatenCount != -1 && eatenCount >= GetTargetCount())
		{
			ImmediateReachedTarget();
		}
	}

	public ZoneDirector.Zone GetZoneId()
	{
		if (cellDirector != null)
		{
			return cellDirector.GetZoneId();
		}
		return ZoneDirector.Zone.NONE;
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && !string.IsNullOrEmpty(base.id))
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterGordo(base.id);
		}
	}

	public bool HasPopped()
	{
		return GetEatenCount() == -1;
	}

	public void InitModel(GadgetModel model)
	{
		((SnareModel)model).gordoTargetCount = targetCount;
	}

	public void SetModel(GadgetModel model)
	{
		snareModel = (SnareModel)model;
		if (snareModel.gordoEatenCount == -1)
		{
			rewards.SetupActiveRewards();
			base.gameObject.SetActive(value: false);
		}
	}

	public void InitModel(GordoModel model)
	{
		model.targetCount = targetCount;
	}

	public virtual void SetModel(GordoModel model)
	{
		gordoModel = model;
		if (gordoModel.gordoEatenCount == -1)
		{
			rewards.SetupActiveRewards();
			base.gameObject.SetActive(value: false);
		}
	}

	public void OnResetEatenCount()
	{
		GetComponent<GordoFaceAnimator>().SetDefaultState();
	}

	public int GetEatenCount()
	{
		if (gordoModel != null)
		{
			return gordoModel.gordoEatenCount;
		}
		if (snareModel != null)
		{
			return snareModel.gordoEatenCount;
		}
		return 0;
	}

	public int GetTargetCount()
	{
		if (gordoModel != null)
		{
			return gordoModel.targetCount;
		}
		if (snareModel != null)
		{
			return snareModel.gordoTargetCount;
		}
		return 0;
	}

	protected void SetEatenCount(int eatenCount)
	{
		if (gordoModel != null)
		{
			gordoModel.gordoEatenCount = eatenCount;
		}
		else if (snareModel != null)
		{
			snareModel.gordoEatenCount = eatenCount;
		}
	}

	public bool CanEat()
	{
		int eatenCount = GetEatenCount();
		if (eatenCount != -1)
		{
			return eatenCount < GetTargetCount();
		}
		return false;
	}

	public bool MaybeEat(Collider col)
	{
		if (!CanEat())
		{
			return false;
		}
		Identifiable component = col.GetComponent<Identifiable>();
		if (component != null && allEats.Contains(component.id) && !eating.Contains(col.gameObject))
		{
			List<SlimeDiet.EatMapEntry> eatMap = slimeDefinition.Diet.EatMap;
			for (int i = 0; i < eatMap.Count; i++)
			{
				SlimeDiet.EatMapEntry eatMapEntry = eatMap[i];
				if (eatMapEntry.eats == component.id)
				{
					DoEat(col.gameObject);
					SetEatenCount(GetEatenCount() + eatMapEntry.NumToProduce());
					if (eatMapEntry.isFavorite)
					{
						SRBehaviour.SpawnAndPlayFX(EatFavoriteFX, col.gameObject.transform.position, col.gameObject.transform.rotation);
					}
					if (GetEatenCount() >= GetTargetCount())
					{
						StartCoroutine(ReachedTarget());
					}
					return true;
				}
			}
			SetEatenCount(gordoModel.gordoEatenCount);
		}
		return false;
	}

	public void Update()
	{
		float num = global::Noise.Noise.PerlinNoise(0.0, 0f, Time.time, 0.1f, vibrationFactor, 2f);
		float percentageFed = GetPercentageFed();
		float num2 = Mathf.Lerp(initScale, initScale * growthFactor, percentageFed);
		float num3 = 0.7f;
		base.transform.localScale = new Vector3(num2, num2, num2);
		float num4 = vibrateInitScale * ((percentageFed <= num3) ? 1f : (1f + num * (percentageFed - num3) / (1f - num3)));
		toVibrate.localScale = new Vector3(num4, num4, num4);
	}

	public void LateUpdate()
	{
		eating.Clear();
	}

	public float GetPercentageFed()
	{
		int eatenCount = GetEatenCount();
		int num = GetTargetCount();
		return (float)((eatenCount == -1) ? num : eatenCount) / (float)num;
	}

	private void DoEat(GameObject obj)
	{
		if (eatFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(eatFX, obj.transform.position, obj.transform.localRotation);
		}
		if (eatCue != null)
		{
			SECTR_AudioSystem.Play(eatCue, obj.transform.position, loop: false);
		}
		Destroyer.DestroyActor(obj, "GordoEat.DoEat");
		eating.Add(obj);
	}

	private void ImmediateReachedTarget()
	{
		rewards.GiveRewards();
		base.gameObject.SetActive(value: false);
		SetEatenCount(-1);
		SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.BURST_GORDOS, 1);
		AnalyticsUtil.CustomEvent("GordoBurst", new Dictionary<string, object> { { "type", base.name } });
		SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(GetPediaId());
		GordoSnare componentInParent = GetComponentInParent<GordoSnare>();
		if (componentInParent != null)
		{
			componentInParent.Destroy();
			Destroyer.Destroy(base.gameObject, 0f, "GordoEat.ImmediateReachedTarget", asActorOk: true);
		}
	}

	protected virtual PediaDirector.Id GetPediaId()
	{
		return PediaDirector.Id.GORDO_SLIME;
	}

	private IEnumerator ReachedTarget()
	{
		WillStartBurst();
		GetComponent<GordoFaceAnimator>().SetTrigger("Strain");
		SECTR_AudioSystem.Play(strainCue, base.transform.position, loop: false);
		yield return new WaitForSeconds(2f);
		SECTR_AudioSystem.Play(burstCue, base.transform.position, loop: false);
		if (destroyFX != null)
		{
			GameObject obj = SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position + Vector3.up * 2f, base.transform.rotation);
			Identifiable component = base.gameObject.GetComponent<Identifiable>();
			Color[] colors = SlimeUtil.GetColors(base.gameObject, (component != null) ? component.id : Identifiable.Id.NONE, isGordo: true);
			RecolorSlimeMaterial[] componentsInChildren = obj.GetComponentsInChildren<RecolorSlimeMaterial>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetColors(colors[0], colors[1], colors[2]);
			}
		}
		DidCompleteBurst();
		ImmediateReachedTarget();
	}

	public bool DropsKey()
	{
		return rewards.HasKeyReward();
	}

	protected virtual void WillStartBurst()
	{
	}

	protected virtual void DidCompleteBurst()
	{
	}

	public string GetDirectFoodGroupsMsg()
	{
		return slimeDefinition.Diet.GetDirectFoodGroupsMsg();
	}

	protected override string IdPrefix()
	{
		return "gordo";
	}
}

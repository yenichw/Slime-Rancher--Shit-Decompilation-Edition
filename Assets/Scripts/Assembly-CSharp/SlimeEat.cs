using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeEat : CollidableActorBehaviour, Collidable, ActorModel.Participant
{
	public delegate void OnEatDelegate(Identifiable.Id id);

	public delegate void OnFinishChompSuccessDelegate(GameObject gameObject);

	public delegate void OnProducePlortsCompleteDelegate();

	public enum FoodGroup
	{
		FRUIT = 0,
		VEGGIES = 1,
		MEAT = 2,
		NONTARRGOLD_SLIMES = 3,
		PLORTS = 4,
		GINGER = 5
	}

	public SlimeDefinition slimeDefinition;

	public OnEatDelegate onEat;

	public Chomper.OnChompStartDelegate onStartChomp;

	public OnFinishChompSuccessDelegate onFinishChompSuccess;

	public OnProducePlortsCompleteDelegate onProducePlortsComplete;

	public float chanceToSkipProduce;

	public const float WIND_UP_TIME = 1f;

	public const float WIND_UP_TIME_QUICK = 0.25f;

	public const float DIGEST_TIME = 2f;

	public int damagePerAttack = 20;

	public GameObject EatFX;

	public GameObject EatFavoriteFX;

	public GameObject TransformFX;

	public GameObject ProduceFX;

	[Header("Food Groups")]
	[Tooltip("Types of food to ignore even if in the food groups.")]
	public Identifiable.Id[] foodGroupsExceptions;

	[Tooltip("Standard set of objects produced by anything covered by the food groups.")]
	public Identifiable.Id[] foodGroupsProduceId;

	[Tooltip("Standard object to become when eating anything covered by the food groups.")]
	public Identifiable.Id foodGroupsBecomesId;

	[Tooltip("Standard driver to use for anything covered by the food groups.")]
	public SlimeEmotions.Emotion foodGroupsDriver;

	[Tooltip("Standard extra drive to use for anything covered by the food groups.")]
	public float foodGroupsExtraDrive;

	[Tooltip("Standard minimum drive to use for anything covered by the food groups.")]
	public float foodGroupsMinDrive;

	[Space(10f)]
	public float minDriveToEat = 0.333f;

	public float drivePerEat = 0.333f;

	public float agitationPerEat = 0.15f;

	public float agitationPerFavEat = 0.3f;

	private Dictionary<Identifiable.Id, DriveCalculator> allEats = new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer);

	private SlimeEmotions emotions;

	private bool isEatingEnabled = true;

	private SlimeAudio slimeAudio;

	private RegionMember regionMember;

	private Chomper chomper;

	private SlimeFaceAnimator faceAnim;

	private Animator bodyAnim;

	private TentacleGrapple tentacleGrapple;

	private LookupDirector lookupDir;

	private ModDirector modDir;

	private static HashSet<GameObject> claimedFood;

	private static readonly Vector3 LOCAL_PRODUCE_LOC;

	private static readonly Vector3 LOCAL_PRODUCE_VEL;

	private const float TRANSFORM_SCALE_UP_TIME = 0.5f;

	private const float PRODUCE_SCALE_UP_TIME = 0.5f;

	private const float FERAL_EXTRA_DRIVE = 0f;

	private const float HONEY_PLORT_EXTRA_DRIVE = 0.5f;

	private static Dictionary<FoodGroup, Identifiable.Id[]> foodGroupIds;

	private int animDigestingId;

	private SlimeModel slimeModel;

	private SlimeAppearanceApplicator appearanceApplicator;

	private static LRUCache<int, Identifiable> recentIds;

	public static void ClearClaimedFood()
	{
		claimedFood.Clear();
	}

	public List<SlimeDiet.EatMapEntry> GetEatMapById(Identifiable.Id id)
	{
		List<SlimeDiet.EatMapEntry> list = new List<SlimeDiet.EatMapEntry>();
		slimeDefinition.Diet.AddEatMapEntries(id, list);
		return list;
	}

	static SlimeEat()
	{
		claimedFood = new HashSet<GameObject>();
		LOCAL_PRODUCE_LOC = new Vector3(0f, 0.5f, 0f);
		LOCAL_PRODUCE_VEL = new Vector3(0f, 1f, 0f);
		foodGroupIds = new Dictionary<FoodGroup, Identifiable.Id[]>();
		recentIds = new LRUCache<int, Identifiable>(200);
		foodGroupIds[FoodGroup.VEGGIES] = new List<Identifiable.Id>(Identifiable.VEGGIE_CLASS).ToArray();
		foodGroupIds[FoodGroup.FRUIT] = new List<Identifiable.Id>(Identifiable.FRUIT_CLASS).ToArray();
		foodGroupIds[FoodGroup.MEAT] = new List<Identifiable.Id>(Identifiable.MEAT_CLASS).ToArray();
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		foreach (Identifiable.Id value in Enum.GetValues(typeof(Identifiable.Id)))
		{
			if (Identifiable.IsSlime(value) && !Identifiable.IsTarr(value) && value != Identifiable.Id.GOLD_SLIME && value != Identifiable.Id.LUCKY_SLIME)
			{
				list.Add(value);
			}
		}
		foodGroupIds[FoodGroup.NONTARRGOLD_SLIMES] = list.ToArray();
		List<Identifiable.Id> list2 = new List<Identifiable.Id>();
		foreach (Identifiable.Id value2 in Enum.GetValues(typeof(Identifiable.Id)))
		{
			if (Identifiable.IsPlort(value2) && value2 != Identifiable.Id.PUDDLE_PLORT && value2 != Identifiable.Id.GOLD_PLORT && value2 != Identifiable.Id.FIRE_PLORT)
			{
				list2.Add(value2);
			}
		}
		foodGroupIds[FoodGroup.PLORTS] = list2.ToArray();
		foodGroupIds[FoodGroup.GINGER] = new Identifiable.Id[1] { Identifiable.Id.GINGER_VEGGIE };
	}

	public static Identifiable.Id[] GetFoodGroupIds(FoodGroup group)
	{
		return foodGroupIds[group];
	}

	public override void Awake()
	{
		base.Awake();
		chomper = GetComponent<Chomper>();
		slimeAudio = GetComponent<SlimeAudio>();
		faceAnim = GetComponent<SlimeFaceAnimator>();
		regionMember = GetComponent<RegionMember>();
		bodyAnim = GetComponentInChildren<Animator>();
		emotions = GetComponent<SlimeEmotions>();
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		modDir = SRSingleton<SceneContext>.Instance.ModDirector;
		tentacleGrapple = GetComponent<TentacleGrapple>();
		animDigestingId = Animator.StringToHash("Digesting");
		appearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
	}

	public void InitModel(ActorModel actorModel)
	{
	}

	public void SetModel(ActorModel actorModel)
	{
		slimeModel = actorModel as SlimeModel;
		InitFood();
	}

	public void InitFood()
	{
		CalculateAllEats();
	}

	private void CalculateAllEats()
	{
		List<SlimeDiet.EatMapEntry> eatMap = slimeDefinition.Diet.EatMap;
		allEats = new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer);
		PlayerState playerState = ((SRSingleton<SceneContext>.Instance == null) ? null : SRSingleton<SceneContext>.Instance.PlayerState);
		for (int i = 0; i < eatMap.Count; i++)
		{
			if (!(playerState != null) || !SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().preventHostiles || !Identifiable.IsTarr(eatMap[i].becomesId))
			{
				float num = ((eatMap[i].eats == Identifiable.Id.HONEY_PLORT) ? 0.5f : 0f);
				allEats[eatMap[i].eats] = new DriveCalculator(eatMap[i].driver, eatMap[i].extraDrive + num, eatMap[i].minDrive);
			}
		}
	}

	public override void Start()
	{
		base.Start();
		ResetEatClock();
		if (!Identifiable.IsTarr(Identifiable.GetId(base.gameObject)) && regionMember.IsInRegion(RegionRegistry.RegionSetId.SLIMULATIONS))
		{
			isEatingEnabled = false;
		}
	}

	public void ProcessCollisionEnter(Collision col)
	{
		MaybeSpinAndChomp(col.gameObject, ignoreEmotions: false);
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	public bool MaybeSpinAndChomp(GameObject obj, bool ignoreEmotions)
	{
		if (isEatingEnabled && chomper.CanChomp())
		{
			Identifiable.Id id = ExtractOtherId(obj);
			if (allEats.ContainsKey(id) && Identifiable.IsEdible(obj) && (ignoreEmotions || slimeModel.isFeral || (emotions != null && allEats[id].Drive(emotions, id) >= minDriveToEat)) && (tentacleGrapple == null || tentacleGrapple.IsGrappling(obj)))
			{
				base.transform.LookAt(obj.transform);
				chomper.StartChomp(obj, id, whileHeld: false, quick: true, onStartChomp, FinishChomp);
				return true;
			}
		}
		return false;
	}

	public bool MaybeChomp(GameObject obj)
	{
		if (isEatingEnabled && chomper.CanChomp())
		{
			Identifiable.Id id = ExtractOtherId(obj);
			if (allEats.ContainsKey(id) && Identifiable.IsEdible(obj) && (slimeModel.isFeral || (emotions != null && allEats[id].Drive(emotions, id) >= minDriveToEat)))
			{
				chomper.StartChomp(obj, id, whileHeld: false, quick: false, onStartChomp, FinishChomp);
				return true;
			}
		}
		return false;
	}

	public void CancelChomp(GameObject obj)
	{
		chomper.CancelChomp(obj);
	}

	private Identifiable.Id ExtractOtherId(GameObject other)
	{
		int instanceID = other.GetInstanceID();
		if (recentIds.contains(instanceID))
		{
			Identifiable identifiable = recentIds.get(instanceID);
			return (!(identifiable == null)) ? identifiable.id : Identifiable.Id.NONE;
		}
		Identifiable component = other.GetComponent<Identifiable>();
		recentIds.put(instanceID, component);
		return (!(component == null)) ? component.id : Identifiable.Id.NONE;
	}

	private void FinishChomp(GameObject chomping, Identifiable.Id chompingId, bool whileHeld, bool wasLaunched)
	{
		slimeAudio.Play(slimeAudio.slimeSounds.chompCue);
		if (chomping == null || claimedFood.Contains(chomping))
		{
			return;
		}
		claimedFood.Add(chomping);
		faceAnim.SetTrigger("triggerChompClosed");
		for (int i = 0; i < slimeDefinition.Diet.EatMap.Count; i++)
		{
			SlimeDiet.EatMapEntry eatMapEntry = slimeDefinition.Diet.EatMap[i];
			if (eatMapEntry.eats == chompingId)
			{
				if (eatMapEntry.producesId != 0)
				{
					EatAndProduce(chomping, eatMapEntry, immediateMode: false, skipDelays: false, skipProduction: false);
				}
				else if (eatMapEntry.becomesId != 0)
				{
					EatAndTransform(chomping, eatMapEntry, immediateMode: false);
				}
				else
				{
					DoDamage(chomping, immediateMode: false);
				}
				OnEat(eatMapEntry.driver, chompingId, wasLaunched, eatMapEntry.isFavorite);
			}
		}
		if (onFinishChompSuccess != null)
		{
			onFinishChompSuccess(chomping);
		}
	}

	private void EatAndTransform(GameObject target, SlimeDiet.EatMapEntry em, bool immediateMode)
	{
		if (!immediateMode)
		{
			SRBehaviour.SpawnAndPlayFX(TransformFX, base.transform.position, base.transform.rotation);
		}
		if (DoDamage(target, immediateMode))
		{
			SlimeEmotions component = GetComponent<SlimeEmotions>();
			Destroyer.DestroyActor(base.gameObject, "SlimeEat.EatAndTransform");
			GameObject gameObject = SRBehaviour.InstantiateActor(lookupDir.GetPrefab(em.becomesId), regionMember.setId, base.transform.position, base.transform.rotation);
			SlimeEmotions component2 = gameObject.GetComponent<SlimeEmotions>();
			if (component2 != null)
			{
				component2.SetAll(component);
			}
			gameObject.transform.DOScale(gameObject.transform.localScale, 0.5f).From(base.gameObject.transform.localScale).SetEase(Ease.OutElastic);
			gameObject.GetComponent<OnTransformed>()?.OnTransformed();
		}
	}

	private void EatAndProduce(GameObject target, SlimeDiet.EatMapEntry em, bool immediateMode, bool skipDelays, bool skipProduction)
	{
		bodyAnim.SetBool(animDigestingId, value: true);
		if (!immediateMode)
		{
			if (em.isFavorite)
			{
				SRBehaviour.SpawnAndPlayFX(EatFavoriteFX, target.transform.position, target.transform.rotation);
			}
			else
			{
				SRBehaviour.SpawnAndPlayFX(EatFX, target.transform.position, target.transform.rotation);
			}
		}
		if (!DoDamage(target, immediateMode))
		{
			return;
		}
		float delay = 2f;
		int num = em.NumToProduce();
		if (immediateMode)
		{
			delay = 0f;
			num = 1;
		}
		if (target != null)
		{
			Destroyer.DestroyActor(target, "SlimeEat.EatAndProduce");
		}
		if (!skipProduction && (chanceToSkipProduce <= 0f || !Randoms.SHARED.GetProbability(chanceToSkipProduce)))
		{
			GameObject prefab = lookupDir.GetPrefab(em.producesId);
			if (Randoms.SHARED.GetProbability(modDir.ChanceRandomPlort()) && Identifiable.IsPlort(em.producesId))
			{
				prefab = lookupDir.GetPrefab(Randoms.SHARED.Pick(Identifiable.PLORT_CLASS, Identifiable.Id.NONE));
			}
			if (em.producesId == Identifiable.Id.GOLD_PLORT && num >= 3)
			{
				SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.GOLD_SLIME_TRIPLE_PLORT, 1);
			}
			if (skipDelays)
			{
				Produce(num, prefab);
			}
			else
			{
				StartCoroutine(ProduceAfterDelay(num, prefab, delay));
			}
		}
		else if (skipDelays)
		{
			Digest();
		}
		else
		{
			StartCoroutine(DigestOnlyAfterDelay(delay));
		}
	}

	public void EatImmediate(GameObject target, Identifiable.Id eatId, List<Identifiable.Id> produceIds, List<Identifiable.Id> alreadyCollectedIds, bool skipDelays)
	{
		if (target != null)
		{
			if (claimedFood.Contains(target))
			{
				return;
			}
			claimedFood.Add(target);
		}
		List<SlimeDiet.EatMapEntry> eatMap = slimeDefinition.Diet.EatMap;
		SlimeDiet.EatMapEntry eatMapEntry = null;
		foreach (Identifiable.Id produceId in produceIds)
		{
			for (int i = 0; i < eatMap.Count; i++)
			{
				if (eatMap[i].eats == eatId && eatMap[i].producesId == produceId)
				{
					eatMapEntry = eatMap[i];
					bool skipProduction = alreadyCollectedIds.Remove(produceId);
					EatAndProduce(target, eatMapEntry, immediateMode: true, skipDelays, skipProduction);
				}
			}
		}
		if (eatMapEntry != null)
		{
			OnEat(eatMapEntry.driver, eatId, eatingLaunchedFood: false, eatMapEntry.isFavorite);
		}
		SlimeDiet.EatMapEntry eatMapEntry2 = null;
		for (int j = 0; j < eatMap.Count; j++)
		{
			if (eatMap[j].eats == eatId && eatMap[j].becomesId != 0)
			{
				eatMapEntry2 = eatMap[j];
			}
		}
		if (eatMapEntry2 != null)
		{
			EatAndTransform(target, eatMapEntry2, immediateMode: true);
			OnEat(eatMapEntry2.driver, eatId, eatingLaunchedFood: false, eatMapEntry2.isFavorite);
		}
	}

	public List<Identifiable.Id> GetProducedIds(Identifiable.Id foodId, List<Identifiable.Id> producedIdList)
	{
		List<SlimeDiet.EatMapEntry> eatMap = slimeDefinition.Diet.EatMap;
		producedIdList.Clear();
		for (int i = 0; i < eatMap.Count; i++)
		{
			if (eatMap[i].eats == foodId && eatMap[i].producesId != 0)
			{
				producedIdList.Add(eatMap[i].producesId);
				if (eatMap[i].isFavorite)
				{
					producedIdList.Add(eatMap[i].producesId);
				}
			}
		}
		return producedIdList;
	}

	private IEnumerator DigestOnlyAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (base.gameObject != null)
		{
			Digest();
		}
	}

	private void Digest()
	{
		bodyAnim.SetBool(animDigestingId, value: false);
	}

	private IEnumerator ProduceAfterDelay(int count, GameObject produces, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (base.gameObject != null)
		{
			Produce(count, produces);
		}
	}

	private void Produce(int count, GameObject produces)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 position = base.transform.TransformPoint(LOCAL_PRODUCE_LOC);
			Vector3 velocity = base.transform.TransformVector(LOCAL_PRODUCE_VEL);
			if (ProduceFX != null)
			{
				RecolorSlimeMaterial[] componentsInChildren = SRBehaviour.SpawnAndPlayFX(ProduceFX, position, base.transform.rotation).GetComponentsInChildren<RecolorSlimeMaterial>();
				if (componentsInChildren != null && componentsInChildren.Length != 0)
				{
					SlimeAppearance.Palette appearancePalette = appearanceApplicator.GetAppearancePalette();
					RecolorSlimeMaterial[] array = componentsInChildren;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].SetColors(appearancePalette.Top, appearancePalette.Middle, appearancePalette.Bottom);
					}
				}
			}
			GameObject gameObject = SRBehaviour.InstantiateActor(produces, regionMember.setId, position, base.transform.rotation);
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity = velocity;
			}
			PlortInvulnerability component2 = gameObject.GetComponent<PlortInvulnerability>();
			if (component2 != null)
			{
				component2.GoInvulnerable();
			}
			gameObject.transform.DOScale(gameObject.transform.localScale, 0.5f).From(new Vector3(0.01f, 0.01f, 0.01f)).SetEase(Ease.Linear);
		}
		slimeAudio.Play(slimeAudio.slimeSounds.plortCue);
		bodyAnim.SetBool(animDigestingId, value: false);
		if (onProducePlortsComplete != null)
		{
			onProducePlortsComplete();
		}
	}

	private void OnEat(SlimeEmotions.Emotion driver, Identifiable.Id otherId, bool eatingLaunchedFood, bool isFavorite)
	{
		ResetEatClock();
		emotions.Adjust(driver, 0f - drivePerEat);
		emotions.Adjust(SlimeEmotions.Emotion.AGITATION, isFavorite ? (0f - agitationPerFavEat) : (0f - agitationPerEat));
		if (onEat != null)
		{
			onEat(otherId);
		}
		if (Identifiable.IsAnimal(otherId) && CellDirector.IsOnRanch(regionMember))
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.CHICKENS_FED_SLIMES, 1);
		}
		if (isFavorite && CellDirector.IsOnRanch(regionMember))
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.FED_FAVORITE, 1);
		}
		if (eatingLaunchedFood)
		{
			SlimeSubbehaviourPlexer component = GetComponent<SlimeSubbehaviourPlexer>();
			if (component != null && !component.IsGrounded())
			{
				SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.FED_AIRBORNE, 1);
			}
		}
	}

	public bool IsChomping()
	{
		return chomper.IsChomping();
	}

	public void ResetEatClock()
	{
		chomper.ResetEatClock();
	}

	public Dictionary<Identifiable.Id, DriveCalculator> GetAllEats()
	{
		return new Dictionary<Identifiable.Id, DriveCalculator>(allEats, Identifiable.idComparer);
	}

	public bool DoesEat(GameObject gameObject)
	{
		if (DoesEat(ExtractOtherId(gameObject)))
		{
			return Identifiable.IsEdible(gameObject);
		}
		return false;
	}

	public bool DoesEat(Identifiable.Id id)
	{
		return allEats.ContainsKey(id);
	}

	public bool WillNowEat(Identifiable.Id id)
	{
		if (allEats.ContainsKey(id))
		{
			if (!slimeModel.isFeral)
			{
				if (emotions != null)
				{
					return allEats[id].Drive(emotions, id) > minDriveToEat;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool WantsToEat()
	{
		if (slimeModel.isFeral)
		{
			return true;
		}
		if (emotions == null)
		{
			return false;
		}
		if (allEats.Count >= 1)
		{
			return emotions.GetCurr(SlimeEmotions.Emotion.HUNGER) > minDriveToEat;
		}
		return false;
	}

	private bool DoDamage(GameObject other, bool immediateMode)
	{
		if (other == null)
		{
			return true;
		}
		if (!immediateMode)
		{
			slimeAudio.Play(slimeAudio.slimeSounds.gulpCue);
		}
		Damageable interfaceComponent = other.GetInterfaceComponent<Damageable>();
		if (interfaceComponent == null)
		{
			if (!immediateMode)
			{
				PlayOnDeathAudio(other);
			}
			Destroyer.DestroyActor(other, "SlimeEat.DoDamage#1");
			return true;
		}
		if (interfaceComponent.Damage(damagePerAttack, base.gameObject))
		{
			DeathHandler.Kill(other, DeathHandler.Source.SLIME_ATTACK, base.gameObject, "SlimeEat.DoDamage#2");
			if (!immediateMode)
			{
				PlayOnDeathAudio(other);
			}
			return true;
		}
		return false;
	}

	private void PlayOnDeathAudio(GameObject other)
	{
		SlimeAudio componentInChildren = other.GetComponentInChildren<SlimeAudio>();
		if (componentInChildren != null && componentInChildren.slimeSounds.voiceDamageCue != null)
		{
			SECTR_AudioSystem.Play(componentInChildren.slimeSounds.voiceDamageCue, other.transform.position, loop: false);
		}
	}

	public IEnumerable<Identifiable.Id> GetProducedIds()
	{
		return slimeDefinition.Diet.Produces.AsEnumerable();
	}
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeEatWater : SRBehaviour
{
	[Tooltip("The plort to produce when we eat.")]
	public GameObject plort;

	public GameObject produceFX;

	public float slimeDensityDistance = 10f;

	public int maxSlimeDensity;

	public float plortDensityDistance = 10f;

	public int maxPlortDensity = 8;

	public float eatRate = 3f;

	private SlimeEmotions emotions;

	private SlimeEat slimeEat;

	private float nextChompTime;

	private HashSet<LiquidSource> waters = new HashSet<LiquidSource>();

	private SlimeAudio slimeAudio;

	private RegionMember regionMember;

	private SlimeFaceAnimator faceAnim;

	private bool tooDenseToProducePlort;

	private float nextDensityCheck;

	private static readonly Vector3 LOCAL_PRODUCE_LOC = new Vector3(0f, 0.5f, 0f);

	private static readonly Vector3 LOCAL_PRODUCE_VEL = new Vector3(0f, 1f, 0f);

	private const float DENSITY_CHECK_PERIOD = 2f;

	private const float PRODUCE_SCALE_UP_TIME = 0.5f;

	private int nearbyFavoriteToys;

	private List<GameObject> objectsInCell = new List<GameObject>();

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		slimeEat = GetComponent<SlimeEat>();
		slimeAudio = GetComponent<SlimeAudio>();
		faceAnim = GetComponent<SlimeFaceAnimator>();
		regionMember = GetComponent<RegionMember>();
		ResetEatClock();
	}

	public void Update()
	{
		if (Time.time >= nextDensityCheck)
		{
			tooDenseToProducePlort = IsSlimeDensityTooHigh();
			faceAnim.SetShouldBlush(tooDenseToProducePlort);
			if (!tooDenseToProducePlort)
			{
				tooDenseToProducePlort = IsPlortDensityTooHigh();
			}
			nextDensityCheck = Time.time + 2f;
		}
		if (waters.Count > 0 && Time.time >= nextChompTime && emotions.GetCurr(SlimeEmotions.Emotion.HUNGER) > slimeEat.minDriveToEat)
		{
			LiquidSource liquidSource = Randoms.SHARED.Pick(waters, null);
			liquidSource.ConsumeLiquid();
			if (!tooDenseToProducePlort)
			{
				StartCoroutine(ProduceAfterDelay(1, plort, 2f));
			}
			OnEat(SlimeEmotions.Emotion.HUNGER, liquidSource.liquidId);
		}
	}

	private bool IsPlortDensityTooHigh()
	{
		int num = 0;
		float num2 = plortDensityDistance * plortDensityDistance;
		int num3 = CalcMaximumPlortDensity();
		objectsInCell.Clear();
		CellDirector.Get(Identifiable.Id.PUDDLE_PLORT, regionMember, objectsInCell);
		int count = objectsInCell.Count;
		for (int i = 0; i < count; i++)
		{
			if (num > num3)
			{
				break;
			}
			if ((objectsInCell[i].transform.position - base.transform.position).sqrMagnitude <= num2)
			{
				num++;
			}
		}
		objectsInCell.Clear();
		return num > num3;
	}

	private bool IsSlimeDensityTooHigh()
	{
		int num = 0;
		float num2 = slimeDensityDistance * slimeDensityDistance;
		int num3 = CalcMaximumSlimeDensity();
		objectsInCell.Clear();
		CellDirector.GetSlimes(regionMember, objectsInCell);
		int count = objectsInCell.Count;
		for (int i = 0; i < count; i++)
		{
			if (num > num3)
			{
				break;
			}
			if ((objectsInCell[i].transform.position - base.transform.position).sqrMagnitude <= num2)
			{
				num++;
			}
		}
		objectsInCell.Clear();
		return num > num3;
	}

	public int CalcMaximumSlimeDensity()
	{
		if (nearbyFavoriteToys > 0)
		{
			return maxSlimeDensity + 1;
		}
		return maxSlimeDensity;
	}

	public int CalcMaximumPlortDensity()
	{
		return maxPlortDensity;
	}

	public void OnTriggerEnter(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId))
		{
			waters.Add(component);
		}
	}

	public void OnTriggerExit(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId))
		{
			waters.Remove(component);
		}
	}

	public void ResetEatClock()
	{
		nextChompTime = Time.time + eatRate;
	}

	public void EnterToyProximity()
	{
		nearbyFavoriteToys++;
	}

	public void ExitToyProximity()
	{
		nearbyFavoriteToys--;
	}

	private void OnEat(SlimeEmotions.Emotion driver, Identifiable.Id otherId)
	{
		ResetEatClock();
		emotions.Adjust(driver, 0f - slimeEat.drivePerEat);
		if (otherId != Identifiable.Id.PLAYER)
		{
			emotions.Adjust(SlimeEmotions.Emotion.AGITATION, 0f - slimeEat.agitationPerEat);
		}
	}

	private IEnumerator ProduceAfterDelay(int count, GameObject produces, float delay)
	{
		yield return new WaitForSeconds(delay);
		Produce(count, produces, immediate: false);
	}

	private void Produce(int count, GameObject produces, bool immediate)
	{
		if (!(base.gameObject != null))
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 position = base.transform.TransformPoint(LOCAL_PRODUCE_LOC);
			Vector3 velocity = base.transform.TransformVector(LOCAL_PRODUCE_VEL);
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
			gameObject.transform.DOScale(gameObject.transform.localScale, 0.5f).From(0.001f);
			if (!immediate && produceFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(produceFX, position, base.transform.rotation);
			}
		}
		slimeAudio.Play(slimeAudio.slimeSounds.plortCue);
	}

	public bool WillNowEat(Identifiable.Id id)
	{
		if (Identifiable.IsWater(id))
		{
			return emotions.GetCurr(SlimeEmotions.Emotion.HUNGER) > slimeEat.minDriveToEat;
		}
		return false;
	}

	public List<Identifiable.Id> GetProducedIds(Identifiable.Id id, List<Identifiable.Id> produced)
	{
		produced.Clear();
		if (Identifiable.IsWater(id))
		{
			produced.Add(Identifiable.Id.PUDDLE_PLORT);
		}
		return produced;
	}

	public void EatImmediate(GameObject target, Identifiable.Id id, List<Identifiable.Id> produced, List<Identifiable.Id> collected, bool skipDelays)
	{
		LiquidSource component = target.GetComponent<LiquidSource>();
		component.ConsumeLiquid();
		if (!tooDenseToProducePlort)
		{
			Produce(1, plort, immediate: true);
		}
		OnEat(SlimeEmotions.Emotion.HUNGER, component.liquidId);
	}
}

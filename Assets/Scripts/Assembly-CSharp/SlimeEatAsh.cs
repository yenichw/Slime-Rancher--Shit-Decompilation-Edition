using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeEatAsh : SRBehaviour
{
	[Tooltip("The plort to produce when we eat.")]
	public GameObject plort;

	public GameObject produceFX;

	public float eatRate = 3f;

	private SlimeEmotions emotions;

	private SlimeEat slimeEat;

	private RegionMember regionMember;

	private float nextChompTime;

	private HashSet<AshSource> ashes = new HashSet<AshSource>();

	private SlimeAudio slimeAudio;

	private static readonly Vector3 LOCAL_PRODUCE_LOC = new Vector3(0f, 0.5f, 0f);

	private static readonly Vector3 LOCAL_PRODUCE_VEL = new Vector3(0f, 1f, 0f);

	private const float PRODUCE_SCALE_UP_TIME = 0.5f;

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		slimeEat = GetComponent<SlimeEat>();
		slimeAudio = GetComponent<SlimeAudio>();
		regionMember = GetComponent<RegionMember>();
		ResetEatClock();
	}

	public void Update()
	{
		if (ashes.Count > 0 && Time.time >= nextChompTime && emotions.GetCurr(SlimeEmotions.Emotion.HUNGER) > slimeEat.minDriveToEat)
		{
			AshSource ashSource = Randoms.SHARED.Pick(ashes, null);
			if (ashSource.Available())
			{
				ashSource.ConsumeAsh();
				StartCoroutine(ProduceAfterDelay(1, plort, 2f));
				OnEat(SlimeEmotions.Emotion.HUNGER, Identifiable.Id.NONE);
			}
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		AshSource component = col.gameObject.GetComponent<AshSource>();
		if (component != null)
		{
			ashes.Add(component);
		}
	}

	public void OnCollisionExit(Collision col)
	{
		AshSource component = col.gameObject.GetComponent<AshSource>();
		if (component != null)
		{
			ashes.Remove(component);
		}
	}

	public void ResetEatClock()
	{
		nextChompTime = Time.time + eatRate;
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
		if (!(base.gameObject != null))
		{
			yield break;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 position = base.transform.TransformPoint(LOCAL_PRODUCE_LOC);
			Vector3 velocity = base.transform.TransformVector(LOCAL_PRODUCE_VEL);
			if (produceFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(produceFX, position, base.transform.rotation);
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
			gameObject.transform.DOScale(gameObject.transform.localScale, 0.5f).From(0.001f);
		}
		slimeAudio.Play(slimeAudio.slimeSounds.plortCue);
	}
}

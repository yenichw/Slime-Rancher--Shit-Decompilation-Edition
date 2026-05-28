using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckySlimeProduceCoins : CollidableActorBehaviour, Collidable
{
	public GameObject coinsPrefab;

	private LuckySlimeFlee flee;

	private HashSet<GameObject> colliders = new HashSet<GameObject>();

	private SlimeAudio slimeAudio;

	private SlimeFaceAnimator sfAnimator;

	private SlimeEat slimeEat;

	private int coinSetsProduced;

	private int coinPrefabsLastHit;

	private const float HIT_THRESHOLD = 0.02f;

	private const float JUMP_ON_HIT_VERT_FORCE = 450f;

	private const float JUMP_ON_HIT_MAX_HORIZ_FORCE = 225f;

	private const int MIN_INIT_COIN_PREFABS = 2;

	private const int MAX_INIT_COIN_PREFABS = 2;

	private const int MAX_TOTAL_COIN_PREFABS = 6;

	private const float DELAY_BETWEEN_COINS = 0.1f;

	private const float ADDL_DELAY = 0.1f;

	public override void Start()
	{
		base.Start();
		flee = GetComponent<LuckySlimeFlee>();
		slimeAudio = GetComponent<SlimeAudio>();
		sfAnimator = GetComponent<SlimeFaceAnimator>();
		slimeEat = GetComponent<SlimeEat>();
	}

	public void ProcessCollisionEnter(Collision collision)
	{
		Identifiable component = collision.gameObject.GetComponent<Identifiable>();
		if (!(component != null) || !Identifiable.IsAnimal(component.id) || colliders.Contains(collision.gameObject))
		{
			return;
		}
		float num = float.NegativeInfinity;
		ContactPoint[] contacts = collision.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			float num2 = Vector3.Dot(contactPoint.normal, collision.relativeVelocity);
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num > 0.02f)
		{
			ProduceCoins(collision.gameObject);
			if (flee != null)
			{
				flee.StartFleeing(SRSingleton<SceneContext>.Instance.Player);
			}
			colliders.Add(collision.gameObject);
			Identifiable component2 = GetComponent<Identifiable>();
			SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(component2.id);
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	private IEnumerator DropCoinsAndJumpDelayed()
	{
		yield return new WaitForSeconds(0.35f);
		GetComponent<Rigidbody>().AddForce(new Vector3(Randoms.SHARED.GetFloat(225f), 450f, Randoms.SHARED.GetFloat(225f)));
		slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
		slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
		int ii = 0;
		while (ii < coinPrefabsLastHit)
		{
			SRBehaviour.InstantiateDynamic(coinsPrefab, base.transform.position, base.transform.rotation);
			yield return new WaitForSeconds(0.1f);
			int num = ii + 1;
			ii = num;
		}
	}

	private void ProduceCoins(GameObject triggerer)
	{
		if (slimeEat.MaybeSpinAndChomp(triggerer, ignoreEmotions: true))
		{
			if (coinPrefabsLastHit == 0)
			{
				coinPrefabsLastHit = Randoms.SHARED.GetInRange(2, 3);
			}
			else
			{
				coinPrefabsLastHit = Math.Min(6, coinPrefabsLastHit * 2);
			}
			StartCoroutine(DropCoinsAndJumpDelayed());
			sfAnimator.SetTrigger("triggerWince");
			coinSetsProduced++;
		}
	}
}

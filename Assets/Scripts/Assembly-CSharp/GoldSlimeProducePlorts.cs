using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GoldSlimeProducePlorts : CollidableActorBehaviour, Collidable, ActorModel.Participant
{
	public GameObject plortPrefab;

	public GameObject plortFX;

	private GoldSlimeFlee flee;

	private RegionMember regionMember;

	private HashSet<GameObject> colliders = new HashSet<GameObject>();

	private SlimeAudio slimeAudio;

	private int plortsProduced;

	private PlayerState playerState;

	private SlimeModel model;

	private const float PLORT_THRESHOLD = 0.02f;

	private const float JUMP_ON_HIT_FORCE = 400f;

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(ActorModel model)
	{
		this.model = (SlimeModel)model;
	}

	public override void Start()
	{
		base.Start();
		flee = GetComponent<GoldSlimeFlee>();
		regionMember = GetComponent<RegionMember>();
		slimeAudio = GetComponent<SlimeAudio>();
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	private bool IdentCausesPlorts(Identifiable.Id id)
	{
		if (id != Identifiable.Id.GINGER_VEGGIE && id != Identifiable.Id.GOLD_PLORT && (Identifiable.IsFood(id) || Identifiable.IsChick(id) || Identifiable.IsPlort(id)))
		{
			return true;
		}
		return false;
	}

	public void ProcessCollisionEnter(Collision collision)
	{
		Identifiable component = collision.gameObject.GetComponent<Identifiable>();
		if (!(component != null))
		{
			return;
		}
		Identifiable.Id id = component.id;
		if (!IdentCausesPlorts(id) || colliders.Contains(collision.gameObject))
		{
			return;
		}
		PlortInvulnerability component2 = collision.gameObject.GetComponent<PlortInvulnerability>();
		if (!(component2 == null) && component2.enabled)
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
			ProducePlort();
			if (flee != null)
			{
				flee.StartFleeing(SRSingleton<SceneContext>.Instance.Player);
			}
			colliders.Add(collision.gameObject);
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	private void ProduceAt(Vector3 dropAt)
	{
		PlortInvulnerability component = SRBehaviour.InstantiateActor(plortPrefab, regionMember.setId, dropAt, base.transform.rotation).GetComponent<PlortInvulnerability>();
		if (component != null)
		{
			component.GoInvulnerable();
		}
		plortsProduced++;
		if (plortsProduced == 3)
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.GOLD_SLIME_TRIPLE_PLORT, 1);
		}
	}

	private void ProducePlort()
	{
		if (!model.isGlitch)
		{
			Vector3 vector = base.transform.position - base.transform.forward;
			ProduceAt(vector);
			if (playerState.HasUpgrade(PlayerState.Upgrade.GOLDEN_SURESHOT))
			{
				ProduceAt(vector - base.transform.forward * 0.25f);
				ProduceAt(vector - base.transform.forward * 0.5f);
			}
			GetComponent<Rigidbody>().AddForce(Vector3.up * 400f);
			slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
			slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
			if (plortFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(plortFX, base.transform.position, base.transform.rotation);
			}
		}
	}
}

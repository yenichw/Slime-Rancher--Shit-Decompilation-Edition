using System.Collections.Generic;
using UnityEngine;

public class ChickenVampirism : FindConsumable, ControllerCollisionListener
{
	private class ChickenDriveCalculator : DriveCalculator
	{
		public ChickenDriveCalculator()
			: base(SlimeEmotions.Emotion.NONE, 0f, 0f)
		{
		}

		public override float Drive(SlimeEmotions emotions, Identifiable.Id id)
		{
			return 1f;
		}
	}

	public GameObject fx;

	public int damagePerTouch = 10;

	public float repeatTime = 1f;

	public float maxJump = 12f;

	private GameObject activeFX;

	private GameObject target;

	private float nextTime;

	private bool isNight;

	private TimeDirector timeDir;

	private ModDirector modDir;

	private float nextLeapAvail;

	private const float INIT_NO_DAMAGE_WINDOW = 0.1f;

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		modDir = SRSingleton<SceneContext>.Instance.ModDirector;
		modDir.RegisterModsListener(SetEnabled);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		modDir.UnregisterModsListener(SetEnabled);
	}

	private void SetEnabled()
	{
		base.enabled = modDir.VampiricChickens();
		if (!base.enabled && activeFX != null)
		{
			Destroyer.Destroy(activeFX, "ChickenVampirism.SetEnabled");
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (!modDir.VampiricChickens())
		{
			base.enabled = false;
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (activeFX != null)
		{
			Destroyer.Destroy(activeFX, "ChickenVampirism.OnDisable");
		}
	}

	public void Update()
	{
		float num = timeDir.CurrDayFraction();
		isNight = num < 0.25f || num > 0.75f;
		if (isNight && activeFX == null)
		{
			activeFX = Object.Instantiate(fx);
			activeFX.transform.SetParent(base.transform, worldPositionStays: false);
		}
		else if (!isNight && activeFX != null)
		{
			Destroyer.Destroy(activeFX, "ChickenVampirism.Update");
		}
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		if (base.enabled && isNight && Time.time >= nextTime)
		{
			if (gameObj.GetInterfaceComponent<Damageable>().Damage(damagePerTouch, base.gameObject))
			{
				DeathHandler.Kill(gameObj, DeathHandler.Source.CHICKEN_VAMPIRISM, base.gameObject, "ChickenVampirism.OnControllerCollision");
			}
			nextTime = Time.time + repeatTime;
		}
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		return new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer) { [Identifiable.Id.PLAYER] = new ChickenDriveCalculator() };
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!base.enabled || !isNight)
		{
			return 0f;
		}
		target = FindNearestConsumable(out var _);
		if (!(target == null))
		{
			return 0.99f;
		}
		return 0f;
	}

	public override void Selected()
	{
	}

	public override void Action()
	{
		if (target != null)
		{
			MoveTowards(SlimeSubbehaviour.GetGotoPos(target), IsBlocked(target), ref nextLeapAvail, maxJump);
		}
	}
}

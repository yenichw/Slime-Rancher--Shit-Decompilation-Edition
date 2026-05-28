using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class WeaponVacuum : SRBehaviour, PlayerModel.Participant
{
	private class IncomingLiquid
	{
		public Identifiable.Id id;

		public float nextUnitTick;

		public GameObject fx;

		public IncomingLiquid(Identifiable.Id id, float nextUnitTick, GameObject fx)
		{
			this.id = id;
			this.nextUnitTick = nextUnitTick;
			this.fx = fx;
		}
	}

	private enum VacMode
	{
		NONE = 0,
		SHOOT = 1,
		VAC = 2,
		GADGET = 3
	}

	private class VacAudioHandler
	{
		private WeaponVacuum vac;

		private bool active;

		public VacAudioHandler(WeaponVacuum vac)
		{
			this.vac = vac;
		}

		public void SetActive(bool active)
		{
			if (active && !this.active)
			{
				vac.vacAudio.Cue = vac.vacStartCue;
				vac.vacAudio.Play();
				vac.vacAudio.Cue = vac.vacRunCue;
				vac.vacAudio.Play();
			}
			else if (!active && this.active)
			{
				vac.vacAudio.Cue = vac.vacEndCue;
				vac.vacAudio.Play();
			}
			this.active = active;
		}
	}

	private class MoveTowards : SRBehaviour
	{
		public Transform dest;

		private float endTime;

		public void Awake()
		{
			endTime = Time.fixedTime + 0.2f;
		}

		public void FixedUpdate()
		{
			float fixedTime = Time.fixedTime;
			float num = endTime - fixedTime;
			float t = ((num <= Time.fixedDeltaTime) ? 1f : (Time.fixedDeltaTime / num));
			base.transform.position = Vector3.Lerp(base.transform.position, dest.position, t);
		}
	}

	public float ejectSpeed = 25f;

	public float shootCooldown = 0.24f;

	public GameObject vacRegion;

	public GameObject vacOrigin;

	public GameObject vacJointPrefab;

	public Joint lockJoint;

	public GameObject vacFX;

	public float maxVacDist = 20f;

	public float captureDist = 1f;

	public float minJointSpeed = 3f;

	public float maxJointSpeed = 6f;

	public float minSpringStrength = 5f;

	public float maxSpringStrength = 20f;

	public float airBurstPower = 1200f;

	public float airBurstRadius = 20f;

	public GameObject airBurstFX;

	public float staminaPerBurst = 20f;

	public GameObject shootFX;

	public GameObject captureFX;

	public GameObject captureFailedFX;

	public GameObject heldFaceTowards;

	public SECTR_AudioCue vacStartCue;

	public SECTR_AudioCue vacRunCue;

	public SECTR_AudioCue vacEndCue;

	public SECTR_AudioCue vacShootCue;

	public SECTR_AudioCue vacShootEmptyCue;

	public SECTR_AudioCue vacAmmoSelectCue;

	public SECTR_AudioCue vacBurstCue;

	public SECTR_AudioCue vacBurstNoEnergyCue;

	public SECTR_AudioCue vacHeldCue;

	public SECTR_AudioCue gadgetModeOnCue;

	public SECTR_AudioCue gadgetModeOffCue;

	public SiloActivator siloActivator;

	public GameObject destroyOnVacFX;

	[Tooltip("Reference to the HeldCamera GameObject.")]
	public GameObject cameraHeld;

	private float nextShot;

	private PlayerState player;

	private GameObject held;

	private double heldStartTime;

	private float heldRad;

	private bool launchedHeld;

	private SECTR_PointSource vacAudio;

	private List<Joint> joints = new List<Joint>();

	private VacAudioHandler vacAudioHandler;

	private Animator vacAnimator;

	private VacColorAnimator vacColorAnimator;

	private Vector3? lastWeaponPos;

	private RegionRegistry regionRegistry;

	private PediaDirector pediaDir;

	private TutorialDirector tutDir;

	private AchievementsDirector achieveDir;

	private TimeDirector timeDir;

	private OptionsDirector optionsDir;

	private ProgressDirector progressDir;

	private int inWaterCount;

	private Dictionary<LiquidSource, IncomingLiquid> liquidDict = new Dictionary<LiquidSource, IncomingLiquid>();

	private vp_FPPlayerEventHandler playerEvents;

	private bool slimeFilter;

	private HashSet<Vacuumable> animatingConsume = new HashSet<Vacuumable>();

	private TrackCollisions tracker;

	private PlayerModel playerModel;

	private RegionMember member;

	private VacMode vacMode;

	private const int VAC_RAY_MASK = -536887557;

	private int animActiveId;

	private int animHoldingId;

	private int animVacModeId;

	private int animSprintingId;

	private int animSwitchSlotsId;

	private const float LIQUID_UNIT_TIME = 0.25f;

	private const float SHOOT_SCALE_UP_TIME = 0.1f;

	private const float SHOOT_SCALE_FACTOR = 0.2f;

	private const float CONSUME_SCALE_DOWN_TIME = 0.2f;

	private const float CONSUME_SCALE_FACTOR = 0.1f;

	private const float HOLD_SCREEN_SHAKE = 1f;

	private const float VALLEY_EJECT_SPEED_FACTOR = 3f;

	private static LRUCache<int, Vacuumable> recentIds = new LRUCache<int, Vacuumable>(1000);

	public float facingSpeed = 10f;

	public void Awake()
	{
		vacAudio = GetComponent<SECTR_PointSource>();
		vacAudioHandler = new VacAudioHandler(this);
		regionRegistry = SRSingleton<SceneContext>.Instance.RegionRegistry;
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		optionsDir = SRSingleton<GameContext>.Instance.OptionsDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		playerEvents = GetComponentInParent<vp_FPPlayerEventHandler>();
		member = GetComponentInParent<RegionMember>();
		tracker = vacRegion.GetComponent<TrackCollisions>();
		animActiveId = Animator.StringToHash("active");
		animHoldingId = Animator.StringToHash("holding");
		animVacModeId = Animator.StringToHash("vacMode");
		animSprintingId = Animator.StringToHash("sprinting");
		animSwitchSlotsId = Animator.StringToHash("switchSlots");
	}

	public void Start()
	{
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		nextShot = Time.fixedTime;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
		cameraHeld.SetActive(value: false);
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
		playerModel = model;
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
	}

	public void TransformChanged(Vector3 pos, Quaternion rot)
	{
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> registeredPotentialAmmo)
	{
	}

	public void KeyAdded()
	{
	}

	public void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		HashSet<GameObject> inVac = tracker.CurrColliders();
		UpdateHud(inVac);
		UpdateSlotForInputs();
		UpdateVacModeForInputs();
		SRSingleton<SceneContext>.Instance.PlayerState.InGadgetMode = vacMode == VacMode.GADGET;
		if (SRInput.Actions.attack.WasPressed || SRInput.Actions.vac.WasPressed || SRInput.Actions.burst.WasPressed)
		{
			launchedHeld = false;
		}
		float num = 1f;
		if (Time.fixedTime >= nextShot && !launchedHeld && vacMode == VacMode.SHOOT)
		{
			Expel(inVac);
			num = GetShootSpeedFactor(inVac);
			nextShot = Time.fixedTime + shootCooldown / num;
		}
		if (vacAnimator != null)
		{
			vacAnimator.speed = num;
		}
		if (!launchedHeld && vacMode == VacMode.VAC)
		{
			vacAudioHandler.SetActive(active: true);
			vacFX.SetActive(held == null);
			siloActivator.enabled = held == null;
			if (held != null)
			{
				UpdateHeld(inVac);
			}
			else
			{
				Consume(inVac);
			}
		}
		else
		{
			ClearVac();
		}
		UpdateVacAnimators();
	}

	private float GetShootSpeedFactor(HashSet<GameObject> inVac)
	{
		float num = 1f;
		foreach (GameObject item in inVac)
		{
			VacShootAccelerator component = item.GetComponent<VacShootAccelerator>();
			if (component != null)
			{
				num = Math.Max(num, component.GetVacShootSpeedFactor());
			}
		}
		return num;
	}

	private void UpdateVacAnimators()
	{
		bool flag = playerModel.hasAirBurst && SRInput.Actions.burst.WasPressed;
		bool flag2 = vacMode == VacMode.SHOOT || vacMode == VacMode.VAC || flag;
		bool value = vacMode == VacMode.VAC;
		if (vacAnimator == null)
		{
			vacAnimator = GetComponentInChildren<Animator>();
			vacColorAnimator = GetComponentInChildren<VacColorAnimator>();
		}
		vacAnimator.SetBool(animActiveId, flag2);
		vacAnimator.SetBool(animVacModeId, value);
		vacAnimator.SetBool(animHoldingId, held != null);
		vacColorAnimator.SetVacActive(flag2);
		vacColorAnimator.SetVacMode(value);
		if (flag)
		{
			AirBurst();
		}
		vacAnimator.SetBool(animSprintingId, playerEvents.Run.Active);
	}

	private void ClearVac()
	{
		ClearLiquids();
		vacAudioHandler.SetActive(active: false);
		vacFX.SetActive(value: false);
		siloActivator.enabled = false;
		foreach (Joint joint in joints)
		{
			if (joint != null && joint.connectedBody != null)
			{
				Vacuumable component = joint.connectedBody.GetComponent<Vacuumable>();
				if (component != null)
				{
					component.release();
				}
			}
		}
		if (held != null)
		{
			Vacuumable component2 = held.GetComponent<Vacuumable>();
			if (component2 != null)
			{
				component2.release();
			}
			lockJoint.connectedBody = null;
			Identifiable component3 = held.GetComponent<Identifiable>();
			held = null;
			SetHeldRad(0f);
			if (component3 != null && Identifiable.IsTarr(component3.id))
			{
				int val = (int)Math.Floor((timeDir.WorldTime() - heldStartTime) * 0.01666666753590107);
				achieveDir.MaybeUpdateMaxStat(AchievementsDirector.IntStat.EXTENDED_TARR_HOLD, val);
			}
			heldStartTime = double.NaN;
		}
	}

	private void SetHeldRad(float rad)
	{
		heldRad = rad;
		Vector3 localPosition = lockJoint.transform.localPosition;
		if (rad == 0f)
		{
			lockJoint.transform.localPosition = new Vector3(localPosition.x, 1f, localPosition.z);
		}
		else
		{
			lockJoint.transform.localPosition = new Vector3(localPosition.x, rad, localPosition.z);
		}
	}

	private void UpdateHeld(HashSet<GameObject> inVac)
	{
		Rigidbody component = held.GetComponent<Rigidbody>();
		if (lockJoint.connectedBody != component)
		{
			held.transform.position = lockJoint.transform.position;
			lockJoint.connectedBody = component;
			lockJoint.anchor = Vector3.zero;
			lockJoint.connectedAnchor = Vector3.zero;
		}
		foreach (GameObject item in inVac)
		{
			if (item != held)
			{
				Vacuumable component2 = item.GetComponent<Vacuumable>();
				if (component2 != null)
				{
					component2.release();
				}
			}
		}
		ClearLiquids();
	}

	public void LateUpdate()
	{
		if (held != null)
		{
			held.transform.position = lockJoint.transform.position;
			held.transform.LookAt(heldFaceTowards.transform, heldFaceTowards.transform.up);
		}
	}

	public bool InVacMode()
	{
		return vacMode == VacMode.VAC;
	}

	private void PlayTransientAudio(SECTR_AudioCue cue)
	{
		SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}

	private void UpdateSlotForInputs()
	{
		if (SRInput.Actions.slot1.WasPressed)
		{
			if (player.Ammo.SetAmmoSlot(0))
			{
				PlayTransientAudio(vacAmmoSelectCue);
				vacAnimator.SetTrigger(animSwitchSlotsId);
			}
		}
		else if (SRInput.Actions.slot2.WasPressed)
		{
			if (player.Ammo.SetAmmoSlot(1))
			{
				PlayTransientAudio(vacAmmoSelectCue);
				vacAnimator.SetTrigger(animSwitchSlotsId);
			}
		}
		else if (SRInput.Actions.slot3.WasPressed)
		{
			if (player.Ammo.SetAmmoSlot(2))
			{
				PlayTransientAudio(vacAmmoSelectCue);
				vacAnimator.SetTrigger(animSwitchSlotsId);
			}
		}
		else if (SRInput.Actions.slot4.WasPressed)
		{
			if (player.Ammo.SetAmmoSlot(3))
			{
				PlayTransientAudio(vacAmmoSelectCue);
				vacAnimator.SetTrigger(animSwitchSlotsId);
			}
		}
		else if (SRInput.Actions.slot5.WasPressed)
		{
			if (player.Ammo.SetAmmoSlot(4))
			{
				PlayTransientAudio(vacAmmoSelectCue);
				vacAnimator.SetTrigger(animSwitchSlotsId);
			}
		}
		else if (SRInput.Actions.prevSlot.WasPressed)
		{
			player.Ammo.PrevAmmoSlot();
			PlayTransientAudio(vacAmmoSelectCue);
			vacAnimator.SetTrigger(animSwitchSlotsId);
		}
		else if (SRInput.Actions.nextSlot.WasPressed)
		{
			player.Ammo.NextAmmoSlot();
			PlayTransientAudio(vacAmmoSelectCue);
			vacAnimator.SetTrigger(animSwitchSlotsId);
		}
	}

	private void UpdateVacModeForInputs()
	{
		if (SRInput.Actions.toggleGadgetMode.WasReleased && progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_LAB))
		{
			if (vacMode == VacMode.GADGET)
			{
				vacMode = VacMode.NONE;
				SRSingleton<Overlay>.Instance.SetEnableGadgetMode(enabled: false);
				PlayTransientAudio(gadgetModeOnCue);
			}
			else
			{
				vacMode = VacMode.GADGET;
				SRSingleton<Overlay>.Instance.SetEnableGadgetMode(enabled: true);
				tutDir.OnGadgetModeActivated();
				PlayTransientAudio(gadgetModeOffCue);
			}
		}
		switch (vacMode)
		{
		case VacMode.NONE:
			if (SRInput.Actions.vac.WasPressed)
			{
				vacMode = VacMode.VAC;
			}
			else if (SRInput.Actions.attack.WasPressed)
			{
				vacMode = VacMode.SHOOT;
			}
			break;
		case VacMode.VAC:
			if (SRInput.Actions.attack.WasPressed)
			{
				vacMode = VacMode.SHOOT;
			}
			else if ((optionsDir.vacLockOnHold && held != null) ? SRInput.Actions.vac.WasPressed : (!SRInput.Actions.vac.IsPressed))
			{
				vacMode = VacMode.NONE;
			}
			break;
		case VacMode.SHOOT:
			if (SRInput.Actions.vac.WasPressed)
			{
				vacMode = VacMode.VAC;
			}
			else if (!SRInput.Actions.attack.IsPressed)
			{
				vacMode = VacMode.NONE;
			}
			break;
		}
	}

	public void EnterWater()
	{
		inWaterCount++;
	}

	public void ExitWater()
	{
		inWaterCount--;
	}

	public bool IsHolding()
	{
		return held != null;
	}

	public Identifiable.Id HeldIdent()
	{
		if (held != null)
		{
			return held.GetComponent<Identifiable>().id;
		}
		return Identifiable.Id.NONE;
	}

	public void DropAllVacced()
	{
		foreach (Joint joint in joints)
		{
			if (joint != null && joint.connectedBody != null)
			{
				Vacuumable component = joint.connectedBody.GetComponent<Vacuumable>();
				if (component != null)
				{
					component.release();
				}
			}
		}
		if (held != null)
		{
			Vacuumable component2 = held.GetComponent<Vacuumable>();
			if (component2 != null)
			{
				component2.release();
			}
			lockJoint.connectedBody = null;
			held = null;
			SetHeldRad(0f);
			heldStartTime = double.NaN;
		}
		vacAudioHandler.SetActive(active: false);
		if (vacAnimator == null)
		{
			vacAnimator = GetComponentInChildren<Animator>();
			vacColorAnimator = GetComponentInChildren<VacColorAnimator>();
		}
		vacAnimator.SetBool(animActiveId, value: false);
		vacAnimator.SetBool(animVacModeId, value: false);
		vacAnimator.SetBool(animHoldingId, value: false);
		vacColorAnimator.SetVacActive(isActive: false);
		vacColorAnimator.SetVacMode(inVacMode: false);
	}

	private void ClearLiquids()
	{
		foreach (IncomingLiquid value in liquidDict.Values)
		{
			Destroyer.Destroy(value.fx, "WeaponVacuum.ClearLiquids");
		}
		liquidDict.Clear();
	}

	private void AirBurst()
	{
		if ((float)player.GetCurrEnergy() < staminaPerBurst)
		{
			PlayTransientAudio(vacBurstNoEnergyCue);
			return;
		}
		AnalyticsUtil.CustomEvent("PulseWaveUsed");
		PlayTransientAudio(vacBurstCue);
		Vector3 position = vacOrigin.transform.position;
		Collider[] array = Physics.OverlapSphere(position, airBurstRadius);
		foreach (Collider collider in array)
		{
			if ((bool)collider && collider.GetComponent<Rigidbody>() != null && collider.gameObject != base.gameObject)
			{
				Identifiable component = collider.gameObject.GetComponent<Identifiable>();
				if (component != null && Identifiable.IsSlime(component.id) && Vector3.Dot(vacOrigin.transform.up, collider.gameObject.transform.position - vacOrigin.transform.position) > 0f)
				{
					Rigidbody component2 = collider.GetComponent<Rigidbody>();
					PhysicsUtil.SoftExplosionForce(airBurstPower * component2.mass, position, airBurstRadius, component2);
				}
			}
		}
		player.SpendEnergy(staminaPerBurst);
		if (airBurstFX != null)
		{
			UnityEngine.Object.Instantiate(airBurstFX, Vector3.zero, Quaternion.identity).transform.SetParent(vacOrigin.transform, worldPositionStays: false);
		}
	}

	private void Expel(HashSet<GameObject> inVac)
	{
		if (held != null)
		{
			ExpelHeld();
		}
		else if (player.Ammo.HasSelectedAmmo())
		{
			ExpelAmmo(inVac);
		}
		else
		{
			PlayTransientAudio(vacShootEmptyCue);
		}
	}

	private void ExpelHeld()
	{
		vp_FPController componentInParent = GetComponentInParent<vp_FPController>();
		Ray ray = new Ray(vacOrigin.transform.position, vacOrigin.transform.up);
		Vector3 origin = ray.origin;
		Vector3 vel = ray.direction * ejectSpeed + componentInParent.Velocity;
		origin = EnsureNotShootingIntoRock(origin, ray, heldRad, ref vel);
		held.transform.position = origin;
		held.GetComponent<Rigidbody>().velocity = vel;
		Vacuumable component = held.GetComponent<Vacuumable>();
		if (component != null)
		{
			component.release();
			component.Launch(Vacuumable.LaunchSource.PLAYER);
			SlimeEat component2 = held.GetComponent<SlimeEat>();
			if (component2 != null)
			{
				component2.CancelChomp(SRSingleton<SceneContext>.Instance.Player);
			}
		}
		DamagePlayerOnTouch component3 = held.GetComponent<DamagePlayerOnTouch>();
		if (component3 != null)
		{
			component3.ResetDamageAmnesty();
		}
		lockJoint.connectedBody = null;
		Identifiable component4 = held.GetComponent<Identifiable>();
		held = null;
		SetHeldRad(0f);
		if (component4 != null && Identifiable.IsTarr(component4.id))
		{
			int val = (int)Math.Floor((timeDir.WorldTime() - heldStartTime) * 0.01666666753590107);
			achieveDir.MaybeUpdateMaxStat(AchievementsDirector.IntStat.EXTENDED_TARR_HOLD, val);
		}
		heldStartTime = double.NaN;
		launchedHeld = true;
		ShootEffect();
	}

	private void ExpelAmmo(HashSet<GameObject> inVac)
	{
		GameObject selectedStored = player.Ammo.GetSelectedStored();
		Identifiable component = selectedStored.GetComponent<Identifiable>();
		Expel(selectedStored);
		player.Ammo.DecrementSelectedAmmo();
		if (component != null)
		{
			tutDir.OnShoot(component.id);
		}
		ShootEffect();
	}

	private float GetSpeed(Identifiable.Id id)
	{
		if ((uint)(id - 171) <= 3u)
		{
			return ejectSpeed * 3f;
		}
		return ejectSpeed;
	}

	public void Expel(GameObject toExpel, bool ignoreEmotions = false)
	{
		vp_FPController componentInParent = GetComponentInParent<vp_FPController>();
		Ray ray = new Ray(vacOrigin.transform.position, vacOrigin.transform.up);
		float num = PhysicsUtil.RadiusOfObject(toExpel);
		float num2 = ((ray.direction.y >= 0f) ? 0f : (-0.5f * ray.direction.y));
		Vector3 startPos = ray.origin + ray.direction * (num * 0.2f + num2);
		Vector3 vel = ray.direction * GetSpeed(player.Ammo.GetSelectedId()) + componentInParent.Velocity;
		startPos = EnsureNotShootingIntoRock(startPos, ray, num, ref vel);
		GameObject gameObject = SRBehaviour.InstantiateActor(toExpel, regionRegistry.GetCurrentRegionSetId(), startPos, Quaternion.identity);
		gameObject.transform.LookAt(base.transform);
		PhysicsUtil.RestoreFreezeRotationConstraints(gameObject);
		SlimeEmotions component = gameObject.GetComponent<SlimeEmotions>();
		if (component != null && player.Ammo.GetSelectedId() != 0 && !ignoreEmotions)
		{
			component.SetAll(player.Ammo.GetSelectedEmotions());
		}
		gameObject.GetComponent<Rigidbody>().velocity = vel;
		gameObject.transform.DOScale(gameObject.transform.localScale, 0.1f).From(gameObject.transform.localScale * 0.2f).SetEase(Ease.Linear);
		gameObject.GetComponent<Vacuumable>().Launch(Vacuumable.LaunchSource.PLAYER);
	}

	public void OnDamageExposure(GlitchMetadata.ExposureMetadata metadata, int count)
	{
		vp_FPController controller = GetComponentInParent<vp_FPController>();
		GameObject prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.GLITCH_SLIME);
		float radius = PhysicsUtil.RadiusOfObject(prefab);
		metadata.OnExposed(null, count: count, origin: base.transform.position, getPositionAndVelocity: delegate(out Vector3 position, out Vector3 velocity)
		{
			Ray ray = new Ray(vacOrigin.transform.position, vacOrigin.transform.up);
			float num = ((ray.direction.y >= 0f) ? 0f : (-0.5f * ray.direction.y));
			velocity = ray.direction * metadata.velocity + controller.Velocity;
			velocity = Quaternion.Euler(metadata.velocityRotationX.GetRandom(), metadata.velocityRotationY.GetRandom(), 0f) * velocity;
			position = ray.origin + ray.direction * (radius * 0.2f + num);
			position = EnsureNotShootingIntoRock(position, ray, radius, ref velocity);
		}, source: null, onInstantiated: delegate(GameObject instance)
		{
			instance.GetComponent<GlitchSlimeFlee>().DisableExposureChance();
			vacAnimator.SetTrigger(animSwitchSlotsId);
			ShootEffect();
		});
	}

	private Vector3 EnsureNotShootingIntoRock(Vector3 startPos, Ray ray, float objRad, ref Vector3 vel)
	{
		float num = 0.5f;
		Ray ray2 = new Ray(ray.origin - ray.direction * num, ray.direction);
		float magnitude = (startPos - ray2.origin).magnitude;
		int layerMask = 270572033;
		Physics.Raycast(ray2, out var hitInfo, magnitude, layerMask, QueryTriggerInteraction.Ignore);
		if (hitInfo.collider != null)
		{
			startPos = hitInfo.point - ray.direction * objRad;
			vel -= Vector3.Project(vel, hitInfo.normal);
		}
		return startPos;
	}

	private void ShootEffect()
	{
		if (shootFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(shootFX, vacOrigin, Vector3.zero, Quaternion.identity);
		}
		PlayTransientAudio(vacShootCue);
	}

	private void CaptureEffect()
	{
		if (captureFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(captureFX, vacOrigin, Vector3.zero, Quaternion.identity);
		}
	}

	private void CaptureFailedEffect()
	{
		if (captureFailedFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(captureFailedFX, vacOrigin, Vector3.zero, Quaternion.identity);
		}
	}

	private void Consume(HashSet<GameObject> inVac)
	{
		ConsumeExistingJointed();
		lastWeaponPos = vacOrigin.transform.position;
		List<LiquidSource> currLiquids = new List<LiquidSource>();
		UnityWorkarounds.SafeRemoveAllNulls(animatingConsume);
		int slimesInVac = 0;
		foreach (GameObject item in inVac)
		{
			ConsumeVacItem(item, ref slimesInVac, ref currLiquids);
		}
		if (slimesInVac > 0 && !CellDirector.IsOnRanch(member))
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.SLIMES_IN_VAC, slimesInVac);
		}
		ConsumeLiquids(currLiquids);
	}

	private void ConsumeLiquids(List<LiquidSource> currLiquids)
	{
		List<LiquidSource> list = new List<LiquidSource>();
		foreach (LiquidSource key in liquidDict.Keys)
		{
			if (!currLiquids.Contains(key))
			{
				list.Add(key);
			}
		}
		foreach (LiquidSource item in list)
		{
			Destroyer.Destroy(liquidDict[item].fx, "WeaponVacuum.ConsumeLiquids");
			liquidDict.Remove(item);
		}
		foreach (LiquidSource currLiquid in currLiquids)
		{
			if (!liquidDict.ContainsKey(currLiquid))
			{
				GameObject gameObject = SRBehaviour.SpawnAndPlayFX(SRSingleton<GameContext>.Instance.LookupDirector.GetLiquidIncomingFX(currLiquid.liquidId));
				gameObject.transform.SetParent(vacOrigin.transform, worldPositionStays: false);
				liquidDict[currLiquid] = new IncomingLiquid(currLiquid.liquidId, Time.time + 0.25f, gameObject);
			}
			else if (Time.time >= liquidDict[currLiquid].nextUnitTick)
			{
				bool num = ConsumeLiquid(currLiquid);
				liquidDict[currLiquid].nextUnitTick += 0.25f;
				if (num)
				{
					currLiquid.ConsumeLiquid();
					CaptureEffect();
				}
				else
				{
					SRBehaviour.SpawnAndPlayFX(SRSingleton<GameContext>.Instance.LookupDirector.GetLiquidVacFailFX(currLiquid.liquidId)).transform.SetParent(vacOrigin.transform, worldPositionStays: false);
				}
			}
		}
	}

	private bool ConsumeLiquid(LiquidSource source)
	{
		if (player.Ammo.MaybeAddToSlot(source.liquidId, null))
		{
			return true;
		}
		if (source.ReplacesExistingLiquidAmmo())
		{
			for (int i = 0; i < player.Ammo.GetUsableSlotCount(); i++)
			{
				Identifiable.Id slotName = player.Ammo.GetSlotName(i);
				if (slotName != source.liquidId && Identifiable.IsLiquid(slotName))
				{
					return player.Ammo.Replace(i, source.liquidId);
				}
			}
		}
		return false;
	}

	private void ConsumeExistingJointed()
	{
		joints.RemoveAll((Joint joint) => joint == null);
		foreach (SpringJoint joint in joints)
		{
			if (!(joint.connectedBody != null) || !joint.connectedBody.isKinematic)
			{
				float magnitude = joint.transform.localPosition.magnitude;
				float num = 0f;
				if (lastWeaponPos.HasValue)
				{
					num = (joint.transform.position - lastWeaponPos.Value).magnitude - magnitude;
				}
				float t = magnitude / maxVacDist;
				float num2 = Mathf.Lerp(maxJointSpeed, minJointSpeed, t);
				float spring = Mathf.Lerp(maxSpringStrength, minSpringStrength, t);
				float num3 = Mathf.Max(0f, magnitude - num2 * Time.deltaTime - num);
				if (magnitude > 0f)
				{
					joint.transform.localPosition = num3 / magnitude * joint.transform.localPosition;
				}
				joint.spring = spring;
			}
		}
	}

	private void ConsumeVacItem(GameObject gameObj, ref int slimesInVac, ref List<LiquidSource> currLiquids)
	{
		Vacuumable component = gameObj.GetComponent<Vacuumable>();
		Identifiable component2 = gameObj.GetComponent<Identifiable>();
		LiquidSource component3 = gameObj.GetComponent<LiquidSource>();
		if (component2 != null && Identifiable.IsSlime(component2.id))
		{
			slimesInVac++;
		}
		if (component != null && component.enabled && (component2 == null || !Identifiable.IsLiquid(component2.id)) && !animatingConsume.Contains(component))
		{
			Vector3 direction = gameObj.transform.position - vacOrigin.transform.position;
			Ray ray = new Ray(vacOrigin.transform.position, direction);
			if (Physics.Raycast(ray, out var hitInfo, maxVacDist, -536887557))
			{
				if (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == gameObj)
				{
					if (component.GetDestroyOnVac())
					{
						SRBehaviour.SpawnAndPlayFX(destroyOnVacFX, gameObj.transform.position, gameObj.transform.rotation);
						if (component2 == null)
						{
							Destroyer.Destroy(gameObj, "WeaponVacuum.ConsumeVacItem#1");
						}
						else
						{
							Destroyer.DestroyActor(gameObj, "WeaponVacuum.ConsumeVacItem#2");
						}
					}
					else if (component.canCapture() && (!slimeFilter || component2 == null || !Identifiable.IsSlime(component2.id)))
					{
						Rigidbody component4 = component.GetComponent<Rigidbody>();
						if (component.isCaptive() && component.IsTornadoed())
						{
							component.release();
						}
						if (!component.isCaptive())
						{
							if (component4.isKinematic)
							{
								component.Pending = true;
							}
							else
							{
								component.capture(CreateJoint(new Ray(vacOrigin.transform.position, vacOrigin.transform.rotation * Vector3.up), component));
							}
						}
						if (!component4.isKinematic && component2 != null && (Identifiable.IsAnimal(component2.id) || Identifiable.IsSlime(component2.id)))
						{
							RotateTowards(gameObj, heldFaceTowards.transform.position - gameObj.transform.position);
						}
					}
				}
				if (component2 != null && component.isCaptive() && Vector3.Distance(gameObj.transform.position, ray.origin) < captureDist)
				{
					if (component2.id != 0 && component.enabled && component.size == Vacuumable.Size.NORMAL)
					{
						if (component != null)
						{
							StartCoroutine(StartConsuming(component, component2.id));
						}
					}
					else if (held == null && component.enabled && component.canCapture())
					{
						held = gameObj;
						SetHeldRad(PhysicsUtil.RadiusOfObject(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(component2.id)));
						component.hold();
						if (Identifiable.IsLargo(component2.id))
						{
							tutDir.MaybeShowPopup(TutorialDirector.Id.LARGO);
							pediaDir.MaybeShowPopup(PediaDirector.Id.LARGO_SLIME);
						}
						SlimeFeral component5 = gameObj.GetComponent<SlimeFeral>();
						if (component5 != null && component5.IsFeral())
						{
							pediaDir.MaybeShowPopup(PediaDirector.Id.FERAL_SLIME);
						}
						heldStartTime = timeDir.WorldTime();
						SlimeEat component6 = held.GetComponent<SlimeEat>();
						if (component6 != null)
						{
							component6.ResetEatClock();
						}
						TentacleGrapple component7 = held.GetComponent<TentacleGrapple>();
						if (component7 != null)
						{
							component7.Release();
						}
						GroundVine component8 = held.GetComponent<GroundVine>();
						if (component8 != null)
						{
							component8.Release();
						}
						pediaDir.MaybeShowPopup(component2.id);
						PlayTransientAudio(vacHeldCue);
						SRSingleton<SceneContext>.Instance.Player.GetComponent<ScreenShaker>().ShakeFrontImpact(1f);
					}
				}
			}
		}
		if (!(component3 != null) || !component3.Available())
		{
			return;
		}
		if (playerEvents.Underwater.Active)
		{
			currLiquids.Add(component3);
			return;
		}
		float num = 1.5f;
		Vector3 up = vacOrigin.transform.up;
		Vector3 origin = vacOrigin.transform.position - up.normalized * num;
		float maxDistance = maxVacDist + num;
		Ray ray2 = new Ray(origin, up);
		float num2 = float.PositiveInfinity;
		float num3 = float.NaN;
		RaycastHit[] array = Physics.RaycastAll(ray2, maxDistance, -536887557, QueryTriggerInteraction.Collide);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.collider.gameObject == gameObj)
			{
				num3 = raycastHit.distance;
			}
			else if (!raycastHit.collider.isTrigger)
			{
				num2 = Math.Min(num2, raycastHit.distance);
			}
		}
		if (num3 <= num2)
		{
			currLiquids.Add(component3);
		}
	}

	private IEnumerator StartConsuming(Vacuumable vacuumable, Identifiable.Id id)
	{
		vacuumable.StartConsumeScale();
		MoveTowards moveTowards = vacuumable.gameObject.AddComponent<MoveTowards>();
		moveTowards.dest = vacOrigin.transform;
		animatingConsume.Add(vacuumable);
		yield return new WaitForSeconds(0.2f);
		Destroyer.Destroy(moveTowards, "WeaponVacuum.StartConsuming");
		if (!(vacuumable != null))
		{
			yield break;
		}
		if (vacuumable.TryConsume())
		{
			CaptureEffect();
			pediaDir.MaybeShowPopup(id);
			tutDir.OnVac(id);
			if (Identifiable.IsPlort(id) && !CellDirector.IsOnRanch(member))
			{
				achieveDir.AddToStat(AchievementsDirector.IntStat.DAY_COLLECT_PLORTS, 1);
			}
		}
		else
		{
			vacuumable.release();
			CaptureFailedEffect();
			animatingConsume.Remove(vacuumable);
			vacuumable.ReverseConsumeScale();
		}
	}

	public void ForceJoint(Vacuumable vacuumable)
	{
		vacuumable.capture(CreateJoint(new Ray(vacOrigin.transform.position, vacOrigin.transform.rotation * Vector3.up), vacuumable));
	}

	private Joint CreateJoint(Ray alongRay, Vacuumable vacuumable)
	{
		Vector3 position = vacuumable.transform.position;
		GameObject obj = UnityEngine.Object.Instantiate(vacJointPrefab);
		obj.transform.position = position;
		obj.transform.SetParent(vacOrigin.transform, worldPositionStays: true);
		SpringJoint component = obj.GetComponent<SpringJoint>();
		component.spring = minSpringStrength;
		joints.Add(component);
		return component;
	}

	private Vector3 ClosestPointOnRay(Ray ray, Vector3 pt)
	{
		Vector3 rhs = pt - ray.origin;
		float num = Vector3.Dot(ray.direction, rhs);
		return ray.origin + ray.direction * num;
	}

	private void UpdateHud(HashSet<GameObject> inVac)
	{
		UpdateCrosshair(inVac);
		UpdateTargetingInfo();
	}

	private void UpdateCrosshair(HashSet<GameObject> inVac)
	{
		bool pointedAtVaccable = false;
		foreach (GameObject item in inVac)
		{
			if (item == null)
			{
				Debug.Log("Null gameobj, skipping: " + ((item == null) ? "null" : item.name));
				if (!Application.isEditor)
				{
				}
				continue;
			}
			try
			{
				Vacuumable vacuumable;
				if (!recentIds.contains(item.GetInstanceID()))
				{
					vacuumable = item.GetComponent<Vacuumable>();
					recentIds.put(item.GetInstanceID(), vacuumable);
				}
				else
				{
					vacuumable = recentIds.get(item.GetInstanceID());
				}
				if (vacuumable != null && vacuumable.enabled)
				{
					pointedAtVaccable = true;
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Got an e, skipping: " + ((item == null) ? "null" : item.name) + " msg: " + ex.Message);
				_ = Application.isEditor;
			}
		}
		player.PointedAtVaccable = pointedAtVaccable;
	}

	private void UpdateTargetingInfo()
	{
		Ray ray = new Ray(vacOrigin.transform.position, vacOrigin.transform.up);
		player.Targeting = null;
		if (Physics.Raycast(ray, out var hitInfo, maxVacDist, -536887557))
		{
			player.Targeting = hitInfo.collider.gameObject;
		}
	}

	public bool InGadgetMode()
	{
		return vacMode == VacMode.GADGET;
	}

	private void RotateTowards(GameObject gameObj, Vector3 dirToTarget)
	{
		Rigidbody component = gameObj.GetComponent<Rigidbody>();
		Vector3 vector = Vector3.Cross(Quaternion.AngleAxis(component.angularVelocity.magnitude * 57.29578f / facingSpeed, component.angularVelocity) * component.transform.forward, dirToTarget);
		component.AddTorque(vector * facingSpeed * facingSpeed);
	}

	public void FixedUpdate()
	{
		if (held != null)
		{
			float num = heldRad;
			float num2 = num * 2f + 0.1f;
			Ray ray = new Ray(lockJoint.transform.position - num2 * lockJoint.transform.up, lockJoint.transform.up);
			float num3 = num2;
			int layerMask = 270567937;
			if (Physics.SphereCast(ray, num, out var hitInfo, num3, layerMask, QueryTriggerInteraction.Ignore))
			{
				vp_FPController componentInParent = GetComponentInParent<vp_FPController>();
				float num4 = (num3 - hitInfo.distance) / num;
				Vector3 force = Vector3.Project(vacOrigin.transform.up, hitInfo.normal) * -0.2f * num4 * num4;
				componentInParent.AddDepenetrationForce(force);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class ResourceCycle : RegisteredActorBehaviour, RegistryUpdateable, Ignitable, ActorModel.Participant
{
	[Serializable]
	public enum State
	{
		UNRIPE = 0,
		RIPE = 1,
		EDIBLE = 2,
		ROTTEN = 3
	}

	[Serializable]
	public class CycleData
	{
		public State state;

		public float progressTime;

		public CycleData(State state, float progressTime)
		{
			this.state = state;
			this.progressTime = progressTime;
		}

		public override bool Equals(object o)
		{
			if (!(o is CycleData))
			{
				return false;
			}
			CycleData cycleData = (CycleData)o;
			if (state == cycleData.state)
			{
				return progressTime == cycleData.progressTime;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return state.GetHashCode() ^ progressTime.GetHashCode();
		}

		public override string ToString()
		{
			return string.Concat(state, ":", progressTime);
		}
	}

	public delegate float AdditionalRipeness();

	public delegate void DetachmentEvent();

	public float unripeGameHours = 6f;

	public float ripeGameHours = 6f;

	public float edibleGameHours = 12f;

	public float rottenGameHours = 6f;

	public Material rottenMat;

	public GameObject destroyFX;

	public SECTR_AudioCue releaseCue;

	public Transform toShake;

	public bool vacuumableWhenRipe = true;

	private DetachmentEvent onDetachment;

	public bool addEjectionForce = true;

	public float releasePrepTime = 1f;

	public GameObject igniteFX;

	private Vacuumable vacuumable;

	private Renderer mainRenderer;

	private TimeDirector timeDir;

	private SafeJointReference joint;

	private Vector3 defaultScale;

	private AdditionalRipeness additionalRipenessDelegate;

	private bool preparingToRelease;

	private float releaseAt;

	private Material rotInProgressMat;

	private const float EJECT_TORQUE = 5f;

	private const float EJECT_FORCE = 80f;

	private const float UNRIPE_SCALE = 0.33f;

	private const float RIPEN_SCALE_TIME = 4f;

	private const float VIBRATE_AMPLITUDE = 0.033f;

	private const float ROT_PROGRESS_PER_SEC = 0.5f;

	private const string ROT_SHADER_PARAM = "_Rot";

	private bool hasVacuumable;

	private Rigidbody body;

	private Vector3 toShakeDefaultPos;

	private float vibrateAmplitude;

	private Identifiable ident;

	private ProduceModel model;

	private MiracleMix preservative;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		vacuumable = GetComponent<Vacuumable>();
		hasVacuumable = vacuumable != null;
		mainRenderer = GetComponentInChildren<Renderer>();
		defaultScale = base.transform.localScale;
		vibrateAmplitude = 0.033f / defaultScale.x;
		body = GetComponent<Rigidbody>();
		toShakeDefaultPos = toShake.localPosition;
		ident = GetComponent<Identifiable>();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		onDetachment = null;
		ident = null;
		if (rotInProgressMat != null)
		{
			Destroyer.Destroy(rotInProgressMat, "ResourceCycle.OnDestroy");
		}
		if (preservative != null)
		{
			preservative.RemoveResourceCycle(this);
		}
	}

	public void InitModel(ActorModel model)
	{
		((ProduceModel)model).progressTime = timeDir.HoursFromNowOrStart(Vary(edibleGameHours));
	}

	public void SetModel(ActorModel model)
	{
		this.model = (ProduceModel)model;
		SetInitState(this.model.state, this.model.progressTime);
	}

	public void Attach(Joint joint, AdditionalRipeness additionalRipenessDelegate = null, DetachmentEvent detachmentDelegate = null)
	{
		model.state = State.UNRIPE;
		DetachFromJoint();
		this.joint = SafeJointReference.AttachSafely(body.gameObject, joint, canDestroyJoint: false);
		joint.anchor = Vector3.zero;
		joint.connectedAnchor = Vector3.zero;
		this.additionalRipenessDelegate = additionalRipenessDelegate;
		onDetachment = detachmentDelegate;
		body.isKinematic = true;
		base.transform.localScale = defaultScale * 0.33f;
		model.progressTime = timeDir.HoursFromNowOrStart(Vary(unripeGameHours));
	}

	public void Reattach(Joint joint)
	{
		DetachFromJoint();
		this.joint = SafeJointReference.AttachSafely(body.gameObject, joint, canDestroyJoint: false);
		joint.anchor = Vector3.zero;
		joint.connectedAnchor = Vector3.zero;
	}

	public void Detach(AdditionalRipeness additionalRipenessDelegate)
	{
		this.additionalRipenessDelegate = (AdditionalRipeness)Delegate.Remove(this.additionalRipenessDelegate, additionalRipenessDelegate);
		if (model.state == State.RIPE)
		{
			MakeEdible();
		}
		DetachFromJoint();
	}

	private void DetachFromJoint()
	{
		if (joint != null)
		{
			if (joint.joint != null)
			{
				joint.joint.connectedBody = null;
			}
			Destroyer.Destroy(joint, "ResourceCycle.DetachFromJoint");
			joint = null;
			if (onDetachment != null)
			{
				onDetachment();
				onDetachment = null;
			}
		}
	}

	public void AttachPreservative(MiracleMix preservative)
	{
		this.preservative = preservative;
		if (model.state == State.EDIBLE)
		{
			additionalRipenessDelegate = preservative.PreservativeRipenessModifier;
		}
	}

	public void DetachPreservative(MiracleMix preservative)
	{
		if (model.state == State.EDIBLE)
		{
			additionalRipenessDelegate = null;
		}
		this.preservative = null;
	}

	public void Ignite(GameObject igniter)
	{
		if (model.state == State.EDIBLE || model.state == State.ROTTEN)
		{
			if (igniteFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(igniteFX);
			}
			if (model.state == State.EDIBLE)
			{
				Destroyer.DestroyActor(base.gameObject, "ResourceCycle.Ignite");
			}
			else
			{
				Destroyer.Destroy(base.gameObject, "ResourceCycle.Ignite");
			}
		}
	}

	public void ImmediatelyRipen(float bonusRipenessHours)
	{
		if (model.state != 0)
		{
			Debug.Log("Trying to ripen already-ripe resource?");
		}
		else
		{
			model.progressTime = timeDir.HoursFromNowOrStart(0f - bonusRipenessHours);
		}
	}

	public void ImmediatelyRot()
	{
		if (model != null && model.state == State.EDIBLE)
		{
			Rot();
			SetRotten(immediate: true);
		}
	}

	public static float Vary(float val)
	{
		return Randoms.SHARED.GetInRange(0.9f, 1.1f) * val;
	}

	public void RegistryUpdate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (additionalRipenessDelegate != null)
		{
			model.progressTime -= (double)additionalRipenessDelegate() * timeDir.DeltaWorldTime();
		}
		Rigidbody rigidbody = body;
		if (model.state == State.UNRIPE && joint == null)
		{
			Destroyer.DestroyActor(base.gameObject, "ResourceCycle.RegistryUpdate#1");
		}
		else if (joint == null && rigidbody.isKinematic)
		{
			rigidbody.isKinematic = false;
		}
		if (hasVacuumable && vacuumableWhenRipe && model.state == State.RIPE && vacuumable.Pending && !preparingToRelease)
		{
			preparingToRelease = true;
			releaseAt = Time.time + releasePrepTime;
		}
		if (preparingToRelease)
		{
			if (hasVacuumable && vacuumable.Pending)
			{
				toShake.localPosition = toShakeDefaultPos + UnityEngine.Random.insideUnitSphere * vibrateAmplitude;
			}
			else
			{
				preparingToRelease = false;
				releaseAt = 0f;
				toShake.localPosition = toShakeDefaultPos;
			}
		}
		ProgressResource(model.progressTime);
		if (rotInProgressMat != null)
		{
			float num = Mathf.Min(1f, rotInProgressMat.GetFloat("_Rot") + 0.5f * Time.deltaTime);
			if (num < 1f)
			{
				rotInProgressMat.SetFloat("_Rot", num);
				return;
			}
			mainRenderer.sharedMaterial = rottenMat;
			Destroyer.Destroy(rotInProgressMat, "ResourceCycle.RegistryUpdate#2");
			rotInProgressMat = null;
		}
	}

	public void UpdateToNow()
	{
		ProgressResource(model.progressTime);
	}

	public bool WouldProgressToRotten(double spawnTime, double worldTime)
	{
		double targetWorldTime = spawnTime + (double)((unripeGameHours + ripeGameHours + edibleGameHours) * 3600f);
		return TimeUtil.HasReached(worldTime, targetWorldTime);
	}

	public void ProgressResource(double nextProgressionTime)
	{
		Rigidbody rigidbody = body;
		bool flag = timeDir.HasReached(model.progressTime + 3600.0);
		model.progressTime = nextProgressionTime;
		while (timeDir.HasReached(model.progressTime) || (preparingToRelease && Time.time >= releaseAt))
		{
			if (model.state == State.UNRIPE && timeDir.HasReached(model.progressTime))
			{
				Ripen();
				if (vacuumableWhenRipe)
				{
					vacuumable.enabled = true;
				}
				if (base.gameObject.transform.localScale.x < defaultScale.x * 0.33f)
				{
					base.gameObject.transform.localScale = defaultScale * 0.33f;
				}
				TweenUtil.ScaleTo(base.gameObject, defaultScale, 4f);
			}
			else if (model.state == State.RIPE && ((preparingToRelease && Time.time >= releaseAt) || timeDir.HasReached(model.progressTime)))
			{
				MakeEdible();
				additionalRipenessDelegate = null;
				rigidbody.isKinematic = false;
				if (preparingToRelease)
				{
					preparingToRelease = false;
					releaseAt = 0f;
					toShake.localPosition = toShakeDefaultPos;
					if (releaseCue != null)
					{
						SECTR_PointSource component = GetComponent<SECTR_PointSource>();
						component.Cue = releaseCue;
						component.Play();
					}
				}
				rigidbody.WakeUp();
				Eject(rigidbody);
				DetachFromJoint();
				if (hasVacuumable)
				{
					vacuumable.Pending = false;
				}
			}
			else if (model.state == State.EDIBLE && timeDir.HasReached(model.progressTime))
			{
				Rot();
				SetRotten(immediate: false);
			}
			else if (model.state == State.ROTTEN && timeDir.HasReached(model.progressTime))
			{
				if (destroyFX != null && !flag)
				{
					SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
				}
				Destroyer.Destroy(base.gameObject, 0f, "ResourceCycle.ProgressResource", asActorOk: true);
				model.progressTime = double.MaxValue;
			}
		}
	}

	private void Ripen()
	{
		model.state = State.RIPE;
		AdvanceProgressTime(ripeGameHours);
	}

	private void MakeEdible()
	{
		model.state = State.EDIBLE;
		if (preservative != null)
		{
			additionalRipenessDelegate = preservative.PreservativeRipenessModifier;
		}
		AdvanceProgressTime(edibleGameHours);
	}

	private void Rot()
	{
		if (preservative != null)
		{
			preservative.RemoveResourceCycle(this);
		}
		model.state = State.ROTTEN;
		AdvanceProgressTime(rottenGameHours);
	}

	internal State GetState()
	{
		return model.state;
	}

	private void AdvanceProgressTime(float progressBaseAmount)
	{
		model.progressTime = Math.Min(timeDir.WorldTime(), model.progressTime) + (double)(Vary(progressBaseAmount) * 3600f);
	}

	public void Eject(Rigidbody rigidbody)
	{
		rigidbody.MoveRotation(Quaternion.Euler(5f, Randoms.SHARED.GetFloat(360f), 0f));
		rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 5f);
		if (joint != null && joint.joint != null && addEjectionForce)
		{
			rigidbody.AddForce(joint.joint.transform.up * 80f);
		}
	}

	private void SetInitState(State state, double progressTime)
	{
		if ((state == State.UNRIPE || state == State.RIPE) && !AttachToNearest())
		{
			state = State.EDIBLE;
			progressTime = (float)timeDir.HoursFromNow(Vary(edibleGameHours));
			Log.Debug("Could not find joint within patch", "resource", base.gameObject);
		}
		model.progressTime = progressTime;
		model.state = state;
		if (hasVacuumable && vacuumableWhenRipe && state != 0)
		{
			vacuumable.enabled = true;
		}
		base.transform.localScale = defaultScale * ((state == State.UNRIPE) ? 0.33f : 1f);
		if (state == State.ROTTEN)
		{
			SetRotten(immediate: true);
		}
	}

	private void SetRotten(bool immediate)
	{
		if (hasVacuumable)
		{
			vacuumable.SetDestroyOnVac(destroy: true);
		}
		SRSingleton<SceneContext>.Instance.GameModel.DestroyActorModel(base.gameObject);
		if (immediate)
		{
			mainRenderer.sharedMaterial = rottenMat;
			return;
		}
		mainRenderer.material = rottenMat;
		rotInProgressMat = mainRenderer.material;
		rotInProgressMat.SetFloat("_Rot", 0f);
	}

	private bool AttachToNearest()
	{
		if (ident != null && ident.id == Identifiable.Id.GINGER_VEGGIE && ZoneDirector.zones.TryGetValue(ZoneDirector.Zone.DESERT, out var value))
		{
			List<GingerPatchNode> currentGingerPatches = value.GetCurrentGingerPatches();
			if (GetNearestIdHandler(currentGingerPatches, 10f, out var picked))
			{
				picked.Grow(base.gameObject);
				return true;
			}
		}
		if (ident != null && ident.id == Identifiable.Id.KOOKADOBA_FRUIT && GetNearest(SRSingleton<SceneContext>.Instance.GameModel.AllKookadobaNodes(), 10f, out var picked2))
		{
			picked2.Grow(base.gameObject);
			return true;
		}
		if (GetNearest(SRSingleton<SceneContext>.Instance.GameModel.AllResourceSpawners(), 10f, out var picked3))
		{
			Joint joint = picked3.NearestJoint(base.transform.position, 0.1f);
			if (joint != null)
			{
				Attach(joint);
				return true;
			}
		}
		return false;
	}

	private bool GetNearest<T>(IEnumerable<T> items, float distance, out T picked) where T : PositionalModel
	{
		picked = null;
		float num = distance * distance;
		foreach (T item in items)
		{
			float sqrMagnitude = (item.pos - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				picked = item;
			}
		}
		return picked != null;
	}

	private bool GetNearestIdHandler<T>(IEnumerable<T> items, float distance, out T picked) where T : IdHandler
	{
		picked = null;
		float num = distance * distance;
		foreach (T item in items)
		{
			float sqrMagnitude = (item.transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				picked = item;
			}
		}
		return picked != null;
	}
}

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Vacuumable : CollidableActorBehaviour, Collidable
{
	public enum Size
	{
		NORMAL = 0,
		LARGE = 1,
		GIANT = 2
	}

	public enum LaunchSource
	{
		PLAYER = 0,
		CHICKEN_CLONER = 1
	}

	public delegate void OnSetHeld(bool held);

	public delegate void OnSetLaunched(bool launched);

	public delegate void Consume();

	private const float CONSUME_SCALE_DOWN_TIME = 0.2f;

	private const float CONSUME_SCALE_FACTOR = 0.1f;

	public Size size;

	[Tooltip("Audio played when this object is shot out of the vacuum.")]
	public SECTR_AudioCue onLaunchCue;

	private bool ignoresGravity;

	private Joint captiveToJoint;

	private bool held;

	private bool launched;

	private float nextVacTime;

	private bool destroyOnVac;

	private const float VAC_RELEASE_DELAY = 1f;

	private const float HELD_OPACITY = 0.75f;

	private const string LAUNCHED_LAYER = "Launched";

	private const string CAPTIVE_LAYER = "ActorIgnorePlayer";

	private const string TORNADOED_LAYER = "ActorEchoes";

	private const string HELD_LAYER = "Held";

	private int defaultLayer;

	private SlimeFaceAnimator sfAnimator;

	protected Rigidbody body;

	private VacDelaunchTrigger delaunchTrigger;

	private Identifiable identifiable;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private bool isTornadoed;

	private double lastPending = double.NaN;

	private int heldShaderPropertyId;

	private TweenerCore<Vector3, Vector3, VectorOptions> consumeScaleTween;

	private float consumeStartScale;

	public bool Pending
	{
		get
		{
			if (!double.IsNaN(lastPending))
			{
				if ((double)Time.time <= lastPending + (double)Time.deltaTime + 0.0010000000474974513)
				{
					return true;
				}
				lastPending = double.NaN;
				return false;
			}
			return false;
		}
		set
		{
			if (value)
			{
				lastPending = Time.time;
			}
			else
			{
				lastPending = double.NaN;
			}
		}
	}

	public event OnSetHeld onSetHeld;

	public event OnSetLaunched onSetLaunched;

	public event Consume consume;

	public override void Awake()
	{
		base.Awake();
		defaultLayer = base.gameObject.layer;
		sfAnimator = GetComponent<SlimeFaceAnimator>();
		body = GetComponent<Rigidbody>();
		identifiable = GetComponent<Identifiable>();
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		if (slimeAppearanceApplicator != null)
		{
			slimeAppearanceApplicator.OnAppearanceChanged += delegate
			{
				ForceUpdateLayer();
			};
		}
		ignoresGravity = !body.useGravity;
		VacDelaunchTrigger[] componentsInChildren = GetComponentsInChildren<VacDelaunchTrigger>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			delaunchTrigger = componentsInChildren[0];
		}
		heldShaderPropertyId = Shader.PropertyToID("_HeldInVac");
		consumeStartScale = base.transform.localScale.x;
	}

	public bool TryConsume()
	{
		if (this.consume != null)
		{
			this.consume();
			return true;
		}
		if (SRSingleton<SceneContext>.Instance.PlayerState.Ammo.MaybeAddToSlot(identifiable.id, identifiable))
		{
			Destroyer.DestroyActor(base.transform.gameObject, "Vacuumable.consume");
			return true;
		}
		return false;
	}

	public void PreventCaptureFor(float seconds)
	{
		nextVacTime = Time.time + seconds;
	}

	public bool canCapture()
	{
		return Time.time >= nextVacTime;
	}

	public void capture(Joint toJoint)
	{
		if (captiveToJoint == null && sfAnimator != null)
		{
			sfAnimator.SetTrigger("triggerAwe");
		}
		if (body != null)
		{
			body.isKinematic = false;
		}
		SetCaptive(toJoint);
	}

	public void release()
	{
		if (isCaptive() || isHeld())
		{
			PreventCaptureFor(1f);
		}
		SetCaptive(null);
		SetHeld(held: false);
		SetTornadoed(tornadoed: false);
		Pending = false;
	}

	public void hold()
	{
		if (isCaptive())
		{
			SetCaptive(null);
		}
		SetHeld(held: true);
	}

	public bool isCaptive()
	{
		return captiveToJoint != null;
	}

	public void SetTornadoed(bool tornadoed)
	{
		isTornadoed = tornadoed;
		UpdateLayer();
	}

	public bool IsTornadoed()
	{
		return isTornadoed;
	}

	public bool isHeld()
	{
		return held;
	}

	public bool isLaunched()
	{
		return launched;
	}

	public void Launch(LaunchSource source)
	{
		if (source == LaunchSource.PLAYER)
		{
			SECTR_AudioSystem.Play(onLaunchCue, base.transform.position, loop: false);
		}
		SetLaunched(launched: true);
	}

	public bool delaunch()
	{
		if (!launched)
		{
			return false;
		}
		SetLaunched(launched: false);
		return true;
	}

	public void SetDestroyOnVac(bool destroy)
	{
		destroyOnVac = destroy;
	}

	public bool GetDestroyOnVac()
	{
		return destroyOnVac;
	}

	protected virtual void SetCaptive(Joint toJoint)
	{
		if (captiveToJoint != null)
		{
			Destroyer.Destroy(captiveToJoint.gameObject, "Vacuumable.SetCaptive");
		}
		captiveToJoint = toJoint;
		body.useGravity = captiveToJoint == null && !ignoresGravity;
		if (captiveToJoint != null)
		{
			captiveToJoint.connectedBody = body;
		}
		UpdateLayer();
	}

	private void SetHeld(bool held)
	{
		if (this.held != held)
		{
			this.held = held;
			if (this.held)
			{
				delaunch();
			}
			SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>().cameraHeld.SetActive(this.held);
			UpdateLayer();
			UpdateMaterialsHeldState();
			if (this.onSetHeld != null)
			{
				this.onSetHeld(this.held);
			}
		}
	}

	public void ProcessCollisionEnter(Collision col)
	{
		if (launched && !col.collider.isTrigger && !(col.collider is CharacterController))
		{
			SetLaunched(launched: false);
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	private void UpdateMaterialsHeldState()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			for (int j = 0; j < renderer.sharedMaterials.Length; j++)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(materialPropertyBlock, j);
				materialPropertyBlock.SetInt(heldShaderPropertyId, held ? 1 : 0);
				renderer.SetPropertyBlock(materialPropertyBlock, j);
			}
		}
	}

	private void UpdateLayer(bool isForced = false)
	{
		if (launched && delaunchTrigger != null)
		{
			SetLayerRecursively(LayerMask.NameToLayer("Launched"), isForced);
		}
		else if (isHeld())
		{
			SetLayerRecursively(LayerMask.NameToLayer("Held"), isForced);
		}
		else if (IsTornadoed())
		{
			SetLayerRecursively(LayerMask.NameToLayer("ActorEchoes"), isForced);
		}
		else if (isCaptive())
		{
			SetLayerRecursively(LayerMask.NameToLayer("ActorIgnorePlayer"), isForced);
		}
		else
		{
			SetLayerRecursively(defaultLayer, isForced);
		}
	}

	public void ForceUpdateLayer()
	{
		UpdateLayer(isForced: true);
	}

	private void SetLayerRecursively(int layerNumber, bool isForced)
	{
		if (isForced || base.gameObject.layer != layerNumber)
		{
			Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = layerNumber;
			}
		}
	}

	private void SetLaunched(bool launched)
	{
		if (launched == this.launched)
		{
			return;
		}
		this.launched = launched;
		UpdateLayer();
		if (delaunchTrigger != null)
		{
			delaunchTrigger.SetTriggerEnabled(launched);
		}
		if (launched)
		{
			base.gameObject.AddComponent<DontGoThroughThings>();
		}
		else
		{
			DontGoThroughThings component = base.gameObject.GetComponent<DontGoThroughThings>();
			if (component != null)
			{
				component.AllowDestroy();
			}
		}
		if (this.onSetLaunched != null)
		{
			this.onSetLaunched(this.launched);
		}
	}

	public Joint CaptiveToJoint()
	{
		return captiveToJoint;
	}

	public void StartConsumeScale()
	{
		if (consumeScaleTween == null || !consumeScaleTween.IsActive())
		{
			consumeScaleTween = base.gameObject.transform.DOScale(consumeStartScale * 0.1f, 0.2f).SetEase(Ease.Linear);
		}
	}

	public void ReverseConsumeScale()
	{
		if (consumeScaleTween != null && consumeScaleTween.IsPlaying())
		{
			consumeScaleTween.Flip();
		}
		else
		{
			base.gameObject.transform.DOScale(consumeStartScale, 0.2f).SetEase(Ease.Linear);
		}
		consumeScaleTween = null;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (captiveToJoint != null)
		{
			Destroyer.Destroy(captiveToJoint.gameObject, "Vacuumable.OnDestroy");
		}
		if (held && SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.Player != null)
		{
			SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>().cameraHeld.SetActive(value: false);
		}
	}
}

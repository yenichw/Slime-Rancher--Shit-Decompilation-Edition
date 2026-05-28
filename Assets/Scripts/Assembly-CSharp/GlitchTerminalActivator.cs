using System.Linq;
using UnityEngine;

public class GlitchTerminalActivator : SRBehaviour, TechActivator
{
	public enum LinkState
	{
		INACTIVE_PROGRESS = 0,
		INACTIVE_AMMO = 1,
		PRE_ACTIVE = 2,
		ACTIVE = 3
	}

	[Tooltip("Teleport destination transform.")]
	public Transform destinationTransform;

	[Tooltip("List of GlitchTeleportDestination ids in SLIMULATIONS representing potential entrance teleporters.")]
	public string[] destinationIds;

	[Tooltip("FX played on successful button press.")]
	public GameObject onButtonPressedSuccessFX;

	[Tooltip("SFX cue on successful button press.")]
	public SECTR_AudioCue onButtonPressedSuccessCue;

	[Tooltip("FX played on unsuccessful button press.")]
	public GameObject onButtonPressedFailureFX;

	[Tooltip("SFX cue on unsuccessful button press.")]
	public SECTR_AudioCue onButtonPressedFailureCue;

	private ProgressDirector progressDirector;

	private TimeDirector timeDirector;

	private GlitchMetadata metadata;

	private GlitchTerminalAnimator animator;

	private GlitchTeleportDestination[] destinations;

	private Animator buttonAnimator;

	private int buttonAnimationId;

	private GameObject onButtonPressedFXInstance;

	public void Awake()
	{
		progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		animator = GetRequiredComponentInParent<GlitchTerminalAnimator>();
		buttonAnimator = GetRequiredComponentInParent<Animator>();
		buttonAnimationId = Animator.StringToHash("ButtonPressed");
	}

	public void Start()
	{
		destinations = destinationIds.Select((string id) => SRSingleton<GlitchRegionHelper>.Instance.destinationsDict[id]).ToArray();
	}

	public void Activate()
	{
		if (onButtonPressedFXInstance != null)
		{
			return;
		}
		bool flag = GetLinkState() == LinkState.ACTIVE;
		SECTR_AudioSystem.Play(flag ? onButtonPressedSuccessCue : onButtonPressedFailureCue, base.transform.position, loop: false);
		buttonAnimator.SetTrigger(buttonAnimationId);
		if (flag && onButtonPressedSuccessFX != null)
		{
			onButtonPressedFXInstance = SRBehaviour.SpawnAndPlayFX(onButtonPressedSuccessFX, base.transform.position, Quaternion.identity);
		}
		else if (!flag && onButtonPressedFailureFX != null)
		{
			onButtonPressedFXInstance = SRBehaviour.SpawnAndPlayFX(onButtonPressedFailureFX, base.transform.position, Quaternion.identity);
		}
		if (flag)
		{
			if (!progressDirector.HasProgress(ProgressDirector.ProgressType.ENTER_ZONE_SLIMULATION))
			{
				progressDirector.SetProgress(ProgressDirector.ProgressType.ENTER_ZONE_SLIMULATION, timeDirector.CurrDay());
			}
			GlitchTeleportDestination glitchTeleportDestination = destinations[(timeDirector.CurrDay() - progressDirector.GetProgress(ProgressDirector.ProgressType.ENTER_ZONE_SLIMULATION)) % destinations.Length];
			glitchTeleportDestination.isPotentialExitDestination = false;
			animator.OnEnter(glitchTeleportDestination.destinationTransform);
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		switch (GetLinkState())
		{
		case LinkState.INACTIVE_PROGRESS:
			return metadata.activatorGuiProgress;
		case LinkState.INACTIVE_AMMO:
			return metadata.activatorGuiAmmo;
		case LinkState.PRE_ACTIVE:
			return metadata.activatorGuiPreActive;
		default:
			return null;
		}
	}

	public LinkState GetLinkState()
	{
		if (!progressDirector.HasProgress(ProgressDirector.ProgressType.UNLOCK_SLIMULATIONS))
		{
			return LinkState.INACTIVE_PROGRESS;
		}
		Ammo ammo = SRSingleton<SceneContext>.Instance.PlayerState.Ammo;
		if (Enumerable.Range(0, 4).Any((int ii) => ammo.GetSlotCount(ii) > 0))
		{
			return LinkState.INACTIVE_AMMO;
		}
		if (SRInput.Instance.GetInputMode() == SRInput.InputMode.DEFAULT)
		{
			return LinkState.ACTIVE;
		}
		return LinkState.PRE_ACTIVE;
	}
}

using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchTeleportDestination : SRBehaviour, GlitchTeleportDestinationModel.Participant
{
	public delegate void OnExitTeleporterBecameActiveDelegate(GlitchTeleportDestination destination);

	private enum State
	{
		DEACTIVATED = 0,
		PREACTIVATED = 1,
		ACTIVATED = 2
	}

	[Tooltip("Teleport destination transform.")]
	public Transform destinationTransform;

	[Tooltip("FX parent when the destination is activated.")]
	public GameObject exitActiveFX;

	private GlitchTeleportDestinationModel model;

	private TutorialDirector tutorialDirector;

	private TimeDirector timeDirector;

	public string id => GetRequiredComponent<IdHandler>().id;

	public bool isPotentialExitDestination { get; set; }

	public event OnExitTeleporterBecameActiveDelegate onExitTeleporterBecameActive;

	public void Awake()
	{
		tutorialDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		isPotentialExitDestination = true;
		exitActiveFX.SetActive(value: false);
		SRSingleton<SceneContext>.Instance.GameModel.Glitch.Register(this);
	}

	public void InitModel(GlitchTeleportDestinationModel model)
	{
		model.activationTime = null;
	}

	public void SetModel(GlitchTeleportDestinationModel model)
	{
		this.model = model;
		if (SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel().currRegionSetId == RegionRegistry.RegionSetId.SLIMULATIONS)
		{
			Reset(this.model.activationTime);
		}
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.Glitch.Unregister(this);
			timeDirector.RemovePassedTimeDelegate(OnExitTeleporterBecameActive);
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (IsLinkActive() && PhysicsUtil.IsPlayerMainCollider(collider))
		{
			GlitchTerminalAnimator.OnExit(null, null, base.gameObject.GetInstanceID());
		}
	}

	public bool IsLinkActive()
	{
		return GetCurrentState() == State.ACTIVATED;
	}

	public void Reset(double? activationTime)
	{
		model.activationTime = activationTime;
		State currentState = GetCurrentState();
		exitActiveFX.SetActive(currentState == State.ACTIVATED);
		isPotentialExitDestination |= timeDirector.WorldTime() >= model.activationTime;
		timeDirector.RemovePassedTimeDelegate(OnExitTeleporterBecameActive);
		switch (currentState)
		{
		case State.PREACTIVATED:
			timeDirector.AddPassedTimeDelegate(model.activationTime.Value, OnExitTeleporterBecameActive);
			break;
		case State.ACTIVATED:
			OnExitTeleporterBecameActive();
			break;
		}
	}

	private void OnExitTeleporterBecameActive()
	{
		SRSingleton<GlitchRegionHelper>.Instance.OnExitTeleporterBecameActive();
		exitActiveFX.SetActive(value: true);
		tutorialDirector.MaybeShowPopup(TutorialDirector.Id.SLIMULATIONS_EXIT_AVAILABLE);
		if (this.onExitTeleporterBecameActive != null)
		{
			this.onExitTeleporterBecameActive(this);
		}
	}

	private State GetCurrentState()
	{
		if (!(timeDirector.WorldTime() < model.activationTime))
		{
			if (!(timeDirector.WorldTime() >= model.activationTime))
			{
				return State.DEACTIVATED;
			}
			return State.ACTIVATED;
		}
		return State.PREACTIVATED;
	}
}

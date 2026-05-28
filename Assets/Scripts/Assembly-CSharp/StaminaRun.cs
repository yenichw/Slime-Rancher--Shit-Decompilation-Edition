using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class StaminaRun : SRBehaviour, EventHandlerRegistrable, PlayerModel.Participant
{
	public float runningStaminaPerSecond = 30f;

	public float runThreshold = 1f;

	private vp_FPPlayerEventHandler playerEvents;

	private PlayerState playerState;

	private float runStaminaThreshold;

	private vp_FPController controller;

	private TimeDirector timeDirector;

	private PlayerModel model;

	private const float MIN_RUN_VEL = 1f;

	private const float SQR_MIN_RUN_VEL = 1f;

	protected virtual void Awake()
	{
		playerEvents = GetComponent<vp_FPPlayerEventHandler>();
		controller = GetComponent<vp_FPController>();
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
	}

	protected virtual void Start()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		runStaminaThreshold = runThreshold * runningStaminaPerSecond;
	}

	protected virtual void OnEnable()
	{
		if (playerEvents != null)
		{
			Register(playerEvents);
		}
	}

	protected virtual void OnDisable()
	{
		if (playerEvents != null)
		{
			Unregister(playerEvents);
		}
	}

	public void Update()
	{
		if (playerEvents.Run.Active)
		{
			bool flag = TooSlow();
			if (timeDirector.HasReached(model.runEnergyDepletionTime) && !flag)
			{
				playerState.SpendEnergy(Time.deltaTime * runningStaminaPerSecond * model.runEfficiency);
			}
			if (!CanContinue_Run(1f))
			{
				playerEvents.Run.TryStop();
			}
		}
	}

	protected virtual bool CanStart_Run()
	{
		return CanContinue_Run(runStaminaThreshold);
	}

	private bool CanContinue_Run(float threshold)
	{
		if ((!timeDirector.HasReached(model.runEnergyDepletionTime) || (float)playerState.GetCurrEnergy() >= threshold) && controller.Grounded)
		{
			return !TooSlow();
		}
		return false;
	}

	private bool TooSlow()
	{
		return playerEvents.Velocity.Get().sqrMagnitude < 1f;
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterActivity("Run", null, null, CanStart_Run, null, null, null);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterActivity("Run", null, null, CanStart_Run, null, null, null);
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
		this.model = model;
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
}

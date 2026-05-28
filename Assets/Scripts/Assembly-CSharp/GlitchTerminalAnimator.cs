using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchTerminalAnimator : SRAnimator, PlayerModel.Participant
{
	public delegate void OnStateChanged(GlitchTerminalAnimatorState.Id id);

	private class TemporaryLockInputMode : IDisposable
	{
		private readonly int inputModeHandle;

		public TemporaryLockInputMode(int inputModeHandle)
		{
			this.inputModeHandle = inputModeHandle;
			SRInput.Instance.SetInputMode(SRInput.InputMode.LOOK_ONLY, inputModeHandle);
		}

		public void Dispose()
		{
			SRInput.Instance.ClearInputMode(inputModeHandle);
		}
	}

	private class TemporaryReplaceSeaMaterial : IDisposable
	{
		private readonly Renderer renderer;

		private readonly Material previousMaterial;

		public TemporaryReplaceSeaMaterial()
		{
			GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
			GlitchRegionHelper instance = SRSingleton<GlitchRegionHelper>.Instance;
			renderer = instance.seaRenderer;
			previousMaterial = renderer.sharedMaterial;
			renderer.sharedMaterial = glitch.animationSeaMaterial;
		}

		public void Dispose()
		{
			renderer.sharedMaterial = previousMaterial;
		}
	}

	public const string STATE_SLEEPING = "state_sleeping";

	public const string STATE_IN_SLIMULATION = "state_in_slimulation";

	public GlitchTerminalActivator activator { get; private set; }

	public event OnStateChanged onStateEnter;

	public override void Awake()
	{
		base.Awake();
		activator = GetRequiredComponentInChildren<GlitchTerminalActivator>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
	}

	public void OnStateEnter(GlitchTerminalAnimatorState.Id id)
	{
		if (this.onStateEnter != null)
		{
			this.onStateEnter(id);
		}
	}

	public void OnEnter(Transform destinationTransform)
	{
		GlitchTerminalAnimator_Player fx = InstantiatePlayerFX();
		SRSingleton<SceneContext>.Instance.StartCoroutine(OnEnter_Coroutine_FX(fx, destinationTransform));
		SRSingleton<SceneContext>.Instance.StartCoroutine(OnEnter_Coroutine_Region(fx));
	}

	private IEnumerator OnEnter_Coroutine_FX(GlitchTerminalAnimator_Player fx, Transform destinationTransform)
	{
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		using (new TemporaryLockInputMode(base.gameObject.GetInstanceID()))
		{
			using (new TemporaryReplaceSeaMaterial())
			{
				SECTR_AudioSystem.Play(glitch.animationOnTeleportInCue, fx.transform, Vector3.zero, loop: false);
				fx.animator.SetTrigger("trigger_enter_slimulation");
				yield return fx.WaitForStateExit(GlitchTerminalAnimator_PlayerState.Id.ENTERING);
				TeleportTo(destinationTransform, RegionRegistry.RegionSetId.SLIMULATIONS);
				yield return fx.WaitForStateExit(GlitchTerminalAnimator_PlayerState.Id.EXITING);
				Destroyer.Destroy(fx.gameObject, "GlitchTerminalAnimator.OnEnter_Coroutine");
			}
		}
	}

	private IEnumerator OnEnter_Coroutine_Region(GlitchTerminalAnimator_Player fx)
	{
		base.animator.SetBool("state_in_slimulation", value: true);
		Region region = base.gameObject.GetRequiredComponentInParent<Region>();
		yield return fx.WaitForAnimationEvent(GlitchTerminalAnimator_Player.AnimationEvent.ENTERING_FULLY_COVERED);
		region.OnRegionSetDeactivated();
	}

	public static void OnExit(Action onMidpoint, Action onComplete, int sourceObjectId)
	{
		SRSingleton<SceneContext>.Instance.StartCoroutine(OnExit_Coroutine_FX(onMidpoint, onComplete, sourceObjectId));
	}

	private static IEnumerator OnExit_Coroutine_FX(Action onMidpoint, Action onComplete, int sourceObjectId)
	{
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		GlitchRegionHelper_Viktor instance = SRSingleton<GlitchRegionHelper_Viktor>.Instance;
		Transform destinationTransform = instance.activator.destinationTransform;
		GlitchTerminalAnimator_Player fx = InstantiatePlayerFX();
		using (new TemporaryLockInputMode(sourceObjectId))
		{
			using (new TemporaryReplaceSeaMaterial())
			{
				SECTR_AudioSystem.Play(glitch.animationOnTeleportOutCue, fx.transform, Vector3.zero, loop: false);
				fx.animator.SetTrigger("trigger_exit_slimulation");
				yield return fx.WaitForStateExit(GlitchTerminalAnimator_PlayerState.Id.EXITING);
				TeleportTo(destinationTransform, RegionRegistry.RegionSetId.VIKTOR_LAB);
				onMidpoint?.Invoke();
				yield return fx.WaitForStateExit(GlitchTerminalAnimator_PlayerState.Id.ENTERING);
				Destroyer.Destroy(fx.gameObject, "GlitchTerminalAnimator.OnExit_Coroutine");
			}
		}
		onComplete?.Invoke();
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
		if (current == RegionRegistry.RegionSetId.VIKTOR_LAB)
		{
			base.animator.SetBool("state_in_slimulation", value: false);
		}
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
	}

	public void TransformChanged(Vector3 position, Quaternion rotation)
	{
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> ammo)
	{
	}

	public void KeyAdded()
	{
	}

	private static void TeleportTo(Transform destinationTransform, RegionRegistry.RegionSetId regionSetId)
	{
		TeleportablePlayer requiredComponent = SRSingleton<SceneContext>.Instance.Player.GetRequiredComponent<TeleportablePlayer>();
		Vector3 position = destinationTransform.position;
		Vector3? rotation = destinationTransform.rotation.eulerAngles;
		requiredComponent.TeleportTo(position, regionSetId, rotation, overlayEnabled: true, audioEnabled: false);
	}

	private static GlitchTerminalAnimator_Player InstantiatePlayerFX()
	{
		return UnityEngine.Object.Instantiate(SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.animationFX, SRSingleton<SceneContext>.Instance.Player.transform, worldPositionStays: false).GetRequiredComponent<GlitchTerminalAnimator_Player>();
	}
}

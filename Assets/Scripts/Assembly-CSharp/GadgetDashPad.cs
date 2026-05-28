using System;
using System.Collections;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;
using UnityEngine.UI;

public class GadgetDashPad : ControllerCollisionListenerBehaviour
{
	[Serializable]
	public class HudFX
	{
		[Tooltip("SFX played during the activation of the HUD overlay. (2D, looping")]
		public SECTR_AudioCue onActiveSFX;

		[Tooltip("SFX played at the deactivation of the HUD overlay. (2D, non-looping")]
		public SECTR_AudioCue onDeactivatedSFX;
	}

	private class FXHelper : SRSingleton<FXHelper>
	{
		private const float FX_FADE_TIME_IN = 0.25f;

		private const float FX_FADE_TIME_OUT = 1f;

		private GameObject overlay;

		private GameObject meter;

		private float alpha;

		private SECTR_AudioCueInstance onActiveSFXInstance;

		private SECTR_AudioCue onDeactivatedSFX;

		private Tween tween;

		public static void OnRunEnergyDepletionTimeChanged(HudFX args)
		{
			SRSingleton<SceneContext>.Instance.StartCoroutine(OnRunEnergyDepletionTimeChanged_Coroutine(args));
		}

		private static IEnumerator OnRunEnergyDepletionTimeChanged_Coroutine(HudFX args)
		{
			TimeDirector timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
			double time = SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel().runEnergyDepletionTime;
			if (SRSingleton<FXHelper>.Instance == null)
			{
				GameObject obj = new GameObject("GadgetDashPad.FXHelper");
				obj.transform.SetParent(SRSingleton<DynamicObjectContainer>.Instance.transform);
				obj.AddComponent<FXHelper>();
				SRSingleton<FXHelper>.Instance.onDeactivatedSFX = args.onDeactivatedSFX;
				SRSingleton<FXHelper>.Instance.onActiveSFXInstance = SECTR_AudioSystem.Play(args.onActiveSFX, Vector3.zero, loop: true);
			}
			DestroyAfterTime obj2 = SRSingleton<FXHelper>.Instance.gameObject.GetComponent<DestroyAfterTime>() ?? SRSingleton<FXHelper>.Instance.gameObject.AddComponent<DestroyAfterTime>();
			obj2.SetDeathTime(time);
			obj2.destroyAsActor = false;
			SRSingleton<FXHelper>.Instance.tween?.Kill();
			yield return new WaitForEndOfFrame();
			float interval = (float)((time - timeDirector.WorldTime()) * 0.01666666753590107) - 1.25f;
			SRSingleton<FXHelper>.Instance.tween = DOTween.Sequence().Append(DOTween.To(() => SRSingleton<FXHelper>.Instance.alpha, SRSingleton<FXHelper>.Instance.SetFXAlpha, 1f, 0.25f)).AppendInterval(interval)
				.Append(DOTween.To(() => SRSingleton<FXHelper>.Instance.alpha, SRSingleton<FXHelper>.Instance.SetFXAlpha, 0f, 1f).OnStart(delegate
				{
					SRSingleton<FXHelper>.Instance.OnFadeOutStart();
				}));
		}

		public override void Awake()
		{
			base.Awake();
			overlay = SRSingleton<Overlay>.Instance.Play(SRSingleton<Overlay>.Instance.dashPadFX);
			meter = SRSingleton<HudUI>.Instance.energyMeter.Play(SRSingleton<HudUI>.Instance.energyMeter.dashPadFX);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			Destroyer.Destroy(overlay, "GadgetDashPad.FXHelper.OnDestroy");
			Destroyer.Destroy(meter, "GadgetDashPad.FXHelper.OnDestroy");
			if (tween != null)
			{
				tween.Kill();
			}
			onActiveSFXInstance.Stop(stopImmediately: true);
		}

		private void SetFXAlpha(float alpha)
		{
			this.alpha = alpha;
			Renderer requiredComponent = overlay.GetRequiredComponent<Renderer>();
			requiredComponent.material.color = GetColor(requiredComponent.material.color, this.alpha);
			Image requiredComponent2 = meter.GetRequiredComponent<Image>();
			requiredComponent2.color = GetColor(requiredComponent2.color, this.alpha);
		}

		private void OnFadeOutStart()
		{
			SECTR_AudioSystem.Play(onDeactivatedSFX, Vector3.zero, loop: false);
			onActiveSFXInstance.Stop(stopImmediately: false);
		}

		private static Color GetColor(Color color, float alpha)
		{
			return new Color(color.r, color.g, color.b, alpha);
		}
	}

	[Tooltip("FX played when the dash pad is activated.")]
	public GameObject onActivationFX;

	[Tooltip("Time before energy begins being depleted again. (in-game minutes)")]
	public float activationDuration;

	[Tooltip("2D HUD overlay properties.")]
	public HudFX hudFX;

	private TimeDirector timeDirector;

	private PlayerModel player;

	private const float COOLDOWN_DURATION = 0.75f;

	private float activationTime;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		player = SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel();
	}

	protected override void OnControllerCollisionEntered()
	{
		base.OnControllerCollisionEntered();
		player.runEnergyDepletionTime = double.MaxValue;
		FXHelper.OnRunEnergyDepletionTimeChanged(hudFX);
		if (onActivationFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(onActivationFX, base.gameObject, Vector3.zero, Quaternion.identity);
		}
	}

	protected override void OnControllerCollisionExited()
	{
		base.OnControllerCollisionExited();
		activationTime = Time.time + 0.75f;
		player.runEnergyDepletionTime = timeDirector.HoursFromNow(activationDuration * (1f / 60f));
		FXHelper.OnRunEnergyDepletionTimeChanged(hudFX);
	}

	protected override bool Predicate(GameObject collision)
	{
		if (collision == SRSingleton<SceneContext>.Instance.Player)
		{
			return Time.time >= activationTime;
		}
		return false;
	}
}

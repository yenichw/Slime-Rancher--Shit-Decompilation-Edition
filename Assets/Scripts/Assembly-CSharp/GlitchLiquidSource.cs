using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GlitchLiquidSource : LiquidSource
{
	[Tooltip("FX objects that should be activated/deactivated based off the current state.")]
	public GameObject[] onAvailableFX;

	[Tooltip("SFX played when the station is consumed. (once, non-looping)")]
	public SECTR_AudioCue onConsumeCue;

	[Tooltip("FX spawned when the station is consumed.")]
	public GameObject onConsumeFX;

	private TutorialDirector tutorialDirector;

	public override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			tutorialDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		}
	}

	protected override void InitModel(LiquidSourceModel model)
	{
		base.InitModel(model);
		model.unitsFilled = 1f;
	}

	protected override void SetModel(LiquidSourceModel model)
	{
		base.SetModel(model);
		OnAvailabilityChanged();
	}

	private void OnAvailabilityChanged()
	{
		bool active = Available();
		GameObject[] array = onAvailableFX;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
	}

	public override bool Available()
	{
		if (base.Available())
		{
			return model.unitsFilled > 0f;
		}
		return false;
	}

	public override void ConsumeLiquid()
	{
		base.ConsumeLiquid();
		model.unitsFilled = 0f;
		tutorialDirector.MaybeShowPopup(TutorialDirector.Id.SLIMULATIONS_DEBUG_SPRAY);
		SRBehaviour.SpawnAndPlayFX(onConsumeFX, base.transform.position, Quaternion.identity);
		SECTR_AudioSystem.Play(onConsumeCue, base.transform.position, loop: false);
		OnAvailabilityChanged();
	}

	public override bool ReplacesExistingLiquidAmmo()
	{
		return true;
	}

	public void ResetLiquidState()
	{
		model.unitsFilled = 1f;
		OnAvailabilityChanged();
	}
}

using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class SiloStorageActivator : MonoBehaviour, TechActivator, LandPlotModel.Participant
{
	[Tooltip("SiloCatcher script to update on slot changed.")]
	public SiloCatcher siloCatcher;

	[Tooltip("SiloSlotUI scripts to update on slot changed.")]
	public List<SiloSlotUI> siloSlotUIs;

	[Tooltip("Animator to control the active silo slot.")]
	public Animator siloSlotAnimator;

	[Tooltip("SFX played when the button is pressed. (optional)")]
	public SECTR_AudioCue onPressButtonCue;

	[Tooltip("Where we fall in the activator order")]
	public int activatorIdx;

	private Animator buttonAnimator;

	private int buttonAnimation;

	private const float TIME_BETWEEN_ACTIVATIONS = 0.4f;

	private float nextActivationTime;

	private LandPlotModel landPlotModel;

	public void Awake()
	{
		buttonAnimator = GetComponentInParent<Animator>();
		buttonAnimation = Animator.StringToHash("ButtonPressed");
	}

	public void InitModel(LandPlotModel model)
	{
	}

	public void SetModel(LandPlotModel model)
	{
		landPlotModel = model;
		OnActiveSlotChanged();
	}

	public void OnEnable()
	{
		if (landPlotModel != null)
		{
			OnActiveSlotChanged();
		}
	}

	public void Activate()
	{
		if (nextActivationTime < Time.time)
		{
			nextActivationTime = Time.time + 0.4f;
			landPlotModel.siloStorageIndices[activatorIdx] = (landPlotModel.siloStorageIndices[activatorIdx] + 1) % siloSlotUIs.Count;
			SECTR_AudioSystem.Play(onPressButtonCue, base.transform.position, loop: false);
			buttonAnimator.SetTrigger(buttonAnimation);
			OnActiveSlotChanged();
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}

	private void OnActiveSlotChanged()
	{
		int num = landPlotModel.siloStorageIndices[activatorIdx];
		siloCatcher.slotIdx = siloSlotUIs[num].slotIdx;
		if (siloSlotAnimator.gameObject.activeInHierarchy)
		{
			siloSlotAnimator.SetInteger("slot", num);
		}
	}
}

using UnityEngine;

public class QuicksilverEnergyReplacer : MonoBehaviour
{
	[Tooltip("Energy generator required to be active to replace ammo.")]
	public QuicksilverEnergyGenerator generator;

	[Tooltip("Time in game hours between ability to trigger this.")]
	public float cooldownHours = 1f;

	[Tooltip("FX to display when the ammo replacer is available. (optional)")]
	public GameObject activeFX;

	[Tooltip("SFX played when the player picks up the energy.")]
	public SECTR_AudioCue onPickupCue;

	private TimeDirector timeDirector;

	private TutorialDirector tutDirector;

	private PlayerState playerState;

	private double activeTime;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		tutDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void Update()
	{
		if (activeFX != null)
		{
			activeFX.SetActive(IsReady());
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		if (IsReady() && PhysicsUtil.IsPlayerMainCollider(col))
		{
			tutDirector.MaybeShowPopup(TutorialDirector.Id.RACE_ENERGYBOOST);
			if (playerState.GetCurrEnergy() < playerState.GetMaxEnergy())
			{
				SECTR_AudioSystem.Play(onPickupCue, base.transform.position, loop: false);
				activeTime = timeDirector.HoursFromNow(cooldownHours);
				playerState.SetEnergy(playerState.GetMaxEnergy());
			}
		}
	}

	private bool IsReady()
	{
		if (generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE)
		{
			return timeDirector.HasReached(activeTime);
		}
		return false;
	}
}

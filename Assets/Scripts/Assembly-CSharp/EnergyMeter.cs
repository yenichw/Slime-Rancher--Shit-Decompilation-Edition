using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EnergyMeter : SRBehaviour
{
	[Tooltip("GameObject containing an FX that is active while the energy meeting is recharging. (optional)")]
	public GameObject onEnergyRechargingFX;

	[Tooltip("FX activated when the dash pad is active.")]
	public GameObject dashPadFX;

	private TimeDirector timeDirector;

	private PlayerModel model;

	private PlayerState state;

	private StatusBar statusBar;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		model = SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel();
		state = SRSingleton<SceneContext>.Instance.PlayerState;
		statusBar = GetComponent<StatusBar>();
	}

	public void Update()
	{
		statusBar.currValue = state.GetCurrEnergy();
		statusBar.maxValue = state.GetMaxEnergy();
		if ((bool)onEnergyRechargingFX)
		{
			onEnergyRechargingFX.SetActive(timeDirector.HasReached(model.energyRecoverAfter) && model.currEnergy < (float)model.maxEnergy);
		}
	}

	public GameObject Play(GameObject fxPrefab)
	{
		Transform parent = statusBar.statusImage.transform;
		GameObject obj = Object.Instantiate(fxPrefab, parent);
		SRBehaviour.PlayFX(obj);
		return obj;
	}
}

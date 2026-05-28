using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

[RequireComponent(typeof(Drone))]
public class DroneNoClip : SRBehaviour, GadgetModel.Participant
{
	private DroneModel model;

	public void InitModel(GadgetModel model)
	{
	}

	public void SetModel(GadgetModel model)
	{
		this.model = (DroneModel)model;
	}

	public void OnEnable()
	{
		base.gameObject.layer = 14;
		model.noClip = true;
	}

	public void OnDisable()
	{
		base.gameObject.layer = 20;
		model.noClip = false;
	}
}

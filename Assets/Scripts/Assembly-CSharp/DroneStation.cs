using UnityEngine;

[RequireComponent(typeof(DroneStationBattery))]
public class DroneStation : SRBehaviour
{
	[Tooltip("Transform guide: resting position/rotation.")]
	public Transform guideRest;

	public DroneStationAnimator animator { get; private set; }

	public DroneStationBattery battery { get; private set; }

	public DroneGadget gadget { get; private set; }

	public void Awake()
	{
		gadget = GetComponentInParent<DroneGadget>();
		animator = GetComponentInChildren<DroneStationAnimator>();
		battery = GetComponent<DroneStationBattery>();
	}
}

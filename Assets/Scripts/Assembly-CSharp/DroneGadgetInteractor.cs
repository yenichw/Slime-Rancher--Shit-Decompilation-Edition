using UnityEngine;

public class DroneGadgetInteractor : MonoBehaviour, GadgetInteractor
{
	public DroneGadget gadget { get; private set; }

	public void Awake()
	{
		gadget = GetComponentInParent<DroneGadget>();
	}

	public bool CanInteract()
	{
		return true;
	}

	public void OnInteract()
	{
		gadget.drone.InstantiateDroneUI();
	}
}

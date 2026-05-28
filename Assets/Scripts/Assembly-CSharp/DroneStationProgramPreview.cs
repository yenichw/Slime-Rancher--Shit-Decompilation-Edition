using UnityEngine;
using UnityEngine.UI;

public class DroneStationProgramPreview : SRBehaviour
{
	[Tooltip("Index into DroneGadget.programs to display.")]
	public int programIndex;

	[Tooltip("Image to update with the program preview.")]
	public Image image;

	public DroneGadget gadget { get; private set; }

	public void Start()
	{
		gadget = GetComponentInParent<DroneGadget>();
		gadget.onProgramsChanged += OnProgramsChanged;
		OnProgramsChanged(gadget.programs);
	}

	private void OnProgramsChanged(DroneMetadata.Program[] programs)
	{
		Sprite sprite = (programs[programIndex].IsComplete() ? programs[programIndex].target.GetImage() : gadget.metadata.imageNone);
		image.enabled = sprite != null;
		image.sprite = sprite;
	}
}

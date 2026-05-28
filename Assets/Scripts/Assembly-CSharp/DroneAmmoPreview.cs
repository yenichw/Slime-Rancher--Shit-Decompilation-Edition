using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Drone))]
public class DroneAmmoPreview : MonoBehaviour
{
	[Tooltip("Root game object to enable/disable based off ammo state.")]
	public GameObject root;

	[Tooltip("Image to update with the ammo preview.")]
	public Image[] images;

	private Drone drone;

	private Identifiable.Id previous;

	private LookupDirector lookup;

	public void Start()
	{
		lookup = SRSingleton<GameContext>.Instance.LookupDirector;
		drone = GetComponent<Drone>();
		root.SetActive(value: false);
	}

	public void LateUpdate()
	{
		Identifiable.Id slotName = drone.ammo.GetSlotName();
		if (slotName != previous)
		{
			root.SetActive(slotName != Identifiable.Id.NONE);
			Sprite sprite = (root.activeSelf ? lookup.GetIcon(slotName) : null);
			Image[] array = images;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = sprite;
			}
			previous = slotName;
		}
	}
}

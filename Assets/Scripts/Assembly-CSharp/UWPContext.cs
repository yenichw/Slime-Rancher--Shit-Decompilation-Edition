using RichPresence;
using UnityEngine;

public class UWPContext : MonoBehaviour, Handler
{
	public GameObject ControllerDisconnectedPopup;

	public GameObject UserSignOutPopup;

	public void SetRichPresence(MainMenuData data)
	{
		SetRichPresence("MainMenu");
	}

	public void SetRichPresence(InZoneData data)
	{
		if (Director.TryGetZoneId(data.zone, out var id))
		{
			SetRichPresence(id);
		}
	}

	private void SetRichPresence(string id)
	{
	}
}

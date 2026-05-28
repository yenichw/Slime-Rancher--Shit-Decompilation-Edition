using RichPresence;
using UnityEngine;

public class XboxOneContext : MonoBehaviour, Handler
{
	public TextAsset xboxEvents;

	public XboxEngagementPopupUI engagementPopupUIPrefab;

	public XboxUserChangePopupUI userChangePopupUIPrefab;

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

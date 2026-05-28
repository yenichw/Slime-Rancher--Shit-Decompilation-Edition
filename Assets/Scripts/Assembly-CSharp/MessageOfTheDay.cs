using System;
using UnityEngine;

[Serializable]
public abstract class MessageOfTheDay
{
	public string id;

	public Sprite sprite;

	public virtual string GetId()
	{
		return id;
	}

	public Sprite GetSprite()
	{
		return sprite;
	}

	public abstract string GetUrl(string lang);

	public abstract string GetAnnouncementText(string lang);

	public abstract string GetTitleText(string lang);

	public abstract string GetBodyText(string lang);

	public abstract string GetButtonText(string lang);
}

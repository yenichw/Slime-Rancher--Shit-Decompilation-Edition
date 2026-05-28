using System;
using DLCPackage;
using UnityEngine;

[Serializable]
public class MessageOfTheDayDirector
{
	public MessageOfTheDayProvider pcProvider;

	public MessageOfTheDayProvider epicProvider;

	public MessageOfTheDayProvider steamProvider;

	public MessageOfTheDayProvider ps4Provider;

	public MessageOfTheDayProvider xboxProvider;

	public MessageOfTheDayProvider tencentProvider;

	public GameContext gameContext;

	private const string DLC_PREFIX = "dlc://";

	public MessageOfTheDayProvider GetProvider()
	{
		return pcProvider;
	}

	public void ActivateLink(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			Log.Warning("MotD url to activate was null or empty.");
			return;
		}
		if (url.StartsWith("dlc://"))
		{
			try
			{
				Id id = (Id)Enum.Parse(typeof(Id), url.Substring("dlc://".Length));
				gameContext.DLCDirector.ShowPackageInStore(id);
				return;
			}
			catch (Exception ex)
			{
				Log.Error("Exception when trying to extract DLC ID from DLC URL in MotD.", "Message", ex.Message, "stackTrace", ex.StackTrace);
				return;
			}
		}
		Application.OpenURL(url);
	}
}

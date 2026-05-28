using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class DataPrivacyHandler : MonoBehaviour
{
	public Button sourceButton;

	private bool urlOpened;

	public void OpenDataPrivacyUrl()
	{
		sourceButton.interactable = false;
		DataPrivacy.FetchPrivacyUrl(OpenUrl, OnFailure);
	}

	private void OnFailure(string reason)
	{
		sourceButton.interactable = true;
		Debug.LogWarning($"Failed to get data privacy url: {reason}");
	}

	private void OpenUrl(string url)
	{
		sourceButton.interactable = true;
		urlOpened = true;
		Application.OpenURL(url);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus && urlOpened)
		{
			urlOpened = false;
			RemoteSettings.ForceUpdate();
		}
	}
}

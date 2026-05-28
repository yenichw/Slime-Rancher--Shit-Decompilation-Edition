using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DroneUIProgramButton : MonoBehaviour
{
	public class Title
	{
		public enum Type
		{
			TARGET = 0,
			SOURCE = 1,
			DESTINATION = 2
		}

		public Type type;

		public int? index;
	}

	[Tooltip("Selection title.")]
	public TMP_Text title;

	[Tooltip("Selection name.")]
	public new TMP_Text name;

	[Tooltip("Selection button.")]
	public Button button;

	[Tooltip("Selection image.")]
	public Image image;

	private Title titleMetadata;

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnMessageBundlesChanged);
		}
	}

	public DroneUIProgramButton Init(DroneMetadata.Program.BaseComponent element, Title title = null)
	{
		name.text = element.GetName();
		image.sprite = element.GetImage();
		titleMetadata = title;
		SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnMessageBundlesChanged);
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnMessageBundlesChanged);
		return this;
	}

	private void OnMessageBundlesChanged(MessageDirector messageDirector)
	{
		if (titleMetadata != null)
		{
			title.gameObject.SetActive(value: true);
			title.text = string.Format("{0}{1}", messageDirector.GetBundle("ui").Get($"l.drone.{titleMetadata.type.ToString().ToLowerInvariant()}"), titleMetadata.index.HasValue ? $" {titleMetadata.index.Value}" : string.Empty);
		}
		else
		{
			title.gameObject.SetActive(value: false);
		}
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlIconUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Text ctrlText;

	[SerializeField]
	private Image ctrlImage;

	[SerializeField]
	private string action;

	public void UpdateIcon()
	{
		string activeDeviceString = SRSingleton<GameContext>.Instance.InputDirector.GetActiveDeviceString(action, isPauseAction: false);
		bool flag = InputDirector.UsingGamepad();
		ctrlImage.sprite = SRSingleton<GameContext>.Instance.InputDirector.GetActiveDeviceIcon(action, isPauseAction: false, out var iconFound);
		ctrlText.text = XlateKeyText.XlateKey(activeDeviceString);
		ctrlImage.gameObject.SetActive(iconFound || flag);
		ctrlText.gameObject.SetActive(!iconFound && !flag);
	}
}

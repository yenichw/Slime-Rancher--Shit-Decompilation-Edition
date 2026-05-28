using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonLine : MonoBehaviour
{
	public TMP_Text keyText;

	public Image btnImage;

	public TMP_Text desc;

	private InputDirector inputDirector;

	private string inputKey;

	public void Awake()
	{
		inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
	}

	public void Init(string inputKey, string descStr)
	{
		this.inputKey = inputKey;
		keyText.text = inputKey;
		desc.text = descStr;
		UpdateInstructionIcon();
	}

	private void UpdateInstructionIcon()
	{
		bool flag = InputDirector.UsingGamepad();
		btnImage.sprite = inputDirector.GetActiveDeviceIcon(inputKey, isPauseAction: false, out var iconFound);
		btnImage.gameObject.SetActive(iconFound || flag);
		keyText.gameObject.SetActive(!iconFound && !flag);
	}
}

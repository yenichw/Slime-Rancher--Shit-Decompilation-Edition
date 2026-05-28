using System;
using TMPro;
using UnityEngine;

public class LoadErrorUI : BaseUI
{
	public GameObject okButton;

	public GameObject closeButton;

	public GameObject contactSupportText;

	public TMP_Text message;

	public TMP_Text okButtonText;

	public TMP_Text closeButtonText;

	private Action onOkAction;

	private Action onCloseAction;

	public static LoadErrorUI OpenLoadErrorUI(LoadErrorUI prefab, string messageKey, bool showContactSupport, string okButtonKey, Action onOkAction, string closeButtonKey, Action onCloseAction)
	{
		LoadErrorUI loadErrorUI = UnityEngine.Object.Instantiate(prefab);
		if (onOkAction == null)
		{
			loadErrorUI.okButton.SetActive(value: false);
		}
		else
		{
			loadErrorUI.okButtonText.SetText(loadErrorUI.uiBundle.Xlate(okButtonKey));
		}
		if (showContactSupport)
		{
			loadErrorUI.contactSupportText.SetActive(value: true);
		}
		else
		{
			loadErrorUI.contactSupportText.SetActive(value: false);
		}
		loadErrorUI.message.SetText(loadErrorUI.uiBundle.Xlate(messageKey));
		loadErrorUI.closeButtonText.SetText(loadErrorUI.uiBundle.Xlate(closeButtonKey));
		loadErrorUI.onOkAction = onOkAction;
		loadErrorUI.onCloseAction = onCloseAction;
		return loadErrorUI;
	}

	public static void OpenLoadErrorUI(LoadErrorUI prefab, string message, bool showContactSupport, string closeButtonKey, Action onCloseAction)
	{
		OpenLoadErrorUI(prefab, message, showContactSupport, null, null, closeButtonKey, onCloseAction);
	}

	public void OnOk()
	{
		onOkAction();
		Close();
	}

	public void OnClose()
	{
		onCloseAction();
		Close();
	}

	protected override bool Closeable()
	{
		return false;
	}
}

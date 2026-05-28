using System;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XlateKeyText : MonoBehaviour
{
	public string bundlePath = "ui";

	public string key;

	public string inputKey;

	private MessageBundle bundle;

	private Text text;

	private TMP_Text meshText;

	public void Awake()
	{
		text = GetComponent<Text>();
		meshText = GetComponent<TMP_Text>();
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
	}

	public void InitBundles(MessageDirector msgDir)
	{
		bundle = msgDir.GetBundle(bundlePath);
		InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		InputDirector inputDirector2 = SRSingleton<GameContext>.Instance.InputDirector;
		inputDirector2.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector2.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		OnKeysChanged();
	}

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
			inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(InitBundles);
		}
	}

	public void OnKeysChanged()
	{
		if (text != null)
		{
			text.text = bundle.Get(key, GetKeyString(bundle, inputKey, primaryOnly: false));
		}
		if (meshText != null)
		{
			meshText.text = bundle.Get(key, GetKeyString(bundle, inputKey, primaryOnly: false));
		}
	}

	public static string GetKeyString(MessageBundle bundle, string inputKey, bool primaryOnly, bool ignoreGamepad = false)
	{
		return GetKeyString(bundle, SRInput.Actions.Get(inputKey), primaryOnly, ignoreGamepad);
	}

	public static string GetKeyString(MessageBundle bundle, PlayerAction inputKey, bool primaryOnly, bool ignoreGamepad = false)
	{
		string buttonKey = SRInput.GetButtonKey(inputKey, SRInput.ButtonType.PRIMARY);
		string buttonKey2 = SRInput.GetButtonKey(inputKey, SRInput.ButtonType.SECONDARY);
		string buttonKey3 = SRInput.GetButtonKey(inputKey, SRInput.ButtonType.GAMEPAD);
		string compoundKey = "m.keys.0";
		if (InputDirector.UsingGamepad() && !ignoreGamepad && buttonKey3 != null)
		{
			compoundKey = MessageUtil.Tcompose("m.keys.1", XlateKey(buttonKey3));
		}
		else if (buttonKey != null && buttonKey2 != null && !primaryOnly)
		{
			compoundKey = MessageUtil.Tcompose("m.keys.2", XlateKey(buttonKey), XlateKey(buttonKey2));
		}
		else if (buttonKey != null)
		{
			compoundKey = MessageUtil.Tcompose("m.keys.1", XlateKey(buttonKey));
		}
		else if (buttonKey2 != null)
		{
			compoundKey = MessageUtil.Tcompose("m.keys.1", XlateKey(buttonKey2));
		}
		return bundle.Xlate(compoundKey);
	}

	public static string XlateKey(KeyCode key)
	{
		return XlateKey(key.ToString());
	}

	public static string XlateKey(string keyStr)
	{
		MessageBundle messageBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("keys");
		if (messageBundle.Exists("m." + GetPlatformStr() + "." + keyStr))
		{
			return messageBundle.Get("m." + GetPlatformStr() + "." + keyStr);
		}
		if (messageBundle.Exists("m." + keyStr))
		{
			return messageBundle.Get("m." + keyStr);
		}
		return keyStr;
	}

	public static string XlateKey(InputControlType key)
	{
		return XlateKey(key.ToString());
	}

	public static string XlateKey(Key key)
	{
		return XlateKey(key.ToString());
	}

	public static string XlateKey(Mouse key)
	{
		return XlateKey(key.ToString());
	}

	private static string GetPlatformStr()
	{
		return "win";
	}
}

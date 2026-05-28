using System;
using TMPro;
using UnityEngine;

public class ActivateUI : MonoBehaviour
{
	public string key = "m.press_to_activate";

	public GameObject normalPrompt;

	public GameObject gamepadPrompt;

	public TMP_Text normalPromptText;

	public TMP_Text preGamepadText;

	public TMP_Text postGamepadText;

	public bool useGamepadAlt;

	private MessageDirector messageDir;

	private InputDirector inputDir;

	public void Awake()
	{
		messageDir = SRSingleton<GameContext>.Instance.MessageDirector;
		messageDir.RegisterBundlesListener(OnBundlesAvailable);
		inputDir = SRSingleton<GameContext>.Instance.InputDirector;
		InputDirector inputDirector = inputDir;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		ResetActivePrompt();
	}

	public void Update()
	{
		ResetActivePrompt();
	}

	public void OnDestroy()
	{
		messageDir.UnregisterBundlesListener(OnBundlesAvailable);
		InputDirector inputDirector = inputDir;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
	}

	private void OnBundlesAvailable(MessageDirector messageDir)
	{
		OnKeysChanged();
	}

	private void OnKeysChanged()
	{
		MessageBundle bundle = messageDir.GetBundle("ui");
		normalPromptText.text = bundle.Get(key, XlateKeyText.GetKeyString(bundle, SRInput.Actions.interact, primaryOnly: true));
		if (preGamepadText != null)
		{
			preGamepadText.text = bundle.Get(key + ".pre_gamepad");
		}
		if (postGamepadText != null)
		{
			postGamepadText.text = bundle.Get(key + ".post_gamepad");
		}
	}

	private void ResetActivePrompt()
	{
		bool flag = useGamepadAlt && InputDirector.UsingGamepad();
		normalPrompt.SetActive(!flag);
		if (gamepadPrompt != null)
		{
			gamepadPrompt.SetActive(flag);
		}
	}
}

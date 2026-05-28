using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SelectImageForAction : MonoBehaviour
{
	public string action;

	public bool isPauseAction;

	private Image img;

	private InputDirector inputDir;

	public void Awake()
	{
		img = GetComponent<Image>();
		inputDir = SRSingleton<GameContext>.Instance.InputDirector;
		InputDirector inputDirector = inputDir;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
	}

	public void OnDestroy()
	{
		InputDirector inputDirector = inputDir;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
	}

	public void OnKeysChanged()
	{
		UpdateButtonImage();
	}

	public void Start()
	{
		UpdateButtonImage();
	}

	public void UpdateButtonImage()
	{
		img.sprite = inputDir.GetActiveDeviceIcon(action, isPauseAction, out var _);
	}
}

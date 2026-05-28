using System;
using UnityEngine;

public class RotationRowUI : MonoBehaviour
{
	[SerializeField]
	private GameObject rotationRow;

	[SerializeField]
	private ControlIconUI[] gamepadControls;

	private InputDirector inputDirector;

	private void Awake()
	{
		inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
		InputDirector obj = inputDirector;
		obj.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(obj.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		UpdateControls();
	}

	private void OnDestroy()
	{
		if (inputDirector != null)
		{
			InputDirector obj = inputDirector;
			obj.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(obj.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		}
	}

	private void OnKeysChanged()
	{
		UpdateControls();
	}

	public void ShowRow()
	{
		rotationRow.SetActive(value: true);
	}

	public void HideRow()
	{
		rotationRow.SetActive(value: false);
	}

	public void UpdateControls()
	{
		ControlIconUI[] array = gamepadControls;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateIcon();
		}
	}
}

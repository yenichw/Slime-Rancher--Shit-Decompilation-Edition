using TMPro;
using UnityEngine;

public class ConfirmResolutionUI : SRBehaviour
{
	public delegate void OnCancel();

	public delegate void OnConfirm();

	public TMP_Text countdownText;

	public OnCancel onCancel;

	public OnConfirm onConfirm;

	private float expireTime;

	private const float COUNTDOWN_TIME = 15f;

	public void Awake()
	{
		expireTime = Time.unscaledTime + 15f;
	}

	public void OK()
	{
		if (onConfirm != null)
		{
			onConfirm();
		}
		Destroyer.Destroy(base.gameObject, "ConfirmResolutionUI.OK");
	}

	public void Cancel()
	{
		if (onCancel != null)
		{
			onCancel();
		}
		Destroyer.Destroy(base.gameObject, "ConfirmResolutionUI.Cancel");
	}

	public void Update()
	{
		float num = expireTime - Time.unscaledTime;
		if (SRInput.PauseActions.cancel.WasPressed || num <= 0f)
		{
			Cancel();
		}
		else
		{
			countdownText.text = Mathf.FloorToInt(num).ToString();
		}
	}
}

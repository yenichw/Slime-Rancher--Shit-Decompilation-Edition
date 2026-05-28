using UnityEngine;
using UnityEngine.UI;

public class StoreGifUI : BaseUI
{
	public delegate void OnConfirm();

	public OnConfirm onConfirm;

	public Button okBtn;

	public Button cancelBtn;

	public float ellipsisChangeTime;

	public float ellipsisCount;

	private bool storing;

	public void OK()
	{
		okBtn.interactable = false;
		cancelBtn.interactable = false;
		storing = true;
		onConfirm();
	}

	public void Cancel()
	{
		Destroyer.Destroy(base.gameObject, "StoreGifUI.Cancel");
	}

	public override void Update()
	{
		base.Update();
		if (storing && Time.unscaledTime > ellipsisChangeTime)
		{
			Status(MessageUtil.Compose("m.ellipsize" + ellipsisCount, "m.gif_storing"));
			ellipsisCount = (ellipsisCount + 1f) % 4f;
			ellipsisChangeTime = Time.unscaledTime + 0.5f;
		}
	}
}

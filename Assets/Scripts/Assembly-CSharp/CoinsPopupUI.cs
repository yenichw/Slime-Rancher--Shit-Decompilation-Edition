using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinsPopupUI : MonoBehaviour
{
	public TMP_Text amountText;

	public Image icon;

	public SECTR_AudioCue cue;

	public CanvasGroup canvasGroup;

	private const float FADE_IN_TIME = 0.1f;

	private const float FADE_OUT_TIME = 0.3f;

	private const float MOVE_TIME = 1f;

	private const float MOVE_AMOUNT = 180f;

	private void Start()
	{
		SECTR_AudioSystem.Play(cue, Vector3.zero, loop: false);
		AnimateCoinPopup();
	}

	public void Init(int amount, Sprite overrideIcon, Color? overrideColor, SECTR_AudioCue overrideCue)
	{
		amountText.text = ((amount >= 0) ? "+" : "") + amount;
		if (overrideIcon != null)
		{
			icon.sprite = overrideIcon;
		}
		if (overrideColor.HasValue)
		{
			amountText.color = overrideColor.Value;
		}
		if (overrideCue != null)
		{
			cue = overrideCue;
		}
	}

	private void AnimateCoinPopup()
	{
		DOTween.Sequence().Append(base.transform.DOBlendableMoveBy(Vector3.up * 180f, 1f)).Join(canvasGroup.DOFade(1f, 0.1f).From(0f))
			.Append(canvasGroup.DOFade(0f, 0.3f))
			.OnComplete(delegate
			{
				Destroyer.Destroy(base.gameObject, "CoinsPopupUI.DoSequence");
			})
			.SetUpdate(isIndependentUpdate: true);
	}
}

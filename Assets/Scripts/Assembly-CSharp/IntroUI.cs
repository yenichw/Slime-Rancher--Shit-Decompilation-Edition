using DG.Tweening;
using UnityEngine;

public class IntroUI : BaseUI
{
	public GameObject background;

	public CanvasGroup introLine1;

	public CanvasGroup introLine2;

	public CanvasGroup introLine3;

	public const float fadeTime = 1f;

	public const float fadeTimeMargin = 0.25f;

	public const float introLineFadeTime = 1f;

	public const float introLineTime = 2.5f;

	public const float introLastLineTime = 7f;

	private bool endReached;

	private CanvasGroup introCanvasGroup;

	public override void Awake()
	{
		base.Awake();
		introCanvasGroup = GetComponent<CanvasGroup>();
	}

	public void OnEnable()
	{
		SECTR_AudioSystem.PauseNonUISFX(pause: true);
		SRSingleton<SceneContext>.Instance.PopupDirector.RegisterSuppressor();
		SRSingleton<SceneContext>.Instance.TutorialDirector.SuppressTutorials();
		AnimateIntro();
	}

	public void OnDisable()
	{
		SECTR_AudioSystem.PauseNonUISFX(pause: false);
		SRSingleton<SceneContext>.Instance.PopupDirector.UnregisterSuppressor();
		SRSingleton<SceneContext>.Instance.TutorialDirector.UnsuppressTutorials();
	}

	private void AnimateIntro()
	{
		DOTween.Sequence().PrependInterval(0.25f).Append(introLine1.DOFade(1f, 1f))
			.AppendInterval(1.5f)
			.Append(introLine2.DOFade(1f, 1f))
			.AppendInterval(1.5f)
			.Append(introLine3.DOFade(1f, 1f))
			.AppendInterval(6f)
			.Append(introLine1.DOFade(0f, 1f))
			.Join(introLine2.DOFade(0f, 1f))
			.Join(introLine3.DOFade(0f, 1f))
			.Append(introCanvasGroup.DOFade(0f, 1f))
			.OnComplete(delegate
			{
				endReached = true;
				Close();
			})
			.SetUpdate(isIndependentUpdate: true);
	}

	protected override bool Closeable()
	{
		return endReached;
	}
}

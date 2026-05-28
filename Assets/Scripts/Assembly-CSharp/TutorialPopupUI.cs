using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : PopupUI<TutorialDirector.IdEntry>
{
	public GameObject buttonLinesPanel;

	public TutorialButtonLine[] buttonLines;

	public TMP_Text titleText;

	public TMP_Text introText;

	public ImageCycler imgCycler;

	public Image completedImg;

	[Tooltip("SFX played when the tutorial action is completed. [2D, non-looping]")]
	public SECTR_AudioCue onCompletedCue;

	protected TutorialDirector tutorialDir;

	private Animator anim;

	private bool wasCompleted;

	private const string ANIM_COMPLETE = "Complete";

	private const string ANIM_CLOSE = "Close";

	private MessageBundle tutorialBundle;

	public virtual void Awake()
	{
		tutorialDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
		inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		anim = GetComponent<Animator>();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<GameContext>.Instance != null)
		{
			InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
			inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		}
		if (tutorialDir != null)
		{
			tutorialDir.PopupDeactivated(this, wasCompleted);
		}
	}

	public void Complete()
	{
		wasCompleted = true;
		anim.SetTrigger("Complete");
		StartCoroutine(DestroyDelayed(0.167f));
	}

	public void Hide()
	{
		wasCompleted = false;
		anim.SetTrigger("Close");
		StartCoroutine(DestroyDelayed(0.167f));
	}

	private IEnumerator DestroyDelayed(float delay)
	{
		yield return new WaitForSeconds(delay);
		Destroyer.Destroy(base.gameObject, "TutorialPopupUI.DestroyDelayed");
	}

	public override void Init(TutorialDirector.IdEntry tutorialIdEntry)
	{
		idEntry = tutorialIdEntry;
		base.Init(tutorialIdEntry);
		imgCycler.transform.localPosition += idEntry.imageOffset;
		imgCycler.transform.localScale = idEntry.imageScale;
	}

	private void OnKeysChanged()
	{
		UpdateTutorial();
	}

	public override void OnBundleAvailable(MessageDirector msgDir)
	{
		tutorialBundle = msgDir.GetBundle("tutorial");
		UpdateTutorial();
	}

	private void UpdateTutorial()
	{
		if (tutorialBundle != null)
		{
			string lowerName = Enum.GetName(typeof(TutorialDirector.Id), idEntry.id).ToLowerInvariant();
			UpdateTutorialInfo(lowerName);
			UpdateButtonLines(lowerName);
		}
	}

	private void UpdateTutorialInfo(string lowerName)
	{
		titleText.text = tutorialBundle.Get("t." + lowerName);
		string key = "m.text." + lowerName;
		if (tutorialBundle.Exists(key))
		{
			introText.text = tutorialBundle.Get(key);
			introText.gameObject.SetActive(value: true);
		}
		else
		{
			introText.gameObject.SetActive(value: false);
		}
		imgCycler.SetSprites(idEntry.images);
	}

	private void UpdateButtonLines(string lowerName)
	{
		string text = (InputDirector.UsingGamepad() ? "gamepad." : "");
		for (int i = 0; i < buttonLines.Length; i++)
		{
			int num = i + 1;
			TutorialButtonLine tutorialButtonLine = buttonLines[i];
			string key = "m.input." + text + lowerName + "." + num;
			string key2 = "m.inputdesc." + text + lowerName + "." + num;
			if (tutorialBundle.Exists(key))
			{
				tutorialButtonLine.gameObject.SetActive(value: true);
				tutorialButtonLine.Init(tutorialBundle.Get(key), tutorialBundle.Get(key2));
			}
			else
			{
				tutorialButtonLine.gameObject.SetActive(value: false);
			}
		}
		buttonLinesPanel.SetActive(buttonLines.Length != 0);
	}

	public TutorialDirector.Id GetId()
	{
		return idEntry.id;
	}

	public void CompletedAction()
	{
		completedImg.gameObject.SetActive(value: true);
		if (onCompletedCue != null)
		{
			SECTR_AudioSystem.Play(onCompletedCue, Vector3.zero, loop: false);
			onCompletedCue = null;
		}
	}

	public static GameObject CreateTutorialPopup(TutorialDirector.IdEntry tutorialIdEntry)
	{
		GameObject obj = UnityEngine.Object.Instantiate(SRSingleton<SceneContext>.Instance.TutorialDirector.tutorialPopupPrefab);
		obj.GetComponent<TutorialPopupUI>().Init(tutorialIdEntry);
		return obj;
	}
}

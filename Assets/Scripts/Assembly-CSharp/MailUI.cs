using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailUI : BaseUI
{
	public GameObject contentPanel;

	public GameObject placeholderPanel;

	public GameObject buttonListPanel;

	public TMP_Text selectedFrom;

	public TMP_Text selectedSubj;

	public TMP_Text selectedBody;

	public GameObject buttonListItemPrefab;

	public ScrollRect contentScroll;

	public Sprite mailUnreadIcon;

	public Sprite mailReadIcon;

	public SECTR_AudioCue openCue;

	public SECTR_AudioCue closeCue;

	private MessageBundle mailBundle;

	private MailDirector mailDir;

	private ProgressDirector progressDir;

	public override void Awake()
	{
		base.Awake();
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		mailBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("mail");
		contentScroll.gameObject.SetActive(value: false);
		placeholderPanel.SetActive(value: true);
		foreach (MailDirector.Mail item in mailDir.GetMailRecentFirst())
		{
			AddButton(item);
		}
		Toggle[] componentsInChildren = buttonListPanel.GetComponentsInChildren<Toggle>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			componentsInChildren[0].gameObject.AddComponent<InitSelected>();
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (i < componentsInChildren.Length - 1)
			{
				navigation.selectOnDown = componentsInChildren[i + 1];
			}
			if (i > 0)
			{
				navigation.selectOnUp = componentsInChildren[i - 1];
			}
			componentsInChildren[i].navigation = navigation;
		}
	}

	public void OnEnable()
	{
		Play(openCue);
	}

	public void OnDisable()
	{
		Play(closeCue);
	}

	public void AddButton(MailDirector.Mail mail)
	{
		GameObject obj = CreateButton(mail);
		obj.transform.SetParent(buttonListPanel.transform, worldPositionStays: false);
		obj.GetComponent<Toggle>().group = buttonListPanel.GetComponentInParent<ToggleGroup>();
	}

	private GameObject CreateButton(MailDirector.Mail mail)
	{
		GameObject buttonObj = Object.Instantiate(buttonListItemPrefab);
		Toggle component = buttonObj.GetComponent<Toggle>();
		TMP_Text component2 = buttonObj.transform.Find("Info/From").gameObject.GetComponent<TMP_Text>();
		TMP_Text component3 = buttonObj.transform.Find("Info/Subject").gameObject.GetComponent<TMP_Text>();
		Image iconImg = buttonObj.transform.Find("Icon").gameObject.GetComponent<Image>();
		component2.text = mailBundle.Xlate("m.from." + mail.key);
		component3.text = mailBundle.Xlate("m.subj." + mail.key);
		iconImg.sprite = (mail.read ? mailReadIcon : mailUnreadIcon);
		UnityAction<bool> onButton = delegate(bool isOn)
		{
			if (isOn)
			{
				Select(mail);
			}
			iconImg.sprite = (mail.read ? mailReadIcon : mailUnreadIcon);
		};
		component.onValueChanged.AddListener(onButton);
		OnSelectDelegator.Create(buttonObj, delegate
		{
			onButton(arg0: true);
			buttonObj.GetComponent<Toggle>().isOn = true;
		});
		return buttonObj;
	}

	public void Select(MailDirector.Mail mail)
	{
		contentScroll.gameObject.SetActive(value: true);
		placeholderPanel.SetActive(value: false);
		selectedFrom.text = mailBundle.Xlate("m.from." + mail.key);
		selectedSubj.text = mailBundle.Xlate("m.subj." + mail.key);
		selectedBody.text = mailBundle.Xlate("m.body." + mail.key);
		StartCoroutine(ScrollAtEndOfFrame());
		if (!mail.read && mail.key.StartsWith("casey_"))
		{
			progressDir.QueueRanchWistfulMusic();
		}
		if (!mail.read && mail.key == "casey_11")
		{
			progressDir.QueueCredits();
		}
		mailDir.MarkRead(mail);
		if (mail.key == RanchDirector.PARTNER_MAIL_KEY)
		{
			SRSingleton<SceneContext>.Instance.PediaDirector.UnlockWithoutPopup(PediaDirector.Id.PARTNER);
			if (!progressDir.HasProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER_UNLOCK))
			{
				progressDir.AddProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER_UNLOCK);
			}
		}
		else if (mail.key == "hobson_1" && !progressDir.HasProgress(ProgressDirector.ProgressType.HOBSON_END_UNLOCK))
		{
			progressDir.AddProgress(ProgressDirector.ProgressType.HOBSON_END_UNLOCK);
		}
	}

	public IEnumerator ScrollAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		contentScroll.verticalNormalizedPosition = 1f;
	}
}

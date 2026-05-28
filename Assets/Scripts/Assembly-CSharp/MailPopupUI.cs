using TMPro;
using UnityEngine;

public class MailPopupUI : PopupUI<MailDirector.Mail>, PopupDirector.Popup
{
	[Tooltip("If not killed before then, how long this popup will stick around.")]
	public float lifetime = 10f;

	protected float timeOfDeath;

	protected PopupDirector popupDir;

	public virtual void Awake()
	{
		timeOfDeath = Time.time + lifetime;
		popupDir = SRSingleton<SceneContext>.Instance.PopupDirector;
		popupDir.PopupActivated(this);
	}

	public override void OnBundleAvailable(MessageDirector msgDir)
	{
		TMP_Text component = base.transform.Find("UIContainer/MainPanel/IntroPanel/Intro").GetComponent<TMP_Text>();
		MessageBundle bundle = msgDir.GetBundle("mail");
		MessageBundle bundle2 = msgDir.GetBundle("ui");
		string key = idEntry.key;
		component.text = bundle2.Get("m.mail_from_wrap", bundle.Get("m.from." + key));
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		popupDir.PopupDeactivated(this);
	}

	public void Update()
	{
		if (Time.time >= timeOfDeath)
		{
			Destroyer.Destroy(base.gameObject, "MailPopupUI.Update");
		}
	}

	public MailDirector.Mail GetId()
	{
		return idEntry;
	}

	public bool ShouldClear()
	{
		return idEntry.read;
	}

	public static GameObject CreateMailPopup(MailDirector.Mail mail)
	{
		GameObject obj = Object.Instantiate(SRSingleton<GameContext>.Instance.UITemplates.mailPrefab);
		obj.GetComponent<MailPopupUI>().Init(mail);
		return obj;
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PediaPopupUI : PopupUI<PediaDirector.IdEntry>, PopupDirector.Popup
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
		TMP_Text component = base.transform.Find("UIContainer/MainPanel/TitlePanel/Title").GetComponent<TMP_Text>();
		TMP_Text component2 = base.transform.Find("UIContainer/MainPanel/IntroPanel/Intro").GetComponent<TMP_Text>();
		Image component3 = base.transform.Find("UIContainer/MainPanel/EntryImage").GetComponent<Image>();
		MessageBundle bundle = msgDir.GetBundle("pedia");
		string text = Enum.GetName(typeof(PediaDirector.Id), idEntry.id).ToLowerInvariant();
		component.text = bundle.Get("t." + text);
		component2.text = bundle.Get("m.intro." + text);
		component3.sprite = idEntry.icon;
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
			Destroyer.Destroy(base.gameObject, "PediaPopupUI.Update");
		}
	}

	public void OpenPediaEntry()
	{
		SRSingleton<SceneContext>.Instance.PediaDirector.ShowPedia(idEntry.id);
	}

	public PediaDirector.Id GetId()
	{
		return idEntry.id;
	}

	public bool ShouldClear()
	{
		return false;
	}

	public static GameObject CreatePediaPopup(PediaDirector.IdEntry pediaIdEntry)
	{
		GameObject obj = UnityEngine.Object.Instantiate(SRSingleton<SceneContext>.Instance.PediaDirector.pediaPopupPrefab);
		obj.GetComponent<PediaPopupUI>().Init(pediaIdEntry);
		return obj;
	}
}

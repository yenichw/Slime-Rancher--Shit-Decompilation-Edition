using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintPopupUI : PopupUI<GadgetDefinition>, PopupDirector.Popup
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
		string text = Enum.GetName(typeof(Gadget.Id), idEntry.id).ToLowerInvariant();
		component.text = bundle.Get("m.gadget.name." + text);
		component2.text = bundle.Get("m.gadget.desc." + text);
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
			Destroyer.Destroy(base.gameObject, "BlueprintPopupUI.Update");
		}
	}

	public Gadget.Id GetId()
	{
		return idEntry.id;
	}

	public bool ShouldClear()
	{
		return false;
	}

	public static GameObject CreateBlueprintPopup(GadgetDefinition gadgetDefinition)
	{
		GameObject obj = UnityEngine.Object.Instantiate(SRSingleton<SceneContext>.Instance.GadgetDirector.gadgetPopupPrefab);
		obj.GetComponent<BlueprintPopupUI>().Init(gadgetDefinition);
		return obj;
	}
}

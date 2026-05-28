using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvailUpgradePopupUI : PopupUI<PlayerState.Upgrade>, PopupDirector.Popup
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
		string text = Enum.GetName(typeof(PlayerState.Upgrade), idEntry).ToLowerInvariant();
		component.text = bundle.Get("m.upgrade.name.personal." + text);
		component2.text = bundle.Get("m.upgrade.desc.personal." + text);
		component3.sprite = SRSingleton<GameContext>.Instance.LookupDirector.GetUpgradeDefinition(idEntry).Icon;
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
			Destroyer.Destroy(base.gameObject, "AvailUpgradePopupUI.Update");
		}
	}

	public PlayerState.Upgrade GetId()
	{
		return idEntry;
	}

	public bool ShouldClear()
	{
		return false;
	}

	public static GameObject CreateAvailUpgradePopup(PlayerState.Upgrade upgrade)
	{
		GameObject obj = UnityEngine.Object.Instantiate(SRSingleton<GameContext>.Instance.UITemplates.availUpgradePrefab);
		obj.GetComponent<AvailUpgradePopupUI>().Init(upgrade);
		return obj;
	}
}

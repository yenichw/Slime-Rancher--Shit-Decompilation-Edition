using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefineryUI : BaseUI
{
	[Tooltip("Internal inventory content panel")]
	public GameObject inventoryGridPanel;

	public GameObject inventoryEntryPrefab;

	public Identifiable.Id[] listedItems;

	public Sprite lockedIcon;

	private MessageBundle actorBundle;

	private const int MIN_ENTRIES = 15;

	public override void Awake()
	{
		base.Awake();
		actorBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("actor");
		GadgetDirector gadgetDirector = SRSingleton<SceneContext>.Instance.GadgetDirector;
		PediaDirector pediaDirector = SRSingleton<SceneContext>.Instance.PediaDirector;
		int num = 0;
		Identifiable.Id[] array = listedItems;
		foreach (Identifiable.Id id in array)
		{
			int refineryCount = gadgetDirector.GetRefineryCount(id);
			Identifiable.Id id2 = id;
			PediaDirector.Id? pediaId = pediaDirector.GetPediaId(Identifiable.IsPlort(id) ? PlortToSlime(id) : id);
			if (refineryCount == 0 && pediaId.HasValue && !pediaDirector.IsUnlocked(pediaId.Value))
			{
				id2 = Identifiable.Id.NONE;
			}
			AddInventory(id2, refineryCount);
			num++;
		}
		for (int j = num; j < 15; j++)
		{
			AddEmptyInventory();
		}
	}

	private Identifiable.Id PlortToSlime(Identifiable.Id plortId)
	{
		string text = plortId.ToString();
		text = text.Replace("_PLORT", "_SLIME");
		return (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), text);
	}

	private void AddInventory(Identifiable.Id id, int count)
	{
		CreateInventoryEntry(id, count).transform.SetParent(inventoryGridPanel.transform, worldPositionStays: false);
	}

	private void AddEmptyInventory()
	{
		CreateEmptyInventoryEntry().transform.SetParent(inventoryGridPanel.transform, worldPositionStays: false);
	}

	private GameObject CreateInventoryEntry(Identifiable.Id id, int count)
	{
		GameObject obj = UnityEngine.Object.Instantiate(inventoryEntryPrefab);
		TMP_Text component = obj.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
		Image component2 = obj.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
		TMP_Text component3 = obj.transform.Find("CountsOuterPanel/CountsPanel/Counts").gameObject.GetComponent<TMP_Text>();
		if (id == Identifiable.Id.NONE)
		{
			component.text = actorBundle.Xlate(MessageUtil.Qualify("pedia", "t.locked"));
			component2.sprite = lockedIcon;
		}
		else
		{
			Sprite icon = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(id);
			component.text = actorBundle.Xlate("l." + id.ToString().ToLowerInvariant());
			component2.sprite = icon;
		}
		component3.text = ((count > 999) ? $"{999}+" : count.ToString());
		return obj;
	}

	private GameObject CreateEmptyInventoryEntry()
	{
		GameObject obj = UnityEngine.Object.Instantiate(inventoryEntryPrefab);
		TMP_Text component = obj.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
		Image component2 = obj.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
		TMP_Text component3 = obj.transform.Find("CountsOuterPanel/CountsPanel/Counts").gameObject.GetComponent<TMP_Text>();
		component.text = "";
		component2.enabled = false;
		component3.text = "";
		return obj;
	}
}

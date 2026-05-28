using System;
using System.Collections.Generic;
using UnityEngine;

public class FabricateGadgetUI : BaseUI
{
	public Sprite titleIcon;

	private PurchaseUI purchaseUI;

	private Dictionary<string, string> categoryMap = new Dictionary<string, string>();

	private const string ERR_INSUF_CRAFT_RESOURCES = "e.insuf_craft_resources";

	private const string ERR_CANNOT_ADD_GADGET = "e.cannot_add_gagdget";

	public override void Awake()
	{
		base.Awake();
		BuildUI();
		SRSingleton<SceneContext>.Instance.TutorialDirector.OnFabricatorOpen();
	}

	public void BuildUI()
	{
		if (purchaseUI != null && purchaseUI.gameObject != null)
		{
			Destroyer.Destroy(purchaseUI.gameObject, "FabricateGadgetUI.BuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		purchaseUI = gameObject.GetComponent<PurchaseUI>();
		statusArea = purchaseUI.statusArea;
	}

	protected GameObject CreatePurchaseUI()
	{
		categoryMap.Clear();
		GadgetDirector gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
		List<PurchaseUI.Purchasable> list = new List<PurchaseUI.Purchasable>();
		Dictionary<PediaDirector.Id, List<PurchaseUI.Purchasable>> dictionary = new Dictionary<PediaDirector.Id, List<PurchaseUI.Purchasable>>();
		foreach (GadgetDefinition gadgetDefinition in SRSingleton<GameContext>.Instance.LookupDirector.GadgetDefinitions)
		{
			string text = Enum.GetName(typeof(Gadget.Id), gadgetDefinition.id).ToLowerInvariant();
			GadgetDefinition finalDefinition = gadgetDefinition;
			Gadget.Id finalId = gadgetDefinition.id;
			string descKey = "m.gadget.desc." + text;
			string text2 = "m.gadget.name." + text;
			PurchaseUI.Purchasable item = new PurchaseUI.Purchasable(text2, gadgetDefinition.icon, gadgetDefinition.icon, descKey, 0, gadgetDefinition.pediaLink, delegate
			{
				Fabricate(finalId);
			}, () => gadgetDir.HasBlueprint(finalId), () => gadgetDir.CanAddGadget(finalDefinition), null, null, () => gadgetDir.GetGadgetCount(finalId), gadgetDefinition.craftCosts);
			list.Add(item);
			categoryMap[text2] = gadgetDefinition.pediaLink.ToString().ToLowerInvariant();
			List<PurchaseUI.Purchasable> list2 = dictionary.Get(gadgetDefinition.pediaLink);
			if (list2 == null)
			{
				list2 = new List<PurchaseUI.Purchasable>();
			}
			list2.Add(item);
			dictionary[gadgetDefinition.pediaLink] = list2;
		}
		GameObject gameObject = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.fabricate_gadget"), list.ToArray(), hideNubuckCost: true, Close);
		List<PurchaseUI.Category> list3 = new List<PurchaseUI.Category>();
		PediaDirector.Id[] sCIENCE_ENTRIES = PediaUI.SCIENCE_ENTRIES;
		for (int i = 0; i < sCIENCE_ENTRIES.Length; i++)
		{
			PediaDirector.Id key = sCIENCE_ENTRIES[i];
			if (dictionary.ContainsKey(key))
			{
				list3.Add(new PurchaseUI.Category(key.ToString().ToLowerInvariant(), dictionary[key].ToArray()));
			}
		}
		gameObject.GetComponent<PurchaseUI>().SetCategories(list3);
		gameObject.GetComponent<PurchaseUI>().SetPurchaseMsgs("b.fabricate", "b.sold_out");
		return gameObject;
	}

	public void Fabricate(Gadget.Id id)
	{
		GadgetDirector gadgetDirector = SRSingleton<SceneContext>.Instance.GadgetDirector;
		AchievementsDirector achievementsDirector = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		GadgetDefinition gadgetDefinition = SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id);
		if (!gadgetDirector.CanAddGadget(gadgetDefinition))
		{
			PlayErrorCue();
			Error("e.cannot_add_gagdget");
		}
		else if (TrySpendResources(gadgetDefinition.craftCosts))
		{
			ClearStatus();
			PlayPurchaseCue();
			gadgetDirector.AddGadget(id);
			achievementsDirector.AddToStat(AchievementsDirector.GameIntStat.FABRICATED_GADGETS, 1);
			if (gadgetDefinition.buyInPairs)
			{
				gadgetDirector.AddGadget(id);
			}
			AnalyticsUtil.CustomEvent("Fabricate", new Dictionary<string, object> { 
			{
				"id",
				id.ToString()
			} });
			purchaseUI.PlayPurchaseFX();
			purchaseUI.Rebuild(unavailInMainList: false);
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_craft_resources");
		}
	}

	private bool TrySpendResources(GadgetDefinition.CraftCost[] costs)
	{
		return SRSingleton<SceneContext>.Instance.GadgetDirector.TryToSpendFromRefinery(costs);
	}

	protected void PlayPurchaseCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseBlueprintCue);
	}
}

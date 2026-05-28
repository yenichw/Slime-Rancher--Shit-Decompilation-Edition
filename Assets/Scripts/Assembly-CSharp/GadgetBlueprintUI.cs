using System;
using System.Collections.Generic;
using UnityEngine;

public class GadgetBlueprintUI : BaseUI
{
	public Sprite titleIcon;

	public override void Awake()
	{
		base.Awake();
		RebuildUI();
		SRSingleton<SceneContext>.Instance.TutorialDirector.OnBuilderShopOpen();
	}

	public void RebuildUI()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Destroyer.Destroy(base.transform.GetChild(i).gameObject, "GadgetBlueprintUI.RebuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		statusArea = gameObject.GetComponent<PurchaseUI>().statusArea;
	}

	protected GameObject CreatePurchaseUI()
	{
		GadgetDirector gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
		List<PurchaseUI.Purchasable> list = new List<PurchaseUI.Purchasable>();
		foreach (GadgetDefinition entry in SRSingleton<GameContext>.Instance.LookupDirector.GadgetDefinitions)
		{
			string arg = Enum.GetName(typeof(Gadget.Id), entry.id).ToLowerInvariant();
			list.Add(new PurchaseUI.Purchasable($"m.gadget.name.{arg}", descKey: $"m.gadget.desc.{arg}", icon: entry.icon, mainImg: entry.icon, cost: entry.blueprintCost, pediaId: entry.pediaLink, onPurchase: delegate
			{
				BuyBlueprint(entry.id);
			}, unlocked: () => gadgetDir.HasBlueprint(entry.id) || gadgetDir.IsBlueprintUnlocked(entry.id), avail: () => !gadgetDir.HasBlueprint(entry.id)));
		}
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.purchase_blueprint"), list.ToArray(), hideNubuckCost: false, Close);
	}

	public void BuyBlueprint(Gadget.Id id)
	{
		PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		GadgetDefinition gadgetDefinition = SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id);
		if (playerState.GetCurrency() >= gadgetDefinition.blueprintCost)
		{
			playerState.SpendCurrency(gadgetDefinition.blueprintCost);
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(id);
			PlayPurchaseCue();
			Close();
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_coins");
		}
	}

	protected void PlayPurchaseCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseBlueprintCue);
	}
}

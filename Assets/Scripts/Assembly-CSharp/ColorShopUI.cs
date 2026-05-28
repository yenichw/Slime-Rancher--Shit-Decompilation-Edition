using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorShopUI : BaseUI
{
	public Sprite titleIcon;

	public SECTR_AudioCue selectCue;

	public RanchDirector.PaletteType[] paletteTypes;

	private PurchaseUI purchaseUI;

	private AchievementsDirector achieveDir;

	public override void Awake()
	{
		base.Awake();
		SRSingleton<SceneContext>.Instance.PediaDirector.UnlockWithoutPopup(PediaDirector.Id.CHROMA);
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		BuildUI();
	}

	public void BuildUI()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Destroyer.Destroy(base.transform.GetChild(i).gameObject, "ColorShopUI.BuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		purchaseUI = gameObject.GetComponent<PurchaseUI>();
		statusArea = purchaseUI.statusArea;
	}

	protected GameObject CreatePurchaseUI()
	{
		RanchDirector ranchDirector = SRSingleton<SceneContext>.Instance.RanchDirector;
		List<PurchaseUI.Purchasable> list = new List<PurchaseUI.Purchasable>();
		List<RanchDirector.PaletteEntry> orderedPalettes = SRSingleton<SceneContext>.Instance.RanchDirector.GetOrderedPalettes();
		List<PurchaseUI.Category> list2 = new List<PurchaseUI.Category>();
		RanchDirector.PaletteType[] array = paletteTypes;
		foreach (RanchDirector.PaletteType paletteType in array)
		{
			PurchaseUI.Purchasable[] array2 = new PurchaseUI.Purchasable[orderedPalettes.Count];
			for (int j = 0; j < orderedPalettes.Count; j++)
			{
				RanchDirector.PaletteEntry entry = orderedPalettes[j];
				PurchaseUI.Purchasable purchasable = CreatePurchasable(ranchDirector, entry, paletteType);
				list.Add(purchasable);
				array2[j] = purchasable;
			}
			string text = Enum.GetName(typeof(RanchDirector.PaletteType), paletteType).ToLowerInvariant();
			list2.Add(new PurchaseUI.Category(text, array2));
		}
		GameObject obj = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.chroma_packs"), list.ToArray(), hideNubuckCost: true, Close, unavailInMainList: true);
		obj.GetComponent<PurchaseUI>().SetCategories(list2);
		obj.GetComponent<PurchaseUI>().SetPurchaseMsgs("b.select", "b.already_selected");
		return obj;
	}

	private PurchaseUI.Purchasable CreatePurchasable(RanchDirector ranchDir, RanchDirector.PaletteEntry entry, RanchDirector.PaletteType paletteType)
	{
		string text = Enum.GetName(typeof(RanchDirector.Palette), entry.palette).ToLowerInvariant();
		RanchDirector.Palette finalPalette = entry.palette;
		return new PurchaseUI.Purchasable("m.palette.name." + text, entry.icon, entry.icon, "m.palette.desc", 0, null, delegate
		{
			SelectPalette(paletteType, finalPalette);
		}, () => ranchDir.HasPalette(finalPalette), () => !ranchDir.IsSelectedPalette(finalPalette, paletteType));
	}

	private void SelectPalette(RanchDirector.PaletteType paletteType, RanchDirector.Palette palette)
	{
		SRSingleton<SceneContext>.Instance.GameModel.GetRanchModel().SelectPalette(paletteType, palette);
		Play(selectCue);
		purchaseUI.PlayPurchaseFX();
		purchaseUI.Rebuild(unavailInMainList: true);
		achieveDir.AddToStat(AchievementsDirector.EnumStat.USE_CHROMAS, paletteType);
	}

	protected void PlayPurchaseCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseCue);
	}
}

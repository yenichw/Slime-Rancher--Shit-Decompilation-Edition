using UnityEngine;

public class GardenUI : LandPlotUI
{
	public UpgradePurchaseItem soil;

	public UpgradePurchaseItem sprinkler;

	public UpgradePurchaseItem scareslime;

	public UpgradePurchaseItem miracleMix;

	public UpgradePurchaseItem deluxe;

	public PurchaseItem clearCrop;

	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	public GameObject plantButtonPanelObject;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[7]
		{
			new PurchaseUI.Purchasable("m.upgrade.name.garden.soil", soil.icon, soil.img, "m.upgrade.desc.garden.soil", soil.cost, PediaDirector.Id.GARDEN, UpgradeSoil, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.SOIL)),
			new PurchaseUI.Purchasable("m.upgrade.name.garden.sprinkler", sprinkler.icon, sprinkler.img, "m.upgrade.desc.garden.sprinkler", sprinkler.cost, PediaDirector.Id.GARDEN, UpgradeSprinkler, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.SPRINKLER)),
			new PurchaseUI.Purchasable("m.upgrade.name.garden.scareslime", scareslime.icon, scareslime.img, "m.upgrade.desc.garden.scareslime", scareslime.cost, PediaDirector.Id.GARDEN, UpgradeScareslime, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.SCARESLIME)),
			new PurchaseUI.Purchasable("m.upgrade.name.garden.miracle_mix", miracleMix.icon, miracleMix.img, "m.upgrade.desc.garden.miracle_mix", miracleMix.cost, PediaDirector.Id.GARDEN, UpgradeMiracleMix, () => SRSingleton<SceneContext>.Instance.ProgressDirector.GetProgress(ProgressDirector.ProgressType.OGDEN_REWARDS) >= 1, () => !activator.HasUpgrade(LandPlot.Upgrade.MIRACLE_MIX)),
			new PurchaseUI.Purchasable("m.upgrade.name.garden.deluxe", deluxe.icon, deluxe.img, "m.upgrade.desc.garden.deluxe", deluxe.cost, PediaDirector.Id.GARDEN, UpgradeDeluxe, () => SRSingleton<SceneContext>.Instance.ProgressDirector.GetProgress(ProgressDirector.ProgressType.OGDEN_REWARDS) >= 2, () => !activator.HasUpgrade(LandPlot.Upgrade.DELUXE_GARDEN)),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "b.clear_crop"), clearCrop.icon, clearCrop.img, MessageUtil.Qualify("ui", "m.desc.clear_crop"), clearCrop.cost, null, ClearCrop, () => activator.HasAttached(), () => true),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish")
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.garden", purchasables, hideNubuckCost: false, Close);
	}

	public void UpgradeSoil()
	{
		Upgrade(LandPlot.Upgrade.SOIL, soil.cost);
	}

	public void UpgradeSprinkler()
	{
		Upgrade(LandPlot.Upgrade.SPRINKLER, sprinkler.cost);
	}

	public void UpgradeScareslime()
	{
		Upgrade(LandPlot.Upgrade.SCARESLIME, scareslime.cost);
	}

	public void UpgradeMiracleMix()
	{
		Upgrade(LandPlot.Upgrade.MIRACLE_MIX, miracleMix.cost);
	}

	public void UpgradeDeluxe()
	{
		Upgrade(LandPlot.Upgrade.DELUXE_GARDEN, deluxe.cost);
	}

	public void ClearCrop()
	{
		if (playerState.GetCurrency() >= clearCrop.cost)
		{
			playerState.SpendCurrency(clearCrop.cost);
			activator.DestroyAttached();
			PlayPurchaseCue();
			Close();
		}
		else
		{
			Error("e.insuf_coins");
		}
	}

	public void Demolish()
	{
		if (playerState.GetCurrency() >= demolish.cost)
		{
			playerState.SpendCurrency(demolish.cost);
			Replace(demolish.plotPrefab);
			PlayPurchaseCue();
		}
		else
		{
			Error("e.insuf_coins");
		}
	}
}

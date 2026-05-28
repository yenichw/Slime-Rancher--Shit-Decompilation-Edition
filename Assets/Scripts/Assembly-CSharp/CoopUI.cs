using UnityEngine;

public class CoopUI : LandPlotUI
{
	public UpgradePurchaseItem walls;

	public UpgradePurchaseItem feeder;

	public UpgradePurchaseItem vitamizer;

	public UpgradePurchaseItem deluxe;

	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[5]
		{
			new PurchaseUI.Purchasable("m.upgrade.name.coop.walls", walls.icon, walls.img, "m.upgrade.desc.coop.walls", walls.cost, PediaDirector.Id.COOP, UpgradeWalls, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.WALLS)),
			new PurchaseUI.Purchasable("m.upgrade.name.coop.feeder", feeder.icon, feeder.img, "m.upgrade.desc.coop.feeder", feeder.cost, PediaDirector.Id.COOP, UpgradeFeeder, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.FEEDER)),
			new PurchaseUI.Purchasable("m.upgrade.name.coop.vitamizer", vitamizer.icon, vitamizer.img, "m.upgrade.desc.coop.vitamizer", vitamizer.cost, PediaDirector.Id.COOP, UpgradeVitamizer, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.VITAMIZER)),
			new PurchaseUI.Purchasable("m.upgrade.name.coop.deluxe", deluxe.icon, deluxe.img, "m.upgrade.desc.coop.deluxe", deluxe.cost, PediaDirector.Id.COOP, UpgradeDeluxe, () => SRSingleton<SceneContext>.Instance.ProgressDirector.GetProgress(ProgressDirector.ProgressType.MOCHI_REWARDS) >= 2, () => !activator.HasUpgrade(LandPlot.Upgrade.DELUXE_COOP)),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish")
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.coop", purchasables, hideNubuckCost: false, Close);
	}

	public void UpgradeWalls()
	{
		Upgrade(LandPlot.Upgrade.WALLS, walls.cost);
	}

	public void UpgradeFeeder()
	{
		Upgrade(LandPlot.Upgrade.FEEDER, feeder.cost);
	}

	public void UpgradeVitamizer()
	{
		Upgrade(LandPlot.Upgrade.VITAMIZER, vitamizer.cost);
	}

	public void UpgradeDeluxe()
	{
		Upgrade(LandPlot.Upgrade.DELUXE_COOP, deluxe.cost);
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

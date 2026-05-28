using UnityEngine;

public class IncineratorUI : LandPlotUI
{
	public UpgradePurchaseItem ashTrough;

	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[2]
		{
			new PurchaseUI.Purchasable("m.upgrade.name.incinerator.ash_trough", ashTrough.icon, ashTrough.img, "m.upgrade.desc.incinerator.ash_trough", ashTrough.cost, PediaDirector.Id.CORRAL, UpgradeAshTrough, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.ASH_TROUGH)),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish")
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.incinerator", purchasables, hideNubuckCost: false, Close);
	}

	public void UpgradeAshTrough()
	{
		Upgrade(LandPlot.Upgrade.ASH_TROUGH, ashTrough.cost);
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

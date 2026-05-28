using UnityEngine;

public class SiloUI : LandPlotUI
{
	public UpgradePurchaseItem storage2;

	public UpgradePurchaseItem storage3;

	public UpgradePurchaseItem storage4;

	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[4]
		{
			new PurchaseUI.Purchasable("m.upgrade.name.silo.storage2", storage2.icon, storage2.img, "m.upgrade.desc.silo.storage2", storage2.cost, PediaDirector.Id.SILO, UpgradeStorage2, () => !activator.HasUpgrade(LandPlot.Upgrade.STORAGE2), () => true),
			new PurchaseUI.Purchasable("m.upgrade.name.silo.storage2", storage3.icon, storage3.img, "m.upgrade.desc.silo.storage2", storage3.cost, PediaDirector.Id.SILO, UpgradeStorage3, () => activator.HasUpgrade(LandPlot.Upgrade.STORAGE2) && !activator.HasUpgrade(LandPlot.Upgrade.STORAGE3), () => true),
			new PurchaseUI.Purchasable("m.upgrade.name.silo.storage2", storage4.icon, storage4.img, "m.upgrade.desc.silo.storage2", storage4.cost, PediaDirector.Id.SILO, UpgradeStorage4, () => activator.HasUpgrade(LandPlot.Upgrade.STORAGE3), () => !activator.HasUpgrade(LandPlot.Upgrade.STORAGE4)),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish", activator.GetComponent<SiloStorage>().GetRelevantAmmo().IsEmpty() ? null : "w.destroying_silo_destroys_contents", null, null, requireHoldToPurchase: true)
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.silo", purchasables, hideNubuckCost: false, Close);
	}

	public void UpgradeStorage2()
	{
		Upgrade(LandPlot.Upgrade.STORAGE2, storage2.cost);
	}

	public void UpgradeStorage3()
	{
		Upgrade(LandPlot.Upgrade.STORAGE3, storage3.cost);
	}

	public void UpgradeStorage4()
	{
		Upgrade(LandPlot.Upgrade.STORAGE4, storage4.cost);
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

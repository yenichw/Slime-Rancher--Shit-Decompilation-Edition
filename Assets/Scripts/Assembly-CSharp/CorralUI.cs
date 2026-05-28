using UnityEngine;

public class CorralUI : LandPlotUI
{
	public UpgradePurchaseItem walls;

	public UpgradePurchaseItem musicBox;

	public UpgradePurchaseItem airNet;

	public UpgradePurchaseItem solarShield;

	public UpgradePurchaseItem plortCollector;

	public UpgradePurchaseItem feeder;

	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[7]
		{
			new PurchaseUI.Purchasable("m.upgrade.name.corral.walls", walls.icon, walls.img, "m.upgrade.desc.corral.walls", walls.cost, PediaDirector.Id.CORRAL, UpgradeWalls, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.WALLS)),
			new PurchaseUI.Purchasable("m.upgrade.name.corral.music_box", musicBox.icon, musicBox.img, "m.upgrade.desc.corral.music_box", musicBox.cost, PediaDirector.Id.CORRAL, UpgradeMusicBox, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.MUSIC_BOX)),
			new PurchaseUI.Purchasable("m.upgrade.name.corral.air_net", airNet.icon, airNet.img, "m.upgrade.desc.corral.air_net", airNet.cost, PediaDirector.Id.CORRAL, UpgradeAirNet, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.AIR_NET)),
			new PurchaseUI.Purchasable("m.upgrade.name.corral.solar_shield", solarShield.icon, solarShield.img, "m.upgrade.desc.corral.solar_shield", solarShield.cost, PediaDirector.Id.CORRAL, UpgradeSolarShield, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.SOLAR_SHIELD)),
			new PurchaseUI.Purchasable("m.upgrade.name.corral.plort_collector", plortCollector.icon, plortCollector.img, "m.upgrade.desc.corral.plort_collector", plortCollector.cost, PediaDirector.Id.CORRAL, UpgradePlortCollector, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.PLORT_COLLECTOR)),
			new PurchaseUI.Purchasable("m.upgrade.name.corral.feeder", feeder.icon, feeder.img, "m.upgrade.desc.corral.feeder", feeder.cost, PediaDirector.Id.CORRAL, UpgradeFeeder, () => true, () => !activator.HasUpgrade(LandPlot.Upgrade.FEEDER)),
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => AllowDemolish(), "b.demolish", AllowDemolish() ? null : "w.cannot_demolish_corral_tutorial")
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.corral", purchasables, hideNubuckCost: false, Close);
	}

	private bool AllowDemolish()
	{
		TutorialDirector tutorialDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		_ = SRSingleton<GameContext>.Instance.OptionsDirector;
		if (!tutorialDirector.IsCompletedOrDisabled(TutorialDirector.Id.SHOOTING))
		{
			return SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser;
		}
		return true;
	}

	public void UpgradeWalls()
	{
		Upgrade(LandPlot.Upgrade.WALLS, walls.cost);
	}

	public void UpgradeMusicBox()
	{
		Upgrade(LandPlot.Upgrade.MUSIC_BOX, musicBox.cost);
	}

	public void UpgradeAirNet()
	{
		Upgrade(LandPlot.Upgrade.AIR_NET, airNet.cost);
	}

	public void UpgradeSolarShield()
	{
		Upgrade(LandPlot.Upgrade.SOLAR_SHIELD, solarShield.cost);
	}

	public void UpgradePlortCollector()
	{
		Upgrade(LandPlot.Upgrade.PLORT_COLLECTOR, plortCollector.cost);
	}

	public void UpgradeFeeder()
	{
		Upgrade(LandPlot.Upgrade.FEEDER, feeder.cost);
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

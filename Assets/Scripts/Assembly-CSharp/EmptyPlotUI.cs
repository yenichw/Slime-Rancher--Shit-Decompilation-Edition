using UnityEngine;

public class EmptyPlotUI : LandPlotUI
{
	[Tooltip("The icon we show next to the overall title for the UI")]
	public Sprite titleIcon;

	[Tooltip("Specifies our info for the corral item")]
	public PlotPurchaseItem corral;

	[Tooltip("Specifies our info for the garden item")]
	public PlotPurchaseItem garden;

	[Tooltip("Specifies our info for the coop item")]
	public PlotPurchaseItem coop;

	[Tooltip("Specifies our info for the silo item")]
	public PlotPurchaseItem silo;

	[Tooltip("Specifies our info for the incinerator item")]
	public PlotPurchaseItem incinerator;

	[Tooltip("Specifies our info for the pond item")]
	public PlotPurchaseItem pond;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[6]
		{
			new PurchaseUI.Purchasable("t.corral", corral.icon, corral.img, "m.intro.corral", corral.cost, PediaDirector.Id.CORRAL, BuyCorral, () => true, () => true),
			new PurchaseUI.Purchasable("t.garden", garden.icon, garden.img, "m.intro.garden", garden.cost, PediaDirector.Id.GARDEN, BuyGarden, () => true, () => true),
			new PurchaseUI.Purchasable("t.coop", coop.icon, coop.img, "m.intro.coop", coop.cost, PediaDirector.Id.COOP, BuyCoop, () => true, () => true),
			new PurchaseUI.Purchasable("t.silo", silo.icon, silo.img, "m.intro.silo", silo.cost, PediaDirector.Id.SILO, BuySilo, () => true, () => true),
			new PurchaseUI.Purchasable("t.incinerator", incinerator.icon, incinerator.img, "m.intro.incinerator", incinerator.cost, PediaDirector.Id.INCINERATOR, BuyIncinerator, () => true, () => true),
			new PurchaseUI.Purchasable("t.pond", pond.icon, pond.img, "m.intro.pond", pond.cost, PediaDirector.Id.POND, BuyPond, () => true, () => true)
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.empty_plot"), purchasables, hideNubuckCost: false, Close);
	}

	public void BuyCorral()
	{
		BuyPlot(corral);
	}

	public void BuyGarden()
	{
		if (BuyPlot(garden))
		{
			SRSingleton<SceneContext>.Instance.TutorialDirector.MaybeShowPopup(TutorialDirector.Id.GARDEN);
		}
	}

	public void BuyCoop()
	{
		BuyPlot(coop);
	}

	public void BuySilo()
	{
		BuyPlot(silo);
	}

	public void BuyIncinerator()
	{
		BuyPlot(incinerator);
	}

	public void BuyPond()
	{
		BuyPlot(pond);
	}
}

using UnityEngine;

public class PondUI : LandPlotUI
{
	public PlotPurchaseItem demolish;

	public Sprite titleIcon;

	protected override GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] purchasables = new PurchaseUI.Purchasable[1]
		{
			new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_plot"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_plot"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish")
		};
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, "t.pond", purchasables, hideNubuckCost: false, Close);
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

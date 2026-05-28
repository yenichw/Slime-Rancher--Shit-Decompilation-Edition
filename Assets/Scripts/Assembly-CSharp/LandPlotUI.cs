using System;
using UnityEngine;

public abstract class LandPlotUI : BaseUI
{
	[Serializable]
	public class PurchaseItem
	{
		public Sprite icon;

		public Sprite img;

		public int cost;
	}

	[Serializable]
	public class PlotPurchaseItem : PurchaseItem
	{
		public GameObject plotPrefab;
	}

	[Serializable]
	public class UpgradePurchaseItem : PurchaseItem
	{
		public LandPlot.Upgrade upgrade;
	}

	protected LandPlot activator;

	protected PlayerState playerState;

	private PurchaseUI purchaseUI;

	private const string ERR_ALREADY_HAS_UPGRADE = "e.already_has_upgrade";

	public override void Awake()
	{
		base.Awake();
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	protected abstract GameObject CreatePurchaseUI();

	public virtual void SetActivator(LandPlot activator)
	{
		this.activator = activator;
		RebuildUI();
	}

	public void RebuildUI()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Destroyer.Destroy(base.transform.GetChild(i).gameObject, "LandPlotUI.RebuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		purchaseUI = gameObject.GetComponent<PurchaseUI>();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		statusArea = gameObject.GetComponent<PurchaseUI>().statusArea;
	}

	protected GameObject Replace(GameObject replacementPrefab)
	{
		GameObject result = activator.transform.parent.GetComponent<LandPlotLocation>().Replace(activator, replacementPrefab);
		Close();
		return result;
	}

	protected void Upgrade(LandPlot.Upgrade upgrade, int cost)
	{
		if (activator.HasUpgrade(upgrade))
		{
			Error("e.already_has_upgrade");
		}
		else if (playerState.GetCurrency() >= cost)
		{
			playerState.SpendCurrency(cost);
			activator.AddUpgrade(upgrade);
			PlayPurchaseUpgradeCue();
			RebuildUI();
			purchaseUI.PlayPurchaseFX();
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_coins");
		}
	}

	protected bool BuyPlot(PlotPurchaseItem plot)
	{
		if (playerState.GetCurrency() >= plot.cost)
		{
			playerState.SpendCurrency(plot.cost);
			PlayPurchaseCue();
			Replace(plot.plotPrefab);
			return true;
		}
		PlayErrorCue();
		Error("e.insuf_coins");
		return false;
	}

	protected void PlayPurchaseUpgradeCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseUpgradeCue);
	}

	protected void PlayPurchaseCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.purchasePlotCue);
	}
}

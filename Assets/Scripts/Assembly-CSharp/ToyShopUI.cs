using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ToyShopUI : BaseUI
{
	public Sprite titleIcon;

	private PlayerState playerState;

	private LookupDirector lookupDir;

	private PurchaseUI purchaseUI;

	private AchievementsDirector achieveDir;

	private ToyDirector toyDirector;

	private const float EJECT_FORCE = 25f;

	[HideInInspector]
	public GameObject ejectionPoint;

	[HideInInspector]
	public RegionRegistry.RegionSetId regionSetId;

	public override void Awake()
	{
		base.Awake();
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		toyDirector = SRSingleton<GameContext>.Instance.ToyDirector;
		SRSingleton<SceneContext>.Instance.PediaDirector.UnlockWithoutPopup(PediaDirector.Id.SLIME_TOYS);
		RebuildUI();
	}

	public void RebuildUI()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Destroyer.Destroy(base.transform.GetChild(i).gameObject, "ToyShopUI.RebuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		purchaseUI = gameObject.GetComponent<PurchaseUI>();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		statusArea = gameObject.GetComponent<PurchaseUI>().statusArea;
	}

	protected GameObject CreatePurchaseUI()
	{
		List<PurchaseUI.Purchasable> list = new List<PurchaseUI.Purchasable>();
		foreach (Identifiable.Id purchaseableToy in toyDirector.GetPurchaseableToys())
		{
			list.Add(CreatePurchasableToy(purchaseableToy));
		}
		return SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.slime_toys"), list.ToArray(), hideNubuckCost: false, Close);
	}

	private PurchaseUI.Purchasable CreatePurchasableToy(Identifiable.Id toyId)
	{
		ToyDefinition toy = lookupDir.GetToyDefinition(toyId);
		return new PurchaseUI.Purchasable($"m.toy.name.{toy.NameKey}", toy.Icon, toy.Icon, $"m.toy.desc.{toy.NameKey}", toy.Cost, null, delegate
		{
			BuyToy(toyId, toy.Cost);
		}, () => true, () => true);
	}

	protected void BuyToy(Identifiable.Id toyId, int cost)
	{
		if (playerState.GetCurrency() >= cost)
		{
			Play(SRSingleton<GameContext>.Instance.UITemplates.purchasePersonalUpgradeCue);
			playerState.SpendCurrency(cost);
			InstantiateToy(toyId);
			purchaseUI.PlayPurchaseFX();
			achieveDir.AddToStat(AchievementsDirector.EnumStat.SLIME_TOYS_BOUGHT, toyId);
			Close();
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_coins");
		}
	}

	private void InstantiateToy(Identifiable.Id toyId)
	{
		if (ejectionPoint != null)
		{
			Rigidbody component = SRBehaviour.InstantiateActor(lookupDir.GetPrefab(toyId), regionSetId, ejectionPoint.transform.position, ejectionPoint.transform.rotation).GetComponent<Rigidbody>();
			component.isKinematic = false;
			component.AddForce(base.transform.forward * 25f);
		}
	}
}

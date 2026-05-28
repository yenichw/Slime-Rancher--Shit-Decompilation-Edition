using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class PlaceGadgetUI : BaseUI
{
	[Serializable]
	public class PurchaseItem
	{
		public Sprite icon;

		public Sprite img;

		public int cost;
	}

	public PurchaseItem demolish;

	public PurchaseItem pickUp;

	public Sprite titleIcon;

	private GadgetSite site;

	private GadgetSiteModel siteModel;

	private PurchaseUI purchaseUI;

	private const string ERR_CANNOT_PICKUP_GADGET = "e.cannot_pickup_gadget";

	private const string ERR_CANNOT_DESTROY_GADGET = "e.cannot_destroy_gadget";

	public void SetSite(GadgetSite site, GadgetSiteModel siteModel)
	{
		this.site = site;
		this.siteModel = siteModel;
		SRSingleton<SceneContext>.Instance.TutorialDirector.OnPlaceGadgetOpen();
		RebuildUI();
	}

	public void RebuildUI()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Destroyer.Destroy(base.transform.GetChild(i).gameObject, "PlaceGadgetUI.RebuildUI");
		}
		GameObject gameObject = CreatePurchaseUI();
		purchaseUI = gameObject.GetComponent<PurchaseUI>();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		statusArea = gameObject.GetComponent<PurchaseUI>().statusArea;
	}

	protected GameObject CreatePurchaseUI()
	{
		GadgetDirector gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
		List<PurchaseUI.Purchasable> list = new List<PurchaseUI.Purchasable>();
		Dictionary<PediaDirector.Id, List<PurchaseUI.Purchasable>> dictionary = new Dictionary<PediaDirector.Id, List<PurchaseUI.Purchasable>>();
		Gadget.Id attachedId = site.GetAttachedId();
		if (attachedId == Gadget.Id.NONE)
		{
			foreach (GadgetDefinition gadgetDefinition in SRSingleton<GameContext>.Instance.LookupDirector.GadgetDefinitions)
			{
				GadgetDirector.PlacementError placementError = gadgetDir.GetPlacementError(site, gadgetDefinition.id);
				string text = Enum.GetName(typeof(Gadget.Id), gadgetDefinition.id).ToLowerInvariant();
				Gadget.Id finalId = gadgetDefinition.id;
				string warning = ((placementError != null) ? placementError.message : ((gadgetDefinition.destroyOnRemoval || Gadget.IsLinkDestroyerType(gadgetDefinition.id)) ? "w.gadget_install_permanent" : null));
				PurchaseUI.Purchasable item = new PurchaseUI.Purchasable("m.gadget.name." + text, gadgetDefinition.icon, gadgetDefinition.icon, "m.gadget.desc." + text, 0, gadgetDefinition.pediaLink, delegate
				{
					Place(finalId);
				}, () => gadgetDir.GetGadgetCount(finalId) > 0, () => gadgetDir.CanPlaceGadget(site, finalId), placementError?.button, warning, () => gadgetDir.GetGadgetCount(finalId));
				list.Add(item);
				List<PurchaseUI.Purchasable> list2 = dictionary.Get(gadgetDefinition.pediaLink);
				if (list2 == null)
				{
					list2 = new List<PurchaseUI.Purchasable>();
				}
				list2.Add(item);
				dictionary[gadgetDefinition.pediaLink] = list2;
			}
		}
		else if (site.DestroysLinkedPairOnRemoval())
		{
			list.Add(new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_linked_gadget"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_linked_gadget"), demolish.cost, null, DemolishPair, () => true, () => true, "b.demolish", site.DestroyingWillDestroyContents() ? "w.destroying_gadget_destroys_contents" : null));
		}
		else if (site.DestroysOnRemoval() || GordoSnare.HasSnaredGordo(site))
		{
			list.Add(new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.demolish_gadget"), demolish.icon, demolish.img, MessageUtil.Qualify("ui", "m.desc.demolish_gadget"), demolish.cost, null, Demolish, () => true, () => true, "b.demolish", site.DestroyingWillDestroyContents() ? "w.destroying_gadget_destroys_contents" : null));
		}
		else
		{
			string warning2 = (((attachedId == Gadget.Id.DRONE || attachedId == Gadget.Id.DRONE_ADVANCED) && site.GetAttached().GetComponentInChildren<Drone>().ammo.Any()) ? "w.drone_reprogram_drops_ammo" : (site.DestroyingWillDestroyContents() ? "w.pick_up_gadget_destroys_contents" : null));
			list.Add(new PurchaseUI.Purchasable(MessageUtil.Qualify("ui", "l.pick_up_gadget"), pickUp.icon, pickUp.img, MessageUtil.Qualify("ui", "m.desc.pick_up_gadget"), pickUp.cost, null, PickUp, () => true, () => true, "b.pick_up", warning2));
		}
		GameObject gameObject = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.place_gadget"), list.ToArray(), hideNubuckCost: true, Close);
		if (attachedId == Gadget.Id.NONE)
		{
			List<PurchaseUI.Category> list3 = new List<PurchaseUI.Category>();
			PediaDirector.Id[] sCIENCE_ENTRIES = PediaUI.SCIENCE_ENTRIES;
			for (int i = 0; i < sCIENCE_ENTRIES.Length; i++)
			{
				PediaDirector.Id key = sCIENCE_ENTRIES[i];
				if (dictionary.ContainsKey(key))
				{
					list3.Add(new PurchaseUI.Category(key.ToString().ToLowerInvariant(), dictionary[key].ToArray()));
				}
			}
			gameObject.GetComponent<PurchaseUI>().SetCategories(list3);
			gameObject.GetComponent<PurchaseUI>().SetPurchaseMsgs("b.place", "b.place");
		}
		return gameObject;
	}

	private void Place(Gadget.Id id)
	{
		GameObject prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id).prefab;
		SRSingleton<SceneContext>.Instance.GameModel.InstantiateGadget(prefab, siteModel);
		site.RotateToPlayer();
		PlayPurchaseCue();
		SRSingleton<SceneContext>.Instance.GadgetDirector.SpendGadget(id);
		Close();
		AnalyticsUtil.CustomEvent("PlaceGadget." + id, new Dictionary<string, object>
		{
			{
				"GadgetSite.Position",
				AnalyticsUtil.GetEventData(site.transform.position)
			},
			{ "GadgetSite.Id", site.id },
			{ "Gadget.Id", id }
		});
	}

	public void Demolish()
	{
		if (site.HasAttached())
		{
			if ((!site.DestroysOnRemoval() && !GordoSnare.HasSnaredGordo(site)) || site.DestroysLinkedPairOnRemoval())
			{
				Error("e.cannot_destroy_gadget");
				return;
			}
			site.DestroyAttached();
			Play(SRSingleton<GameContext>.Instance.UITemplates.removeGadgetCue);
			RebuildUI();
			purchaseUI.PlayPurchaseFX();
		}
		else
		{
			Error("e.cannot_destroy_gadget");
		}
	}

	public void DemolishPair()
	{
		if (site.HasAttached())
		{
			if (!site.DestroysLinkedPairOnRemoval())
			{
				Error("e.cannot_destroy_gadget");
				return;
			}
			site.DestroyAttachedWithPair();
			Play(SRSingleton<GameContext>.Instance.UITemplates.removeGadgetCue);
			RebuildUI();
			purchaseUI.PlayPurchaseFX();
		}
		else
		{
			Error("e.cannot_destroy_gadget");
		}
	}

	public void PickUp()
	{
		if (site.HasAttached())
		{
			if (site.DestroysOnRemoval() || site.DestroysLinkedPairOnRemoval())
			{
				Error("e.cannot_pickup_gadget");
				return;
			}
			Gadget.Id attachedId = site.GetAttachedId();
			site.DestroyAttached();
			Play(SRSingleton<GameContext>.Instance.UITemplates.removeGadgetCue);
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddGadget(attachedId);
			RebuildUI();
			purchaseUI.PlayPurchaseFX();
		}
		else
		{
			Error("e.cannot_pickup_gadget");
		}
	}

	protected void PlayPurchaseCue()
	{
		Play(SRSingleton<GameContext>.Instance.UITemplates.placeGadgetCue);
	}
}

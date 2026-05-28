using UnityEngine;

public class AccessDoorUI : BaseUI
{
	public Sprite titleIcon;

	private PlayerState playerState;

	private AccessDoor door;

	public override void Awake()
	{
		base.Awake();
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void SetAccessDoor(AccessDoor door)
	{
		this.door = door;
		if (door.CurrState == AccessDoor.State.LOCKED)
		{
			GameObject gameObject = CreatePurchaseUI();
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			statusArea = gameObject.GetComponent<PurchaseUI>().statusArea;
		}
		else
		{
			Close();
			door.CurrState = AccessDoor.State.OPEN;
			SRSingleton<SceneContext>.Instance.PediaDirector.ShowPedia(door.lockedRegionId);
		}
	}

	protected GameObject CreatePurchaseUI()
	{
		PurchaseUI.Purchasable[] array = new PurchaseUI.Purchasable[1]
		{
			new PurchaseUI.Purchasable("t." + door.lockedRegionId.ToString().ToLowerInvariant(), door.doorPurchase.icon, door.doorPurchase.img, "m.intro." + door.lockedRegionId.ToString().ToLowerInvariant(), door.doorPurchase.cost, door.lockedRegionId, UnlockDoor, () => true, () => true)
		};
		GameObject obj = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(titleIcon, MessageUtil.Qualify("ui", "t.access_door"), array, hideNubuckCost: false, Close);
		obj.GetComponent<PurchaseUI>().Select(array[0]);
		obj.GetComponent<PurchaseUI>().HideSelectionPanel();
		return obj;
	}

	public void UnlockDoor()
	{
		if (playerState.GetCurrency() >= door.doorPurchase.cost)
		{
			playerState.SpendCurrency(door.doorPurchase.cost);
			door.CurrState = AccessDoor.State.OPEN;
			if (door.linkedDoors != null)
			{
				AccessDoor[] linkedDoors = door.linkedDoors;
				foreach (AccessDoor accessDoor in linkedDoors)
				{
					if (accessDoor.CurrState == AccessDoor.State.LOCKED)
					{
						accessDoor.CurrState = AccessDoor.State.CLOSED;
					}
				}
			}
			Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseExpansionCue);
			Close();
			SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_coins");
		}
	}
}

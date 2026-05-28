using System.Collections.Generic;
using UnityEngine;

public class PopupElementsUI : SRSingleton<PopupElementsUI>
{
	private class CoinsEntry
	{
		public int amount;

		public PlayerState.CoinsType coinsType;

		public CoinsEntry(int amount, PlayerState.CoinsType coinsType)
		{
			this.amount = amount;
			this.coinsType = coinsType;
		}
	}

	public RectTransform container;

	public GameObject coinsPopup;

	public Sprite mochiIcon;

	public Color mochiColor;

	public SECTR_AudioCue mochiCue;

	public DroneMetadata droneMetadata;

	private HashSet<GameObject> blockers = new HashSet<GameObject>();

	private Queue<CoinsEntry> queuedCoins = new Queue<CoinsEntry>();

	private float nextCoinAt;

	private const float MIN_TIME_BETWEEN_COINS = 0.1f;

	public void CreateCoinsPopup(int amount, PlayerState.CoinsType coinsType)
	{
		if (amount != 0 && coinsType != PlayerState.CoinsType.NONE)
		{
			queuedCoins.Enqueue(new CoinsEntry(amount, coinsType));
		}
	}

	public void Update()
	{
		if (blockers.Count == 0 && queuedCoins.Count > 0 && Time.unscaledTime >= nextCoinAt)
		{
			CoinsPopupUI component = Object.Instantiate(coinsPopup, container).GetComponent<CoinsPopupUI>();
			CoinsEntry coinsEntry = queuedCoins.Dequeue();
			component.Init(coinsEntry.amount, GetSprite(coinsEntry.coinsType), GetColorOverride(coinsEntry.coinsType), GetSFXOverride(coinsEntry.coinsType));
			nextCoinAt = Time.unscaledTime + 0.1f;
		}
	}

	public void RegisterBlocker(GameObject blocker)
	{
		blockers.Add(blocker);
	}

	public void DeregisterBlocker(GameObject blocker)
	{
		blockers.Remove(blocker);
	}

	private Sprite GetSprite(PlayerState.CoinsType coinsType)
	{
		switch (coinsType)
		{
		case PlayerState.CoinsType.MOCHI:
			return mochiIcon;
		case PlayerState.CoinsType.DRONE:
			return droneMetadata.coinsIcon;
		default:
			return null;
		}
	}

	private Color? GetColorOverride(PlayerState.CoinsType coinsType)
	{
		switch (coinsType)
		{
		case PlayerState.CoinsType.MOCHI:
			return mochiColor;
		case PlayerState.CoinsType.DRONE:
			return droneMetadata.coinsColor;
		default:
			return null;
		}
	}

	private SECTR_AudioCue GetSFXOverride(PlayerState.CoinsType coinsType)
	{
		switch (coinsType)
		{
		case PlayerState.CoinsType.MOCHI:
			return mochiCue;
		case PlayerState.CoinsType.DRONE:
			return droneMetadata.coinsCue;
		default:
			return null;
		}
	}
}

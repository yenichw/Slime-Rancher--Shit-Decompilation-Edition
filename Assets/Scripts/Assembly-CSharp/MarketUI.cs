using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketUI : MonoBehaviour
{
	[Serializable]
	public class PlortEntry
	{
		public Identifiable.Id id;

		public ProgressDirector.ProgressType[] toUnlock;
	}

	[Serializable]
	public class PricesPanelEntry
	{
		public GameObject panel;

		public int entryCount;
	}

	public PlortEntry[] plorts;

	public GameObject pricesPanelGroup;

	public PricesPanelEntry[] pricesPanels;

	public GameObject priceEntryPrefab;

	public GameObject priceEntryEmptyPrefab;

	public GameObject shutdownPanel;

	public GameObject[] toShutdown;

	public Sprite upImg;

	public Sprite downImg;

	public Sprite unchImg;

	public Sprite bonusCompleteImg;

	private EconomyDirector econDir;

	private ProgressDirector progressDir;

	private LookupDirector lookupDir;

	private Dictionary<PlortEntry, GameObject> amountMap;

	public void Awake()
	{
		amountMap = new Dictionary<PlortEntry, GameObject>();
		econDir = SRSingleton<SceneContext>.Instance.EconomyDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
	}

	public void Start()
	{
		int num = 0;
		int num2 = 0;
		PlayerState.GameMode currGameMode = SRSingleton<SceneContext>.Instance.GameModel.currGameMode;
		PlortEntry[] array = plorts;
		foreach (PlortEntry plortEntry in array)
		{
			if (plortEntry.id != Identifiable.Id.SABER_PLORT || currGameMode != PlayerState.GameMode.TIME_LIMIT_V2)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(priceEntryPrefab);
				gameObject.GetComponent<PriceEntry>().itemIcon.sprite = lookupDir.GetIcon(plortEntry.id);
				amountMap[plortEntry] = gameObject;
				gameObject.transform.SetParent(pricesPanels[num].panel.transform, worldPositionStays: false);
				num2++;
				if (num2 >= pricesPanels[num].entryCount)
				{
					num++;
					num2 = 0;
				}
			}
		}
		while (num < pricesPanels.Length)
		{
			UnityEngine.Object.Instantiate(priceEntryEmptyPrefab).transform.SetParent(pricesPanels[num].panel.transform, worldPositionStays: false);
			num2++;
			if (num2 >= pricesPanels[num].entryCount)
			{
				num++;
				num2 = 0;
			}
		}
		EconUpdate();
		EconomyDirector economyDirector = econDir;
		economyDirector.didUpdateDelegate = (EconomyDirector.DidUpdate)Delegate.Combine(economyDirector.didUpdateDelegate, new EconomyDirector.DidUpdate(EconUpdate));
		EconomyDirector economyDirector2 = econDir;
		economyDirector2.onRegisterSold = (EconomyDirector.OnRegisterSold)Delegate.Combine(economyDirector2.onRegisterSold, new EconomyDirector.OnRegisterSold(PlortCountUpdate));
		ProgressDirector progressDirector = progressDir;
		progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Combine(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(EconUpdate));
	}

	public void OnDestroy()
	{
		EconomyDirector economyDirector = econDir;
		economyDirector.didUpdateDelegate = (EconomyDirector.DidUpdate)Delegate.Remove(economyDirector.didUpdateDelegate, new EconomyDirector.DidUpdate(EconUpdate));
		EconomyDirector economyDirector2 = econDir;
		economyDirector2.onRegisterSold = (EconomyDirector.OnRegisterSold)Delegate.Remove(economyDirector2.onRegisterSold, new EconomyDirector.OnRegisterSold(PlortCountUpdate));
		ProgressDirector progressDirector = progressDir;
		progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Remove(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(EconUpdate));
	}

	private void EconUpdate()
	{
		foreach (KeyValuePair<PlortEntry, GameObject> item in amountMap)
		{
			PriceEntry component = item.Value.GetComponent<PriceEntry>();
			component.amountText.text = econDir.GetCurrValue(item.Key.id).Value.ToString();
			PlortCountUpdate(item.Key, component);
		}
	}

	public void Update()
	{
		bool flag = econDir.IsMarketShutdown();
		pricesPanelGroup.SetActive(!flag);
		shutdownPanel.SetActive(flag);
		GameObject[] array = toShutdown;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!flag);
		}
	}

	private void PlortCountUpdate(Identifiable.Id id)
	{
		foreach (KeyValuePair<PlortEntry, GameObject> item in amountMap)
		{
			if (item.Key.id == id)
			{
				PlortCountUpdate(item.Key, item.Value.GetComponent<PriceEntry>());
				break;
			}
		}
	}

	private void PlortCountUpdate(PlortEntry plort, PriceEntry price)
	{
		int value = 0;
		SRSingleton<SceneContext>.Instance.AchievementsDirector.GetGameIdDictStat(AchievementsDirector.GameIdDictStat.PLORT_TYPES_SOLD).TryGetValue(plort.id, out value);
		price.bonusFill.minValue = 0f;
		price.bonusFill.currValue = value;
		price.bonusFill.maxValue = 25f;
		price.bonusFill.enabled = SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2;
		int change = econDir.GetChangeInValue(plort.id) ?? 0;
		price.changeIcon.sprite = GetChangeIcon(plort.id, change, value);
		price.changeIcon.enabled = price.changeIcon.sprite != null;
		price.changeAmountText.text = GetChangeText(plort.id, change);
		float a = (IsPlortUnlocked(plort, value) ? 1f : 0.5f);
		price.amountText.color = AdjustAlpha(price.amountText.color, a);
		price.changeAmountText.color = AdjustAlpha(price.changeAmountText.color, a);
		price.changeIcon.color = AdjustAlpha(price.changeIcon.color, a);
		price.coinIcon.color = AdjustAlpha(price.coinIcon.color, a);
		price.itemIcon.color = AdjustAlpha(price.itemIcon.color, a);
	}

	private Color AdjustAlpha(Color c, float a)
	{
		c.a = a;
		return c;
	}

	private Sprite GetChangeIcon(Identifiable.Id id, int change, int collected)
	{
		if (SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2)
		{
			if (!GameModeSettings.PlortBonusReached(collected))
			{
				return null;
			}
			return bonusCompleteImg;
		}
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().plortMarketDynamic)
		{
			if (change != 0)
			{
				if (change >= 0)
				{
					return upImg;
				}
				return downImg;
			}
			return unchImg;
		}
		return null;
	}

	private string GetChangeText(Identifiable.Id id, int change)
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().plortMarketDynamic)
		{
			return Math.Abs(change).ToString();
		}
		return string.Empty;
	}

	private bool IsPlortUnlocked(PlortEntry plort, int collected)
	{
		if (collected > 0)
		{
			return true;
		}
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser)
		{
			return true;
		}
		if (plort.toUnlock.Length == 0)
		{
			return true;
		}
		ProgressDirector.ProgressType[] toUnlock = plort.toUnlock;
		foreach (ProgressDirector.ProgressType type in toUnlock)
		{
			if (progressDir.HasProgress(type))
			{
				return true;
			}
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : BaseUI
{
	private class PlortEntry : IComparable<PlortEntry>
	{
		public Identifiable.Id id;

		public int count;

		public int price;

		public PlortEntry(Identifiable.Id id, int count)
		{
			this.id = id;
			this.count = count;
			price = count * SRSingleton<SceneContext>.Instance.EconomyDirector.GetCurrValue(id).Value;
		}

		public int CompareTo(PlortEntry that)
		{
			return price.CompareTo(that.price);
		}
	}

	public TMP_Text currencyText;

	public TMP_Text deathsText;

	public TMP_Text spentText;

	public Button takeScreenshotButton;

	public EndGameUIPlortLine[] plortLines;

	public TMP_Text noPlortsText;

	public override void Awake()
	{
		base.Awake();
		takeScreenshotButton.gameObject.SetActive(value: true);
		int currency = SRSingleton<SceneContext>.Instance.PlayerState.GetCurrency();
		currencyText.text = currency.ToString();
		AchievementsDirector achievementsDirector = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		deathsText.text = achievementsDirector.GetGameIntStat(AchievementsDirector.GameIntStat.DEATHS).ToString();
		spentText.text = achievementsDirector.GetGameIntStat(AchievementsDirector.GameIntStat.CURRENCY_SPENT).ToString();
		Dictionary<Identifiable.Id, int> gameIdDictStat = achievementsDirector.GetGameIdDictStat(AchievementsDirector.GameIdDictStat.PLORT_TYPES_SOLD);
		List<PlortEntry> list = new List<PlortEntry>();
		foreach (KeyValuePair<Identifiable.Id, int> item in gameIdDictStat)
		{
			list.Add(new PlortEntry(item.Key, item.Value));
		}
		list.Sort();
		list.Reverse();
		for (int i = 0; i < plortLines.Length; i++)
		{
			if (list.Count > i)
			{
				plortLines[i].Init(list[i].id, list[i].count, list[i].price);
			}
			else
			{
				plortLines[i].gameObject.SetActive(value: false);
			}
		}
		noPlortsText.gameObject.SetActive(list.Count == 0);
	}

	public void OnScreenshot()
	{
		SRSingleton<GameContext>.Instance.TakeScreenshot();
	}

	public void OnOK()
	{
		if (SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow())
		{
			SRSingleton<SceneContext>.Instance.OnSessionEnded();
			SceneManager.LoadScene("MainMenu");
		}
	}

	protected override bool Closeable()
	{
		return false;
	}
}

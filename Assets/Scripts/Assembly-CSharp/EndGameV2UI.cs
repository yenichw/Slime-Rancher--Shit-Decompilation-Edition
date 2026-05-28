using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameV2UI : EndGameUI
{
	[Tooltip("Text displaying the plort bonus percentage.")]
	public TMP_Text plortBonusText;

	[Tooltip("Text displaying the score.")]
	public TMP_Text scoreText;

	[Tooltip("Parent GameObject containing the plort bonus images.")]
	public GameObject plortBonusLines;

	[Tooltip("Text displayed if there are no plort bonuses.")]
	public TMP_Text plortBonusNoneText;

	public override void Awake()
	{
		base.Awake();
		SRSingleton<GameContext>.Instance.MusicDirector.SetRushCreditsMode(enabled: true);
		AchievementsDirector achievementsDirector = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		Dictionary<Identifiable.Id, int> gameIdDictStat = achievementsDirector.GetGameIdDictStat(AchievementsDirector.GameIdDictStat.PLORT_TYPES_SOLD);
		InitPlortBonuses(gameIdDictStat);
		int currency = SRSingleton<SceneContext>.Instance.PlayerState.GetCurrency();
		float scoreMultiplier = GetScoreMultiplier(gameIdDictStat);
		int num = Mathf.CeilToInt((float)currency * scoreMultiplier);
		currencyText.text = $"{currency}";
		plortBonusText.text = uiBundle.Get("m.percentage", Mathf.Round((scoreMultiplier - 1f) * 100f));
		scoreText.text = $"{num}";
		AnalyticsUtil.CustomEvent("TimeLimitV2GameEnd", new Dictionary<string, object>
		{
			{ "currency", currency },
			{ "multiplier", scoreMultiplier },
			{ "score", num }
		});
		achievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.TIME_LIMIT_V2_CURRENCY, num);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MusicDirector.SetRushCreditsMode(enabled: false);
		}
	}

	private void InitPlortBonuses(IEnumerable<KeyValuePair<Identifiable.Id, int>> plorts)
	{
		Image[] componentsInChildren = plortBonusLines.GetComponentsInChildren<Image>(includeInactive: true);
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		foreach (KeyValuePair<Identifiable.Id, int> plort in plorts)
		{
			if (plort.Value >= 25)
			{
				list.Add(plort.Key);
			}
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Image image = componentsInChildren[i];
			image.gameObject.SetActive(i < list.Count);
			if (image.gameObject.activeSelf)
			{
				image.sprite = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(list[i]);
			}
		}
		plortBonusNoneText.gameObject.SetActive(list.Count == 0);
	}

	private static float GetScoreMultiplier(IEnumerable<KeyValuePair<Identifiable.Id, int>> plorts)
	{
		float num = 1f;
		foreach (KeyValuePair<Identifiable.Id, int> plort in plorts)
		{
			num += ((plort.Value >= 25) ? GameModeSettings.GetScoreMultiplier(plort.Key) : 0f);
		}
		return num;
	}
}

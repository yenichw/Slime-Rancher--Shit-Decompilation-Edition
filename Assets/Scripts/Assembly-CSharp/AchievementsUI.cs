using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsUI : BaseUI
{
	[Tooltip("Internal achievement content panel.")]
	public GameObject achievementListPanel;

	[Tooltip("The prefab from which to create individual achievement panels.")]
	public GameObject achievementListItemPrefab;

	[Tooltip("Internal overall achievement count text.")]
	public TMP_Text overallText;

	private MessageBundle achieveBundle;

	private AchievementsDirector achieveDir;

	public override void Awake()
	{
		base.Awake();
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		achieveBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("achieve");
		foreach (AchievementsDirector.Achievement value in Enum.GetValues(typeof(AchievementsDirector.Achievement)))
		{
			AddAchievement(value);
		}
		achieveDir.GetOverallProgress(out var progress, out var outOf);
		overallText.text = uiBundle.Get("m.achieve_overall_progress", progress, outOf);
	}

	public void AddAchievement(AchievementsDirector.Achievement achievement)
	{
		CreateAchievement(achievement).transform.SetParent(achievementListPanel.transform, worldPositionStays: false);
	}

	private GameObject CreateAchievement(AchievementsDirector.Achievement achievement)
	{
		bool flag = achieveDir.HasAchievement(achievement);
		achieveDir.GetProgress(achievement, out var progress, out var outOf);
		GameObject gameObject = UnityEngine.Object.Instantiate(achievementListItemPrefab);
		TMP_Text component = gameObject.transform.Find("InfoPanel/Name").GetComponent<TMP_Text>();
		TMP_Text component2 = gameObject.transform.Find("InfoPanel/Requirement").GetComponent<TMP_Text>();
		Image component3 = gameObject.transform.Find("Icon").GetComponent<Image>();
		Image component4 = gameObject.transform.Find("Complete").GetComponent<Image>();
		GameObject gameObject2 = gameObject.transform.Find("InfoPanel/Progress").gameObject;
		StatusBar component5 = gameObject.transform.Find("InfoPanel/Progress/ProgressMeter").GetComponent<StatusBar>();
		TMP_Text component6 = gameObject.transform.Find("InfoPanel/Progress/ProgressText").GetComponent<TMP_Text>();
		string text = achievement.ToString().ToLowerInvariant();
		component.text = achieveBundle.Xlate("t." + text);
		component2.text = achieveBundle.Xlate("m.reqmt." + text);
		component3.sprite = achieveDir.GetAchievementImage(text, achievement);
		if (!flag)
		{
			component3.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}
		component4.enabled = flag;
		if (outOf > 1 && !flag)
		{
			gameObject2.SetActive(value: true);
			component5.maxValue = outOf;
			component5.currValue = progress;
			component6.text = uiBundle.Get("m.achieve_progress", progress, outOf);
		}
		else
		{
			gameObject2.SetActive(value: false);
		}
		return gameObject;
	}
}

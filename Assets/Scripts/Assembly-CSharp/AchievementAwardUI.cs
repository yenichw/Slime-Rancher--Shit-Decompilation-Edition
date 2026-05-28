using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementAwardUI : PopupUI<AchievementsDirector.Achievement>
{
	public Image img;

	public TMP_Text titleText;

	[Tooltip("If not killed before then, how long this popup will stick around.")]
	public float lifetime = 10f;

	protected float timeOfDeath;

	private AchievementsDirector achieveDir;

	public virtual void Awake()
	{
		timeOfDeath = Time.time + lifetime;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		achieveDir.PopupActivated(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		achieveDir.PopupDeactivated(this);
	}

	public void Update()
	{
		if (Time.time >= timeOfDeath)
		{
			Destroyer.Destroy(base.gameObject, "AchievementAwardUI.Update");
		}
	}

	public override void OnBundleAvailable(MessageDirector msgDir)
	{
		string text = Enum.GetName(typeof(AchievementsDirector.Achievement), idEntry).ToLowerInvariant();
		Sprite achievementImage = achieveDir.GetAchievementImage(text, idEntry);
		MessageBundle bundle = msgDir.GetBundle("achieve");
		img.sprite = achievementImage;
		titleText.text = bundle.Get("t." + text);
	}
}

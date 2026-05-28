using UnityEngine;

public class JournalEntry : UIActivator
{
	[Tooltip("The key used to specify which journal entry to display on interaction.")]
	public string entryKey;

	[Tooltip("If set, ensure the player has these progresses when they read this journal.")]
	public ProgressDirector.ProgressType[] ensureProgress;

	public void Start()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().suppressStory)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public override GameObject Activate()
	{
		GameObject gameObject = Object.Instantiate(uiPrefab);
		gameObject.GetComponent<JournalUI>().SetJournalKey(entryKey);
		ProgressDirector.ProgressType[] array = ensureProgress;
		foreach (ProgressDirector.ProgressType type in array)
		{
			SRSingleton<SceneContext>.Instance.ProgressDirector.SetProgress(type, 1);
		}
		return gameObject;
	}
}

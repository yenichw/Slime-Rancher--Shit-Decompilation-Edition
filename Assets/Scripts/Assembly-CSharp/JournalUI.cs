using TMPro;

public class JournalUI : BaseUI
{
	public TMP_Text journalText;

	public SECTR_AudioCue openCue;

	public SECTR_AudioCue closeCue;

	public void OnEnable()
	{
		Play(openCue);
	}

	public void OnDisable()
	{
		Play(closeCue);
	}

	public void SetJournalKey(string journalKey)
	{
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("mail");
		journalText.text = bundle.Get("m.journal." + journalKey);
	}
}

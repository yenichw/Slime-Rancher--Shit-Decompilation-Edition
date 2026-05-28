using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeItemEntryUI : MonoBehaviour
{
	public Image icon;

	public TMP_Text progressText;

	public TMP_Text nameText;

	private LookupDirector lookupDir;

	private MessageBundle uiBundle;

	public void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		uiBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
	}

	public void SetEntry(ExchangeDirector.ItemEntry entry)
	{
		if (entry == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		icon.sprite = lookupDir.GetIcon(entry.id);
		progressText.text = uiBundle.Get("l.ammo", entry.count);
		nameText.text = Identifiable.GetName(entry.id);
	}
}

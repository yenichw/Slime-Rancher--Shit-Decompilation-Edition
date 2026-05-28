using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSummaryEntry : MonoBehaviour
{
	public Image gameIcon;

	public TMP_Text gameNameText;

	public string gameName;

	public string saveName;

	public void Init(GameData.Summary gameSummary)
	{
		gameIcon.sprite = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon((gameSummary.iconId == Identifiable.Id.NONE) ? Identifiable.Id.CARROT_VEGGIE : gameSummary.iconId);
		gameName = gameSummary.name;
		saveName = gameSummary.saveName;
		gameNameText.text = gameSummary.displayName;
	}

	public string GetGameName()
	{
		return gameName;
	}

	public string GetSaveName()
	{
		return saveName;
	}
}

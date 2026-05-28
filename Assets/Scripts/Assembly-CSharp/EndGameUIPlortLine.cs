using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUIPlortLine : MonoBehaviour
{
	public Image icon;

	public TMP_Text countText;

	public TMP_Text currencyText;

	public void Init(Identifiable.Id id, int amount, int price)
	{
		icon.sprite = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(id);
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		countText.text = bundle.Get("m.plort_amount", amount);
		currencyText.text = price.ToString();
	}
}

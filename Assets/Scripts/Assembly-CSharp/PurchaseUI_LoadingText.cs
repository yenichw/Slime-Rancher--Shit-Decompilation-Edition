using System.Collections;
using TMPro;
using UnityEngine;

public class PurchaseUI_LoadingText : SRBehaviour
{
	private TMP_Text text;

	private string message;

	public void Awake()
	{
		text = GetRequiredComponent<TMP_Text>();
		message = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get("l.loading");
	}

	public void OnEnable()
	{
		StartCoroutine(UpdateText_Coroutine());
	}

	private IEnumerator UpdateText_Coroutine()
	{
		int maxLoops = 0;
		while (maxLoops < int.MaxValue)
		{
			int num;
			for (int dotCount = 0; dotCount <= 3; dotCount = num)
			{
				text.text = message;
				for (int i = 0; i < dotCount; i++)
				{
					text.text += ".";
				}
				yield return new WaitForSecondsRealtime(0.5f);
				num = dotCount + 1;
			}
			num = maxLoops + 1;
			maxLoops = num;
		}
	}
}

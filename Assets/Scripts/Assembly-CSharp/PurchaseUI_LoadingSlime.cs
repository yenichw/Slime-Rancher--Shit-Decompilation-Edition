using UnityEngine;
using UnityEngine.UI;

public class PurchaseUI_LoadingSlime : SRBehaviour
{
	[Tooltip("List of potential loading slime icons.")]
	public Sprite[] icons;

	public void Awake()
	{
		GetRequiredComponent<Image>().sprite = Randoms.SHARED.Pick(icons);
	}
}

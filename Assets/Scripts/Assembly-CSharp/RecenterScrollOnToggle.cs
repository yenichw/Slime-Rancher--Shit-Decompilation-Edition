using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Toggle))]
public class RecenterScrollOnToggle : MonoBehaviour
{
	private RecenterableScroll scrollCenterScript;

	private Toggle toggle;

	private bool toggleWasOn;

	public void Start()
	{
		scrollCenterScript = GetComponentInParent<RecenterableScroll>();
		toggle = GetComponent<Toggle>();
	}

	public void Update()
	{
		bool isOn = toggle.isOn;
		if (isOn != toggleWasOn)
		{
			if (isOn)
			{
				scrollCenterScript.ScrollToItem((RectTransform)base.transform);
			}
			toggleWasOn = isOn;
		}
	}
}

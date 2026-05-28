using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollByMenuKeys : MonoBehaviour
{
	public float scrollPerFrame = 5f;

	public bool scrollWithMainMenuKeysAlso;

	private ScrollRect scroller;

	public void Awake()
	{
		scroller = GetComponent<ScrollRect>();
	}

	public void Update()
	{
		float height = ((RectTransform)scroller.transform).rect.height;
		float num = ((RectTransform)scroller.content.transform).rect.height - height;
		if (!(num > 0f))
		{
			return;
		}
		if (SRInput.PauseActions.menuScrollUp.IsPressed || (scrollWithMainMenuKeysAlso && SRInput.PauseActions.menuUp.IsPressed))
		{
			if (num > 0f)
			{
				scroller.verticalNormalizedPosition = Mathf.Clamp01(scroller.verticalNormalizedPosition + scrollPerFrame / num);
			}
		}
		else if ((SRInput.PauseActions.menuScrollDown.IsPressed || (scrollWithMainMenuKeysAlso && SRInput.PauseActions.menuDown.IsPressed)) && num > 0f)
		{
			scroller.verticalNormalizedPosition = Mathf.Clamp01(scroller.verticalNormalizedPosition - scrollPerFrame / num);
		}
	}
}

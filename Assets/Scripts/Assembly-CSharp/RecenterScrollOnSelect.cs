using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class RecenterScrollOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public RectTransform transformToShow;

	private RecenterableScroll scrollCenterScript;

	public void Awake()
	{
		if (transformToShow == null)
		{
			transformToShow = (RectTransform)base.transform;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		StartCoroutine(SelectAfterFrame());
	}

	private IEnumerator SelectAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		if (scrollCenterScript == null)
		{
			scrollCenterScript = GetComponentInParent<RecenterableScroll>();
		}
		if (scrollCenterScript != null)
		{
			scrollCenterScript.ScrollToItem(transformToShow);
		}
	}
}

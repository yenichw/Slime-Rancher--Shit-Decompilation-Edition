using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OnSelectDelegator : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	private UnityAction onSelectDel;

	public void SetDelegate(UnityAction onSelectDel)
	{
		this.onSelectDel = onSelectDel;
	}

	public void OnSelect(BaseEventData data)
	{
		onSelectDel();
	}

	public static OnSelectDelegator Create(GameObject obj, UnityAction onSelectDel)
	{
		OnSelectDelegator onSelectDelegator = obj.AddComponent<OnSelectDelegator>();
		onSelectDelegator.SetDelegate(onSelectDel);
		return onSelectDelegator;
	}
}

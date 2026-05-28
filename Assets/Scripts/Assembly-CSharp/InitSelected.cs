using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InitSelected : MonoBehaviour
{
	private Selectable selectable;

	public static InitSelected Current;

	public void Awake()
	{
		selectable = GetComponent<Selectable>();
	}

	public void OnEnable()
	{
		selectable.Select();
		selectable.OnSelect(null);
		Current = this;
	}

	public void OnDisable()
	{
		selectable.OnDeselect(null);
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		if (Current == this)
		{
			Current = null;
		}
	}
}

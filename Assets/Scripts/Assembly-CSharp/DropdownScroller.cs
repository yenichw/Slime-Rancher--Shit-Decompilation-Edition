using UnityEngine;
using UnityEngine.UI;

public class DropdownScroller : MonoBehaviour
{
	public Dropdown dropdown;

	public ScrollRect scrollRect;

	[SerializeField]
	private float scrollPosition = 1f;

	public void Awake()
	{
		dropdown.onValueChanged.AddListener(UpdateScrollPosition);
	}

	public void Start()
	{
		scrollRect.verticalNormalizedPosition = scrollPosition;
	}

	public void OnDestroy()
	{
		dropdown.onValueChanged.RemoveListener(UpdateScrollPosition);
	}

	private void UpdateScrollPosition(int index)
	{
		scrollPosition = 1f - 1f * (float)index / (float)dropdown.options.Count;
	}
}

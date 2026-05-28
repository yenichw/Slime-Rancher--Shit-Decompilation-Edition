using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EnableImageWhenSelected : MonoBehaviour
{
	public bool gamepadModeOnly = true;

	private Selectable selectableParent;

	private Image img;

	public void Start()
	{
		img = GetComponent<Image>();
		selectableParent = GetComponentInParent<Selectable>();
	}

	public void Update()
	{
		img.enabled = (!gamepadModeOnly || InputDirector.UsingGamepad()) && selectableParent != null && selectableParent.gameObject == EventSystem.current.currentSelectedGameObject;
	}
}

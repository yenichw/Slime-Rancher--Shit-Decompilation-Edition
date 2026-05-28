using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EnableImageGamepadMode : MonoBehaviour
{
	private Image img;

	public void Start()
	{
		img = GetComponent<Image>();
	}

	public void Update()
	{
		img.enabled = InputDirector.UsingGamepad();
	}
}

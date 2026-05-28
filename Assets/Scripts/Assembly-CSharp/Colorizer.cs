using UnityEngine;

[ExecuteInEditMode]
public class Colorizer : MonoBehaviour
{
	public Color TintColor;

	public bool UseInstanceWhenNotEditorMode = true;

	private Color oldColor;

	private void Start()
	{
	}

	private void Update()
	{
		if (oldColor != TintColor)
		{
			ChangeColor(base.gameObject, TintColor);
		}
		oldColor = TintColor;
	}

	private void ChangeColor(GameObject effect, Color color)
	{
		Renderer[] componentsInChildren = effect.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material material = (UseInstanceWhenNotEditorMode ? renderer.material : renderer.sharedMaterial);
			if (!(material == null) && material.HasProperty("_TintColor"))
			{
				color.a = material.GetColor("_TintColor").a;
				material.SetColor("_TintColor", color);
			}
		}
		Light componentInChildren = effect.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.color = color;
		}
	}
}

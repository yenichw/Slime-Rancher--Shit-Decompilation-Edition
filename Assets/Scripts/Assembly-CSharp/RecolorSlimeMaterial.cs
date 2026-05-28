using UnityEngine;

public class RecolorSlimeMaterial : MonoBehaviour
{
	protected MaterialPropertyBlock propertyBlock;

	protected Renderer slimeRenderer;

	private static int topColorNameId = Shader.PropertyToID("_TopColor");

	private static int middleColorNameId = Shader.PropertyToID("_MiddleColor");

	private static int bottomColorNameId = Shader.PropertyToID("_BottomColor");

	public virtual void Awake()
	{
		slimeRenderer = GetComponent<Renderer>();
		propertyBlock = new MaterialPropertyBlock();
	}

	protected virtual Material GetMaterial()
	{
		slimeRenderer = GetComponent<Renderer>();
		if (slimeRenderer != null)
		{
			return slimeRenderer.material;
		}
		return null;
	}

	public void SetColors(Color topColor, Color midColor, Color btmColor)
	{
		slimeRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetColor(topColorNameId, topColor);
		propertyBlock.SetColor(middleColorNameId, midColor);
		propertyBlock.SetColor(bottomColorNameId, btmColor);
		slimeRenderer.SetPropertyBlock(propertyBlock);
	}
}

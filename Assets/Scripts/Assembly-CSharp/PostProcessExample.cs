using UnityEngine;

[ExecuteInEditMode]
public class PostProcessExample : MonoBehaviour
{
	public Material PostProcessMat;

	private void Awake()
	{
		if (PostProcessMat == null)
		{
			base.enabled = false;
		}
		else
		{
			PostProcessMat.mainTexture = PostProcessMat.mainTexture;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, PostProcessMat);
	}
}

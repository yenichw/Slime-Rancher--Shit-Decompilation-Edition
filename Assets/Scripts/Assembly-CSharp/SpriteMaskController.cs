using UnityEngine;
using UnityEngine.Sprites;

[ExecuteInEditMode]
public class SpriteMaskController : MonoBehaviour
{
	private SpriteRenderer m_spriteRenderer;

	private Vector4 m_uvs;

	private void OnEnable()
	{
		m_spriteRenderer = GetComponent<SpriteRenderer>();
		m_uvs = DataUtility.GetInnerUV(m_spriteRenderer.sprite);
		m_spriteRenderer.sharedMaterial.SetVector("_CustomUVS", m_uvs);
	}
}

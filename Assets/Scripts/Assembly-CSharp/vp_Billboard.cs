using UnityEngine;

public class vp_Billboard : MonoBehaviour
{
	public Transform m_CameraTransform;

	private Transform m_Transform;

	private Renderer m_Renderer;

	protected virtual void Start()
	{
		m_Transform = base.transform;
		m_Renderer = GetComponent<Renderer>();
		if (m_CameraTransform == null)
		{
			m_CameraTransform = Camera.main.transform;
		}
	}

	protected virtual void Update()
	{
		if (m_Renderer != null && m_Renderer.isVisible && m_CameraTransform != null)
		{
			m_Transform.eulerAngles = m_CameraTransform.eulerAngles;
		}
	}
}

using UnityEngine;

public class SimpleMoveExample : MonoBehaviour
{
	private Vector3 m_previous;

	private Vector3 m_target;

	private Vector3 m_originalPosition;

	public Vector3 BoundingVolume = new Vector3(3f, 1f, 3f);

	public float Speed = 10f;

	private void Start()
	{
		m_originalPosition = base.transform.position;
		m_previous = base.transform.position;
		m_target = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = Vector3.Slerp(m_previous, m_target, Time.deltaTime * Speed);
		m_previous = base.transform.position;
		if (Vector3.Distance(m_target, base.transform.position) < 0.1f)
		{
			m_target = base.transform.position + Random.onUnitSphere * Random.Range(0.7f, 4f);
			m_target.Set(Mathf.Clamp(m_target.x, m_originalPosition.x - BoundingVolume.x, m_originalPosition.x + BoundingVolume.x), Mathf.Clamp(m_target.y, m_originalPosition.y - BoundingVolume.y, m_originalPosition.y + BoundingVolume.y), Mathf.Clamp(m_target.z, m_originalPosition.z - BoundingVolume.z, m_originalPosition.z + BoundingVolume.z));
		}
	}
}

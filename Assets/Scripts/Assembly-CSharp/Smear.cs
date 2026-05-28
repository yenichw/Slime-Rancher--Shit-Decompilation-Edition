using System.Collections.Generic;
using UnityEngine;

public class Smear : MonoBehaviour
{
	private Queue<Vector3> m_recentPositions = new Queue<Vector3>();

	public int FramesBufferSize;

	public Renderer Renderer;

	private Material m_instancedMaterial;

	private Material InstancedMaterial
	{
		get
		{
			return m_instancedMaterial;
		}
		set
		{
			m_instancedMaterial = value;
		}
	}

	private void Start()
	{
		InstancedMaterial = Renderer.material;
	}

	private void LateUpdate()
	{
		if (m_recentPositions.Count > FramesBufferSize)
		{
			InstancedMaterial.SetVector("_PrevPosition", m_recentPositions.Dequeue());
		}
		InstancedMaterial.SetVector("_Position", base.transform.position);
		m_recentPositions.Enqueue(base.transform.position);
	}
}

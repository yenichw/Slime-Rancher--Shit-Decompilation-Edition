using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxColliderSolid : MonoBehaviour
{
	public Color color = new Color(1f, 1f, 1f, 0.8f);

	public void OnDrawGizmos()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		Gizmos.color = color;
		Gizmos.DrawCube(base.transform.TransformPoint(component.center), base.transform.TransformVector(component.size));
	}
}

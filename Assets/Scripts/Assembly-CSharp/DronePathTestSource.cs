using System.Collections.Generic;
using UnityEngine;

public class DronePathTestSource : MonoBehaviour
{
	public void OnDrawGizmos()
	{
		DroneNetwork componentInParent = GetComponentInParent<DroneNetwork>();
		Queue<Vector3> queue = componentInParent.GeneratePath(end: componentInParent.GetComponentInChildren<DronePathTestDest>().transform.position, start: base.transform.position);
		if (queue != null)
		{
			Gizmos.color = Color.green;
			Vector3 vector = queue.Dequeue();
			Gizmos.DrawLine(base.transform.position, vector);
			while (queue.Count > 0)
			{
				Vector3 vector2 = queue.Dequeue();
				Gizmos.DrawLine(vector, vector2);
				vector = vector2;
			}
		}
	}
}

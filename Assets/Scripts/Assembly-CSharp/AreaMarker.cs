using UnityEngine;

public class AreaMarker : SRBehaviour
{
	private class LinkMarker : SRBehaviour
	{
		public Color color;

		public Vector3 from;

		public Vector3 to;
	}

	public Color color;

	public float radius;

	public static GameObject CreateMarkerObject(Transform parent, Vector3 pos, Color color, float radius)
	{
		GameObject obj = new GameObject();
		obj.transform.parent = parent;
		obj.transform.localPosition = pos;
		AreaMarker areaMarker = obj.AddComponent<AreaMarker>();
		areaMarker.color = color;
		areaMarker.radius = radius;
		return obj;
	}

	public static void Link(GameObject gameObj1, GameObject gameObj2, Color color)
	{
		LinkMarker linkMarker = gameObj1.AddComponent<LinkMarker>();
		linkMarker.color = color;
		linkMarker.from = gameObj1.transform.position;
		linkMarker.to = gameObj2.transform.position;
	}

	public static void Link(Vector3 pos1, Vector3 pos2, Color color)
	{
		LinkMarker linkMarker = new GameObject().AddComponent<LinkMarker>();
		linkMarker.color = color;
		linkMarker.from = pos1;
		linkMarker.to = pos2;
	}
}

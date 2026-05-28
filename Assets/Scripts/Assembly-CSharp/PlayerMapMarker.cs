using UnityEngine;

public class PlayerMapMarker : MapMarker
{
	public RectTransform arrowRect;

	public RectTransform iconRect;

	public override void Rotate(Quaternion rotation)
	{
		arrowRect.rotation = Quaternion.Euler(0f, 0f, 0f - (rotation.eulerAngles.y + 45f));
	}
}

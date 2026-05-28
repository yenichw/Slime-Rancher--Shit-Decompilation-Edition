using UnityEngine;

public class MapMarker : MonoBehaviour
{
	private RectTransform rect;

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	public virtual Quaternion GetRotation()
	{
		return rect.localRotation;
	}

	public virtual void Rotate(Quaternion rotation)
	{
		rect.rotation = rotation;
	}

	public virtual Vector2 GetSize()
	{
		return rect.sizeDelta;
	}

	public virtual void SetSize(float height, float width)
	{
		rect.sizeDelta = new Vector2(width, height);
	}

	public virtual void SetAnchoredPosition(Vector3 position)
	{
		rect.anchoredPosition = position;
	}

	public virtual Vector3 GetLocalPosition()
	{
		return base.gameObject.transform.localPosition;
	}
}

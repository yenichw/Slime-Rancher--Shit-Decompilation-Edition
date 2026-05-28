using DG.Tweening;
using UnityEngine;

public class TweenUtil : MonoBehaviour
{
	private const float MIN_SCALE = 0.001f;

	public static Tweener ScaleIn(GameObject obj, float time, Ease easeType = Ease.OutQuad)
	{
		Vector3 fromValue = new Vector3(0.001f, 0.001f, 0.001f);
		Vector3 localScale = obj.transform.localScale;
		if (obj.GetComponent<ScaleYOnlyMarker>() != null)
		{
			fromValue = new Vector3(localScale.x, 0.001f, localScale.z);
		}
		else if (obj.GetComponent<ScaleZOnlyMarker>() != null)
		{
			fromValue = new Vector3(localScale.x, localScale.y, 0.001f);
		}
		return obj.transform.DOScale(obj.transform.localScale, time).From(fromValue).SetEase(easeType);
	}

	public static Tweener ScaleOut(GameObject obj, float time, Ease easeType = Ease.InQuad)
	{
		Vector3 endValue = new Vector3(0.001f, 0.001f, 0.001f);
		Vector3 localScale = obj.transform.localScale;
		if (obj.GetComponent<ScaleYOnlyMarker>() != null)
		{
			endValue = new Vector3(localScale.x, 0.001f, localScale.z);
		}
		else if (obj.GetComponent<ScaleZOnlyMarker>() != null)
		{
			endValue = new Vector3(localScale.x, localScale.y, 0.001f);
		}
		return obj.transform.DOScale(endValue, time).SetEase(easeType);
	}

	public static Tweener ScaleTo(GameObject obj, Vector3 scaleTo, float time, Ease easeType = Ease.InOutQuad)
	{
		return obj.transform.DOScale(scaleTo, time).SetEase(easeType);
	}
}

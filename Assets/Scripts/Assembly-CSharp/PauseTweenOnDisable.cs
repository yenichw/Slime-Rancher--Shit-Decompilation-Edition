using DG.Tweening;
using UnityEngine;

public class PauseTweenOnDisable : MonoBehaviour
{
	public Tween tween;

	public void OnEnable()
	{
		if (tween != null && tween.IsActive() && !tween.IsComplete())
		{
			tween.Play();
		}
	}

	public void OnDisable()
	{
		if (tween != null && tween.IsActive() && !tween.IsComplete())
		{
			tween.Pause();
		}
	}
}

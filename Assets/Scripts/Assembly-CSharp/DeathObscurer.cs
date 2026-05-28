using System;
using UnityEngine;
using UnityEngine.UI;

public class DeathObscurer : MonoBehaviour
{
	private Image bgImage;

	private float targetAlpha;

	private float adjust;

	public void Start()
	{
		LockOnDeath instance = SRSingleton<LockOnDeath>.Instance;
		instance.onLockChanged = (LockOnDeath.OnLockChanged)Delegate.Combine(instance.onLockChanged, new LockOnDeath.OnLockChanged(OnLocked));
		bgImage = GetComponent<Image>();
		adjust = 1f;
		SRSingleton<SceneContext>.Instance.PlayerState.onEndGame += delegate
		{
			Color color = bgImage.color;
			color.a = 0f;
			bgImage.color = color;
			targetAlpha = 0f;
		};
	}

	public void OnLocked(bool locked)
	{
		if (locked)
		{
			targetAlpha = 1f;
			base.gameObject.SetActive(value: true);
		}
		else
		{
			targetAlpha = 0f;
		}
	}

	public void Update()
	{
		float a = bgImage.color.a;
		if (a < targetAlpha)
		{
			bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, Mathf.Min(targetAlpha, a + adjust * Time.deltaTime));
		}
		else if (a > targetAlpha)
		{
			bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, Mathf.Max(targetAlpha, a - adjust * Time.deltaTime));
		}
		if (bgImage.color.a == 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}

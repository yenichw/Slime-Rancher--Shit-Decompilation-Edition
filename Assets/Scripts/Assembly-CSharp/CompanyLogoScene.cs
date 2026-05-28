using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanyLogoScene : MonoBehaviour
{
	private enum LogoSceneState
	{
		None = 0,
		FirstFrameProcessed = 1,
		StartedLogoFadeIn = 2,
		LogoWaitTime = 3,
		StartedLogoFadeOut = 4,
		SplashPreFadeInWait = 5,
		StartedSplashFadeIn = 6,
		ReadyToStartLoad = 7,
		Loading = 8
	}

	public CanvasGroup logo;

	public CanvasGroup splashBackground;

	public float logoFadeInTime;

	public float logoHoldTime;

	public float logoFadeOutTime;

	public float splashFadeInWaitTime;

	public float splashFadeInTime;

	private float timeAcc;

	private LogoSceneState currentState;

	public void Update()
	{
		if (currentState == LogoSceneState.None)
		{
			currentState = LogoSceneState.FirstFrameProcessed;
		}
		else if (currentState == LogoSceneState.FirstFrameProcessed)
		{
			currentState = LogoSceneState.StartedLogoFadeIn;
			timeAcc = 0f;
		}
		else if (currentState == LogoSceneState.StartedLogoFadeIn)
		{
			timeAcc += Time.deltaTime / logoFadeInTime;
			if ((double)timeAcc > 1.0)
			{
				currentState = LogoSceneState.LogoWaitTime;
				logo.alpha = 1f;
				timeAcc = 0f;
			}
			else
			{
				logo.alpha = Mathf.Lerp(0f, 1f, timeAcc);
			}
		}
		else if (currentState == LogoSceneState.LogoWaitTime)
		{
			timeAcc += Time.deltaTime / logoHoldTime;
			if ((double)timeAcc > 1.0)
			{
				currentState = LogoSceneState.StartedLogoFadeOut;
				timeAcc = 0f;
			}
		}
		else if (currentState == LogoSceneState.StartedLogoFadeOut)
		{
			timeAcc += Time.deltaTime / logoFadeOutTime;
			if ((double)timeAcc > 1.0)
			{
				currentState = LogoSceneState.SplashPreFadeInWait;
				logo.alpha = 0f;
				timeAcc = 0f;
			}
			else
			{
				logo.alpha = Mathf.Lerp(1f, 0f, timeAcc);
			}
		}
		else if (currentState == LogoSceneState.SplashPreFadeInWait)
		{
			timeAcc += Time.deltaTime / splashFadeInWaitTime;
			if ((double)timeAcc > 1.0)
			{
				currentState = LogoSceneState.StartedSplashFadeIn;
				timeAcc = 0f;
			}
		}
		else if (currentState == LogoSceneState.StartedSplashFadeIn)
		{
			timeAcc += Time.deltaTime / splashFadeInTime;
			if ((double)timeAcc > 1.0)
			{
				currentState = LogoSceneState.ReadyToStartLoad;
				splashBackground.alpha = 1f;
				timeAcc = 0f;
			}
			else
			{
				splashBackground.alpha = Mathf.Lerp(0f, 1f, timeAcc);
			}
		}
		else if (currentState == LogoSceneState.ReadyToStartLoad)
		{
			currentState = LogoSceneState.Loading;
			SceneManager.LoadScene("StandaloneStart", LoadSceneMode.Single);
		}
	}
}

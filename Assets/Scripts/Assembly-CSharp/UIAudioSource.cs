using UnityEngine;

public class UIAudioSource : SRBehaviour
{
	public SECTR_AudioCue onEnable;

	public bool skipOnEnableIfPaused;

	public void OnEnable()
	{
		if (onEnable != null && (!skipOnEnableIfPaused || Time.timeScale > 0f))
		{
			SECTR_AudioSystem.Play(onEnable, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
		}
	}

	public void PlayClick()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.clickCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}

	public void PlayPurchase()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}

	public void PlayPurchaseExpansion()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseExpansionCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}

	public void PlayPurchasePlot()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.purchasePlotCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}

	public void PlayPurchaseUpgrade()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseUpgradeCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}

	public void PlayPurchasePersonalUpgrade()
	{
		SECTR_AudioSystem.Play(SRSingleton<GameContext>.Instance.UITemplates.purchasePersonalUpgradeCue, SECTR_AudioSystem.Listener, Vector3.zero, loop: false);
	}
}

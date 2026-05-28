using UnityEngine;

public class ActivateOnInstrumentsUnlocked : MonoBehaviour
{
	public void Start()
	{
		SRSingleton<SceneContext>.Instance.InstrumentDirector.onInstrumentUnlocked += OnInstrumentUnlocked;
		OnInstrumentUnlocked();
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.InstrumentDirector != null)
		{
			SRSingleton<SceneContext>.Instance.InstrumentDirector.onInstrumentUnlocked -= OnInstrumentUnlocked;
		}
	}

	private void OnInstrumentUnlocked()
	{
		base.gameObject.SetActive(SRSingleton<SceneContext>.Instance.InstrumentDirector.GetUnlockedInstruments().Count > 1);
	}
}

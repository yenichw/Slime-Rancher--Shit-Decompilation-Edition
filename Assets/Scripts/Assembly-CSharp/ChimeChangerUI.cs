using UnityEngine;
using UnityEngine.UI;

public class ChimeChangerUI : MonoBehaviour
{
	public Image iconImage;

	public void Start()
	{
		ShowInstrument(SRSingleton<SceneContext>.Instance.InstrumentDirector.currentInstrument);
		SRSingleton<SceneContext>.Instance.InstrumentDirector.onInstrumentChanged += ShowInstrument;
	}

	public void ShowInstrument(EchoNoteGameMetadata instrument)
	{
		iconImage.sprite = instrument.icon;
	}

	private void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.InstrumentDirector != null)
		{
			SRSingleton<SceneContext>.Instance.InstrumentDirector.onInstrumentChanged -= ShowInstrument;
		}
	}
}

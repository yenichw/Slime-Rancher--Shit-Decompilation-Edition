using UnityEngine.UI;

public class CannotStoreGifUI : SRBehaviour
{
	public Toggle bufferForGifToggle;

	public void Awake()
	{
		bufferForGifToggle.isOn = SRSingleton<GameContext>.Instance.OptionsDirector.bufferForGif;
	}

	public void OK()
	{
		Destroyer.Destroy(base.gameObject, "CannotStoreGifUI.OK");
	}

	public void ToggleBufferForGif()
	{
		SRSingleton<GameContext>.Instance.OptionsDirector.bufferForGif = bufferForGifToggle.isOn;
		SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveProfile();
	}
}

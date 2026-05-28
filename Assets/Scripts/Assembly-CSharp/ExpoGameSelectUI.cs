using UnityEngine;

public class ExpoGameSelectUI : BaseUI
{
	public GameObject mainMenuUIPrefab;

	public void LoadGame(TextAsset asset)
	{
		SRSingleton<GameContext>.Instance.AutoSaveDirector.BeginLoad("", asset.name, delegate
		{
		});
	}

	public override void Close()
	{
		Object.Instantiate(mainMenuUIPrefab);
		base.Close();
	}
}

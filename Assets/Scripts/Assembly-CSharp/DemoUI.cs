using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoUI : BaseUI
{
	private const uint appId = 433340u;

	private const string steamStoreUrl = "http://store.steampowered.com/app/433340/";

	protected override bool Closeable()
	{
		return false;
	}

	public void OpenStore()
	{
		Application.OpenURL("http://store.steampowered.com/app/433340/");
		Quit();
	}

	public void Quit()
	{
		Close();
		SRSingleton<SceneContext>.Instance.OnSessionEnded();
		SceneManager.LoadScene("MainMenu");
	}
}

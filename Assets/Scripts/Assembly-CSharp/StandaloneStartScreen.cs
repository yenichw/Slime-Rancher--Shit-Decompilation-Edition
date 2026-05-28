using UnityEngine;
using UnityEngine.SceneManagement;

public class StandaloneStartScreen : MonoBehaviour
{
	private bool isLoading;

	private bool pastFirstFrame;

	public void Update()
	{
		if (!pastFirstFrame)
		{
			pastFirstFrame = true;
		}
		else if (!isLoading)
		{
			isLoading = true;
			SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
		}
	}
}

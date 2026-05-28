using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Levels
{
	public const string COMPANY_LOGO = "CompanyLogoScene";

	public const string STANDALONE_START = "StandaloneStart";

	public const string MAIN_MENU = "MainMenu";

	public const string XBOX_ONE_START = "XboxOneStart";

	public const string UWP_START = "UWPStart";

	public const string PS4_START = "PS4Start";

	public const string GAMECORE_XBOX_START = "GameCoreXboxStart";

	public const string WORLD = "worldGenerated";

	private static bool isActiveSceneSpecial;

	private static HashSet<string> specialScenes;

	static Levels()
	{
		isActiveSceneSpecial = true;
		specialScenes = new HashSet<string> { "CompanyLogoScene", "StandaloneStart", "XboxOneStart", "GameCoreXboxStart", "UWPStart", "PS4Start", "MainMenu" };
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		isActiveSceneSpecial = isSpecial(SceneManager.GetActiveScene().name);
	}

	private static void OnActiveSceneChanged(Scene replaced, Scene next)
	{
		isActiveSceneSpecial = isSpecial(next.name);
	}

	public static bool isSpecialNonAlloc()
	{
		return isActiveSceneSpecial;
	}

	public static bool isSpecial()
	{
		return isSpecial(SceneManager.GetActiveScene().name);
	}

	private static bool isSpecial(string name)
	{
		return specialScenes.Contains(name);
	}

	public static bool isMainMenu()
	{
		return IsLevel("MainMenu");
	}

	public static bool IsLevel(string name)
	{
		return SceneManager.GetActiveScene().name == name;
	}
}

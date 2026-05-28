using UnityEngine;

public class ScoreUI : SRBehaviour
{
	public GUISkin skin;

	private PlayerState player;

	private void Start()
	{
		player = ((SRSingleton<GameContext>.Instance == null) ? null : SRSingleton<SceneContext>.Instance.PlayerState);
	}

	private void OnGUI()
	{
		GUI.skin = skin;
		GUI.Label(new Rect(25f, 25f, 250f, 40f), "$" + player.GetCurrency());
	}
}

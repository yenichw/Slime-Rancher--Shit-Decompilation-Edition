using InControl;
using UnityEngine;
using UnityEngine.UI;

public class GameCoreXboxStartScreen : MonoBehaviour
{
	private class EngagementScreenActions : PlayerActionSet
	{
		public PlayerAction Engage;

		public EngagementScreenActions()
		{
			Engage = CreatePlayerAction("Engage");
		}
	}

	public Text actionText;

	public Text gamerNameText;

	public GameObject happySlime;

	private EngagementScreenActions engagementActions;
}

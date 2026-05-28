using UnityEngine;

[CreateAssetMenu(menuName = "Player/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
	[SerializeField]
	private PlayerState.Upgrade upgrade;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private int cost;

	public PlayerState.Upgrade Upgrade => upgrade;

	public Sprite Icon => icon;

	public int Cost => cost;
}

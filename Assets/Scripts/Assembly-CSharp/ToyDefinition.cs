using UnityEngine;

[CreateAssetMenu(menuName = "Toy/Toy Definition")]
public class ToyDefinition : ScriptableObject
{
	[SerializeField]
	private Identifiable.Id toyId;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private int cost;

	[SerializeField]
	private string nameKey;

	public Identifiable.Id ToyId => toyId;

	public Sprite Icon => icon;

	public int Cost => cost;

	public string NameKey => nameKey;
}

using UnityEngine;

[CreateAssetMenu(menuName = "World/Liquid Definition")]
public class LiquidDefinition : ScriptableObject
{
	[SerializeField]
	private Identifiable.Id id;

	[SerializeField]
	private GameObject inFX;

	[SerializeField]
	private GameObject vacFailFX;

	public Identifiable.Id Id => id;

	public GameObject InFx => inFX;

	public GameObject VacFailFx => vacFailFX;
}

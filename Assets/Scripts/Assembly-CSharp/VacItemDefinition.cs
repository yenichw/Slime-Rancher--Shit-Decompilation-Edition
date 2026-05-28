using UnityEngine;

[CreateAssetMenu(menuName = "Vac/Vac Item Definition")]
public class VacItemDefinition : ScriptableObject
{
	[SerializeField]
	private Identifiable.Id id;

	[SerializeField]
	private Color color;

	[SerializeField]
	private Sprite icon;

	public Identifiable.Id Id => id;

	public Color Color => color;

	public Sprite Icon => icon;

	public static VacItemDefinition CreateVacItemDefinition(Identifiable.Id id, Color color, Sprite icon)
	{
		VacItemDefinition vacItemDefinition = ScriptableObject.CreateInstance<VacItemDefinition>();
		vacItemDefinition.id = id;
		vacItemDefinition.color = color;
		vacItemDefinition.icon = icon;
		return vacItemDefinition;
	}
}

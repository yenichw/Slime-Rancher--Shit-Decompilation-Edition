using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Slime Appearance Element")]
public class SlimeAppearanceElement : ScriptableObject
{
	public string Name;

	public SlimeAppearanceObject[] Prefabs;
}

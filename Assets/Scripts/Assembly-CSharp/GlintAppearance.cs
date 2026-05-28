using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Slimes/Appearance Extras/Glint Appearance")]
public class GlintAppearance : ScriptableObject
{
	public GameObject suspendedGlintPrefab;

	public GameObject readyGlintPrefab;

	public GameObject freeGlintPrefab;
}

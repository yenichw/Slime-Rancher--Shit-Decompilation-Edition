using System.Collections.Generic;
using UnityEngine;

public class DebugDirector : SRBehaviour
{
	[Tooltip("Debug UI prefab (default).")]
	public GameObject uiDefaultPrefab;

	[Tooltip("Debug UI prefab (gamepad).")]
	public GameObject uiGamepadPrefab;

	[Tooltip("Ginger tracker prefab.")]
	public GameObject gingerTrackerPrefab;

	[Tooltip("Slime Appearance Director")]
	public SlimeAppearanceDirector slimeAppearanceDirector;

	[Tooltip("List of explicit prefabs to be spawnable.")]
	public List<GameObject> spawnablePrefabs;

	[Tooltip("List of ui prefabs to be instantiated.")]
	public List<GameObject> uiPrefabs;

	[Tooltip("List of Viktor imposto prefabs. (generated)")]
	public List<GameObject> impostos;

	[Tooltip("List of EchoNoteGameMetadata assets.")]
	public List<EchoNoteGameMetadata> echoNoteGameMetadatas;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EchoNoteGordo", menuName = "Echo Notes/Create Cluster")]
public class EchoNoteClusterMetadata : ScriptableObject
{
	[Tooltip("List of ordered clip indices in 'cue'; add multiple clips separated by commas. (eg. '1, 2, 3').")]
	public List<string> clips;

	[Tooltip("Distance between each clip (generation only).")]
	[Range(0f, 20f)]
	public float distance = 2f;
}

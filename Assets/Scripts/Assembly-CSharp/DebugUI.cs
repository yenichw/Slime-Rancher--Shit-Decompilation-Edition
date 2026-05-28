using UnityEngine;

public class DebugUI : SRBehaviour
{
	[Tooltip("Grid object to parent buttons to.")]
	public GameObject grid;

	[Tooltip("Debug button prefab.")]
	public GameObject buttonPrefab;

	[Tooltip("Tab button left/previous. (optional)")]
	public GameObject buttonTabLeft;

	[Tooltip("Tab button right/next. (optional)")]
	public GameObject buttonTabRight;

	[Tooltip("Number of buttons to display on each page.")]
	public int buttonsPerPage = 10;
}

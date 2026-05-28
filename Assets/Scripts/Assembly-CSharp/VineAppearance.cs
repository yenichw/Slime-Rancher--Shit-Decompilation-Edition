using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Slimes/Appearance Extras/Vine Appearance")]
public class VineAppearance : ScriptableObject
{
	public GameObject vinePrefab;

	public GameObject vineEnterFx;

	public GameObject vineExitFx;
}

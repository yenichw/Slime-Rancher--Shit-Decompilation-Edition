using System.Collections.Generic;
using UnityEngine;

public class TutorialRadar : MonoBehaviour
{
	public TutorialDirector.Id tutorialId;

	public static List<TutorialRadar> allRadars = new List<TutorialRadar>();

	public void Awake()
	{
		allRadars.Add(this);
	}

	public void OnDestroy()
	{
		allRadars.Remove(this);
	}
}

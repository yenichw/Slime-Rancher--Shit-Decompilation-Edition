using System.Collections.Generic;
using UnityEngine;

public class MusicBoxRegion : SRBehaviour
{
	public const float EXTRA_CALMING_FACTOR = 1f;

	private List<SlimeEmotions> currentEmotions = new List<SlimeEmotions>();

	private List<SlimeEmotions> newEmotions = new List<SlimeEmotions>();

	public void OnDisable()
	{
		foreach (SlimeEmotions currentEmotion in currentEmotions)
		{
			if (currentEmotion != null)
			{
				currentEmotion.RemoveMusicBox(this);
				newEmotions.Add(currentEmotion);
			}
		}
		List<SlimeEmotions> list = currentEmotions;
		list.Clear();
		currentEmotions = newEmotions;
		newEmotions = list;
	}

	public void OnTriggerEnter(Collider collider)
	{
		SlimeEmotions component = collider.gameObject.GetComponent<SlimeEmotions>();
		if (component != null)
		{
			component.AddMusicBox(this);
			currentEmotions.Add(component);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		SlimeEmotions component = collider.gameObject.GetComponent<SlimeEmotions>();
		if (component != null)
		{
			component.RemoveMusicBox(this);
			currentEmotions.Remove(component);
		}
	}
}

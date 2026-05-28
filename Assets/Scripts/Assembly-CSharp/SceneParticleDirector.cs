using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneParticleDirector : MonoBehaviour
{
	private List<PooledSceneParticle> waitingForSecondFrame = new List<PooledSceneParticle>();

	private bool hasAlreadyNotified;

	public void InitForLevel()
	{
		StartCoroutine(NotifyOnSecondFrame());
	}

	public void AddSecondFrameListener(PooledSceneParticle particle)
	{
		if (hasAlreadyNotified)
		{
			particle.OnSecondFrame();
		}
		else
		{
			waitingForSecondFrame.Add(particle);
		}
	}

	private IEnumerator NotifyOnSecondFrame()
	{
		yield return null;
		foreach (PooledSceneParticle item in waitingForSecondFrame)
		{
			item.OnSecondFrame();
		}
		waitingForSecondFrame.Clear();
		hasAlreadyNotified = true;
	}
}

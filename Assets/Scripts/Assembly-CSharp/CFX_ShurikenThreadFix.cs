using System.Collections;
using UnityEngine;

public class CFX_ShurikenThreadFix : MonoBehaviour
{
	private ParticleSystem[] systems;

	private void OnEnable()
	{
		systems = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = systems;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = false;
		}
		StartCoroutine("WaitFrame");
	}

	private IEnumerator WaitFrame()
	{
		yield return null;
		ParticleSystem[] array = systems;
		foreach (ParticleSystem obj in array)
		{
			ParticleSystem.EmissionModule emission = obj.emission;
			emission.enabled = true;
			obj.Play(withChildren: true);
		}
	}
}

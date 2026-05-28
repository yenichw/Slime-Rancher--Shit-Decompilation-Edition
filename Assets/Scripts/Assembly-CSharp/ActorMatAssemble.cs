using System.Collections.Generic;
using UnityEngine;

public class ActorMatAssemble : MonoBehaviour
{
	public List<GameObject> objectsToAssemble;

	public float assembleDuration = 1.5f;

	private float assembleValue;

	private bool assembleDirection;

	private bool assembleComplete;

	public void Update()
	{
		if (assembleComplete)
		{
			return;
		}
		assembleValue += Time.deltaTime / assembleDuration * (float)(assembleDirection ? 1 : (-1));
		assembleValue = Mathf.Clamp(assembleValue, 0f, 1f);
		assembleComplete = assembleValue == (float)(assembleDirection ? 1 : 0);
		foreach (GameObject item in objectsToAssemble)
		{
			item.SetActive(assembleValue != 0f);
			item.GetComponent<Renderer>().material.SetFloat("_Assemble", assembleValue);
		}
	}

	public bool Assemble(bool direction)
	{
		assembleDirection = direction;
		assembleComplete = assembleValue == (float)(assembleDirection ? 1 : 0);
		return !assembleComplete;
	}
}

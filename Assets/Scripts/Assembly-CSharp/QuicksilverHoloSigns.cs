using System;
using System.Collections.Generic;
using UnityEngine;

public class QuicksilverHoloSigns : MonoBehaviour
{
	[Tooltip("Energy generator required to activate the signs.")]
	public QuicksilverEnergyGenerator generator;

	[Tooltip("Renderers expecting '_SpiralColor' to be updated.")]
	public List<Renderer> rendersToUpdate;

	[Tooltip("Objects to set active/inactive based off the generator state.")]
	public List<GameObject> partsToToggle;

	public void Awake()
	{
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Combine(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	public void OnDestroy()
	{
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Remove(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	private void OnGeneratorStateChanged()
	{
		bool flag = generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE;
		foreach (GameObject item in partsToToggle)
		{
			item.SetActive(flag);
		}
		float value = (flag ? 0.5f : 0f);
		foreach (Renderer item2 in rendersToUpdate)
		{
			item2.material.SetFloat("_SpiralColor", value);
		}
	}
}

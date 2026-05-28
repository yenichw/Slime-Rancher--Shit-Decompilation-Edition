using System.Collections.Generic;
using UnityEngine;

public class QuicksilverOreStack : MonoBehaviour
{
	public List<GameObject> oreRocks;

	public float springStrength = 200f;

	public float springDamper = 5f;

	public ParticleSystem activeFX;

	public ParticleSystem idleFX;

	private bool isActive;

	private SpringJoint[] oreJoints;

	private void Start()
	{
		oreJoints = new SpringJoint[oreRocks.Count];
		for (int i = 0; i < oreRocks.Count; i++)
		{
			SpringJoint component = oreRocks[i].gameObject.GetComponent<SpringJoint>();
			oreJoints[i] = component;
		}
		DeactivateFX();
	}

	private void ActivateSprings()
	{
		for (int i = 0; i < oreJoints.Length; i++)
		{
			oreJoints[i].spring = springStrength;
			oreJoints[i].damper = springDamper;
			oreRocks[i].gameObject.GetComponent<Rigidbody>().WakeUp();
		}
		ActivateFX();
	}

	private void DeactivateSprings()
	{
		for (int i = 0; i < oreJoints.Length; i++)
		{
			oreJoints[i].spring = 0f;
			oreJoints[i].damper = 0f;
			oreRocks[i].gameObject.GetComponent<Rigidbody>().WakeUp();
		}
		DeactivateFX();
	}

	private void ActivateFX()
	{
		activeFX.Play();
		idleFX.Stop();
	}

	private void DeactivateFX()
	{
		activeFX.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		idleFX.Play();
	}

	public void ToggleOre()
	{
		if (isActive)
		{
			DeactivateSprings();
		}
		else
		{
			ActivateSprings();
		}
	}
}

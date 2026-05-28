using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGateActivator : MonoBehaviour
{
	[Serializable]
	public class SequenceEntry
	{
		public GameObject toDeactivate;

		public SECTR_AudioCue cue;
	}

	public float sequenceStepDelay = 1f;

	public float stingerDelay = 6f;

	public SequenceEntry[] deactivateSequence;

	public GameObject deactivateAfterSequence;

	public AccessDoor gateDoor;

	[Tooltip("The sound to play after all slot cues are played.")]
	public SECTR_AudioCue stingerCue;

	private int playersPresent;

	public void OnTriggerEnter(Collider col)
	{
		if (PhysicsUtil.IsPlayerMainCollider(col))
		{
			playersPresent++;
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (PhysicsUtil.IsPlayerMainCollider(col))
		{
			playersPresent--;
		}
	}

	public void Update()
	{
		if (playersPresent > 0)
		{
			TryToActivate();
		}
	}

	public void TryToActivate()
	{
		if (gateDoor.CurrState == AccessDoor.State.CLOSED)
		{
			gateDoor.CurrState = AccessDoor.State.OPEN;
			StartCoroutine(DoDeactivateSequence());
			AnalyticsUtil.CustomEvent("PuzzleOpened", new Dictionary<string, object> { { "name", base.name } });
		}
	}

	private IEnumerator DoDeactivateSequence()
	{
		SequenceEntry[] array = deactivateSequence;
		foreach (SequenceEntry sequenceEntry in array)
		{
			SECTR_AudioSystem.Play(sequenceEntry.cue, sequenceEntry.toDeactivate.transform.position, loop: false);
			sequenceEntry.toDeactivate.SetActive(value: false);
			yield return new WaitForSeconds(sequenceStepDelay);
		}
		if (deactivateAfterSequence != null)
		{
			deactivateAfterSequence.SetActive(value: false);
		}
		if (deactivateSequence.Length != 0)
		{
			yield return new WaitForSeconds(stingerDelay - sequenceStepDelay);
			SECTR_AudioSystem.Play(stingerCue, deactivateSequence[deactivateSequence.Length - 1].toDeactivate.transform.position, loop: false);
		}
		yield return null;
	}
}

using System;
using UnityEngine;

public class PuzzleSlotLockable : SRBehaviour
{
	[Tooltip("The slots we monitor.")]
	public PuzzleSlot[] slots;

	[Tooltip("Any objects we need to activate on unlock.")]
	public GameObject[] activateOnUnlock;

	[Tooltip("The sounds to play when the slots are opened.")]
	public SECTR_AudioCue[] slotCues;

	[Tooltip("If true, the game will be automatically save when the puzzle is unlocked.")]
	public bool autoSaveOnUnlock;

	public void Awake()
	{
		PuzzleSlot[] array = slots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RegisterLock(this);
		}
	}

	public virtual void NotifySlotChanged(bool immediate = false)
	{
		if (ShouldUnlock())
		{
			ActivateOnUnlock();
		}
	}

	private void ActivateOnUnlock()
	{
		if (activateOnUnlock == null)
		{
			return;
		}
		GameObject[] array = activateOnUnlock;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
			if (autoSaveOnUnlock)
			{
				SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
			}
		}
	}

	public bool ShouldUnlock()
	{
		PuzzleSlot[] array = slots;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsLocked())
			{
				return false;
			}
		}
		return true;
	}

	public SECTR_AudioCue GetCueForLastSlot()
	{
		int num = 0;
		PuzzleSlot[] array = slots;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsLocked())
			{
				num++;
			}
		}
		return slotCues[Math.Max(0, Math.Min(slotCues.Length - 1, num - 1))];
	}

	public void PlayCue(SECTR_AudioCue cue)
	{
		SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}
}

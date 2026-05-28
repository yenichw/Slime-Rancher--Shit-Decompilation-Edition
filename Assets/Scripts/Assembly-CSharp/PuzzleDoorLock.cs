using UnityEngine;

public class PuzzleDoorLock : PuzzleSlotLockable
{
	[Tooltip("The door we control the lock on.")]
	public AccessDoor door;

	public override void NotifySlotChanged(bool immediate = false)
	{
		if (ShouldUnlock() && door.CurrState == AccessDoor.State.LOCKED)
		{
			door.CurrState = AccessDoor.State.CLOSED;
		}
		base.NotifySlotChanged(immediate);
	}
}

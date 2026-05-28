using System.Collections.Generic;
using UnityEngine;

public class PopupDirector : MonoBehaviour
{
	public interface Popup
	{
		bool ShouldClear();
	}

	public abstract class PopupCreator
	{
		public abstract override bool Equals(object other);

		public abstract override int GetHashCode();

		public abstract void Create();

		public abstract bool ShouldClear();
	}

	private Queue<PopupCreator> popupQueue = new Queue<PopupCreator>();

	private Popup currPopup;

	private bool quitting;

	private TimeDirector timeDir;

	private int suppressors;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void InitForLevel()
	{
		popupQueue.Clear();
	}

	public bool IsQueued(PopupCreator creator)
	{
		return popupQueue.Contains(creator);
	}

	public void QueueForPopup(PopupCreator creator)
	{
		popupQueue.Enqueue(creator);
		MaybePopupNext();
	}

	public void MaybePopupNext()
	{
		if (SRSingleton<SceneContext>.Instance != null && popupQueue.Count > 0 && currPopup == null && suppressors <= 0)
		{
			PopupCreator popupCreator = popupQueue.Dequeue();
			if (popupCreator.ShouldClear())
			{
				MaybePopupNext();
			}
			else
			{
				popupCreator.Create();
			}
		}
	}

	public void CheckShouldClear()
	{
		if (currPopup != null && currPopup.ShouldClear())
		{
			Destroyer.Destroy(((Component)currPopup).gameObject, "PopupDirector.CheckShouldClear");
		}
	}

	public void PopupActivated(Popup popup)
	{
		if (currPopup != null)
		{
			Log.Warning("Popup arrived with already-active popup.");
		}
		currPopup = popup;
	}

	public void PopupDeactivated(Popup popup)
	{
		if (currPopup == popup && !quitting)
		{
			currPopup = null;
			timeDir.OnUnpause(OnUnpause);
		}
		else
		{
			Log.Warning("Popup deactivated, but wasn't current popup.");
		}
	}

	public void RegisterSuppressor()
	{
		suppressors++;
	}

	public void UnregisterSuppressor()
	{
		suppressors--;
		if (suppressors <= 0)
		{
			MaybePopupNext();
		}
	}

	public void OnUnpause()
	{
		MaybePopupNext();
	}

	public void OnApplicationQuit()
	{
		quitting = true;
	}

	public void OnDestroy()
	{
		timeDir.ClearOnUnpause(OnUnpause);
	}
}

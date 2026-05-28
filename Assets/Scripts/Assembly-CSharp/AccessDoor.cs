using System;
using System.Collections;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class AccessDoor : IdHandler, AccessDoorModel.Participant
{
	public enum State
	{
		LOCKED = 0,
		OPEN = 1,
		CLOSED = 2
	}

	[Serializable]
	public class DoorPurchaseItem
	{
		public Sprite icon;

		public Sprite img;

		public int cost;
	}

	private class DoorAnimatorUpdater : MonoBehaviour
	{
		private AccessDoor door;

		private int animOpenId;

		private bool updateRequest = true;

		public void Init(AccessDoor door)
		{
			this.door = door;
			animOpenId = Animator.StringToHash("Open");
			ForceUpdate();
		}

		public void OnEnable()
		{
			ForceUpdate();
		}

		public void Update()
		{
			if (updateRequest)
			{
				ForceUpdateBarrierController();
				ForceUpdateAnimator();
				updateRequest = false;
			}
		}

		public void ForceUpdate()
		{
			updateRequest = true;
			ForceUpdateBarrierController();
		}

		private void ForceUpdateBarrierController()
		{
			if (door != null && door.model != null)
			{
				BarrierController componentInChildren = GetComponentInChildren<BarrierController>();
				if (componentInChildren != null)
				{
					componentInChildren.SetIsOpen(door.model.state == State.OPEN);
				}
			}
		}

		private void ForceUpdateAnimator()
		{
			if (door != null && door.model != null)
			{
				Animator componentInChildren = GetComponentInChildren<Animator>();
				if (componentInChildren != null)
				{
					componentInChildren.SetBool(animOpenId, door.model.state == State.OPEN);
				}
			}
		}
	}

	public DoorPurchaseItem doorPurchase;

	public PediaDirector.Id lockedRegionId;

	public AccessDoor[] linkedDoors;

	[Tooltip("Progress to record when the door is unlocked.")]
	public ProgressDirector.ProgressType[] progress;

	public SECTR_AudioCue openCue;

	public float openCueDelay = 3f;

	[Tooltip("Other elements to include in the open/close animation.")]
	public Animator[] externalAnimators;

	public GameObject[] deactivateOnImmediateOpen;

	private int animOpenId;

	private int animOpenImmediateId;

	private AccessDoorModel model;

	private bool updateRequest = true;

	private bool updateRequestImmediate;

	public State CurrState
	{
		get
		{
			return model.state;
		}
		set
		{
			if (openCue != null && value == State.OPEN && model.state != State.OPEN)
			{
				StartCoroutine(DelayedPlayCue(openCueDelay));
			}
			model.state = value;
			MaybeRecountProgress();
			ForceUpdate(immediate: false);
		}
	}

	public virtual void Awake()
	{
		animOpenId = Animator.StringToHash("Open");
		animOpenImmediateId = Animator.StringToHash("OpenImmediate");
		Animator[] array = externalAnimators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.AddComponent<DoorAnimatorUpdater>().Init(this);
		}
		SRSingleton<SceneContext>.Instance.GameModel.RegisterDoor(base.id, base.gameObject);
	}

	public virtual void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterDoor(base.id);
		}
	}

	public void OnEnable()
	{
		if (model != null)
		{
			ForceUpdate(immediate: true);
		}
	}

	public void InitModel(AccessDoorModel model)
	{
		model.progress = progress;
	}

	public void SetModel(AccessDoorModel model)
	{
		this.model = model;
		MaybeRecountProgress();
		ForceUpdate(immediate: true);
	}

	public IEnumerator DelayedPlayCue(float delay)
	{
		yield return new WaitForSeconds(delay);
		SECTR_AudioSystem.Play(openCue, base.transform.position, loop: false);
	}

	public virtual bool MaybeRecountProgress()
	{
		if (CurrState != 0)
		{
			ProgressDirector progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
			ProgressDirector.ProgressType[] array = progress;
			foreach (ProgressDirector.ProgressType progressType in array)
			{
				int num = 0;
				foreach (AccessDoorModel value in SRSingleton<SceneContext>.Instance.GameModel.AllDoors().Values)
				{
					if (value.state != 0 && Array.IndexOf(value.progress, progressType) != -1)
					{
						num++;
					}
				}
				progressDirector.SetProgress(progressType, num);
			}
			return true;
		}
		return false;
	}

	private void ForceUpdate(bool immediate)
	{
		updateRequest = true;
		updateRequestImmediate |= immediate;
		ForceUpdateBarrierController();
		Animator[] array = externalAnimators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<DoorAnimatorUpdater>().ForceUpdate();
		}
	}

	public virtual void Update()
	{
		if (updateRequest)
		{
			ForceUpdateBarrierController();
			ForceUpdateAnimator();
			updateRequest = false;
			updateRequestImmediate = false;
		}
	}

	private void ForceUpdateBarrierController()
	{
		BarrierController componentInChildren = GetComponentInChildren<BarrierController>();
		if (updateRequestImmediate && CurrState == State.OPEN)
		{
			if (componentInChildren != null)
			{
				componentInChildren.SetIsOpen(isOpen: true);
			}
			if (deactivateOnImmediateOpen != null)
			{
				GameObject[] array = deactivateOnImmediateOpen;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
		}
		if (componentInChildren != null)
		{
			componentInChildren.SetIsOpen(CurrState == State.OPEN);
		}
	}

	private void ForceUpdateAnimator()
	{
		Animator componentInChildren = GetComponentInChildren<Animator>();
		if (updateRequestImmediate && CurrState == State.OPEN && componentInChildren != null)
		{
			componentInChildren.SetTrigger(animOpenImmediateId);
		}
		if (componentInChildren != null)
		{
			componentInChildren.SetBool(animOpenId, CurrState == State.OPEN);
		}
	}

	protected override string IdPrefix()
	{
		return "door";
	}
}

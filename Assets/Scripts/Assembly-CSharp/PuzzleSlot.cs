using System.Collections;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class PuzzleSlot : IdHandler, PuzzleSlotModel.Participant
{
	public Identifiable.Id catchId;

	public GameObject changeFX;

	public GameObject[] activateOnFill;

	public bool fillOnAwake;

	public SECTR_AudioCue localFillCue;

	private PuzzleSlotLockable puzLockable;

	private PuzzleSlotModel model;

	private const float LOCK_CUE_DELAY = 0.5f;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterSlot(base.id, base.gameObject);
	}

	public void RegisterLock(PuzzleSlotLockable puzzleLockable)
	{
		puzLockable = puzzleLockable;
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterSlot(base.id);
		}
	}

	public void InitModel(PuzzleSlotModel model)
	{
		if (fillOnAwake)
		{
			model.filled = true;
		}
	}

	public void SetModel(PuzzleSlotModel model)
	{
		this.model = model;
		OnFilledChanged();
	}

	public void OnFilledChangedFromModel()
	{
		OnFilledChanged();
	}

	private void OnFilledChanged()
	{
		if (model.filled)
		{
			ActivateOnFill();
		}
		if (puzLockable != null)
		{
			puzLockable.NotifySlotChanged(immediate: true);
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (collider.isTrigger)
		{
			return;
		}
		GameObject gameObject = collider.gameObject;
		Identifiable component = gameObject.GetComponent<Identifiable>();
		if (component != null && component.id == catchId && !model.filled)
		{
			model.filled = true;
			SRBehaviour.SpawnAndPlayFX(changeFX, gameObject.transform.position, gameObject.transform.rotation);
			ActivateOnFill();
			DestroyOnTouching component2 = gameObject.GetComponent<DestroyOnTouching>();
			if (component2 != null)
			{
				component2.NoteDestroying();
			}
			Destroyer.DestroyActor(gameObject, "PuzzleSlot.OnTriggerEnter");
			if (puzLockable != null)
			{
				puzLockable.NotifySlotChanged();
				SECTR_AudioCue cueForLastSlot = puzLockable.GetCueForLastSlot();
				SECTR_AudioSystem.Play(localFillCue, base.transform.position, loop: false);
				StartCoroutine(DelayedPlayLockCue(cueForLastSlot));
			}
		}
	}

	public IEnumerator DelayedPlayLockCue(SECTR_AudioCue cue)
	{
		yield return new WaitForSeconds(0.5f);
		puzLockable.PlayCue(cue);
	}

	public bool IsLocked()
	{
		if (model != null)
		{
			return !model.filled;
		}
		return true;
	}

	protected override string IdPrefix()
	{
		return "puz";
	}

	private void ActivateOnFill()
	{
		if (activateOnFill != null)
		{
			GameObject[] array = activateOnFill;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}
}

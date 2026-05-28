using UnityEngine;
using UnityEngine.UI;

public abstract class StorageSlotUI : MonoBehaviour
{
	public Image slotIcon;

	public Image frontFrameIcon;

	public Image backFrameIcon;

	public WorldStatusBar bar;

	public Sprite backEmpty;

	public Sprite backFilled;

	public Sprite frontEmpty;

	public Sprite frontFilled;

	private LookupDirector lookupDir;

	private Identifiable.Id? currentlyStoredId;

	public virtual void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
	}

	public void Update()
	{
		Identifiable.Id currentId = GetCurrentId();
		if (currentId != currentlyStoredId)
		{
			if (currentId == Identifiable.Id.NONE)
			{
				slotIcon.enabled = false;
				bar.currValue = 0f;
				bar.barColor = Color.black;
				frontFrameIcon.sprite = frontEmpty;
				backFrameIcon.sprite = backEmpty;
			}
			else
			{
				slotIcon.sprite = lookupDir.GetIcon(currentId);
				slotIcon.enabled = true;
				bar.barColor = lookupDir.GetColor(currentId);
				frontFrameIcon.sprite = frontFilled;
				backFrameIcon.sprite = backFilled;
			}
			currentlyStoredId = currentId;
		}
		if (currentId != 0)
		{
			bar.currValue = GetCurrentCount();
		}
	}

	protected abstract Identifiable.Id GetCurrentId();

	protected abstract int GetCurrentCount();
}

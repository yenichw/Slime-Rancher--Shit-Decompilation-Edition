using UnityEngine;

public class GordoStealth : MonoBehaviour
{
	private int vacTriggerCount;

	private bool stealth;

	private double goStealthAt = double.PositiveInfinity;

	private float tgtOpacity;

	private float currOpacity;

	private TimeDirector timeDir;

	private MaterialStealthController materialStealthController;

	private const float OPACITY_CHANGE_PER_SEC = 2f;

	private const float STEALTH_OPACITY = 0f;

	private const float STEALTH_DELAY_HRS = 1f / 12f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		materialStealthController = new MaterialStealthController(base.gameObject);
		SetStealth(stealth: true);
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.GetComponentInParent<TrackCollisions>() != null)
		{
			vacTriggerCount++;
			if (vacTriggerCount == 1)
			{
				SetStealth(stealth: false);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (col.GetComponentInParent<TrackCollisions>() != null)
		{
			vacTriggerCount--;
			if (vacTriggerCount == 0)
			{
				goStealthAt = timeDir.HoursFromNow(1f / 12f);
			}
		}
	}

	public void Update()
	{
		if (!stealth && timeDir.HasReached(goStealthAt))
		{
			SetStealth(stealth: true);
		}
		UpdateStealthOpacity();
	}

	private void UpdateStealthOpacity()
	{
		if (tgtOpacity > currOpacity)
		{
			currOpacity = Mathf.Min(tgtOpacity, currOpacity + 2f * Time.deltaTime);
		}
		else if (tgtOpacity < currOpacity)
		{
			currOpacity = Mathf.Max(tgtOpacity, currOpacity - 2f * Time.deltaTime);
		}
		materialStealthController.SetOpacity(currOpacity);
	}

	private void SetStealth(bool stealth)
	{
		this.stealth = stealth;
		tgtOpacity = (stealth ? 0f : 1f);
		goStealthAt = double.PositiveInfinity;
	}
}

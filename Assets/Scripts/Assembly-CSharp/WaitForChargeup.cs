using System;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;
using UnityEngine.UI;

public class WaitForChargeup : MonoBehaviour, GadgetModel.Participant
{
	public float waitTimeGameHrs = 2f;

	private TimeDirector timeDir;

	private GadgetDirector gadgetDir;

	private GameObject waitingObj;

	private Text waitingText;

	private int lastWaitingMins = -1;

	private GadgetModel model;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
	}

	public void InitModel(GadgetModel model)
	{
		model.waitForChargeupTime = timeDir.HoursFromNow(waitTimeGameHrs);
	}

	public void SetModel(GadgetModel model)
	{
		this.model = model;
	}

	public void Update()
	{
		bool flag = !timeDir.HasReached(model.waitForChargeupTime);
		if (flag && waitingObj == null)
		{
			waitingObj = UnityEngine.Object.Instantiate(gadgetDir.waitForChargeupPrefab, base.transform.position, base.transform.rotation, base.transform);
			waitingText = waitingObj.transform.Find("InstallationRing/TextUI/ClockPanel/TimeText").GetComponent<Text>();
		}
		else if (!flag && waitingObj != null)
		{
			Destroyer.Destroy(waitingObj, "WaitForChargeup.Update");
			waitingObj = null;
			waitingText = null;
			lastWaitingMins = -1;
		}
		if (waitingText != null)
		{
			int num = (int)Math.Ceiling(timeDir.HoursUntil(model.waitForChargeupTime) * 60.0);
			if (num != lastWaitingMins)
			{
				waitingText.text = timeDir.FormatTime(num);
				lastWaitingMins = num;
			}
		}
	}

	public bool IsWaiting()
	{
		return waitingObj != null;
	}
}

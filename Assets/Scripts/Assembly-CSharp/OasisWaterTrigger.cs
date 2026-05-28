using UnityEngine;

public class OasisWaterTrigger : SRBehaviour, LiquidConsumer
{
	public Oasis oasisToScale;

	public SECTR_AudioCue scaleCue;

	public GameObject scaleFX;

	public GameObject indicatorObj;

	public GameObject indicatorReplacesObj;

	private bool hasAlreadyActivated;

	public void Start()
	{
		bool flag = oasisToScale.IsLive();
		if (indicatorObj != null)
		{
			indicatorObj.SetActive(flag);
		}
		if (indicatorReplacesObj != null)
		{
			indicatorReplacesObj.SetActive(!flag);
		}
		hasAlreadyActivated = flag;
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (oasisToScale != null && liquidId == Identifiable.Id.MAGIC_WATER_LIQUID && !hasAlreadyActivated)
		{
			oasisToScale.SetLive(immediate: false);
			if (scaleCue != null)
			{
				SECTR_AudioSystem.Play(scaleCue, base.transform.position, loop: false);
			}
			if (scaleFX != null)
			{
				SRBehaviour.InstantiateDynamic(scaleFX, base.transform.position, base.transform.rotation);
			}
			if (indicatorObj != null)
			{
				indicatorObj.SetActive(value: true);
			}
			if (indicatorReplacesObj != null)
			{
				indicatorReplacesObj.SetActive(value: false);
			}
			hasAlreadyActivated = true;
		}
	}
}

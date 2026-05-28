using UnityEngine;

public class CalmedByWaterSpray : MonoBehaviour, LiquidConsumer
{
	public float agitationReduction = 0.1f;

	public float calmedHours = 0.3333f;

	private double calmedUntil;

	private SlimeEmotions emotions;

	private TimeDirector timeDir;

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			emotions.Adjust(SlimeEmotions.Emotion.AGITATION, (0f - agitationReduction) * units);
			calmedUntil = timeDir.HoursFromNow(calmedHours);
		}
	}

	public bool IsCalmed()
	{
		return !timeDir.HasReached(calmedUntil);
	}
}

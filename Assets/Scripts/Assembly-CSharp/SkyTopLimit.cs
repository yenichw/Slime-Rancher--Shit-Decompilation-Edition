public class SkyTopLimit : SRSingleton<SkyTopLimit>
{
	private const float GRAV_PER_Y = 0.04f;

	private float bottomY;

	public override void Awake()
	{
		base.Awake();
		bottomY = base.transform.position.y - 0.5f * base.transform.localScale.y;
	}

	public float DownwardExtraGravity(float y)
	{
		float num = y - bottomY;
		if (num <= 0f)
		{
			return 0f;
		}
		return num * 0.04f;
	}
}

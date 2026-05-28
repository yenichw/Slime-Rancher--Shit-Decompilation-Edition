using System;

[Serializable]
public class Range
{
	public float min;

	public float max;

	public float Random()
	{
		return Random(Randoms.SHARED);
	}

	public float Random(Randoms rand)
	{
		return rand.GetInRange(min, max);
	}
}

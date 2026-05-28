using System.Collections.Generic;

public class SlimeAppearanceEqualityComparer : IEqualityComparer<SlimeAppearance>
{
	public static SlimeAppearanceEqualityComparer Default = new SlimeAppearanceEqualityComparer();

	public bool Equals(SlimeAppearance x, SlimeAppearance y)
	{
		return x == y;
	}

	public int GetHashCode(SlimeAppearance obj)
	{
		return obj.GetHashCode();
	}
}

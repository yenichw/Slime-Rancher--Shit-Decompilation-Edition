using System.Collections.Generic;

public class SlimeAppearanceObjectComparer : IEqualityComparer<SlimeAppearanceObject>
{
	public bool Equals(SlimeAppearanceObject id1, SlimeAppearanceObject id2)
	{
		return id1.GetInstanceID() == id2.GetInstanceID();
	}

	public int GetHashCode(SlimeAppearanceObject obj)
	{
		return obj.GetHashCode();
	}
}

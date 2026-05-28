using System.Collections.Generic;

public class SlimeDefinitionEqualityComparer : IEqualityComparer<SlimeDefinition>
{
	public static SlimeDefinitionEqualityComparer Default = new SlimeDefinitionEqualityComparer();

	public bool Equals(SlimeDefinition x, SlimeDefinition y)
	{
		return x == y;
	}

	public int GetHashCode(SlimeDefinition obj)
	{
		return obj.GetHashCode();
	}
}

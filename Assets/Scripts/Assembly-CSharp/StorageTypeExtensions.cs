using System;
using System.Collections.Generic;

public static class StorageTypeExtensions
{
	private static Dictionary<SiloStorage.StorageType, HashSet<Identifiable.Id>> getContentsCache = new Dictionary<SiloStorage.StorageType, HashSet<Identifiable.Id>>(SiloStorage.StorageTypeComparer.Instance);

	public static bool Contains(this SiloStorage.StorageType type, Identifiable.Id id)
	{
		return type.GetContents().Contains(id);
	}

	public static HashSet<Identifiable.Id> GetContents(this SiloStorage.StorageType type)
	{
		if (getContentsCache.TryGetValue(type, out var value))
		{
			return value;
		}
		value = new HashSet<Identifiable.Id>(Identifiable.idComparer);
		switch (type)
		{
		case SiloStorage.StorageType.NON_SLIMES:
			value.UnionWith(Identifiable.NON_SLIMES_CLASS);
			value.UnionWith(Identifiable.ORNAMENT_CLASS);
			value.UnionWith(Identifiable.ECHO_CLASS);
			value.UnionWith(Identifiable.ECHO_NOTE_CLASS);
			break;
		case SiloStorage.StorageType.PLORT:
			value.UnionWith(Identifiable.PLORT_CLASS);
			break;
		case SiloStorage.StorageType.FOOD:
			value.UnionWith(Identifiable.FOOD_CLASS);
			value.UnionWith(Identifiable.CHICK_CLASS);
			break;
		case SiloStorage.StorageType.CRAFTING:
			value.UnionWith(Identifiable.PLORT_CLASS);
			value.UnionWith(Identifiable.CRAFT_CLASS);
			break;
		case SiloStorage.StorageType.ELDER:
			value.Add(Identifiable.Id.ELDER_HEN);
			value.Add(Identifiable.Id.ELDER_ROOSTER);
			break;
		default:
			throw new ArgumentException($"Failed to get contents for storage type. [type={type}]");
		}
		value.Remove(Identifiable.Id.QUICKSILVER_PLORT);
		return getContentsCache[type] = value;
	}
}

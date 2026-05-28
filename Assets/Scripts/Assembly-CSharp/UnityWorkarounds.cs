using System.Collections.Generic;
using UnityEngine;

public static class UnityWorkarounds
{
	public static void SafeRemoveAllNulls<T>(HashSet<T> inputSet) where T : Object
	{
		inputSet.RemoveWhere((T o) => o == null);
	}
}

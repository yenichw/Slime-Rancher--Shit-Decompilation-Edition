using System.Collections.Generic;

namespace DLCPackage
{
	public class StateComparer : IEqualityComparer<State>
	{
		public static StateComparer Instance = new StateComparer();

		public bool Equals(State a, State b)
		{
			return a == b;
		}

		public int GetHashCode(State a)
		{
			return (int)a;
		}
	}
}

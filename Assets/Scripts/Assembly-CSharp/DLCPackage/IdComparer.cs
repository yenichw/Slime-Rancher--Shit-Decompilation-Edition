using System.Collections.Generic;

namespace DLCPackage
{
	public class IdComparer : IEqualityComparer<Id>
	{
		public static IdComparer Instance = new IdComparer();

		public bool Equals(Id a, Id b)
		{
			return a == b;
		}

		public int GetHashCode(Id a)
		{
			return (int)a;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TestUtil
{
	public class SequenceComparer<T> : IEqualityComparer<IEnumerable<T>>
	{
		private IEqualityComparer<T> elemComparer;

		public SequenceComparer(IEqualityComparer<T> elemComparer = null)
		{
			this.elemComparer = elemComparer;
		}

		public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
		{
			return x.SequenceEqual(y, elemComparer);
		}

		public int GetHashCode(IEnumerable<T> obj)
		{
			throw new NotImplementedException();
		}
	}

	public class ListComparer<T> : IEqualityComparer<List<T>>
	{
		private IEqualityComparer<T> elemComparer;

		public ListComparer(IEqualityComparer<T> elemComparer = null)
		{
			this.elemComparer = elemComparer;
		}

		public bool Equals(List<T> x, List<T> y)
		{
			return x.SequenceEqual(y, elemComparer);
		}

		public int GetHashCode(List<T> obj)
		{
			throw new NotImplementedException();
		}
	}

	public class ArrayComparer<T> : IEqualityComparer<T[]>
	{
		private IEqualityComparer<T> elemComparer;

		public ArrayComparer(IEqualityComparer<T> elemComparer = null)
		{
			this.elemComparer = elemComparer;
		}

		public bool Equals(T[] x, T[] y)
		{
			return x.SequenceEqual(y, elemComparer);
		}

		public int GetHashCode(T[] obj)
		{
			throw new NotImplementedException();
		}
	}

	public class DictComparer<K, V> : IEqualityComparer<IEnumerable<KeyValuePair<K, V>>>
	{
		private KeyValueComparer<K, V> keyValComparer;

		public DictComparer()
			: this((IEqualityComparer<K>)null, (IEqualityComparer<V>)null)
		{
		}

		public DictComparer(IEqualityComparer<K> keyComparer, IEqualityComparer<V> valComparer)
		{
			keyValComparer = new KeyValueComparer<K, V>(keyComparer, valComparer);
		}

		public bool Equals(IEnumerable<KeyValuePair<K, V>> x, IEnumerable<KeyValuePair<K, V>> y)
		{
			if (x.Count() != y.Count())
			{
				return false;
			}
			List<KeyValuePair<K, V>> list = new List<KeyValuePair<K, V>>(y);
			foreach (KeyValuePair<K, V> item in x)
			{
				bool flag = false;
				foreach (KeyValuePair<K, V> item2 in list)
				{
					if (keyValComparer.Equals(item, item2))
					{
						flag = true;
						list.Remove(item2);
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(IEnumerable<KeyValuePair<K, V>> obj)
		{
			throw new NotImplementedException();
		}
	}

	public class KeyValueComparer<K, V> : IEqualityComparer<KeyValuePair<K, V>>
	{
		private IEqualityComparer<K> keyComparer;

		private IEqualityComparer<V> valComparer;

		public KeyValueComparer(IEqualityComparer<K> keyComparer, IEqualityComparer<V> valComparer)
		{
			this.keyComparer = keyComparer;
			this.valComparer = valComparer;
		}

		public bool Equals(KeyValuePair<K, V> x, KeyValuePair<K, V> y)
		{
			if ((keyComparer == null) ? x.Key.Equals(y.Key) : keyComparer.Equals(x.Key, y.Key))
			{
				if (valComparer != null)
				{
					return valComparer.Equals(x.Value, y.Value);
				}
				return x.Value.Equals(y.Value);
			}
			return false;
		}

		public int GetHashCode(KeyValuePair<K, V> obj)
		{
			return ((keyComparer == null) ? obj.Key.GetHashCode() : keyComparer.GetHashCode(obj.Key)) ^ ((valComparer == null) ? obj.Value.GetHashCode() : valComparer.GetHashCode(obj.Value));
		}
	}

	public class Vector3Comparer : IEqualityComparer<Vector3>
	{
		private float tolerance;

		private bool wraparound360;

		public Vector3Comparer(float tolerance = 0.001f, bool wraparound360 = false)
		{
			this.tolerance = tolerance;
			this.wraparound360 = wraparound360;
		}

		public bool Equals(Vector3 v1, Vector3 v2)
		{
			float num = Math.Abs(v1.x - v2.x);
			float num2 = Math.Abs(v1.y - v2.y);
			float num3 = Math.Abs(v1.z - v2.z);
			if (wraparound360)
			{
				while (num >= 360f - tolerance)
				{
					num -= 360f;
				}
				while (num2 >= 360f - tolerance)
				{
					num2 -= 360f;
				}
				while (num3 >= 360f - tolerance)
				{
					num3 -= 360f;
				}
			}
			if (Math.Abs(num) <= tolerance && Math.Abs(num2) <= tolerance)
			{
				return Math.Abs(num3) <= tolerance;
			}
			return false;
		}

		public int GetHashCode(Vector3 obj)
		{
			return Math.Round(obj.x).GetHashCode() ^ Math.Round(obj.y).GetHashCode() ^ Math.Round(obj.z).GetHashCode();
		}
	}

	public static void AreApproximatelyEqual(Vector3 vA, Vector3 vB, float tolerance, string message)
	{
	}

	public static void AssertAreEqual<K, V>(Dictionary<K, V> expected, Dictionary<K, V> actual, Action<V, V> valueAssertion, string dictName)
	{
		foreach (KeyValuePair<K, V> item in expected)
		{
			valueAssertion(item.Value, actual[item.Key]);
		}
	}

	public static void AssertVersionedAreEqual<K, V1, V2>(Dictionary<K, V1> expected, Dictionary<K, V2> actual, Action<V1, V2> valueAssertion, string dictName)
	{
		foreach (KeyValuePair<K, V1> item in expected)
		{
			valueAssertion(item.Value, actual[item.Key]);
		}
	}

	public static void AssertAreEqual<T>(List<T> expected, List<T> actual, string field = "")
	{
		AssertAreEqual(expected, actual, delegate
		{
		}, field);
	}

	public static void AssertAreEqual<T1, T2>(List<T1> expected, List<T2> actual, Action<T1, T2, string> valueAssertion, string field = "")
	{
		if (expected != null || actual != null)
		{
			for (int i = 0; i < expected.Count; i++)
			{
				valueAssertion(expected[i], actual[i], $"{field}[{i}]");
			}
		}
	}

	public static bool AssertNullness(object expected, object actual)
	{
		if (expected != null)
		{
			return actual != null;
		}
		return false;
	}
}

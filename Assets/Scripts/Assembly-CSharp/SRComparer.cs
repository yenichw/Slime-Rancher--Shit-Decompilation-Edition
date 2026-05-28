using System;
using System.Collections.Generic;

public abstract class SRComparer<T> : Comparer<T>
{
	private List<Func<T, T, int>> comparisons = new List<Func<T, T, int>>();

	public override int Compare(T a, T b)
	{
		foreach (Func<T, T, int> comparison in comparisons)
		{
			int num = comparison(a, b);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	public SRComparer<T> OrderBy<K>(Func<T, K> keyFunction)
	{
		comparisons.Add((T a, T b) => Comparer<K>.Default.Compare(keyFunction(a), keyFunction(b)));
		return this;
	}

	public SRComparer<T> OrderByDescending<K>(Func<T, K> keyFunction)
	{
		comparisons.Add((T a, T b) => Comparer<K>.Default.Compare(keyFunction(b), keyFunction(a)));
		return this;
	}
}

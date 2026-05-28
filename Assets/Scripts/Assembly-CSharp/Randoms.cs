using System;
using System.Collections.Generic;
using System.Linq;

public class Randoms
{
	public static Randoms SHARED = new Randoms();

	private readonly Random rand;

	private double nextNextGaussian;

	private bool haveNextNextGaussian;

	private object gaussLock = new object();

	public Randoms()
	{
		rand = new Random();
	}

	public Randoms(int seed)
	{
		rand = new Random(seed);
	}

	public int GetInt(int high)
	{
		return NextInt(high);
	}

	public int GetInt()
	{
		return rand.Next();
	}

	public int GetInRange(int low, int high)
	{
		if (low == high)
		{
			return low;
		}
		return low + NextInt(high - low);
	}

	public float GetFloat(float high)
	{
		return (float)(rand.NextDouble() * (double)high);
	}

	public float GetInRange(float low, float high)
	{
		if (low == high)
		{
			return low;
		}
		return (float)((double)low + rand.NextDouble() * (double)(high - low));
	}

	public bool GetChance(int n)
	{
		return NextInt(n) == 0;
	}

	public bool GetProbability(float p)
	{
		return rand.NextDouble() < (double)p;
	}

	public bool GetBoolean()
	{
		return NextInt(2) == 0;
	}

	public float GetNormal(float mean, float dev)
	{
		return (float)NextGaussian() * dev + mean;
	}

	public T Pick<T>(T[] vals)
	{
		return vals[GetInt(vals.Length)];
	}

	public T Pick<T>(IEnumerator<T> iterator, T ifEmpty)
	{
		if (!iterator.MoveNext())
		{
			return ifEmpty;
		}
		T result = iterator.Current;
		int num = 2;
		while (iterator.Current != null && iterator.MoveNext())
		{
			T current = iterator.Current;
			if (NextInt(num) == 0)
			{
				result = current;
			}
			num++;
		}
		return result;
	}

	public T Pick<T>(IEnumerable<T> enumerable, T ifEmpty)
	{
		return Pick(enumerable.GetEnumerator(), ifEmpty);
	}

	public IEnumerable<T> Pick<T>(List<T> collection, int count)
	{
		List<int> options = Enumerable.Range(0, collection.Count()).ToList();
		int ii = 0;
		while (ii < count && options.Any())
		{
			yield return collection[Pluck(options, 0)];
			int num = ii + 1;
			ii = num;
		}
	}

	public IEnumerable<T> Pick<T>(List<T> collection, int count, Func<T, float> weightFunction)
	{
		List<int> options = Enumerable.Range(0, collection.Count()).ToList();
		Func<int, float> optionsWeightFunction = (int idx) => weightFunction(collection[idx]);
		int ii = 0;
		while (ii < count && options.Any())
		{
			int randomIndex = Pick(options, optionsWeightFunction, -1);
			if (randomIndex != -1)
			{
				yield return collection[randomIndex];
				options.Remove(randomIndex);
				int num = ii + 1;
				ii = num;
				continue;
			}
			break;
		}
	}

	public IEnumerable<T> Pick<T>(List<T> collection, int min, int max)
	{
		return Pick(collection, GetInRange(min, max));
	}

	public T Pick<T>(ICollection<T> iterable, T ifEmpty)
	{
		return PickPluck(iterable, ifEmpty, remove: false);
	}

	public T Pick<T>(IDictionary<T, float> weightMap, T ifEmpty)
	{
		T result = ifEmpty;
		float num = 0f;
		foreach (KeyValuePair<T, float> item in weightMap)
		{
			float value = item.Value;
			if ((double)value > 0.0)
			{
				num += value;
				if (num == value || GetFloat(num) < value)
				{
					result = item.Key;
				}
			}
			else if ((double)value < 0.0)
			{
				throw new ArgumentException("weightMap", "Weight less than 0: " + item);
			}
		}
		return result;
	}

	public T Pick<T>(ICollection<T> iterable, Func<T, float> weightFunction, T ifEmpty)
	{
		T result = ifEmpty;
		float num = 0f;
		foreach (T item in iterable)
		{
			float num2 = weightFunction(item);
			if ((double)num2 > 0.0)
			{
				num += num2;
				if (num == num2 || GetFloat(num) < num2)
				{
					result = item;
				}
			}
			else if ((double)num2 < 0.0)
			{
				throw new ArgumentException("weightMap", "Weight less than 0: " + item);
			}
		}
		return result;
	}

	public T Pluck<T>(ICollection<T> iterable, T ifEmpty)
	{
		return PickPluck(iterable, ifEmpty, remove: true);
	}

	protected T PickPluck<T>(ICollection<T> coll, T ifEmpty, bool remove)
	{
		int count = coll.Count;
		if (count == 0)
		{
			return ifEmpty;
		}
		if (coll is IList<T>)
		{
			IList<T> list = (IList<T>)coll;
			int index = NextInt(count);
			T result = list[index];
			if (remove)
			{
				list.RemoveAt(index);
			}
			return result;
		}
		IEnumerator<T> enumerator = coll.GetEnumerator();
		enumerator.MoveNext();
		for (int num = NextInt(count); num > 0; num--)
		{
			enumerator.MoveNext();
		}
		try
		{
			return enumerator.Current;
		}
		finally
		{
			if (remove)
			{
				coll.Remove(enumerator.Current);
			}
		}
	}

	protected static void Swap<T>(IList<T> list, int ii, int jj)
	{
		T value = list[jj];
		list[jj] = list[ii];
		list[ii] = value;
	}

	protected static void Swap<T>(T[] array, int ii, int jj)
	{
		T val = array[ii];
		array[ii] = array[jj];
		array[jj] = val;
	}

	private int NextInt(int n)
	{
		if (n <= 0)
		{
			throw new ArgumentOutOfRangeException("n", "must be positive");
		}
		int num;
		int num2;
		do
		{
			num = rand.Next();
			num2 = num % n;
		}
		while (num - num2 + (n - 1) < 0);
		return num2;
	}

	private double NextGaussian()
	{
		lock (gaussLock)
		{
			if (haveNextNextGaussian)
			{
				haveNextNextGaussian = false;
				return nextNextGaussian;
			}
			double num;
			double num2;
			double num3;
			do
			{
				num = 2.0 * rand.NextDouble() - 1.0;
				num2 = 2.0 * rand.NextDouble() - 1.0;
				num3 = num * num + num2 * num2;
			}
			while (num3 >= 1.0 || num3 == 0.0);
			double num4 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
			nextNextGaussian = num2 * num4;
			haveNextNextGaussian = true;
			return num * num4;
		}
	}
}

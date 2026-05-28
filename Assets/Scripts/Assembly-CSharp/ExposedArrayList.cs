using System;

public class ExposedArrayList<T>
{
	private const int ARRAY_START_SIZE = 1000;

	private const int ARRAY_GROWTH_SIZE = 100;

	public T[] Data;

	private int _count;

	public ExposedArrayList()
		: this(1000)
	{
	}

	public ExposedArrayList(int startSize)
	{
		Data = new T[startSize];
	}

	public int GetCount()
	{
		return _count;
	}

	public void Add(T item)
	{
		if (_count >= Data.Length)
		{
			Array.Resize(ref Data, Data.Length + 100);
		}
		Data[_count] = item;
		_count++;
	}

	public void Remove(T item)
	{
		int num = IndexOf(item);
		if (num > -1 && num <= _count)
		{
			Data[num] = Data[_count - 1];
			Data[_count - 1] = default(T);
			_count--;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < _count; i++)
		{
			Data[i] = default(T);
		}
		_count = 0;
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < _count; i++)
		{
			if (item.Equals(Data[i]))
			{
				return i;
			}
		}
		return -1;
	}
}

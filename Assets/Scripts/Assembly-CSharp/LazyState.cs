using System;

public class LazyState<T> where T : struct
{
	private T? state;

	public static implicit operator T(LazyState<T> instance)
	{
		if (!instance.state.HasValue)
		{
			throw new InvalidOperationException();
		}
		return instance.state.Value;
	}

	public LazyState()
	{
		state = null;
	}

	public LazyState(T initialValue)
	{
		state = initialValue;
	}

	public bool Update(T current)
	{
		if (!current.Equals(state))
		{
			state = current;
			return true;
		}
		return false;
	}
}
public class LazyState : LazyState<bool>
{
	public LazyState()
	{
	}

	public LazyState(bool initialValue)
		: base(initialValue)
	{
	}
}

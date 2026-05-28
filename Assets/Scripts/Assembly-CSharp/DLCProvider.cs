using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DLCPackage;

public abstract class DLCProvider
{
	private HashSet<Id> supported;

	private Dictionary<Id, State> cache;

	public DLCProvider(IEnumerable<Id> supported)
	{
		this.supported = new HashSet<Id>(supported, IdComparer.Instance);
		cache = this.supported.ToDictionary((Id id) => id, (Id id) => State.AVAILABLE, IdComparer.Instance);
	}

	protected void ResetAllPackageStates()
	{
		cache = supported.ToDictionary((Id id) => id, (Id id) => State.AVAILABLE, IdComparer.Instance);
	}

	public abstract IEnumerator Refresh();

	public abstract void ShowInStore(Id id);

	public IEnumerable<Id> GetSupported()
	{
		return supported;
	}

	public State GetState(Id id)
	{
		cache.TryGetValue(id, out var value);
		return value;
	}

	protected bool SetState(Id id, State state)
	{
		State state2 = GetState(id);
		if (state2 > state)
		{
			Log.Error("Attempting to downgrade DLC state.", "id", id, "current", state2, "updated", state);
			return false;
		}
		cache[id] = state;
		return true;
	}
}

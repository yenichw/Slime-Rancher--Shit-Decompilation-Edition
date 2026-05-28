using System.Collections.Generic;
using UnityEngine;

public class IdDirector : SRBehaviour, ISerializationCallbackReceiver
{
	private Dictionary<IdHandler, string> persistenceDict = new Dictionary<IdHandler, string>();

	[SerializeField]
	[HideInInspector]
	private List<IdHandler> persistenceKeys = new List<IdHandler>();

	[SerializeField]
	[HideInInspector]
	private List<string> persistenceValues = new List<string>();

	public string GetPersistenceIdentifier(IdHandler instance)
	{
		if (persistenceDict.TryGetValue(instance, out var value))
		{
			return value;
		}
		return null;
	}

	public void OnBeforeSerialize()
	{
		persistenceKeys = new List<IdHandler>(persistenceDict.Keys);
		persistenceValues = new List<string>(persistenceDict.Values);
	}

	public void OnAfterDeserialize()
	{
		persistenceDict = new Dictionary<IdHandler, string>();
		for (int i = 0; i < persistenceKeys.Count; i++)
		{
			IdHandler idHandler = persistenceKeys[i];
			if (idHandler != null)
			{
				persistenceDict[idHandler] = persistenceValues[i];
			}
		}
		persistenceKeys = null;
		persistenceValues = null;
	}
}

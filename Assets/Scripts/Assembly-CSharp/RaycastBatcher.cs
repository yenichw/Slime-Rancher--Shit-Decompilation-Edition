using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class RaycastBatcher : MonoBehaviour
{
	private List<KeyValuePair<RaycastCommand, Action<RaycastHit>>> m_Requests = new List<KeyValuePair<RaycastCommand, Action<RaycastHit>>>();

	private void FixedUpdate()
	{
		int count = m_Requests.Count;
		if (count > 0)
		{
			NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
			NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(count, Allocator.TempJob);
			for (int i = 0; i < count; i++)
			{
				commands[i] = m_Requests[i].Key;
			}
			RaycastCommand.ScheduleBatch(commands, results, count / 3).Complete();
			for (int j = 0; j < count; j++)
			{
				m_Requests[j].Value(results[j]);
			}
			commands.Dispose();
			results.Dispose();
			m_Requests.Clear();
		}
	}

	public void QueueRaycast(RaycastCommand command, Action<RaycastHit> callback)
	{
		if (callback != null)
		{
			m_Requests.Add(new KeyValuePair<RaycastCommand, Action<RaycastHit>>(command, callback));
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

public class AshSource : MonoBehaviour
{
	public static List<AshSource> allAshes = new List<AshSource>();

	public virtual void Awake()
	{
		allAshes.Add(this);
	}

	public virtual void OnDestroy()
	{
		allAshes.Remove(this);
	}

	public virtual bool Available()
	{
		return true;
	}

	public virtual void ConsumeAsh()
	{
	}
}

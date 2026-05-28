using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SECTR/Stream/SECTR Trigger Loader")]
public class SECTR_TriggerLoader : SECTR_Loader
{
	protected int loadedCount;

	protected bool chunksReferenced;

	[SECTR_ToolTip("List of Sectors to load when entering this trigger.")]
	public List<SECTR_Sector> Sectors = new List<SECTR_Sector>();

	[SECTR_ToolTip("Should the Sectors be unloaded when trigger is exited.")]
	public bool UnloadOnExit = true;

	public override bool Loaded
	{
		get
		{
			bool flag = true;
			int count = Sectors.Count;
			for (int i = 0; i < count && flag; i++)
			{
				SECTR_Sector sECTR_Sector = Sectors[i];
				if (sECTR_Sector.Frozen)
				{
					SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
					if ((bool)component && !component.IsLoaded())
					{
						flag = false;
						break;
					}
				}
			}
			return flag;
		}
	}

	private void OnDisable()
	{
		_RefChunks();
		loadedCount = 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (loadedCount == 0)
		{
			_RefChunks();
		}
		loadedCount++;
	}

	private void OnTriggerExit(Collider other)
	{
		if (loadedCount > 0)
		{
			loadedCount--;
			if (loadedCount == 0 && UnloadOnExit)
			{
				_UnrefChunks();
			}
		}
	}

	private void _RefChunks()
	{
		if (chunksReferenced)
		{
			return;
		}
		int count = Sectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = Sectors[i];
			if ((bool)sECTR_Sector)
			{
				SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
				if ((bool)component)
				{
					component.AddReference();
				}
			}
		}
		chunksReferenced = true;
	}

	private void _UnrefChunks()
	{
		if (!chunksReferenced)
		{
			return;
		}
		int count = Sectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = Sectors[i];
			if ((bool)sECTR_Sector)
			{
				SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
				if ((bool)component)
				{
					component.RemoveReference();
				}
			}
		}
		chunksReferenced = false;
	}
}

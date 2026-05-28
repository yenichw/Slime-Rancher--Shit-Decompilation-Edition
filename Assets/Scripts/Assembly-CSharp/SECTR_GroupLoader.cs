using System.Collections.Generic;

public class SECTR_GroupLoader : SECTR_Loader
{
	[SECTR_ToolTip("The Sectors to load and unload together.")]
	public List<SECTR_Sector> Sectors = new List<SECTR_Sector>();

	public override bool Loaded
	{
		get
		{
			bool result = true;
			int count = Sectors.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Sector sECTR_Sector = Sectors[i];
				if ((bool)sECTR_Sector && sECTR_Sector.Frozen)
				{
					SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
					if ((bool)component && !component.IsLoaded())
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
	}

	private void OnEnable()
	{
		int count = Sectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = Sectors[i];
			if ((bool)sECTR_Sector)
			{
				SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
				if ((bool)component)
				{
					component.ReferenceChange += ChunkChanged;
				}
			}
		}
	}

	private void OnDisable()
	{
		int count = Sectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = Sectors[i];
			if ((bool)sECTR_Sector)
			{
				SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
				if ((bool)component)
				{
					component.ReferenceChange -= ChunkChanged;
				}
			}
		}
	}

	private void ChunkChanged(SECTR_Chunk source, bool loaded)
	{
		int count = Sectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = Sectors[i];
			if (!sECTR_Sector)
			{
				continue;
			}
			SECTR_Chunk component = sECTR_Sector.GetComponent<SECTR_Chunk>();
			if ((bool)component && component != source)
			{
				component.ReferenceChange -= ChunkChanged;
				if (loaded)
				{
					component.AddReference();
				}
				else
				{
					component.RemoveReference();
				}
				component.ReferenceChange += ChunkChanged;
			}
		}
	}
}

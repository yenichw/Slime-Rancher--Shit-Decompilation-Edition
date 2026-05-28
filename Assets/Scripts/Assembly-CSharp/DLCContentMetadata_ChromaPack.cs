using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Content/Chroma Pack Metadata")]
public class DLCContentMetadata_ChromaPack : DLCContentMetadata
{
	public RanchDirector.PaletteEntry paletteEntry;

	public override void Register()
	{
		SRSingleton<SceneContext>.Instance.RanchDirector.RegisterPalette(paletteEntry);
	}
}

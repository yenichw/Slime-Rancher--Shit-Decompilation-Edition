using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EchoNoteGameMetadata : ScriptableObject
{
	[Tooltip("Instrument this note set corresponds to.")]
	public InstrumentModel.Instrument instrument;

	[Tooltip("Translation key of the name of the instrument.")]
	public string xlateKey;

	[Tooltip("Icon for this instrument.")]
	public Sprite icon;

	[Tooltip("SFX cue to use with echo notes.")]
	public SECTR_AudioCue cue;
}

using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class InstrumentModel
	{
		public enum Instrument
		{
			NONE = -1,
			MARIMBA = 0,
			BELLS = 1,
			CORA = 2,
			GLOCK = 3,
			GLOCK_LOW = 4,
			MUSIC_BOX = 5,
			ORGAN = 6,
			PIANO = 7,
			PIANO_ELECTRIC = 8,
			PLUNKY = 9,
			SINGING_LONG = 10,
			SINGING_SHORT = 11,
			VIBE = 12,
			VIBE_LOW = 13,
			CHIPTUNE = 14,
			VIOLIN = 15,
			FLUTE = 16
		}

		public interface Participant
		{
			void InitModel(InstrumentModel model);

			void SetModel(InstrumentModel model);
		}

		private Participant participant;

		private HashSet<Instrument> instrumentUnlocks = new HashSet<Instrument>();

		private Instrument instrumentSelection;

		public void SetParticipant(Participant participant)
		{
			if (this.participant != null)
			{
				throw new Exception("Replacing participant on InstrumentModel");
			}
			this.participant = participant;
		}

		public void Init()
		{
			participant?.InitModel(this);
		}

		public void NotifyParticipants()
		{
			participant?.SetModel(this);
		}

		public void UnlockInstrument(Instrument instrument)
		{
			instrumentUnlocks.Add(instrument);
		}

		public void SelectInstrument(Instrument instrument)
		{
			instrumentSelection = instrument;
		}

		public HashSet<Instrument> GetUnlockedInstruments()
		{
			return instrumentUnlocks;
		}

		public Instrument GetCurrentlySelectedInstrument()
		{
			return instrumentSelection;
		}

		public void Push(InstrumentV01 persistence)
		{
			instrumentSelection = persistence.selection;
			instrumentUnlocks = new HashSet<Instrument>(persistence.unlocks);
		}

		public InstrumentV01 Pull()
		{
			return new InstrumentV01
			{
				selection = instrumentSelection,
				unlocks = instrumentUnlocks.ToList()
			};
		}
	}
}

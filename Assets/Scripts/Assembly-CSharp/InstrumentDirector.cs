using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class InstrumentDirector : SRBehaviour, InstrumentModel.Participant
{
	public delegate void OnInstrumentChangedDelegate(EchoNoteGameMetadata instrument);

	public delegate void OnInstrumentUnlockedDelegate();

	[Tooltip("Order in which instruments are unlocked.")]
	public InstrumentModel.Instrument[] unlockOrder;

	[Tooltip("Echo note metadata for all available instruments.")]
	public EchoNoteGameMetadata[] instruments;

	[Tooltip("EchoNotes metadata for the currently selected instrument.")]
	public EchoNoteGameMetadata currentInstrument;

	public InstrumentPopupUI popupUI;

	private readonly Dictionary<InstrumentModel.Instrument, EchoNoteGameMetadata> instrumentNoteData = new Dictionary<InstrumentModel.Instrument, EchoNoteGameMetadata>();

	private InstrumentModel model;

	public event OnInstrumentChangedDelegate onInstrumentChanged = delegate
	{
	};

	public event OnInstrumentUnlockedDelegate onInstrumentUnlocked = delegate
	{
	};

	private void Awake()
	{
		EchoNoteGameMetadata[] array = instruments;
		foreach (EchoNoteGameMetadata echoNoteGameMetadata in array)
		{
			if (instrumentNoteData.ContainsKey(echoNoteGameMetadata.instrument))
			{
				throw new Exception("Duplicate instrument data for instrument type: " + Enum.GetName(typeof(InstrumentModel.Instrument), echoNoteGameMetadata.instrument));
			}
			if (echoNoteGameMetadata.instrument == InstrumentModel.Instrument.NONE)
			{
				throw new Exception("Invalid instrument data - no instrument type set: " + echoNoteGameMetadata);
			}
			instrumentNoteData[echoNoteGameMetadata.instrument] = echoNoteGameMetadata;
		}
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterInstrument(this);
	}

	public void InitModel(InstrumentModel model)
	{
	}

	public void SetModel(InstrumentModel model)
	{
		this.model = model;
		if (model.GetCurrentlySelectedInstrument() == InstrumentModel.Instrument.NONE && SRSingleton<SceneContext>.Instance.PediaDirector.IsUnlocked(PediaDirector.Id.ECHO_NOTES))
		{
			model.UnlockInstrument(InstrumentModel.Instrument.MARIMBA);
			model.SelectInstrument(InstrumentModel.Instrument.MARIMBA);
		}
		UpdateCurrentEchoNotes();
	}

	public void UnlockInstrument(InstrumentModel.Instrument instrument)
	{
		if (!IsUnlocked(instrument))
		{
			model.UnlockInstrument(instrument);
			if (model.GetCurrentlySelectedInstrument() == InstrumentModel.Instrument.NONE)
			{
				SelectInstrument(instrument);
			}
			if (model.GetUnlockedInstruments().Count > 1)
			{
				PopupDirector popupDirector = SRSingleton<SceneContext>.Instance.PopupDirector;
				popupDirector.QueueForPopup(new InstrumentPopupUI.PopupCreator(instrumentNoteData[instrument]));
				popupDirector.MaybePopupNext();
			}
			this.onInstrumentUnlocked();
		}
	}

	public HashSet<InstrumentModel.Instrument> GetUnlockedInstruments()
	{
		return model.GetUnlockedInstruments();
	}

	public bool IsUnlocked(InstrumentModel.Instrument instrument)
	{
		return model.GetUnlockedInstruments().Contains(instrument);
	}

	public void UnlockNextInstrument()
	{
		InstrumentModel.Instrument[] array = unlockOrder;
		foreach (InstrumentModel.Instrument instrument in array)
		{
			if (!IsUnlocked(instrument))
			{
				UnlockInstrument(instrument);
				break;
			}
		}
	}

	public void SelectInstrument(InstrumentModel.Instrument instrument)
	{
		model.SelectInstrument(instrument);
		UpdateCurrentEchoNotes();
	}

	public void SelectNextInstrument()
	{
		if (model.GetCurrentlySelectedInstrument() == InstrumentModel.Instrument.NONE)
		{
			throw new Exception("Trying to select next instrument with no instruments available.");
		}
		List<InstrumentModel.Instrument> list = unlockOrder.Where(IsUnlocked).ToList();
		list.AddRange(from instrument in model.GetUnlockedInstruments()
			where !unlockOrder.Contains(instrument)
			select instrument);
		int num = list.IndexOf(model.GetCurrentlySelectedInstrument());
		SelectInstrument(list[(num + 1) % list.Count]);
		UpdateCurrentEchoNotes();
	}

	private void UpdateCurrentEchoNotes()
	{
		if (model.GetCurrentlySelectedInstrument() != InstrumentModel.Instrument.NONE)
		{
			currentInstrument = instrumentNoteData[model.GetCurrentlySelectedInstrument()];
		}
		else
		{
			currentInstrument = instrumentNoteData[unlockOrder[0]];
		}
		this.onInstrumentChanged(currentInstrument);
	}
}

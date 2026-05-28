using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstrumentPopupUI : PopupUI<EchoNoteGameMetadata>, PopupDirector.Popup
{
	public class PopupCreator : PopupDirector.PopupCreator
	{
		private readonly EchoNoteGameMetadata instrument;

		public PopupCreator(EchoNoteGameMetadata instrument)
		{
			this.instrument = instrument;
		}

		public override void Create()
		{
			Object.Instantiate(SRSingleton<SceneContext>.Instance.InstrumentDirector.popupUI).Init(instrument);
		}

		public override bool Equals(object other)
		{
			if (other is PopupCreator popupCreator)
			{
				return popupCreator.instrument == instrument;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return instrument.GetHashCode();
		}

		public override bool ShouldClear()
		{
			return false;
		}
	}

	[Tooltip("Lifetime of the popup (seconds).")]
	public float lifetime;

	[Tooltip("Text representing the instrument name.")]
	public TMP_Text instrumentName;

	[Tooltip("Image representing the instrument icon.")]
	public Image instrumentIcon;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.PopupDirector.PopupActivated(this);
		Destroyer.Destroy(base.gameObject, lifetime, "InstrumentPopupUI");
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.PopupDirector.PopupDeactivated(this);
		}
	}

	public override void Init(EchoNoteGameMetadata instrument)
	{
		base.Init(instrument);
		instrumentIcon.sprite = instrument.icon;
	}

	public override void OnBundleAvailable(MessageDirector messageDirector)
	{
		MessageBundle bundle = messageDirector.GetBundle("actor");
		instrumentName.text = bundle.Get(idEntry.xlateKey);
	}

	public bool ShouldClear()
	{
		return false;
	}
}

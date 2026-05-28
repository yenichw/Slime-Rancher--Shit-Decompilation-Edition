using Assets.Script.Util.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeAppearancePopupUI : PopupUI<SlimeAppearance>, PopupDirector.Popup
{
	public class PopupCreator : PopupDirector.PopupCreator
	{
		private readonly SlimeAppearance appearance;

		public PopupCreator(SlimeAppearance appearance)
		{
			this.appearance = appearance;
		}

		public override void Create()
		{
			Object.Instantiate(SRSingleton<SceneContext>.Instance.SlimeAppearanceDirector.appearancePopupUI).GetRequiredComponent<SlimeAppearancePopupUI>().Init(appearance);
		}

		public override bool Equals(object other)
		{
			if (other is PopupCreator)
			{
				return ((PopupCreator)other).appearance == appearance;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return appearance.GetHashCode();
		}

		public override bool ShouldClear()
		{
			return false;
		}
	}

	[Tooltip("Lifetime of the popup (seconds).")]
	public float lifetime;

	[Tooltip("Text representing the appearance name.")]
	public TMP_Text appearanceName;

	[Tooltip("Image representing the appearance icon.")]
	public Image appearanceIcon;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.PopupDirector.PopupActivated(this);
		Destroyer.Destroy(base.gameObject, lifetime, "SlimeAppearancePopupUI");
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.PopupDirector.PopupDeactivated(this);
		}
	}

	public override void Init(SlimeAppearance appearance)
	{
		base.Init(appearance);
		appearanceIcon.sprite = appearance.Icon;
	}

	public override void OnBundleAvailable(MessageDirector messageDirector)
	{
		MessageBundle bundle = messageDirector.GetBundle("actor");
		appearanceName.text = bundle.Get(idEntry.NameXlateKey);
	}

	public bool ShouldClear()
	{
		return false;
	}
}

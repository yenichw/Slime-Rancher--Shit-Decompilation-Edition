using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Slime Appearance")]
public class SlimeAppearance : ScriptableObject
{
	public enum SlimeBone
	{
		None = 0,
		Root = 1,
		Attachment = 2,
		Slime = 3,
		Core = 4,
		JiggleBack = 5,
		JiggleBottom = 6,
		JiggleFront = 7,
		JiggleLeft = 8,
		JiggleRight = 9,
		JiggleTop = 10,
		Spinner = 11,
		LeftWing = 12,
		RightWing = 13
	}

	public enum AppearanceSaveSet
	{
		NONE = 0,
		CLASSIC = 1,
		SECRET_STYLE = 2
	}

	public class BoneComparer : IEqualityComparer<SlimeBone>
	{
		public bool Equals(SlimeBone bone1, SlimeBone bone2)
		{
			return bone1 == bone2;
		}

		public int GetHashCode(SlimeBone bone)
		{
			return (int)bone;
		}
	}

	private class SlimeAppearanceStructureComparer : IEqualityComparer<SlimeAppearanceStructure>
	{
		public bool Equals(SlimeAppearanceStructure x, SlimeAppearanceStructure y)
		{
			return x.Element.GetHashCode() == y.Element.GetHashCode();
		}

		public int GetHashCode(SlimeAppearanceStructure obj)
		{
			return obj.Element.GetHashCode();
		}
	}

	[Serializable]
	public struct Palette
	{
		private static int TopColorPropertyId = Shader.PropertyToID("_TopColor");

		private static int MiddleColorPropertyId = Shader.PropertyToID("_MiddleColor");

		private static int BottomColorPropertyId = Shader.PropertyToID("_BottomColor");

		public static Palette Default = new Palette
		{
			Top = Color.grey,
			Middle = Color.grey,
			Bottom = Color.grey,
			Ammo = Color.grey
		};

		public Color Top;

		public Color Middle;

		public Color Bottom;

		public Color Ammo;

		public static Palette FromMaterial(Material material)
		{
			Palette result = default(Palette);
			result.Top = material.GetColor(TopColorPropertyId);
			result.Middle = material.GetColor(MiddleColorPropertyId);
			result.Bottom = material.GetColor(BottomColorPropertyId);
			result.Ammo = Color.black;
			return result;
		}
	}

	public string NameXlateKey;

	public Sprite Icon;

	public RuntimeAnimatorController AnimatorOverride;

	public SlimeAppearanceStructure[] Structures;

	public SlimeFace Face;

	public SlimeAppearance QubitAppearance;

	public SlimeAppearance ShockedAppearance;

	public SlimeAppearance[] DependentAppearances;

	public Palette ColorPalette;

	public GlintAppearance GlintAppearance;

	public TornadoAppearance TornadoAppearance;

	public VineAppearance VineAppearance;

	public CrystalAppearance CrystalAppearance;

	public ExplosionAppearance ExplosionAppearance;

	public DeathAppearance DeathAppearance;

	public AppearanceSaveSet SaveSet;

	public static SlimeAppearanceEqualityComparer DefaultComparer = new SlimeAppearanceEqualityComparer();

	public static BoneComparer DefaultBoneComparer = new BoneComparer();

	public void MaybeShowPopupUI()
	{
		if (SaveSet == AppearanceSaveSet.SECRET_STYLE)
		{
			PopupDirector popupDirector = SRSingleton<SceneContext>.Instance.PopupDirector;
			popupDirector.QueueForPopup(new SlimeAppearancePopupUI.PopupCreator(this));
			popupDirector.MaybePopupNext();
		}
	}

	public static SlimeAppearance CombineAppearances(SlimeAppearance appearance1, SlimeAppearance appearance2)
	{
		SlimeAppearance slimeAppearance = ScriptableObject.CreateInstance<SlimeAppearance>();
		slimeAppearance.Face = appearance1.Face;
		HashSet<SlimeAppearanceStructure> hashSet = new HashSet<SlimeAppearanceStructure>(new SlimeAppearanceStructureComparer());
		SlimeAppearanceStructure[] structures = appearance1.Structures;
		foreach (SlimeAppearanceStructure item in structures)
		{
			hashSet.Add(item);
		}
		structures = appearance2.Structures;
		foreach (SlimeAppearanceStructure item2 in structures)
		{
			if (!hashSet.Contains(item2))
			{
				hashSet.Add(item2);
			}
		}
		slimeAppearance.Structures = hashSet.Select((SlimeAppearanceStructure s) => new SlimeAppearanceStructure(s)).ToArray();
		slimeAppearance.DependentAppearances = new SlimeAppearance[2] { appearance1, appearance2 };
		slimeAppearance.GlintAppearance = appearance1.GlintAppearance ?? appearance2.GlintAppearance;
		slimeAppearance.TornadoAppearance = appearance1.TornadoAppearance ?? appearance2.TornadoAppearance;
		slimeAppearance.VineAppearance = appearance1.VineAppearance ?? appearance2.VineAppearance;
		slimeAppearance.CrystalAppearance = appearance1.CrystalAppearance ?? appearance2.CrystalAppearance;
		slimeAppearance.ExplosionAppearance = appearance1.ExplosionAppearance ?? appearance2.ExplosionAppearance;
		return slimeAppearance;
	}
}

using System;
using UnityEngine;

[Serializable]
public class SlimeAppearanceStructure
{
	public Material[] DefaultMaterials;

	public SlimeAppearanceElement Element;

	public SlimeAppearanceMaterials[] ElementMaterials;

	public bool SupportsFaces;

	public SlimeFaceRules[] FaceRules;

	public SlimeAppearanceStructure(SlimeAppearanceStructure slimeAppearanceStructure)
	{
		DefaultMaterials = new Material[slimeAppearanceStructure.DefaultMaterials.Length];
		Array.Copy(slimeAppearanceStructure.DefaultMaterials, DefaultMaterials, DefaultMaterials.Length);
		Element = slimeAppearanceStructure.Element;
		ElementMaterials = new SlimeAppearanceMaterials[slimeAppearanceStructure.ElementMaterials.Length];
		Array.Copy(slimeAppearanceStructure.ElementMaterials, ElementMaterials, ElementMaterials.Length);
		SupportsFaces = slimeAppearanceStructure.SupportsFaces;
		FaceRules = new SlimeFaceRules[slimeAppearanceStructure.FaceRules.Length];
		Array.Copy(slimeAppearanceStructure.FaceRules, FaceRules, FaceRules.Length);
	}

	public bool ElementMaterialCountIsValid()
	{
		if (Element != null)
		{
			return ElementMaterials.Length == Element.Prefabs.Length;
		}
		return true;
	}
}

using System;
using UnityEngine;

[Serializable]
public class SlimeAppearanceObject : MonoBehaviour
{
	public SlimeAppearance.SlimeBone ParentBone;

	public SlimeAppearance.SlimeBone RootBone;

	public SlimeAppearance.SlimeBone[] AttachedBones;

	public bool IgnoreLODIndex;

	public int LODIndex;

	[Tooltip("Indicates that this object should be referenced by the slime's rubber bone effect. Only the highest quality LOD body of the slime should check this.")]
	public bool AttachRubberBoneEffect;

	[Tooltip("If this object is attached to the rubber bone effect of the slime, use this rubber type. Should generally be Slime or SlimeTarr")]
	public RubberBoneEffect.RubberType RubberType = RubberBoneEffect.RubberType.Slime;
}

using System;
using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using UnityEngine;

[ExecuteInEditMode]
public class SlimeAppearanceApplicator : MonoBehaviour
{
	public delegate void OnAppearanceChangedDelegate(SlimeAppearance newAppearance);

	[Serializable]
	public struct AppearanceObjectPair
	{
		public SlimeAppearanceObject Prefab;

		public SlimeAppearanceObject AppearanceObject;

		public AppearanceObjectPair(SlimeAppearanceObject prefab, SlimeAppearanceObject appearanceObject)
		{
			Prefab = prefab;
			AppearanceObject = appearanceObject;
		}
	}

	[Serializable]
	public struct BoneMapping
	{
		public SlimeAppearance.SlimeBone Bone;

		public GameObject BoneObject;
	}

	public struct FaceRenderer
	{
		public Renderer Renderer;

		public bool ShowEyes;

		public bool ShowMouth;
	}

	private class RecalculateBoundsHelper : MonoBehaviour
	{
		private SlimeAppearanceApplicator parent;

		public static void RecalculateBounds(SlimeAppearanceApplicator parent)
		{
			if (!(parent.recalculateBoundsHelper != null))
			{
				if (!Application.isPlaying || parent.animator == null)
				{
					parent.LODGroup.RecalculateBounds();
				}
				else if (!TryRecalculateBounds(parent))
				{
					parent.recalculateBoundsHelper = parent.gameObject.AddComponent<RecalculateBoundsHelper>();
					parent.recalculateBoundsHelper.parent = parent;
				}
			}
		}

		private static bool TryRecalculateBounds(SlimeAppearanceApplicator parent)
		{
			if (!parent.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (parent.animatorState == null)
			{
				parent.animatorState = parent.animator.GetBehaviour<SlimeAnimatorStateIdle>();
			}
			if (!parent.animatorState.IsInitialized)
			{
				parent.LODGroup.RecalculateBounds();
				return true;
			}
			if (parent.animatorState.IsCurrentState)
			{
				parent.LODGroup.RecalculateBounds();
				return true;
			}
			return false;
		}

		public void Update()
		{
			if (TryRecalculateBounds(parent))
			{
				Destroyer.Destroy(this, "RecalculateBoundsHelper.Update");
			}
		}
	}

	public SlimeAppearanceDirector SlimeAppearanceDirector;

	public SlimeAppearance Appearance;

	public SlimeDefinition SlimeDefinition;

	public BoneMapping[] Bones;

	public SlimeAppearanceObjectProvider AppearanceObjectProvider;

	public LODGroup LODGroup;

	public GameObject RootAppearanceObject;

	private Dictionary<SlimeAppearance.SlimeBone, GameObject> _boneLookup;

	private List<AppearanceObjectPair> _currentAppearanceObjects = new List<AppearanceObjectPair>();

	private List<FaceRenderer> _faceRenderers = new List<FaceRenderer>();

	private const int EYES_MATERIAL_INDEX_OFFSET = 2;

	private const int MOUTH_MATERIAL_INDEX_OFFSET = 1;

	private const int LOD_GROUP_LEVEL_COUNT = 4;

	private bool _isInitialized;

	public SlimeFace.SlimeExpression SlimeExpression = SlimeFace.SlimeExpression.Happy;

	private Animator animator;

	private RecalculateBoundsHelper recalculateBoundsHelper;

	private SlimeAnimatorStateIdle animatorState;

	public event OnAppearanceChangedDelegate OnAppearanceChanged = delegate
	{
	};

	public void Initialize(bool force = false)
	{
		if (_isInitialized && !force)
		{
			return;
		}
		if (_boneLookup == null)
		{
			_boneLookup = new Dictionary<SlimeAppearance.SlimeBone, GameObject>(SlimeAppearance.DefaultBoneComparer);
		}
		else
		{
			_boneLookup.Clear();
		}
		BoneMapping[] bones = Bones;
		for (int i = 0; i < bones.Length; i++)
		{
			BoneMapping boneMapping = bones[i];
			if (_boneLookup.ContainsKey(boneMapping.Bone))
			{
				Log.Error("Duplicate bone in SlimeAppearanceApplicator: {0}", boneMapping.Bone);
			}
			else
			{
				_boneLookup.Add(boneMapping.Bone, boneMapping.BoneObject);
			}
		}
	}

	public void Awake()
	{
		if (SlimeDefinition != null && SlimeAppearanceDirector != null)
		{
			SlimeAppearance chosenSlimeAppearance = SlimeAppearanceDirector.GetChosenSlimeAppearance(SlimeDefinition);
			if (Appearance != chosenSlimeAppearance)
			{
				Appearance = chosenSlimeAppearance;
			}
			SlimeAppearanceDirector.onSlimeAppearanceChanged += HandleChosenAppearanceChanged;
			animator = RootAppearanceObject.GetRequiredComponent<Animator>();
			ApplyAppearance();
		}
	}

	public void OnDestroy()
	{
		if (AppearanceObjectProvider != null)
		{
			foreach (AppearanceObjectPair currentAppearanceObject in _currentAppearanceObjects)
			{
				if (currentAppearanceObject.AppearanceObject != null)
				{
					AppearanceObjectProvider.Put(currentAppearanceObject.Prefab, currentAppearanceObject.AppearanceObject);
				}
			}
		}
		if (SlimeAppearanceDirector != null)
		{
			SlimeAppearanceDirector.onSlimeAppearanceChanged -= HandleChosenAppearanceChanged;
		}
	}

	public void ApplyAppearance()
	{
		Initialize();
		if (AppearanceObjectProvider == null)
		{
			ObjectPool appearanceObjectPool = SRSingleton<SceneContext>.Instance.appearanceObjectPool;
			AppearanceObjectProvider = new PooledSlimeAppearanceObjectProvider(appearanceObjectPool);
		}
		ClearAppearance();
		if (Appearance == null)
		{
			return;
		}
		if (animator != null)
		{
			if (Appearance.AnimatorOverride != null)
			{
				animator.runtimeAnimatorController = Appearance.AnimatorOverride;
			}
			else
			{
				animator.runtimeAnimatorController = SlimeAppearanceDirector.defaultAnimatorController;
			}
		}
		List<Renderer>[] array = new List<Renderer>[4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<Renderer>();
		}
		SlimeAppearanceStructure[] structures = Appearance.Structures;
		foreach (SlimeAppearanceStructure appearanceStructure in structures)
		{
			ApplyAppearanceStructure(appearanceStructure, array);
		}
		LOD[] lODs = LODGroup.GetLODs();
		for (int k = 0; k < lODs.Length; k++)
		{
			lODs[k].renderers = array[k].ToArray();
		}
		LODGroup.SetLODs(lODs);
		RecalculateBoundsHelper.RecalculateBounds(this);
		SetExpression(SlimeFace.SlimeExpression.Happy);
		this.OnAppearanceChanged(Appearance);
	}

	public Transform GetFashionParent(Fashion.Slot fashionSlot)
	{
		switch (fashionSlot)
		{
		case Fashion.Slot.TOP:
			return _boneLookup[SlimeAppearance.SlimeBone.JiggleTop].transform;
		case Fashion.Slot.FRONT:
			return _boneLookup[SlimeAppearance.SlimeBone.JiggleBack].transform;
		default:
			Log.Error("Unhandled fashion slot", "slot", fashionSlot);
			return null;
		}
	}

	private void HandleChosenAppearanceChanged(SlimeDefinition definition, SlimeAppearance newAppearance)
	{
		if (SlimeDefinition == definition)
		{
			Appearance = newAppearance;
			ApplyAppearance();
		}
	}

	private void ClearAppearance()
	{
		foreach (AppearanceObjectPair currentAppearanceObject in _currentAppearanceObjects)
		{
			AppearanceObjectProvider.Put(currentAppearanceObject.Prefab, currentAppearanceObject.AppearanceObject);
		}
		_currentAppearanceObjects.Clear();
		_faceRenderers.Clear();
	}

	private void ApplyAppearanceStructure(SlimeAppearanceStructure appearanceStructure, List<Renderer>[] lods)
	{
		for (int i = 0; i < appearanceStructure.Element.Prefabs.Length; i++)
		{
			ApplyAppearanceObject(appearanceStructure, appearanceStructure.Element, appearanceStructure.Element.Prefabs[i], i, lods);
		}
	}

	private void ApplyAppearanceObject(SlimeAppearanceStructure structure, SlimeAppearanceElement element, SlimeAppearanceObject appearancePrefab, int objectIndex, List<Renderer>[] lods)
	{
		GameObject gameObject = RootAppearanceObject;
		if (appearancePrefab.ParentBone != 0)
		{
			gameObject = _boneLookup.Get(appearancePrefab.ParentBone);
			if (gameObject == null)
			{
				Log.Error("Unable to find ParentBone for element.", "ParentBone", appearancePrefab.ParentBone, "AppearanceObject", appearancePrefab.name);
				return;
			}
		}
		SlimeAppearanceObject slimeAppearanceObject = null;
		try
		{
			slimeAppearanceObject = AppearanceObjectProvider.Get(appearancePrefab, gameObject);
		}
		catch (Exception ex)
		{
			Log.Error("caught exception e", "prefab", appearancePrefab, "exception", ex);
			throw;
		}
		_currentAppearanceObjects.Add(new AppearanceObjectPair(appearancePrefab, slimeAppearanceObject));
		Renderer component = slimeAppearanceObject.GetComponent<Renderer>();
		if (component != null)
		{
			int num = 0;
			if (structure.SupportsFaces)
			{
				SlimeFaceRules slimeFaceRules = structure.FaceRules[objectIndex];
				if (slimeFaceRules.ShowEyes || slimeFaceRules.ShowMouth)
				{
					_faceRenderers.Add(new FaceRenderer
					{
						Renderer = component,
						ShowEyes = structure.FaceRules[objectIndex].ShowEyes,
						ShowMouth = structure.FaceRules[objectIndex].ShowMouth
					});
				}
				num += (slimeFaceRules.ShowEyes ? 1 : 0) + (slimeFaceRules.ShowMouth ? 1 : 0);
			}
			Material[] array = (structure.ElementMaterials[objectIndex].OverrideDefaults ? structure.ElementMaterials[objectIndex].Materials : structure.DefaultMaterials);
			num += array.Length;
			Material[] array2 = new Material[num];
			Array.Copy(array, array2, array.Length);
			component.materials = array2;
			if (component is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = component as SkinnedMeshRenderer;
				Transform[] array3 = new Transform[appearancePrefab.AttachedBones.Length];
				for (int i = 0; i < appearancePrefab.AttachedBones.Length; i++)
				{
					array3[i] = _boneLookup[appearancePrefab.AttachedBones[i]].transform;
				}
				skinnedMeshRenderer.bones = array3;
				skinnedMeshRenderer.rootBone = _boneLookup[appearancePrefab.RootBone].transform;
			}
			if (!appearancePrefab.IgnoreLODIndex)
			{
				lods[appearancePrefab.LODIndex].Add(component);
			}
		}
		if (appearancePrefab.AttachRubberBoneEffect && component is SkinnedMeshRenderer)
		{
			RubberBoneEffect component2 = RootAppearanceObject.GetComponent<RubberBoneEffect>();
			component2.skinRenderer = component as SkinnedMeshRenderer;
			component2.Presets = appearancePrefab.RubberType;
		}
	}

	public void SetExpression(SlimeFace.SlimeExpression slimeExpression)
	{
		SlimeExpressionFace expressionFace = Appearance.Face.GetExpressionFace(slimeExpression);
		foreach (FaceRenderer faceRenderer in _faceRenderers)
		{
			Material[] sharedMaterials = faceRenderer.Renderer.sharedMaterials;
			int num = sharedMaterials.Length - 2;
			int num2 = sharedMaterials.Length - 1;
			if (faceRenderer.ShowEyes != faceRenderer.ShowMouth)
			{
				num = num2;
			}
			if (faceRenderer.ShowEyes && expressionFace.Eyes != null)
			{
				sharedMaterials[num] = expressionFace.Eyes;
			}
			if (faceRenderer.ShowMouth && expressionFace.Mouth != null)
			{
				sharedMaterials[num2] = expressionFace.Mouth;
			}
			faceRenderer.Renderer.sharedMaterials = sharedMaterials;
		}
	}

	public SlimeAppearance.Palette GetAppearancePalette()
	{
		if (Appearance == null)
		{
			Log.Warning("Appearance was null when retrieving appearance palette. Returning default palette");
			return SlimeAppearance.Palette.Default;
		}
		return Appearance.ColorPalette;
	}

	private void SetHideFlags(GameObject gameObject)
	{
		gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		if (gameObject.transform.childCount > 0)
		{
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			}
		}
	}
}

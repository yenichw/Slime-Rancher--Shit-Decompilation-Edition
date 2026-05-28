using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeLineup : MonoBehaviour
{
	public LookupDirector lookupDirector;

	public SlimeDefinitions slimeDefinitions;

	public Vector2 spacing;

	private List<GameObject> slimePreviews = new List<GameObject>();

	private static readonly Type[] componentBlacklist = new Type[12]
	{
		typeof(SlimeSubbehaviourPlexer),
		typeof(SlimeSubbehaviour),
		typeof(DestroyOnTouching),
		typeof(DestroyAfterTime),
		typeof(GlitchTarrSterilizeOnWater),
		typeof(GlintController),
		typeof(SlimeHealth),
		typeof(DestroyOutsideHoursOfDay),
		typeof(MaybeCullOnReenable),
		typeof(DamagePlayerOnTouch),
		typeof(FireSlimeIgnition),
		typeof(RegionMember)
	};

	public void ShowSlime(SlimeDefinition slimeDefinition)
	{
		CreateSlimePreviews(new List<SlimeDefinition>(new SlimeDefinition[1] { slimeDefinition }));
	}

	public void ShowSlimeAndLargos(SlimeDefinition slimeDefinition)
	{
		List<SlimeDefinition> list = new List<SlimeDefinition>(new SlimeDefinition[1] { slimeDefinition });
		list.AddRange(slimeDefinitions.Slimes.Where((SlimeDefinition slime) => slime.BaseSlimes.Contains(slimeDefinition)));
		CreateSlimePreviews(list);
	}

	public void ShowAllBaseSlimes()
	{
		CreateSlimePreviews(slimeDefinitions.Slimes.Where((SlimeDefinition slime) => !slime.IsLargo).ToList());
	}

	public void ShowAllSlimes()
	{
		CreateSlimePreviews(new List<SlimeDefinition>(slimeDefinitions.Slimes));
	}

	private void CreateSlimePreviews(List<SlimeDefinition> definitions)
	{
		ClearPreviews();
		int num = Mathf.CeilToInt(Mathf.Sqrt(definitions.Count));
		float num2 = spacing.x * ((float)num / 2f);
		float num3 = spacing.y * ((float)num / 2f);
		for (int i = 0; i < definitions.Count; i++)
		{
			Vector3 position = new Vector3((float)(i % num) * spacing.x - num2 + base.transform.position.x, base.transform.position.y, (float)(i / num) * spacing.y - num3 + base.transform.position.z);
			slimePreviews.Add(CreatePreviewSlime(definitions[i], position));
		}
	}

	private GameObject CreatePreviewSlime(SlimeDefinition slimeDefinition, Vector3 position)
	{
		GameObject gameObject = SRSingleton<SceneContext>.Instance.GameModel.InstantiateActor(lookupDirector.GetPrefab(slimeDefinition.IdentifiableId), SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId(), position, Quaternion.Euler(0f, 180f, 0f));
		Type[] array = componentBlacklist;
		foreach (Type type in array)
		{
			Component[] components = gameObject.GetComponents(type);
			for (int j = 0; j < components.Length; j++)
			{
				UnityEngine.Object.Destroy(components[j]);
			}
		}
		return gameObject;
	}

	private void ClearPreviews()
	{
		foreach (GameObject slimePreview in slimePreviews)
		{
			UnityEngine.Object.Destroy(slimePreview);
		}
		slimePreviews.Clear();
	}
}

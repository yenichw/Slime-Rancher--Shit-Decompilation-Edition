using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class OrthoLineup : MonoBehaviour
{
	public SlimeAppearanceApplicator appearancePrefab;

	public SlimeDefinitions definitions;

	public Vector2 viewSpacing = new Vector2(1.5f, 1.25f);

	public float extraLabelSpacing = 1f;

	public float extraAppearanceSpacing = 1f;

	public Camera cam;

	public float cameraSpeed = 2f;

	public bool showLabels = true;

	public TextMesh labelPrefab;

	public RuntimeAnimatorController animatorController;

	public AnimationClip idle;

	public AnimationClip idleOverride;

	public bool includeLargos;

	public Quaternion[] views = new Quaternion[3]
	{
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity
	};

	private readonly Type[] blacklistedObjectTypes = new Type[2]
	{
		typeof(RadExpandMarker),
		typeof(TrailRenderer)
	};

	public void Start()
	{
		Time.timeScale = 0f;
		SRQualitySettings.CurrentLevel = SRQualitySettings.Level.VERY_HIGH;
		ShowLineup();
	}

	public void Update()
	{
		float axisRaw = Input.GetAxisRaw("Horizontal");
		float axisRaw2 = Input.GetAxisRaw("Vertical");
		Vector3 vector = (float)((!Input.GetKey(KeyCode.LeftShift)) ? 1 : 3) * cameraSpeed * Time.unscaledDeltaTime * new Vector3(axisRaw, axisRaw2, 0f).normalized;
		cam.transform.position += vector;
		if (Input.GetKeyDown(KeyCode.Space))
		{
			string text = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar}orthoslimes-{DateTime.Now.ToFileTime()}.png";
			ScreenCapture.CaptureScreenshot(text);
			Log.Debug("Screenshot saved as " + text);
		}
	}

	public void ShowLineup()
	{
		List<SlimeDefinition> list = definitions.Slimes.Where((SlimeDefinition slime) => includeLargos || !slime.IsLargo).ToList();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animatorController);
		animatorOverrideController.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>(new KeyValuePair<AnimationClip, AnimationClip>[1]
		{
			new KeyValuePair<AnimationClip, AnimationClip>(idle, idleOverride)
		}));
		for (int i = 0; i < list.Count; i++)
		{
			SlimeDefinition slimeDefinition = list[i];
			Vector3 position = new Vector3(0f, (float)i * (0f - viewSpacing.y) + (showLabels ? ((float)i * (0f - extraLabelSpacing)) : 0f));
			GameObject gameObject = new GameObject(slimeDefinition.Name);
			gameObject.transform.position = position;
			gameObject.transform.parent = base.transform;
			for (int j = 0; j < slimeDefinition.Appearances.Count(); j++)
			{
				SlimeAppearance slimeAppearance = slimeDefinition.Appearances.ElementAt(j);
				Vector3 localPosition = new Vector3((float)j * (viewSpacing.x * (float)views.Length + extraAppearanceSpacing), 0f, 0f);
				string text = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("actor").Xlate(slimeAppearance.NameXlateKey);
				if (string.IsNullOrEmpty(text))
				{
					text = "Classic";
				}
				GameObject gameObject2 = new GameObject(text);
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = localPosition;
				if (showLabels)
				{
					string text2 = $"{slimeDefinition.Name} ({text})";
					TextMesh textMesh = UnityEngine.Object.Instantiate(labelPrefab, gameObject2.transform);
					textMesh.transform.localPosition = Vector3.zero;
					textMesh.transform.name = "Label";
					textMesh.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					textMesh.text = text2;
				}
				for (int k = 0; k < views.Length; k++)
				{
					SlimeAppearanceApplicator slimeAppearanceApplicator = LineupUtils.GenerateAppearancePreview(appearancePrefab, slimeDefinition, slimeAppearance);
					slimeAppearanceApplicator.GetComponentInChildren<Animator>().runtimeAnimatorController = animatorOverrideController;
					Type[] array = blacklistedObjectTypes;
					foreach (Type t in array)
					{
						Component[] componentsInChildren = slimeAppearanceApplicator.GetComponentsInChildren(t);
						for (int m = 0; m < componentsInChildren.Length; m++)
						{
							componentsInChildren[m].gameObject.SetActive(value: false);
						}
					}
					Vector3 localPosition2 = new Vector3((float)k * viewSpacing.x, showLabels ? (0f - extraLabelSpacing) : 0f, 0f);
					slimeAppearanceApplicator.transform.parent = gameObject2.transform;
					slimeAppearanceApplicator.transform.localPosition = localPosition2;
					slimeAppearanceApplicator.transform.rotation = views[k];
				}
			}
		}
	}
}

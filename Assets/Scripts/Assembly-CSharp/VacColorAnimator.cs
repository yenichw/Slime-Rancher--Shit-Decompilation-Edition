using UnityEngine;

public class VacColorAnimator : MonoBehaviour
{
	public Renderer spiralRenderer;

	public Renderer dialRenderer;

	private bool isActive;

	private bool inVacMode;

	private float colorTarget = 0.5f;

	private float colorVal = 0.5f;

	private Material vacSpiralMat;

	private Material vacDialMat;

	private PlayerState playerState;

	private LookupDirector lookupDir;

	private RanchDirector ranchDir;

	public SlimeAppearanceDirector slimeAppearanceDirector;

	private static readonly int PROPERTY_SPIRAL_COLOR = Shader.PropertyToID("_SpiralColor");

	private static readonly int PROPERTY_AMMO_FULLNESS = Shader.PropertyToID("_AmmoFullness");

	private static readonly int PROPERTY_AMMO_COLOR = Shader.PropertyToID("_AmmoColor");

	private const float SECS_TO_TRANSITION = 0.5f;

	private const float TRANS_PER_SEC = 2f;

	public void Awake()
	{
		vacSpiralMat = spiralRenderer.material;
		vacDialMat = dialRenderer.material;
		ranchDir = SRSingleton<SceneContext>.Instance.RanchDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		slimeAppearanceDirector = SRSingleton<SceneContext>.Instance.SlimeAppearanceDirector;
		ranchDir.RegisterVacRecolorMat(vacSpiralMat);
		ranchDir.RegisterVacRecolorMat(vacDialMat);
	}

	public void OnDestroy()
	{
		ranchDir.UnregisterVacRecolorMat(vacSpiralMat);
		ranchDir.UnregisterVacRecolorMat(vacDialMat);
		Destroyer.Destroy(vacSpiralMat, "VacColorAnimator.OnDestroy#1");
		Destroyer.Destroy(vacDialMat, "VacColorAnimator.OnDestroy#2");
	}

	public void SetVacActive(bool isActive)
	{
		this.isActive = isActive;
		UpdateColorTarget();
	}

	public void SetVacMode(bool inVacMode)
	{
		this.inVacMode = inVacMode;
		UpdateColorTarget();
	}

	public void Update()
	{
		if (colorVal > colorTarget)
		{
			colorVal = Mathf.Max(colorTarget, colorVal - Time.deltaTime * 2f);
		}
		else if (colorVal < colorTarget)
		{
			colorVal = Mathf.Min(colorTarget, colorVal + Time.deltaTime * 2f);
		}
		float selectedFullness = playerState.Ammo.GetSelectedFullness();
		Color value = Color.black;
		GameObject selectedStored = playerState.Ammo.GetSelectedStored();
		if (selectedStored != null)
		{
			Identifiable component = selectedStored.GetComponent<Identifiable>();
			if (component != null)
			{
				value = GetCurrentColor(component.id);
			}
		}
		vacSpiralMat.SetFloat(PROPERTY_SPIRAL_COLOR, colorVal);
		vacDialMat.SetFloat(PROPERTY_SPIRAL_COLOR, colorVal);
		vacDialMat.SetFloat(PROPERTY_AMMO_FULLNESS, selectedFullness);
		vacDialMat.SetColor(PROPERTY_AMMO_COLOR, value);
	}

	private void UpdateColorTarget()
	{
		colorTarget = ((!isActive) ? 0.5f : (inVacMode ? 0f : 1f));
	}

	private Color GetCurrentColor(Identifiable.Id id)
	{
		if (Identifiable.IsSlime(id))
		{
			return slimeAppearanceDirector.GetChosenSlimeAppearance(id).ColorPalette.Ammo;
		}
		if (id != 0)
		{
			return lookupDir.GetColor(id);
		}
		return Color.clear;
	}
}

using UnityEngine;

public class SENBDLMainCube : MonoBehaviour
{
	private Color[] glowColors = new Color[4];

	public GameObject orbitingCube;

	public GameObject glowingOrbitingCube;

	public GameObject cubeEmissivePart;

	public GameObject particles;

	private const float newColorFrequency = 8f;

	private float newColorCounter;

	private Color currentColor;

	private Color previousColor;

	[HideInInspector]
	public Color glowColor;

	private int currentColorIndex;

	private float bloomAmount = 0.04f;

	private float lensDirtAmount = 0.3f;

	private float fps;

	private float deltaTime;

	private SENaturalBloomAndDirtyLens bloomShader;

	private void Start()
	{
		glowColors[0] = new Color(1f, 0.47058824f, 0.050980393f);
		glowColors[2] = new Color(28f / 85f, 0.6392157f, 1f);
		glowColors[1] = new Color(31f / 51f, 1f, 0.11764706f);
		glowColors[3] = new Color(1f, 0.18431373f, 0f);
		currentColor = glowColors[0];
		SENBDLGlobal.sphereOfCubesRotation = Quaternion.identity;
		for (int i = 0; i < 150; i++)
		{
			Object.Instantiate(orbitingCube, Vector3.zero, Quaternion.identity);
		}
		for (int j = 0; j < 19; j++)
		{
			Object.Instantiate(glowingOrbitingCube, Vector3.zero, Quaternion.identity);
		}
		Camera.main.backgroundColor = new Color(0.08f, 0.08f, 0.08f);
		SENBDLGlobal.mainCube = this;
		bloomShader = Camera.main.GetComponent<SENaturalBloomAndDirtyLens>();
	}

	private void OnGUI()
	{
	}

	private void Update()
	{
		deltaTime = Time.deltaTime / Time.timeScale;
		AnimateColor();
		RotateSphereOfCubes();
		float num = 40f;
		Vector3 vector = Vector3.up * num;
		vector = Quaternion.Euler(Vector3.right * Time.time * num * 0.5f) * vector;
		base.transform.Rotate(vector * Time.deltaTime);
		IncrementCounters();
		GetInput();
		UpdateShaderValues();
		SmoothFPSCounter();
	}

	private void AnimateColor()
	{
		if (newColorCounter >= 8f)
		{
			newColorCounter = 0f;
			currentColorIndex = (currentColorIndex + 1) % glowColors.Length;
			previousColor = currentColor;
			currentColor = glowColors[currentColorIndex];
		}
		float t = Mathf.Clamp01(newColorCounter / 8f * 5f);
		glowColor = Color.Lerp(previousColor, currentColor, t);
		Color color = glowColor * Mathf.Pow(Mathf.Sin(Time.time) * 0.48f + 0.52f, 4f);
		cubeEmissivePart.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
		GetComponent<Light>().color = color;
		Color a = default(Color);
		a.r = 1f - glowColor.r;
		a.g = 1f - glowColor.g;
		a.b = 1f - glowColor.b;
		a = Color.Lerp(a, Color.white, 0.1f);
		particles.GetComponent<Renderer>().material.SetColor("_TintColor", a);
	}

	private void RotateSphereOfCubes()
	{
		SENBDLGlobal.sphereOfCubesRotation = Quaternion.Euler(Vector3.up * Time.time * 20f);
	}

	private void IncrementCounters()
	{
		newColorCounter += Time.deltaTime;
	}

	private void GetInput()
	{
		if (Input.GetKey(KeyCode.RightArrow))
		{
			bloomAmount += 0.2f * deltaTime;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			bloomAmount -= 0.2f * deltaTime;
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			lensDirtAmount += 0.4f * deltaTime;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			lensDirtAmount -= 0.4f * deltaTime;
		}
		if (Input.GetKey(KeyCode.Period))
		{
			Time.timeScale += 0.5f * deltaTime;
		}
		if (Input.GetKey(KeyCode.Comma))
		{
			Time.timeScale -= 0.5f * deltaTime;
		}
		bloomAmount = Mathf.Clamp(bloomAmount, 0f, 0.4f);
		lensDirtAmount = Mathf.Clamp(lensDirtAmount, 0f, 0.95f);
		Time.timeScale = Mathf.Clamp(Time.timeScale, 0.1f, 1f);
		if (Input.GetKeyDown(KeyCode.Space))
		{
			bloomAmount = 0.05f;
			lensDirtAmount = 0.1f;
			Time.timeScale = 1f;
		}
	}

	private void UpdateShaderValues()
	{
		bloomShader.bloomIntensity = bloomAmount;
		bloomShader.lensDirtIntensity = lensDirtAmount;
	}

	private void SmoothFPSCounter()
	{
		fps = Mathf.Lerp(fps, 1f / deltaTime, 5f * deltaTime);
	}
}

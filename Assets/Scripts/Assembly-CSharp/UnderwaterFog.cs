using UnityEngine;

public class UnderwaterFog : MonoBehaviour
{
	private Color normalColor;

	private float normalDensity;

	private Color underwaterColor;

	private bool isUnderwater;

	private void Start()
	{
		normalColor = RenderSettings.fogColor;
		normalDensity = RenderSettings.fogDensity;
		underwaterColor = new Color(0.22f, 0.65f, 0.77f, 0.5f);
	}

	private void Update()
	{
		if (GetComponent<Collider>().bounds.Contains(Camera.main.transform.position))
		{
			SetUnderwater();
		}
		else
		{
			SetNormal();
		}
	}

	private void SetNormal()
	{
		if (isUnderwater)
		{
			RenderSettings.fogColor = normalColor;
			RenderSettings.fogDensity = normalDensity;
			isUnderwater = false;
		}
	}

	private void SetUnderwater()
	{
		if (!isUnderwater)
		{
			RenderSettings.fogColor = underwaterColor;
			RenderSettings.fogDensity = 0.05f;
			isUnderwater = true;
		}
	}
}

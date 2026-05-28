using UnityEngine;

public class WeatherEffectAttachment : MonoBehaviour
{
	private GameObject currWeather;

	private int blockers;

	public void OnTriggerEnter(Collider col)
	{
		if (col.GetComponent<WeatherBlockingTrigger>() != null)
		{
			blockers++;
			UpdateBlocked();
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (col.GetComponent<WeatherBlockingTrigger>() != null)
		{
			blockers--;
			UpdateBlocked();
		}
	}

	public void SetWeather(GameObject weatherPrefab)
	{
		if (currWeather != null)
		{
			Destroyer.Destroy(currWeather, "WeatherEffectAttachment.SetWeather");
		}
		if (weatherPrefab != null)
		{
			currWeather = Object.Instantiate(weatherPrefab);
			currWeather.transform.SetParent(base.transform, worldPositionStays: false);
			UpdateBlocked();
		}
	}

	private void UpdateBlocked()
	{
		if (currWeather != null)
		{
			currWeather.SetActive(blockers == 0);
		}
	}
}

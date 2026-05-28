using UnityEngine;

public class EnableOnlyDuringTimeWindow : MonoBehaviour
{
	public float startHour;

	public float endHour;

	public GameObject[] toEnable;

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void Update()
	{
		float num = timeDir.CurrHour();
		bool active = (startHour <= num && num <= endHour) || (startHour > endHour && (num >= startHour || num <= endHour));
		GameObject[] array = toEnable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
	}
}

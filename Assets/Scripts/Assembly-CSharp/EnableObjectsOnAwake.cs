using UnityEngine;

public class EnableObjectsOnAwake : MonoBehaviour
{
	public GameObject[] toEnable;

	public void Awake()
	{
		GameObject[] array = toEnable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}
}

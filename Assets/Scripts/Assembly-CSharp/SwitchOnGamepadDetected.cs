using UnityEngine;

public class SwitchOnGamepadDetected : MonoBehaviour
{
	public GameObject showOnGamepadDetected;

	public GameObject showOnGamepadNotDetected;

	private void Update()
	{
		bool flag = InputDirector.UsingGamepad();
		showOnGamepadDetected.SetActive(flag);
		showOnGamepadNotDetected.SetActive(!flag);
	}
}

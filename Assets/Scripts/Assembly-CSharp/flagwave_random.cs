using UnityEngine;

public class flagwave_random : MonoBehaviour
{
	public void OnEnable()
	{
		Animation component = GetComponent<Animation>();
		component["flagwave_loop"].normalizedTime = Randoms.SHARED.GetInRange(0, 1);
		component["flagwave_loop"].normalizedSpeed = Randoms.SHARED.GetInRange(0.16f, 0.18f);
	}
}

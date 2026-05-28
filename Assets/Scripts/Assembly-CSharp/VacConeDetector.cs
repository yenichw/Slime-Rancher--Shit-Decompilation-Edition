using UnityEngine;

public class VacConeDetector : MonoBehaviour
{
	private GordoFaceAnimator faceAnim;

	private int vacTriggerCount;

	public void Awake()
	{
		faceAnim = GetComponent<GordoFaceAnimator>();
	}

	public void OnEnable()
	{
		vacTriggerCount = 0;
	}

	public void OnTriggerEnter(Collider col)
	{
		if ((bool)col.GetComponentInParent<TrackCollisions>())
		{
			vacTriggerCount++;
			if (vacTriggerCount == 1)
			{
				faceAnim.SetInVac(val: true);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if ((bool)col.GetComponentInParent<TrackCollisions>())
		{
			vacTriggerCount--;
			if (vacTriggerCount == 0)
			{
				faceAnim.SetInVac(val: false);
			}
		}
	}
}

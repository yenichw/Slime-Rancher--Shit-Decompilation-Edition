public class TimeOfDayRotator : SRBehaviour
{
	public bool isNightLight;

	public void Start()
	{
		SRSingleton<SceneContext>.Instance.AmbianceDirector.RegisterTimeOfDayRotator(base.gameObject, isNightLight);
	}
}

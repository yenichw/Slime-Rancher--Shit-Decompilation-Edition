public class UIInputLocker : SRBehaviour
{
	public bool lockEvenSpecialScenes;

	public void OnEnable()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.TimeDirector.Pause(pauseSFX: true, lockEvenSpecialScenes);
		}
		if (!Levels.isSpecial() || lockEvenSpecialScenes)
		{
			SECTR_AudioSystem.PauseNonUISFX(pause: true);
		}
	}

	public void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.TimeDirector.Unpause(unpauseSFX: true, lockEvenSpecialScenes);
		}
		if (!Levels.isSpecial() || lockEvenSpecialScenes)
		{
			SECTR_AudioSystem.PauseNonUISFX(pause: false);
		}
	}
}

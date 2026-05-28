using System.Collections;
using UnityEngine;

public class DestroyAfterTime : RegisteredActorBehaviour, RegistryUpdateable
{
	public float lifeTimeHours = 72f;

	public float nightLifeTimeFactor = 1f;

	public GameObject destroyFX;

	public bool destroyAsActor = true;

	private TimeDirector timeDir;

	private double deathTime;

	private bool scaleDownOnDestroy;

	private SECTR_AudioCue scaleDownCue;

	private bool fizzleParticlesOnDestroy;

	private bool destroying;

	private DestroyAfterTimeCondition destroyAfterTimeCondition;

	private bool hasDestroyAfterTimeCondition;

	private const float SCALE_DOWN_TIME = 4f;

	private const float FIZZLE_TIME = 4f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		float num = timeDir.CurrHourOrStart();
		bool flag = num < 6f || num > 18f;
		deathTime = timeDir.HoursFromNowOrStart((flag ? nightLifeTimeFactor : 1f) * lifeTimeHours);
		destroyAfterTimeCondition = GetComponent<DestroyAfterTimeCondition>();
		hasDestroyAfterTimeCondition = destroyAfterTimeCondition != null;
	}

	public void RegistryUpdate()
	{
		if (!timeDir.HasReached(deathTime) || destroying || (hasDestroyAfterTimeCondition && !destroyAfterTimeCondition.ReadyToDestroy()))
		{
			return;
		}
		destroying = true;
		bool num = timeDir.HasReached(deathTime + 3600.0);
		GetComponent<DestroyAfterTimeListener>()?.WillDestroyAfterTime();
		if (num)
		{
			DoDestroy("DestroyAfterTime.RegistryUpdate (skippedFX)");
			return;
		}
		if (scaleDownOnDestroy)
		{
			StartCoroutine(ScaleThenDestroy());
		}
		else if (fizzleParticlesOnDestroy)
		{
			StartCoroutine(FizzleThenDestroy());
		}
		else
		{
			DoDestroy("DestroyAfterTime.RegistryUpdate");
		}
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, Quaternion.identity);
		}
	}

	private void DoDestroy(string reason)
	{
		if (destroyAsActor)
		{
			Destroyer.DestroyActor(base.gameObject, reason);
		}
		else
		{
			Destroyer.Destroy(base.gameObject, reason);
		}
	}

	private IEnumerator FizzleThenDestroy()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
		yield return new WaitForSeconds(4f);
		DoDestroy("DestroyAfterTime.FizzleThenDestroy");
	}

	private IEnumerator ScaleThenDestroy()
	{
		if (scaleDownCue != null)
		{
			SECTR_AudioSystem.Play(scaleDownCue, base.transform, Vector3.zero, loop: false);
		}
		TweenScaleDownItem(base.gameObject);
		yield return new WaitForSeconds(4f);
		DoDestroy("DestroyAfterTime.ScaleThenDestroy");
	}

	private void TweenScaleDownItem(GameObject obj)
	{
		TweenUtil.ScaleOut(obj, 4f);
	}

	public void AdvanceHours(float hours)
	{
		deathTime -= hours * 3600f;
	}

	public void MultiplyRemainingHours(float factor)
	{
		double num = deathTime - timeDir.WorldTime();
		deathTime = timeDir.WorldTime() + num * (double)factor;
	}

	public void SetDeathTime(double time)
	{
		deathTime = time;
	}

	public void ScaleDownOnDestroy()
	{
		scaleDownOnDestroy = true;
	}

	public void SetScaleDownCue(SECTR_AudioCue cue)
	{
		scaleDownCue = cue;
	}

	public void FizzleParticlesOnDestroy()
	{
		fizzleParticlesOnDestroy = true;
	}

	public double GetDeathTime()
	{
		return deathTime;
	}
}

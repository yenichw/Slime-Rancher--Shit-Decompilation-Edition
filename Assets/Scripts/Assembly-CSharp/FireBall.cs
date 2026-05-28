using UnityEngine;

public class FireBall : SRBehaviour
{
	public int bouncesToDie = 3;

	public float hoursToLive = 0.1f;

	private int numBounces;

	private double timeToDie;

	protected bool defused;

	public GameObject vaporizeFx;

	public GameObject expireFX;

	public SECTR_AudioCue spawnCue;

	public SECTR_AudioCue bounceCue;

	public SECTR_AudioCue shatterCue;

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		timeToDie = timeDir.HoursFromNow(hoursToLive);
	}

	public void OnEnable()
	{
		Reset();
	}

	private void Reset()
	{
		timeToDie = timeDir.HoursFromNow(hoursToLive);
		SECTR_AudioSystem.Play(spawnCue, base.transform.position, loop: false);
		numBounces = 0;
		defused = false;
	}

	public void Update()
	{
		if (HasBouncedTooMuch() || timeDir.HasReached(timeToDie))
		{
			OnExpire();
			RequestDestroy("FireBall.Update");
		}
	}

	private bool HasBouncedTooMuch()
	{
		if (bouncesToDie > 0 && numBounces >= bouncesToDie)
		{
			return true;
		}
		return false;
	}

	protected virtual void OnExpire()
	{
		if (expireFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(expireFX, base.transform.position, base.transform.rotation);
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (base.name.Contains("Fireball"))
		{
			Log.Warning("Bounced!", "this.name", base.name, "col.name", col.gameObject.name);
		}
		numBounces++;
		Ignitable component = col.gameObject.GetComponent<Ignitable>();
		SECTR_AudioSystem.Play(bounceCue, base.transform.position, loop: false);
		component?.Ignite(base.gameObject);
	}

	public void Vaporize()
	{
		DefuseAndDestroy();
		SECTR_AudioSystem.Play(shatterCue, base.transform.position, loop: false);
		if (vaporizeFx != null)
		{
			SRBehaviour.SpawnAndPlayFX(vaporizeFx, base.gameObject.transform.position, Quaternion.identity);
		}
	}

	public void DefuseAndDestroy()
	{
		defused = true;
		RequestDestroy("FireBall.DefuseAndDestroy");
	}
}

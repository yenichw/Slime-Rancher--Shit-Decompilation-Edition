using UnityEngine;

public class SlimeHealth : SRBehaviour, Damageable
{
	public delegate void OnDamage(GameObject damageSource);

	public int maxHealth = 100;

	private int currHealth;

	public OnDamage onDamage;

	public virtual void Start()
	{
		currHealth = maxHealth;
	}

	public int GetCurrHealth()
	{
		return currHealth;
	}

	public bool Damage(int healthLoss, GameObject source)
	{
		currHealth -= healthLoss;
		if (onDamage != null)
		{
			onDamage(source);
		}
		if (currHealth <= 0)
		{
			currHealth = 0;
			return true;
		}
		SlimeFaceAnimator component = GetComponent<SlimeFaceAnimator>();
		if (component != null)
		{
			component.SetTrigger("triggerWince");
		}
		return false;
	}
}

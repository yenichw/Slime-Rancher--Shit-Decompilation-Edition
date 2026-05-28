using UnityEngine;

public class DestroyOnIgnite : SRBehaviour, Ignitable
{
	public GameObject igniteFX;

	public void Ignite(GameObject igniter)
	{
		if (igniteFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(igniteFX);
		}
		Destroyer.DestroyActor(base.gameObject, "DestroyOnIgnite.Ignite");
	}
}

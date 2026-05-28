using UnityEngine;

public class DervishVortexAdder : VortexAdder
{
	public bool allowNonDervishSlimes;

	protected override bool CanAdd(GameObject gameObj)
	{
		if (!base.CanAdd(gameObj))
		{
			return false;
		}
		Identifiable.Id id = Identifiable.GetId(gameObj);
		if (!Identifiable.IsNonSlimeResource(id))
		{
			if (allowNonDervishSlimes && Identifiable.IsSlime(id))
			{
				return gameObj.GetComponent<DervishSlimeSpin>() == null;
			}
			return false;
		}
		return true;
	}
}

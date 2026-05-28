using System.Collections.Generic;

public class ToyDirector
{
	private static readonly List<Identifiable.Id> BASE_TOYS = new List<Identifiable.Id>
	{
		Identifiable.Id.BEACH_BALL_TOY,
		Identifiable.Id.BIG_ROCK_TOY,
		Identifiable.Id.YARN_BALL_TOY,
		Identifiable.Id.NIGHT_LIGHT_TOY,
		Identifiable.Id.POWER_CELL_TOY,
		Identifiable.Id.BOMB_BALL_TOY,
		Identifiable.Id.BUZZY_BEE_TOY,
		Identifiable.Id.RUBBER_DUCKY_TOY,
		Identifiable.Id.OCTO_BUDDY_TOY
	};

	private const int CORPORATE_LEVEL_UNLOCK = 10;

	private static readonly List<Identifiable.Id> UPGRADED_TOYS = new List<Identifiable.Id>
	{
		Identifiable.Id.CRYSTAL_BALL_TOY,
		Identifiable.Id.STUFFED_CHICKEN_TOY,
		Identifiable.Id.PUZZLE_CUBE_TOY,
		Identifiable.Id.DISCO_BALL_TOY,
		Identifiable.Id.GYRO_TOP_TOY,
		Identifiable.Id.CHARCOAL_BRICK_TOY,
		Identifiable.Id.SOL_MATE_TOY,
		Identifiable.Id.STEGO_BUDDY_TOY
	};

	private List<Identifiable.Id> registered = new List<Identifiable.Id>();

	public void Register(Identifiable.Id id)
	{
		registered.RemoveAll((Identifiable.Id it) => it == id);
		registered.Add(id);
	}

	public IEnumerable<Identifiable.Id> GetPurchaseableToys()
	{
		ProgressDirector progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
		int progress = progressDirector.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
		foreach (Identifiable.Id bASE_TOY in BASE_TOYS)
		{
			yield return bASE_TOY;
		}
		if (progress >= 10)
		{
			foreach (Identifiable.Id uPGRADED_TOY in UPGRADED_TOYS)
			{
				yield return uPGRADED_TOY;
			}
		}
		foreach (Identifiable.Id item in registered)
		{
			yield return item;
		}
	}
}

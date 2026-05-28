using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class LiquidSourceModel : IdHandlerModel
	{
		public Vector3 pos;

		public bool isScaling;

		public float unitsFilled;

		public void Push(float unitsFilled)
		{
			this.unitsFilled = unitsFilled;
		}

		public void Pull(out float unitsFilled)
		{
			unitsFilled = this.unitsFilled;
		}
	}
}

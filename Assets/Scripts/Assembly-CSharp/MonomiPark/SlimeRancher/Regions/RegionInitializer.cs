using UnityEngine;

namespace MonomiPark.SlimeRancher.Regions
{
	public class RegionInitializer : MonoBehaviour
	{
		public void Start()
		{
			GetComponent<Region>().CheckReferences();
		}
	}
}

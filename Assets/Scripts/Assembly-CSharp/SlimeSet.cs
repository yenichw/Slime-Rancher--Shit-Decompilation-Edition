using System;
using UnityEngine;

[Serializable]
public class SlimeSet
{
	[Serializable]
	public class Member
	{
		public GameObject prefab;

		public float weight = 1f;
	}

	public Member[] members;
}

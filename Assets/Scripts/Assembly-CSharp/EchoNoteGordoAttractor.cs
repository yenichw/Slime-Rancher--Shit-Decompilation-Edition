using UnityEngine;

public class EchoNoteGordoAttractor : Attractor
{
	[Tooltip("Factor applied to the slimes to determine aweness.")]
	[Range(0f, 1f)]
	public float attractionFactor;

	public void Awake()
	{
		SetAweFactor(attractionFactor);
	}

	public override bool CauseMoveTowards()
	{
		return true;
	}
}

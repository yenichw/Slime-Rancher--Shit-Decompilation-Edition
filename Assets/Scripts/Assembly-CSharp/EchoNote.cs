using UnityEngine;

public class EchoNote : MonoBehaviour
{
	[Tooltip("Note renderer; used to adjust animation when triggered.")]
	public Renderer renderer;

	[Tooltip("Clip from the onCollisionCue to play. (1-indexed)")]
	[Range(1f, 13f)]
	public int clip;

	public void OnTriggerEnter(Collider collider)
	{
		Identifiable.Id id = Identifiable.GetId(collider.gameObject);
		if (PhysicsUtil.IsPlayerMainCollider(collider) || Identifiable.IsSlime(id))
		{
			renderer.material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
			SECTR_AudioSystem.Play(SRSingleton<SceneContext>.Instance.InstrumentDirector.currentInstrument.cue, null, base.transform.position, loop: false, clip - 1, id == Identifiable.Id.PLAYER);
		}
	}
}

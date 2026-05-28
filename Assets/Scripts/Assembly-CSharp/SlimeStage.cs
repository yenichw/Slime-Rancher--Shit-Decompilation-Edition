using System.Collections;
using UnityEngine;

public class SlimeStage : MonoBehaviour
{
	public Rigidbody jointBody;

	public Rigidbody largoJointBody;

	public Attractor attractor;

	public GameObject activationFX;

	public float jointBreakForce = 20f;

	public float jointBreakTorque = float.PositiveInfinity;

	private Joint joint;

	private bool isJointActive;

	private GameObject slime;

	private Animator anim;

	private int animActiveId;

	public void Awake()
	{
		anim = GetComponentInParent<Animator>();
		animActiveId = Animator.StringToHash("active");
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger || !(joint == null))
		{
			return;
		}
		Identifiable.Id id = Identifiable.GetId(col.gameObject);
		if (Identifiable.IsSlime(id))
		{
			if (Identifiable.IsTarr(id))
			{
				SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.SLIME_STAGE_TARRS, 1);
			}
			slime = col.gameObject;
			slime.transform.rotation = Quaternion.Euler(new Vector3(0f, slime.transform.rotation.eulerAngles.y, 0f));
			slime.GetComponent<SlimeSubbehaviourPlexer>().RegisterBehaviorBlocker();
			slime.GetComponent<SlimeFaceAnimator>().SetTrigger("triggerAwe");
			joint = CreateJoint(((Identifiable.IsLargo(id) || Identifiable.IsTarr(id)) ? largoJointBody : jointBody).gameObject);
			SafeJointReference.AttachSafely(slime, joint);
			StartCoroutine(DelayedSetJointBreakForce());
			isJointActive = true;
			jointBody.transform.localRotation = Quaternion.Euler(Vector3.zero);
			largoJointBody.transform.localRotation = Quaternion.Euler(Vector3.zero);
			SRBehaviour.InstantiateDynamic(activationFX, base.transform.position, base.transform.rotation);
			anim.SetBool(animActiveId, value: true);
			attractor.SetAweFactor(1f);
		}
	}

	private IEnumerator DelayedSetJointBreakForce()
	{
		yield return new WaitForSeconds(1f);
		if (joint != null)
		{
			joint.breakForce = jointBreakForce;
			joint.breakTorque = jointBreakTorque;
		}
	}

	public void FixedUpdate()
	{
		if (isJointActive && joint == null)
		{
			if (slime != null)
			{
				slime.GetComponent<SlimeSubbehaviourPlexer>().UnregisterBehaviorBlocker();
				slime = null;
			}
			anim.SetBool(animActiveId, value: false);
			attractor.SetAweFactor(0f);
			isJointActive = false;
		}
		if (joint != null)
		{
			jointBody.transform.Rotate(Vector3.up, 90f * Time.fixedDeltaTime);
			largoJointBody.transform.Rotate(Vector3.up, 90f * Time.fixedDeltaTime);
			if (joint.connectedBody == null)
			{
				Destroyer.Destroy(joint, "SlimeStage.FixedUpdate");
			}
		}
	}

	private static Joint CreateJoint(GameObject parent)
	{
		ConfigurableJoint configurableJoint = parent.AddComponent<ConfigurableJoint>();
		configurableJoint.anchor = Vector3.zero;
		configurableJoint.autoConfigureConnectedAnchor = false;
		configurableJoint.connectedAnchor = Vector3.zero;
		SoftJointLimitSpring softJointLimitSpring = new SoftJointLimitSpring
		{
			damper = 0.2f,
			spring = 1000f
		};
		configurableJoint.xMotion = ConfigurableJointMotion.Limited;
		configurableJoint.yMotion = ConfigurableJointMotion.Limited;
		configurableJoint.zMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
		configurableJoint.linearLimitSpring = softJointLimitSpring;
		configurableJoint.angularXLimitSpring = softJointLimitSpring;
		configurableJoint.angularYZLimitSpring = softJointLimitSpring;
		return configurableJoint;
	}
}

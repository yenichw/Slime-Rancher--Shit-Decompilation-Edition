using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class FashionPod : SRBehaviour
{
	public Transform fashionItemPos;

	public Identifiable.Id fashionId;

	public GameObject spawnFX;

	private GameObject fashionPrefab;

	private Joint fashionJoint;

	private Region region;

	private const float CLEAR_RAD = 0.4f;

	public void Awake()
	{
		fashionPrefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(fashionId);
		region = GetComponentInParent<Region>();
	}

	public void Update()
	{
		if (fashionJoint != null && fashionJoint.connectedBody == null)
		{
			Destroyer.Destroy(fashionJoint, "FashionPod.Update");
			fashionJoint = null;
		}
		if (fashionJoint == null && !Physics.CheckSphere(fashionItemPos.position, 0.4f))
		{
			GameObject toAttach = SRBehaviour.InstantiateActor(fashionPrefab, region.setId, fashionItemPos.position, fashionItemPos.rotation);
			ConfigurableJoint configurableJoint = fashionItemPos.gameObject.AddComponent<ConfigurableJoint>();
			SafeJointReference.AttachSafely(toAttach, configurableJoint);
			configurableJoint.anchor = Vector3.zero;
			configurableJoint.autoConfigureConnectedAnchor = false;
			configurableJoint.connectedAnchor = Vector3.zero;
			SoftJointLimitSpring softJointLimitSpring = default(SoftJointLimitSpring);
			softJointLimitSpring.damper = 0.2f;
			softJointLimitSpring.spring = 1000f;
			configurableJoint.xMotion = ConfigurableJointMotion.Limited;
			configurableJoint.yMotion = ConfigurableJointMotion.Limited;
			configurableJoint.zMotion = ConfigurableJointMotion.Limited;
			configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
			configurableJoint.linearLimitSpring = softJointLimitSpring;
			configurableJoint.angularXLimitSpring = softJointLimitSpring;
			configurableJoint.angularYZLimitSpring = softJointLimitSpring;
			configurableJoint.breakForce = 20f;
			fashionJoint = configurableJoint;
			fashionItemPos.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if (spawnFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(spawnFX, fashionItemPos.position, fashionItemPos.rotation);
			}
		}
	}

	public void FixedUpdate()
	{
		if (fashionJoint != null)
		{
			fashionItemPos.transform.Rotate(Vector3.up, 90f * Time.fixedDeltaTime);
		}
	}
}

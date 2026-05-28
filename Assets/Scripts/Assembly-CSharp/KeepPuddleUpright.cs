using UnityEngine;

public class KeepPuddleUpright : KeepUpright, RegistryFixedUpdateable, RegistryLateUpdateable
{
	private EnableBasedOnGrounded[] toggleOnGrounded;

	private SlimeSubbehaviourPlexer plexer;

	private Transform slimeRoot;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	public override void Start()
	{
		base.Start();
		plexer = GetComponent<SlimeSubbehaviourPlexer>();
		toggleOnGrounded = GetComponentsInChildren<EnableBasedOnGrounded>(includeInactive: true);
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += delegate
		{
			toggleOnGrounded = GetComponentsInChildren<EnableBasedOnGrounded>(includeInactive: true);
		};
		slimeRoot = base.transform.Find("prefab_slimeBase/bone_root/bone_slime");
	}

	public void RegistryLateUpdate()
	{
		if ((object)slimeRoot != null)
		{
			slimeRoot.localRotation = Quaternion.identity;
		}
	}

	public override void RegistryFixedUpdate()
	{
		if (plexer == null)
		{
			return;
		}
		bool flag = plexer.IsGrounded();
		if (flag)
		{
			RaycastHit raycastHit = plexer.GroundHit();
			if (raycastHit.rigidbody == null)
			{
				DoUpright(raycastHit.normal);
			}
			else
			{
				DoUpright(Vector3.up);
			}
		}
		else
		{
			DoUpright(Vector3.up);
		}
		EnableBasedOnGrounded[] array = toggleOnGrounded;
		foreach (EnableBasedOnGrounded enableBasedOnGrounded in array)
		{
			if (enableBasedOnGrounded != null)
			{
				enableBasedOnGrounded.gameObject.SetActive(enableBasedOnGrounded.enableOnGrounded ^ flag);
			}
		}
	}
}

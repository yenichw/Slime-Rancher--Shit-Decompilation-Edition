using UnityEngine;

public class SwitchableSlidingDoor : SwitchHandler.Switchable
{
	public Transform upTrans;

	public Transform downTrans;

	private SECTR_PointSource slidingSound;

	private Vector3 upPos;

	private Vector3 downPos;

	private float tgtSlideDownAmt;

	private float slideDownAmt;

	private bool forceMove;

	private float MOVE_RATE = 0.5f;

	private SwitchHandler.State currentState;

	public void Awake()
	{
		upPos = upTrans.position;
		downPos = downTrans.position;
		slidingSound = base.gameObject.GetComponent<SECTR_PointSource>();
	}

	public override void SetState(SwitchHandler.State state, bool immediate = false)
	{
		if (state != currentState)
		{
			slidingSound.Play();
			currentState = state;
		}
		tgtSlideDownAmt = ((state != 0) ? 1 : 0);
		if (immediate)
		{
			slideDownAmt = tgtSlideDownAmt;
			forceMove = true;
		}
	}

	public void FixedUpdate()
	{
		if (forceMove || slideDownAmt != tgtSlideDownAmt)
		{
			if (slideDownAmt < tgtSlideDownAmt)
			{
				slideDownAmt = Mathf.Min(tgtSlideDownAmt, slideDownAmt + MOVE_RATE * Time.fixedDeltaTime);
			}
			else if (slideDownAmt > tgtSlideDownAmt)
			{
				slideDownAmt = Mathf.Max(tgtSlideDownAmt, slideDownAmt - MOVE_RATE * Time.fixedDeltaTime);
			}
			base.transform.position = Vector3.Lerp(upPos, downPos, slideDownAmt);
			forceMove = false;
		}
	}
}

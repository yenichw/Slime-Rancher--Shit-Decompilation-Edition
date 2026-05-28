using System;
using System.Collections.Generic;
using UnityEngine;

public class SECTRDirector : MonoBehaviour
{
	private const int MIN_LOCAL_ARRAY_RESIZE_AMOUNT = 50;

	private List<SECTR_Member> _allMembers = new List<SECTR_Member>();

	private List<SECTR_Member> _offsetUpdates = new List<SECTR_Member>();

	private List<SECTR_Member> _nonOffsetUpdates = new List<SECTR_Member>();

	private ExposedArrayList<SECTR_Hibernator> _hibernatorsToUpdate = new ExposedArrayList<SECTR_Hibernator>(1000);

	private List<SECTR_Member> _toRegister = new List<SECTR_Member>();

	private SECTR_Hibernator[] Update_localHibernators = new SECTR_Hibernator[50];

	public void RegisterMember(SECTR_Member member)
	{
		RemoveMember(member);
		_allMembers.Add(member);
		if (member.BoundsUpdateMode != SECTR_Member.BoundsUpdateModes.Static)
		{
			if (member.BoundsUpdateMode == SECTR_Member.BoundsUpdateModes.Offset)
			{
				_offsetUpdates.Add(member);
			}
			else
			{
				_nonOffsetUpdates.Add(member);
			}
		}
	}

	public void DeregisterMember(SECTR_Member member)
	{
		_allMembers.Remove(member);
		if (member.BoundsUpdateMode != SECTR_Member.BoundsUpdateModes.Static)
		{
			if (member.BoundsUpdateMode == SECTR_Member.BoundsUpdateModes.Offset)
			{
				_offsetUpdates.Remove(member);
			}
			else
			{
				_nonOffsetUpdates.Remove(member);
			}
		}
	}

	private void RemoveMember(SECTR_Member member)
	{
		_allMembers.Remove(member);
		_nonOffsetUpdates.Remove(member);
		_offsetUpdates.Remove(member);
	}

	public void RegisterHibernator(SECTR_Hibernator hibernator)
	{
		_hibernatorsToUpdate.Add(hibernator);
	}

	public void DeregisterHibernator(SECTR_Hibernator hibernator)
	{
		_hibernatorsToUpdate.Remove(hibernator);
	}

	public void ClearRegistrations()
	{
		_allMembers.Clear();
		_nonOffsetUpdates.Clear();
		_offsetUpdates.Clear();
		_hibernatorsToUpdate.Clear();
	}

	private void Start()
	{
		Log.Debug("Starting SECTRDirector");
	}

	private void Update()
	{
		if (_hibernatorsToUpdate.Data.Length > Update_localHibernators.Length)
		{
			Array.Resize(ref Update_localHibernators, Math.Max(_hibernatorsToUpdate.Data.Length, 50));
		}
		int count = _hibernatorsToUpdate.GetCount();
		_hibernatorsToUpdate.Data.CopyTo(Update_localHibernators, 0);
		for (int i = 0; i < count; i++)
		{
			try
			{
				Update_localHibernators[i].OnUpdate();
			}
			catch (NullReferenceException)
			{
				Log.Debug("Null reference caught in SECTRDirector update.", "position", i);
			}
		}
	}

	private void LateUpdate()
	{
		SECTR_Member sECTR_Member = null;
		for (int i = 0; i < _offsetUpdates.Count; i++)
		{
			sECTR_Member = _offsetUpdates[i];
			if (sECTR_Member.enabled && !sECTR_Member.IsHibernating && (sECTR_Member.BoundsUpdateMode == SECTR_Member.BoundsUpdateModes.Always || sECTR_Member.memberTransform.hasChanged))
			{
				if (sECTR_Member.BoundsUpdateMode != SECTR_Member.BoundsUpdateModes.Static)
				{
					sECTR_Member.OffsetLateUpdate();
				}
				else
				{
					_toRegister.Add(sECTR_Member);
				}
			}
		}
		for (int j = 0; j < _nonOffsetUpdates.Count; j++)
		{
			sECTR_Member = _nonOffsetUpdates[j];
			if (sECTR_Member.enabled && !sECTR_Member.IsHibernating && (sECTR_Member.BoundsUpdateMode == SECTR_Member.BoundsUpdateModes.Always || sECTR_Member.memberTransform.hasChanged))
			{
				if (sECTR_Member.BoundsUpdateMode != SECTR_Member.BoundsUpdateModes.Static)
				{
					sECTR_Member.NonOffsetLateUpdate();
				}
				else
				{
					_toRegister.Add(sECTR_Member);
				}
			}
		}
		for (int k = 0; k < _toRegister.Count; k++)
		{
			if (_toRegister[k] != null)
			{
				RegisterMember(_toRegister[k]);
			}
		}
		_toRegister.Clear();
	}
}

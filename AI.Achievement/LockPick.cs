using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Achievement;

public class LockPick : PerformAction
{
	private OCL m_ocl;

	private Vector3 m_startPos = Vector3.zero;

	private List<string> m_hiddenSlots = new List<string>();

	public override int Priority => 5;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => false;

	public override GameObject CurrentTarget => m_ocl.gameObject;

	public OCL OCL
	{
		get
		{
			return m_ocl;
		}
		set
		{
			m_ocl = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_ocl = null;
		m_startPos = Vector3.zero;
		m_hiddenSlots.Clear();
	}

	public override void OnEnter()
	{
		m_action = AnimationController.ActionType.Use;
		m_variation = 1;
		if (m_startPos.sqrMagnitude > float.Epsilon)
		{
			if (GameUtilities.V3SqrDistance2D(m_startPos, m_owner.transform.position) > 2.25f)
			{
				m_ai.StateManager.PopCurrentState();
				return;
			}
		}
		else
		{
			m_startPos = m_owner.transform.position;
		}
		m_animation.OnEventShowSlot += anim_OnShowSlot;
		m_animation.OnEventHideSlot += anim_OnHideSlot;
		base.OnEnter();
		if ((bool)m_animation)
		{
			m_animation.ClearInterrupt();
		}
	}

	public override void OnExit()
	{
		m_animation.OnEventShowSlot -= anim_OnShowSlot;
		m_animation.OnEventHideSlot -= anim_OnHideSlot;
		if (m_hiddenSlots.Count > 0)
		{
			Equipment component = m_ai.gameObject.GetComponent<Equipment>();
			if (component != null)
			{
				EventArgs args = new EventArgs();
				foreach (string hiddenSlot in m_hiddenSlots)
				{
					component.HandleAnimShowSlot(hiddenSlot, args);
				}
			}
			m_hiddenSlots.Clear();
		}
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
		m_ai.FaceTarget(null);
	}

	protected override void OnComplete()
	{
		if (m_ocl != null)
		{
			m_ocl.TriggerLockPick(m_ai.gameObject);
		}
	}

	public void anim_OnHideSlot(object obj, EventArgs args)
	{
		if (obj != null)
		{
			string item = obj.ToString();
			if (!m_hiddenSlots.Contains(item))
			{
				m_hiddenSlots.Add(item);
			}
		}
	}

	public void anim_OnShowSlot(object obj, EventArgs args)
	{
		if (obj != null)
		{
			string item = obj.ToString();
			if (m_hiddenSlots.Contains(item))
			{
				m_hiddenSlots.Remove(item);
			}
		}
	}

	public override bool AllowEngagementUpdate()
	{
		return false;
	}
}

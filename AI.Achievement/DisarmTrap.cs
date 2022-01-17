using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Achievement;

public class DisarmTrap : PerformAction
{
	private Trap m_trap;

	private bool m_fromOCL;

	private List<string> m_hiddenSlots = new List<string>();

	public override int Priority => 5;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => false;

	public override GameObject CurrentTarget
	{
		get
		{
			if (m_trap != null)
			{
				return m_trap.gameObject;
			}
			return null;
		}
	}

	public Trap Trap
	{
		get
		{
			return m_trap;
		}
		set
		{
			m_trap = value;
		}
	}

	public bool FromOCL
	{
		get
		{
			return m_fromOCL;
		}
		set
		{
			m_fromOCL = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_trap = null;
		m_fromOCL = false;
		m_hiddenSlots.Clear();
	}

	public override void OnEnter()
	{
		m_action = AnimationController.ActionType.Use;
		m_variation = 2;
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
		if (m_trap != null && m_trap.TriggerDisarm(m_owner, m_fromOCL))
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(m_ai.gameObject, SoundSet.SoundAction.TaskComplete, SoundSet.s_VeryShortVODelay, forceInterrupt: false);
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

using System;
using System.Collections.Generic;

namespace AI.Achievement;

public class ConsumePotion : PerformAction
{
	private GenericAbility m_ability;

	private bool m_abilityTriggered;

	private int m_consumeAnimation = 100;

	private List<string> m_hiddenSlots = new List<string>();

	public GenericAbility Ability
	{
		get
		{
			return m_ability;
		}
		set
		{
			m_ability = value;
		}
	}

	public int ConsumeAnimation
	{
		get
		{
			return m_consumeAnimation;
		}
		set
		{
			m_consumeAnimation = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_ability = null;
		m_abilityTriggered = false;
		m_consumeAnimation = 100;
		m_hiddenSlots.Clear();
	}

	public override void OnEnter()
	{
		if (m_abilityTriggered)
		{
			m_ai.StateManager.PopCurrentState();
			return;
		}
		m_animation.OnEventHit += anim_OnEventHit;
		m_animation.OnEventShowSlot += anim_OnShowSlot;
		m_animation.OnEventHideSlot += anim_OnHideSlot;
		m_action = AnimationController.ActionType.Attack;
		m_variation = ConsumeAnimation;
		base.OnEnter();
		if ((bool)m_animation)
		{
			m_animation.ClearInterrupt();
		}
	}

	public override void OnExit()
	{
		m_animation.OnEventHit -= anim_OnEventHit;
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

	private void anim_OnEventHit(object sender, EventArgs e)
	{
		if (!m_abilityTriggered)
		{
			m_abilityTriggered = true;
			if ((bool)m_ability)
			{
				m_ability.Activate(base.Owner);
			}
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
}

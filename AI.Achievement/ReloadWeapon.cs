using UnityEngine;

namespace AI.Achievement;

public class ReloadWeapon : GameAIState
{
	private AnimationController m_ownerAnim;

	private AnimationController m_wpnAnim;

	private AttackFirearm m_firearm;

	private bool m_interrupted;

	private bool m_waitForIdle;

	private float m_reloadTime;

	private float m_speedMultiplier = 1f;

	private float m_animTime;

	private bool m_reloadAnimStarted;

	private const float m_minAnimDuration = 0.2f;

	public override bool InCombat
	{
		get
		{
			GameObject currentTarget = m_ai.StateManager.CurrentTarget;
			if (currentTarget != null && m_ai.IsTargetable(currentTarget))
			{
				if (GameState.InCombat)
				{
					return !GameState.IsInTrapTriggeredCombat;
				}
				return false;
			}
			return false;
		}
	}

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_firearm = null;
		m_ownerAnim = null;
		m_wpnAnim = null;
		m_interrupted = false;
		m_waitForIdle = false;
		m_reloadTime = 0f;
		m_speedMultiplier = 1f;
		m_animTime = 0f;
		m_reloadAnimStarted = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_firearm == null)
		{
			Debug.LogError("ReloadWeapon state was activated without a weapon to reload!", m_owner);
			base.Manager.PopCurrentState();
			return;
		}
		CharacterStats component = m_owner.GetComponent<CharacterStats>();
		if (component != null)
		{
			float num = 1f;
			if (m_firearm != null)
			{
				num = m_firearm.CalculateAttackSpeed();
			}
			m_speedMultiplier = component.ReloadSpeedMultiplier * num;
		}
		m_ownerAnim = m_owner.GetComponent<AnimationController>();
		m_wpnAnim = m_firearm.GetComponent<AnimationController>();
		m_reloadTime = m_firearm.ReloadTime - m_firearm.RemainingReloadTime;
		m_animTime = 0f;
		if (m_ownerAnim.Idle)
		{
			InitAnimation();
			return;
		}
		m_ownerAnim.ReloadTime = 0f;
		if (m_wpnAnim != null)
		{
			m_wpnAnim.ReloadTime = 0f;
		}
		m_reloadAnimStarted = false;
	}

	private void InitAnimation()
	{
		m_ownerAnim.DesiredAction.m_actionType = AnimationController.ActionType.Reload;
		m_ownerAnim.DesiredAction.m_variation = (int)m_firearm.ReloadAnim;
		m_ownerAnim.DesiredAction.m_speed = m_speedMultiplier;
		m_ownerAnim.ReloadTime = m_reloadTime * m_speedMultiplier;
		m_ownerAnim.Loop = true;
		if (m_wpnAnim != null)
		{
			m_wpnAnim.DesiredAction.m_actionType = AnimationController.ActionType.Reload;
			m_wpnAnim.DesiredAction.m_variation = (int)m_firearm.ReloadAnim;
			m_wpnAnim.DesiredAction.m_speed = m_speedMultiplier;
			m_wpnAnim.ReloadTime = m_reloadTime * m_speedMultiplier;
			m_wpnAnim.Loop = true;
		}
		else
		{
			Debug.LogError(m_firearm.name + " does not have an animation controller!", m_firearm);
		}
		m_reloadAnimStarted = true;
		if (!GameState.InCombat && m_firearm != null)
		{
			m_firearm.Reload();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_ownerAnim != null)
		{
			m_ownerAnim.ReloadTime = 0f;
		}
		if (m_wpnAnim != null)
		{
			m_wpnAnim.ReloadTime = 0f;
		}
		if ((bool)m_firearm)
		{
			m_firearm.RemainingReloadTime = Mathf.Max(0f, m_firearm.ReloadTime - m_reloadTime);
		}
	}

	public override void OnAbort()
	{
		base.OnAbort();
		InterruptReload();
		if ((bool)m_firearm)
		{
			m_firearm.RemainingReloadTime = Mathf.Max(0f, m_firearm.ReloadTime - m_reloadTime);
		}
	}

	public override void Update()
	{
		if (m_interrupted)
		{
			if (m_animation.Idle)
			{
				base.Manager.PopCurrentState();
			}
			return;
		}
		m_reloadTime += Time.deltaTime * m_speedMultiplier;
		if (!m_waitForIdle && !m_reloadAnimStarted)
		{
			if (m_ownerAnim.Idle)
			{
				InitAnimation();
			}
			else if (m_reloadTime < m_firearm.ReloadTime - 0.5f)
			{
				return;
			}
		}
		if (m_reloadTime >= m_firearm.ReloadTime - 0.5f)
		{
			m_ownerAnim.Loop = false;
			if (m_wpnAnim != null)
			{
				m_wpnAnim.Loop = false;
			}
		}
		if (m_waitForIdle && m_ownerAnim.Idle)
		{
			m_reloadTime = 0f;
			m_firearm.Reload();
			base.Manager.PopCurrentState();
		}
		else if (m_ownerAnim.CurrentAction.m_actionType != 0 && m_ownerAnim.CurrentAction.m_actionType != AnimationController.ActionType.Pending && !m_ownerAnim.IsPerformingAction(AnimationController.ActionType.Reload, (int)m_firearm.ReloadAnim) && m_animTime > 0.2f)
		{
			m_ownerAnim.DesiredAction.Reset();
			if (m_wpnAnim != null)
			{
				m_wpnAnim.DesiredAction.Reset();
			}
			m_waitForIdle = true;
		}
		else if (!m_waitForIdle && m_ownerAnim.DesiredAction.m_actionType != AnimationController.ActionType.Reload)
		{
			InitAnimation();
		}
		m_ai.FaceTarget(m_firearm);
		m_animTime += Time.deltaTime;
	}

	public void Setup(AttackFirearm attack)
	{
		m_firearm = attack;
	}

	public void InterruptReload()
	{
		m_interrupted = true;
		if (m_ownerAnim != null)
		{
			m_ownerAnim.ClearActions();
			m_ownerAnim.Loop = false;
			m_ownerAnim.DesiredAction.Reset();
			m_ownerAnim.Interrupt();
			m_ownerAnim.ReloadTime = 0f;
		}
		if (m_wpnAnim != null)
		{
			m_wpnAnim.ClearActions();
			m_wpnAnim.Loop = false;
			m_wpnAnim.DesiredAction.Reset();
			m_wpnAnim.Interrupt();
			m_wpnAnim.ReloadTime = 0f;
		}
	}

	public override void OnCancel()
	{
		base.OnCancel();
		InterruptReload();
	}
}

using AI.Achievement;
using UnityEngine;

namespace AI.Player;

public class ReloadWeapon : PlayerState
{
	private bool m_inCombat;

	private AttackFirearm m_attackToUse;

	public AttackFirearm AttackToUse
	{
		get
		{
			return m_attackToUse;
		}
		set
		{
			m_attackToUse = value;
		}
	}

	public override bool InCombat
	{
		get
		{
			GameObject currentTarget = CurrentTarget;
			if (CurrentTarget != null && m_ai.IsTargetable(currentTarget))
			{
				return m_inCombat;
			}
			return false;
		}
	}

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_inCombat = false;
		m_attackToUse = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		AttackFirearm attackFirearm = m_attackToUse;
		m_inCombat = GameState.InCombat && !GameState.IsInTrapTriggeredCombat;
		if (attackFirearm == null)
		{
			AttackFirearm attackFirearm2 = m_partyMemberAI.GetPrimaryAttack() as AttackFirearm;
			AttackFirearm attackFirearm3 = m_partyMemberAI.GetSecondaryAttack() as AttackFirearm;
			if (attackFirearm2 != null && attackFirearm3 != null)
			{
				attackFirearm = ((attackFirearm2.RemainingShots > attackFirearm3.RemainingShots) ? attackFirearm2 : attackFirearm3);
			}
			else if ((bool)attackFirearm2)
			{
				attackFirearm = attackFirearm2;
			}
			else
			{
				if (!attackFirearm3)
				{
					base.Manager.PopCurrentState();
					return;
				}
				attackFirearm = attackFirearm3;
			}
		}
		base.Manager.PopCurrentState();
		if (attackFirearm.RequiresReload)
		{
			PushState<AI.Achievement.ReloadWeapon>().Setup(attackFirearm);
		}
	}

	public override void OnExit()
	{
		m_inCombat = false;
		base.OnExit();
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		return false;
	}
}

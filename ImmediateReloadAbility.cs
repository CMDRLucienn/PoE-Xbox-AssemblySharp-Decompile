using UnityEngine;

public class ImmediateReloadAbility : GenericAbility
{
	private AttackFirearm m_readyAttack;

	public override bool Ready
	{
		get
		{
			Equipment component = Owner.GetComponent<Equipment>();
			if (!component)
			{
				return false;
			}
			AttackFirearm attackFirearm = component.PrimaryAttack as AttackFirearm;
			if (attackFirearm == null)
			{
				attackFirearm = component.SecondaryAttack as AttackFirearm;
			}
			if (attackFirearm == null)
			{
				return false;
			}
			if (attackFirearm.RequiresReload)
			{
				m_readyAttack = attackFirearm;
			}
			if (!attackFirearm.RequiresReload)
			{
				return false;
			}
			return base.Ready;
		}
	}

	protected override float AttackRange
	{
		get
		{
			if (m_readyAttack == null)
			{
				return 0f;
			}
			return m_readyAttack.TotalAttackDistance;
		}
	}

	public override void Activate(GameObject target)
	{
		base.Activate(target);
		if (m_readyAttack.RequiresReload)
		{
			m_readyAttack.RemainingShots = 1;
		}
		if (!(m_readyAttack != null))
		{
			return;
		}
		GameEventArgs gameEventArgs = new GameEventArgs();
		gameEventArgs.Type = GameEventType.Ability;
		gameEventArgs.GameObjectData = new GameObject[2];
		gameEventArgs.GenericData = new object[2];
		gameEventArgs.GenericData[0] = "activate";
		gameEventArgs.GenericData[1] = "restricted";
		gameEventArgs.GameObjectData[0] = m_readyAttack.gameObject;
		gameEventArgs.GameObjectData[1] = Owner;
		AIController component = Owner.GetComponent<AIController>();
		if ((bool)component)
		{
			PartyMemberAI partyMemberAI = component as PartyMemberAI;
			if (partyMemberAI != null)
			{
				partyMemberAI.TriggerAbility(gameEventArgs, null);
			}
			else
			{
				component.OnEvent(gameEventArgs);
			}
		}
	}
}

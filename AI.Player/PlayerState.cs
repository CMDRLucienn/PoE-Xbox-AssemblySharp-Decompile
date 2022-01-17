using UnityEngine;

namespace AI.Player;

public class PlayerState : GameAIState
{
	protected PartyMemberAI m_partyMemberAI;

	protected int m_selectionSlot;

	public override void Reset()
	{
		base.Reset();
		m_partyMemberAI = null;
		m_selectionSlot = 0;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_partyMemberAI = m_owner.GetComponent<PartyMemberAI>();
		if (m_partyMemberAI == null)
		{
			Debug.LogError("AI is using a player state without a PartyMemberAI component!", m_owner);
		}
	}

	public override void Update()
	{
		if (!GameState.Paused && !(m_partyMemberAI == null))
		{
			base.Update();
		}
	}

	protected AttackBase FindAttackBase(GameObject obj)
	{
		AttackBase attackBase = obj.GetComponent<AttackBase>();
		if (attackBase == null)
		{
			attackBase = m_partyMemberAI.GetPrimaryAttack();
		}
		return attackBase;
	}

	protected bool OutOfCharges(AttackBase attack)
	{
		if (attack == null || attack.AbilityOrigin == null)
		{
			return false;
		}
		CharacterStats component = m_owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return false;
		}
		GenericAbility abilityOrigin = attack.AbilityOrigin;
		GenericSpell genericSpell = abilityOrigin as GenericSpell;
		if ((bool)genericSpell && genericSpell.MasteryLevel == 0 && genericSpell.UsesLeft() <= 0)
		{
			return true;
		}
		if (!abilityOrigin.Ready && (abilityOrigin.WhyNotReady & GenericAbility.NotReadyValue.FailedPrerequisite) != 0 && abilityOrigin.WhyNotReadyPrereq == PrerequisiteType.CasterPhraseCount)
		{
			return true;
		}
		if (abilityOrigin.UsesLeft() <= 0)
		{
			return true;
		}
		GenericCipherAbility genericCipherAbility = abilityOrigin as GenericCipherAbility;
		if (genericCipherAbility != null && genericCipherAbility.FocusCost > component.Focus)
		{
			return true;
		}
		return false;
	}
}

using UnityEngine;

namespace AI;

public class AIPosessableController : AIPackageController
{
	public SpellList PosessedInstructionSet;

	private SpellList m_originalSpellList;

	private AttackBase m_attackToUse;

	protected GameObject m_target;

	public GameObject Possesor { get; set; }

	public void SetPossessed(GameObject posessor, bool isPosessed)
	{
		if (m_stats == null)
		{
			m_stats = GetComponent<CharacterStats>();
		}
		if (isPosessed)
		{
			Possesor = posessor;
			m_originalSpellList = InstructionSet;
			InstructionSet = PosessedInstructionSet;
			m_instructions.Clear();
			m_stats.ActiveAbilities.Clear();
			InitSpellCaster();
		}
		else
		{
			InstructionSet = m_originalSpellList;
			m_instructions.Clear();
			m_stats.ActiveAbilities.Clear();
			InitSpellCaster();
		}
	}

	public override SpellCastData GetNextInstruction()
	{
		SpellCastData nextInstruction = base.GetNextInstruction();
		if (nextInstruction == null)
		{
			return null;
		}
		while (nextInstruction != null && nextInstruction.CastInstruction == SpellCastData.Instruction.Flee && GetTargetScanner() is CasterTargetScanner casterTargetScanner && !casterTargetScanner.ConditionalIsValid(m_ai.CurrentState.Owner, nextInstruction, base.gameObject))
		{
			if (AIController.s_possibleInstructions.Contains(nextInstruction))
			{
				AIController.s_possibleInstructions.Remove(nextInstruction);
			}
			nextInstruction = base.GetNextInstruction();
		}
		if (nextInstruction == null)
		{
			return nextInstruction;
		}
		if (nextInstruction.CastInstruction == SpellCastData.Instruction.Flee)
		{
			SpellCastData spellCastData = new SpellCastData();
			spellCastData.CastInstruction = SpellCastData.Instruction.Cast;
			m_target = Possesor;
			spellCastData.Target = SpellCastData.CastTarget.CurrentEnemy;
			spellCastData.Spell = nextInstruction.Spell;
			m_attackToUse = spellCastData.Spell.GetComponent<AttackRanged>();
			m_attackToUse.OnHit += CurrentAttack_OnHit;
			m_stats.RecoveryTimer = 0f;
			SetPossessed(null, isPosessed: false);
			return spellCastData;
		}
		return nextInstruction;
	}

	private void CurrentAttack_OnHit(GameObject source, CombatEventArgs args)
	{
		if (Possesor != null)
		{
			ThaosController component = Possesor.GetComponent<ThaosController>();
			if (component != null && component.gameObject.activeInHierarchy)
			{
				component.RegainControl();
			}
			Possesor = null;
		}
	}

	public override bool BeingKited()
	{
		return false;
	}
}

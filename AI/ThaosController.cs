using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;

namespace AI;

public class ThaosController : AIPackageController
{
	public List<AIPosessableController> Vessels = new List<AIPosessableController>();

	public GameObject ImmuneToDamageVFX;

	public GameObject SoulJumpCutscene;

	private AIPosessableController m_currentVessel;

	private bool m_launchingSoulJump;

	private StatusEffect m_invulnerableStatusEffect;

	private bool m_playedSoulJumpLine;

	private GameObject m_activeImmuneVFX;

	private AttackBase m_attackToUse;

	private List<SpellCastData> m_usedSoulJumpInstructions = new List<SpellCastData>();

	private float m_vesselStartStamina;

	private float m_vesselStaminaToLose;

	private const float RETURN_STAMINA_PERCENTAGE = 0.3f;

	public override void Start()
	{
		base.Start();
		m_stats = GetComponent<CharacterStats>();
		m_usedSoulJumpInstructions.Clear();
	}

	public void RegainControl()
	{
		if (m_activeImmuneVFX != null)
		{
			GameUtilities.ShutDownLoopingEffect(m_activeImmuneVFX);
			GameUtilities.Destroy(m_activeImmuneVFX, 2f);
			m_activeImmuneVFX = null;
		}
		if (m_currentVessel != null)
		{
			m_currentVessel = null;
		}
		Health component = GetComponent<Health>();
		if (component != null)
		{
			component.TakesDamage = true;
			component.PlaysHitReactions = true;
			component.Targetable = true;
			component.AddHealth(component.MaxHealth - component.CurrentHealth);
			component.AddStamina(component.MaxStamina - component.CurrentStamina);
		}
		m_launchingSoulJump = false;
		if (m_invulnerableStatusEffect != null)
		{
			m_stats.ClearStatusEffects(StatusEffect.ModifiedStat.StasisShield);
			m_invulnerableStatusEffect = null;
		}
		if (m_ai.CurrentState is Attack attack)
		{
			attack.Parameters.ForceLoop = false;
			attack.ForceAnimToEnd();
		}
		m_ai.PopAllStates();
	}

	public override SpellCastData GetNextInstruction()
	{
		if (m_launchingSoulJump)
		{
			Attack obj = m_ai.CurrentState as Attack;
			Wait wait = m_ai.CurrentState as Wait;
			if (obj == null && wait == null)
			{
				wait = AIStateManager.StatePool.Allocate<Wait>();
				m_ai.PushState(wait, clearStack: true);
			}
			AIController.s_defaultInstruction = null;
			return null;
		}
		foreach (SpellCastData usedSoulJumpInstruction in m_usedSoulJumpInstructions)
		{
			if (AIController.s_possibleInstructions.Contains(usedSoulJumpInstruction))
			{
				AIController.s_possibleInstructions.Remove(usedSoulJumpInstruction);
			}
		}
		SpellCastData nextInstruction = base.GetNextInstruction();
		if (nextInstruction == null)
		{
			return nextInstruction;
		}
		for (int num = Vessels.Count - 1; num >= 0; num--)
		{
			if (Vessels[num] == null)
			{
				Vessels.RemoveAt(num);
			}
			else
			{
				Health component = Vessels[num].GetComponent<Health>();
				if (component == null || component.Dead)
				{
					Vessels.RemoveAt(num);
				}
			}
		}
		while (nextInstruction != null && nextInstruction.CastInstruction == SpellCastData.Instruction.Flee && GetTargetScanner() is CasterTargetScanner casterTargetScanner && (!casterTargetScanner.ConditionalIsValid(m_ai.CurrentState.Owner, nextInstruction, base.gameObject) || Vessels.Count == 0))
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
			m_usedSoulJumpInstructions.Add(nextInstruction);
			if (Vessels.Count == 0)
			{
				Debug.LogError("Instruction set logic error. There are no more Vessels for Thaos to inhabit!");
				InstructionProcessed(nextInstruction);
				return null;
			}
			if (!m_launchingSoulJump)
			{
				int index = 0;
				if (m_usedSoulJumpInstructions.Count > 1 && Vessels.Count > 1)
				{
					index = 1;
				}
				m_currentVessel = Vessels[index];
				Vessels.RemoveAt(index);
				if (m_currentVessel == null)
				{
					return null;
				}
				m_vesselStartStamina = 0f;
				m_vesselStaminaToLose = 0f;
				Health component2 = m_currentVessel.GetComponent<Health>();
				if (component2 != null)
				{
					m_vesselStartStamina = component2.CurrentStamina;
					m_vesselStaminaToLose = component2.MaxStamina * 0.3f;
					component2.OnDeath += ThaosController_OnDeath;
				}
				Health component3 = GetComponent<Health>();
				component3.Targetable = false;
				m_currentVessel.SetPossessed(base.gameObject, isPosessed: true);
				m_launchingSoulJump = true;
				SpellCastData spellCastData = new SpellCastData();
				spellCastData.CastInstruction = SpellCastData.Instruction.Cast;
				spellCastData.Target = SpellCastData.CastTarget.NearestAlly;
				spellCastData.Spell = nextInstruction.Spell;
				spellCastData.HandledInternally = true;
				m_attackToUse = spellCastData.Spell.GetComponent<AttackRanged>();
				m_stats.RecoveryTimer = 0f;
				m_ai.PopAllStates();
				Attack attack = AIStateManager.StatePool.Allocate<Attack>();
				m_ai.PushState(attack);
				attack.Parameters.Attack = m_attackToUse;
				attack.Parameters.TargetObject = m_currentVessel.gameObject;
				attack.Parameters.ForceLoop = true;
				attack.Parameters.CheckAnimErrors = false;
				attack.Parameters.Invulnerable = true;
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.StasisShield;
				statusEffectParams.Duration = 0f;
				statusEffectParams.Value = float.MaxValue;
				statusEffectParams.IsHostile = false;
				m_invulnerableStatusEffect = StatusEffect.Create(m_ai.Owner, m_attackToUse.AbilityOrigin, statusEffectParams, GenericAbility.AbilityType.Ability, null, deleteOnClear: false);
				m_stats.ApplyStatusEffectImmediate(m_invulnerableStatusEffect);
				Cutscene component4 = SoulJumpCutscene.GetComponent<Cutscene>();
				if ((bool)component4)
				{
					component4.StartCutscene();
				}
				if (component3 != null)
				{
					component3.TakesDamage = false;
					component3.PlaysHitReactions = false;
				}
				m_activeImmuneVFX = GameUtilities.LaunchLoopingEffect(ImmuneToDamageVFX, 1f, base.transform, null);
				CancelAllEngagements();
				SoundSetComponent component5 = base.gameObject.GetComponent<SoundSetComponent>();
				if (component5 != null)
				{
					if (!m_playedSoulJumpLine)
					{
						component5.PlaySound(SoundSet.SoundAction.Hello);
						m_playedSoulJumpLine = true;
					}
					else
					{
						component5.PlaySound(SoundSet.SoundAction.IAttack, 1);
					}
				}
				return spellCastData;
			}
			AIController.s_defaultInstruction = null;
			return null;
		}
		return nextInstruction;
	}

	private void ThaosController_OnDeath(GameObject myObject, GameEventArgs args)
	{
		myObject.GetComponent<Health>().OnDeath -= ThaosController_OnDeath;
		if (!m_launchingSoulJump)
		{
			return;
		}
		Cutscene component = SoulJumpCutscene.GetComponent<Cutscene>();
		if ((bool)component)
		{
			ThaosTeleport_Cutscene component2 = component.GetComponent<ThaosTeleport_Cutscene>();
			if ((bool)component2)
			{
				component2.bSoulJumpReturn = true;
				component.StartCutscene();
			}
		}
	}

	private void ThaosController_OnHit(GameObject source, GameEventArgs args)
	{
		if (m_launchingSoulJump && m_currentVessel != null && m_currentVessel.gameObject == source && m_currentVessel.GetComponent<Health>().CurrentStamina <= m_vesselStartStamina - m_vesselStaminaToLose)
		{
			RegainControl();
		}
	}

	public override bool BeingKited()
	{
		return false;
	}

	public override bool EngagementsEnabled()
	{
		if (m_launchingSoulJump)
		{
			return false;
		}
		return true;
	}
}

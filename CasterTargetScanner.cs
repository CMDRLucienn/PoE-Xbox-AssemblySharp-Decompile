using System.Collections.Generic;
using System.Text;
using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;

public class CasterTargetScanner : TargetScanner
{
	private const float SEARCH_RANGE = 20f;

	private const float IN_COMBAT_SEARCH_RANGE = 30f;

	private const float STAMINA_DISTANCE_WEIGHT = 0.6f;

	private const float STAMINA_HEALTH_WEIGHT = 1.75f;

	private const float ENGAGER_WEIGHT = 5f;

	private const float DAMAGE_INFLICTOR_WEIGHT = 5f;

	private const float MIN_AFFLICTION_TIME_LEFT = 2f;

	private static List<GameObject> s_potentialTargets = new List<GameObject>();

	private static List<StatusEffect> s_hostileStatusEffects = new List<StatusEffect>();

	private StringBuilder m_debugText = new StringBuilder();

	private GenericAbility m_selectedAbillity;

	public GenericAbility SelectedAbility => m_selectedAbillity;

	private static float GetSearchRange(AIController controller)
	{
		if ((bool)controller && controller.InCombat)
		{
			return 30f;
		}
		return 20f;
	}

	public override void Reset()
	{
		base.Reset();
		m_debugText.Remove(0, m_debugText.Length);
	}

	public override void SelectAttack(GameObject owner, GameObject originalTarget, AIController aiController, bool isForceAttack, out GameObject finalTarget)
	{
		finalTarget = null;
		m_selectedAbillity = null;
		m_debugText.Length = 0;
		AIController aIController = GameUtilities.FindActiveAIController(owner);
		if (aIController == null)
		{
			return;
		}
		if (!aIController.ReadyForNextInstruction)
		{
			m_debugText.AppendLine("** No Instructions Ready **");
			return;
		}
		AIController.InitInstructions(aIController);
		while (AIController.PossibleInstructions.Count > 0)
		{
			SpellCastData spellCastData = aIController.GetNextInstruction();
			if (spellCastData == null)
			{
				spellCastData = AIController.DefaultInstruction;
				if (spellCastData == null)
				{
					break;
				}
			}
			if (spellCastData.HandledInternally)
			{
				aIController.InstructionProcessed(spellCastData);
				AIController.PossibleInstructions.Clear();
				break;
			}
			if (SelectAttack(owner, originalTarget, spellCastData, aiController, isForceAttack, out finalTarget))
			{
				m_selectedAbillity = GetAbilityFromInstruction(owner, aiController, spellCastData);
				m_debugText.AppendLine("**" + spellCastData.DebugName + "**");
				aIController.InstructionProcessed(spellCastData);
				return;
			}
			spellCastData.StartNoValidTargetTimer();
			AIController.PossibleInstructions.Remove(spellCastData);
			AIController.TotalRandomScore -= spellCastData.CastingPriority;
		}
		m_attackToUse = null;
		finalTarget = null;
		m_debugText.AppendLine("** No Valid Targets For Available Instructions **");
	}

	private bool SelectAttack(GameObject owner, GameObject originalTarget, SpellCastData instruction, AIController aiController, bool isForceAttack, out GameObject finalTarget)
	{
		finalTarget = null;
		if (instruction.CastInstruction == SpellCastData.Instruction.UseConsumable)
		{
			return true;
		}
		if (instruction.CastInstruction == SpellCastData.Instruction.Flee)
		{
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			aiController.StateManager.PushState(pathToPosition);
			if (instruction.Waypoint != null)
			{
				pathToPosition.Parameters.Target = instruction.Waypoint.gameObject;
			}
			instruction.StartTimer();
			instruction.IncrementCastCount();
			return true;
		}
		Vector3 targetPosition = Vector3.zero;
		GameObject gameObject = GetTargetFromInstruction(owner, originalTarget, instruction, isForceAttack);
		if (gameObject != null)
		{
			targetPosition = gameObject.transform.position;
		}
		else
		{
			if (!GetTargetPosition(owner, originalTarget, instruction, out targetPosition))
			{
				return false;
			}
			gameObject = originalTarget;
		}
		if (!ConditionalIsValid(owner, instruction, gameObject))
		{
			return false;
		}
		GenericAbility abilityFromInstruction = GetAbilityFromInstruction(owner, aiController, instruction);
		if (abilityFromInstruction != null && abilityFromInstruction.Modal && abilityFromInstruction.Activated)
		{
			return false;
		}
		switch (instruction.CastInstruction)
		{
		case SpellCastData.Instruction.Cast:
			finalTarget = gameObject;
			if (abilityFromInstruction != null)
			{
				m_attackToUse = abilityFromInstruction.GetComponent<AttackBase>();
			}
			else
			{
				m_attackToUse = null;
			}
			if (m_attackToUse == null && (!(aiController is PartyMemberAI) || (!abilityFromInstruction.UsePrimaryAttack && !abilityFromInstruction.UseFullAttack)))
			{
				TriggerAbilities(owner, abilityFromInstruction, finalTarget, aiController as PartyMemberAI);
			}
			return true;
		case SpellCastData.Instruction.CastAtWaypoint:
			finalTarget = instruction.Waypoint.gameObject;
			if (abilityFromInstruction != null)
			{
				m_attackToUse = abilityFromInstruction.GetComponent<AttackBase>();
			}
			else
			{
				m_attackToUse = null;
			}
			if (m_attackToUse == null && !abilityFromInstruction.UsePrimaryAttack && !abilityFromInstruction.UseFullAttack)
			{
				TriggerAbilities(owner, abilityFromInstruction, finalTarget, aiController as PartyMemberAI);
			}
			return true;
		case SpellCastData.Instruction.UseWeapon:
			base.SelectAttack(owner, originalTarget, aiController, isForceAttack, out finalTarget);
			if (m_attackToUse != null)
			{
				finalTarget = gameObject;
				return true;
			}
			break;
		}
		return true;
	}

	private void TriggerAbilities(GameObject owner, GenericAbility spell, GameObject target, PartyMemberAI partyMemberAI)
	{
		if (spell == null)
		{
			return;
		}
		GenericAbility component = spell.GetComponent<GenericAbility>();
		if (!(component != null))
		{
			return;
		}
		if (component.Modal && partyMemberAI != null)
		{
			AI.Player.Ability ability = AIStateManager.StatePool.Allocate<AI.Player.Ability>();
			ability.QueuedAbility = component;
			ability.QueuedAbility.ForceTriggerFromUI();
			partyMemberAI.StateManager.PushState(ability);
		}
		else if (component.Ready)
		{
			component.Activate(target);
		}
		else if (partyMemberAI != null)
		{
			AI.Player.Ability ability2 = AIStateManager.StatePool.Allocate<AI.Player.Ability>();
			ability2.QueuedAbility = component;
			partyMemberAI.StateManager.PushState(ability2);
		}
		else if (owner != null)
		{
			AIController aIController = GameUtilities.FindActiveAIController(owner);
			if (aIController != null)
			{
				AI.Plan.Ability ability3 = AIStateManager.StatePool.Allocate<AI.Plan.Ability>();
				ability3.QueuedAbility = component;
				ability3.Target = target;
				aIController.StateManager.PushState(ability3);
			}
		}
	}

	private GenericAbility GetAbilityFromInstruction(GameObject owner, AIController aiController, SpellCastData instruction)
	{
		if (aiController as PartyMemberAI != null)
		{
			CharacterStats component = owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				return component.FindAbilityInstance(instruction.Spell);
			}
			return null;
		}
		return instruction.Spell;
	}

	public override bool HasAvailableAttack(GameObject owner)
	{
		if (owner == null)
		{
			return false;
		}
		AIPackageController component = owner.GetComponent<AIPackageController>();
		if (component == null)
		{
			return false;
		}
		List<SpellCastData> instructions = component.Instructions;
		for (int i = 0; i < instructions.Count; i++)
		{
			if (instructions[i].Ready)
			{
				return true;
			}
		}
		return false;
	}

	private static float GetMaxRangeForInstructionBasedOnMobility(GameObject owner, SpellCastData instruction, float desiredMaxRange)
	{
		float result = desiredMaxRange;
		Mover component = owner.GetComponent<Mover>();
		if ((bool)component && component.GetRunSpeed() <= 0f)
		{
			AttackBase attackBase = null;
			if (instruction.CastInstruction == SpellCastData.Instruction.Cast)
			{
				attackBase = ((!(instruction.Spell != null)) ? null : instruction.Spell.GetComponent<AttackBase>());
			}
			else if (instruction.CastInstruction == SpellCastData.Instruction.UseWeapon)
			{
				attackBase = AIController.GetPrimaryAttack(owner);
			}
			if (attackBase != null)
			{
				result = attackBase.AttackDistance;
			}
		}
		return result;
	}

	private GameObject GetTargetFromInstruction(GameObject owner, GameObject closestTarget, SpellCastData instruction, bool isForceAttack)
	{
		m_debugText.AppendLine("Targeting " + instruction.Target);
		float searchRange = GetSearchRange(GameUtilities.FindActiveAIController(owner));
		switch (instruction.Target)
		{
		case SpellCastData.CastTarget.CurrentEnemy:
		{
			if (isForceAttack)
			{
				s_potentialTargets.Add(closestTarget);
			}
			else
			{
				PopulatePotentialTargets(owner, instruction, searchRange);
			}
			for (int num11 = s_potentialTargets.Count - 1; num11 >= 0; num11--)
			{
				if (s_potentialTargets[num11] == null)
				{
					s_potentialTargets.RemoveAt(num11);
				}
				else
				{
					Health component4 = s_potentialTargets[num11].GetComponent<Health>();
					if ((component4 != null && component4.Unconscious) || (Stealth.IsInStealthMode(s_potentialTargets[num11]) && !isForceAttack))
					{
						s_potentialTargets.RemoveAt(num11);
					}
				}
			}
			if (s_potentialTargets.Count <= 0)
			{
				return null;
			}
			GameObject targetFromPreference2 = GetTargetFromPreference(owner, instruction, s_potentialTargets);
			s_potentialTargets.Clear();
			return targetFromPreference2;
		}
		case SpellCastData.CastTarget.Self:
			return owner;
		case SpellCastData.CastTarget.AllyLowestStamina:
		{
			m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
			GameObject[] array4 = GameUtilities.FriendsInRange(owner.transform.position, searchRange, owner, includeUnconscious: false);
			if (array4 == null)
			{
				return null;
			}
			GameObject result4 = null;
			float num10 = 1f;
			foreach (GameObject gameObject4 in array4)
			{
				if (!(gameObject4 == null))
				{
					Health component3 = gameObject4.GetComponent<Health>();
					if ((bool)component3 && component3.StaminaPercentage < num10)
					{
						num10 = component3.StaminaPercentage;
						result4 = gameObject4;
					}
				}
			}
			return result4;
		}
		case SpellCastData.CastTarget.AllyUnconscious:
		{
			m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
			GameObject[] array2 = GameUtilities.FriendsInRange(owner.transform.position, searchRange, owner, includeUnconscious: true);
			if (array2 == null)
			{
				return null;
			}
			GameObject result3 = null;
			float num8 = float.MaxValue;
			foreach (GameObject gameObject3 in array2)
			{
				if (gameObject3 == null)
				{
					continue;
				}
				Health component2 = gameObject3.GetComponent<Health>();
				if ((bool)component2 && component2.Unconscious)
				{
					float num9 = GameUtilities.V3SqrDistance2D(gameObject3.transform.position, owner.transform.position);
					if (num9 < num8)
					{
						num8 = num9;
						result3 = gameObject3;
					}
				}
			}
			return result3;
		}
		case SpellCastData.CastTarget.PreferredAlly:
		{
			m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
			GameObject[] array3 = GameUtilities.FriendsInRange(owner.transform.position, searchRange, owner, includeUnconscious: true);
			if (array3 == null)
			{
				return null;
			}
			s_potentialTargets.Clear();
			s_potentialTargets.AddRange(array3);
			GameObject targetFromPreference = GetTargetFromPreference(owner, instruction, s_potentialTargets);
			s_potentialTargets.Clear();
			return targetFromPreference;
		}
		case SpellCastData.CastTarget.NearestAlly:
		case SpellCastData.CastTarget.CloseAlliesCenter:
		{
			m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
			GameObject[] array5 = GameUtilities.FriendsInRange(owner.transform.position, searchRange, owner, includeUnconscious: false);
			if (array5 == null)
			{
				return null;
			}
			GameObject result5 = array5[0];
			float num12 = float.MaxValue;
			foreach (GameObject gameObject5 in array5)
			{
				float num13 = GameUtilities.V3SqrDistance2D(gameObject5.transform.position, owner.transform.position);
				if (num13 < num12)
				{
					result5 = gameObject5;
					num12 = num13;
				}
			}
			return result5;
		}
		case SpellCastData.CastTarget.NearestEnemy:
		case SpellCastData.CastTarget.CloseEnemiesCenter:
		{
			if (isForceAttack)
			{
				s_potentialTargets.Add(closestTarget);
			}
			else
			{
				m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
				PopulatePotentialTargets(owner, instruction, searchRange);
			}
			for (int num14 = s_potentialTargets.Count - 1; num14 >= 0; num14--)
			{
				if (s_potentialTargets[num14] == null)
				{
					s_potentialTargets.RemoveAt(num14);
				}
				else
				{
					Health component5 = s_potentialTargets[num14].GetComponent<Health>();
					if ((component5 != null && component5.Unconscious) || Stealth.IsInStealthMode(s_potentialTargets[num14]))
					{
						s_potentialTargets.RemoveAt(num14);
					}
				}
			}
			if (s_potentialTargets.Count <= 0)
			{
				return null;
			}
			GameObject result6 = s_potentialTargets[0];
			float num15 = float.MaxValue;
			for (int n = 0; n < s_potentialTargets.Count; n++)
			{
				GameObject gameObject6 = s_potentialTargets[n];
				float num16 = GameUtilities.V3SqrDistance2D(gameObject6.transform.position, owner.transform.position);
				if (num16 < num15)
				{
					result6 = gameObject6;
					num15 = num16;
				}
			}
			s_potentialTargets.Clear();
			return result6;
		}
		case SpellCastData.CastTarget.FarthestEnemyWithinRange:
		{
			if (isForceAttack)
			{
				s_potentialTargets.Add(closestTarget);
			}
			else
			{
				m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
				PopulatePotentialTargets(owner, instruction, searchRange);
			}
			for (int num4 = s_potentialTargets.Count - 1; num4 >= 0; num4--)
			{
				if (s_potentialTargets[num4] == null)
				{
					s_potentialTargets.RemoveAt(num4);
				}
				else
				{
					Health component = s_potentialTargets[num4].GetComponent<Health>();
					if ((component != null && component.Unconscious) || Stealth.IsInStealthMode(s_potentialTargets[num4]))
					{
						s_potentialTargets.RemoveAt(num4);
					}
				}
			}
			if (s_potentialTargets.Count <= 0)
			{
				return null;
			}
			GameObject result2 = s_potentialTargets[0];
			float num5 = 0f;
			AttackBase attackBase2 = null;
			attackBase2 = ((!(instruction.Spell != null)) ? GetNextAttackFromEquipment(owner) : instruction.Spell.GetComponent<AttackBase>());
			if (attackBase2 != null)
			{
				float num6 = attackBase2.AttackDistance * attackBase2.AttackDistance;
				for (int j = 0; j < s_potentialTargets.Count; j++)
				{
					GameObject gameObject2 = s_potentialTargets[j];
					if (!(gameObject2 == null))
					{
						float num7 = GameUtilities.V3SqrDistance2D(gameObject2.transform.position, owner.transform.position);
						if (!(num7 > num6) && num7 > num5)
						{
							result2 = gameObject2;
							num5 = num7;
						}
					}
				}
			}
			s_potentialTargets.Clear();
			return result2;
		}
		case SpellCastData.CastTarget.FarthestAllyWithinRange:
		{
			m_debugText.AppendLine("  Search Range  = " + searchRange.ToString("###0.00") + " meters.");
			GameObject[] array = GameUtilities.FriendsInRange(owner.transform.position, searchRange, owner, includeUnconscious: false);
			if (array == null)
			{
				return null;
			}
			GameObject result = array[0];
			float num = 0f;
			AttackBase attackBase = null;
			attackBase = ((!(instruction.Spell != null)) ? GetNextAttackFromEquipment(owner) : instruction.Spell.GetComponent<AttackBase>());
			if (attackBase != null)
			{
				float num2 = attackBase.AttackDistance * attackBase.AttackDistance;
				foreach (GameObject gameObject in array)
				{
					float num3 = GameUtilities.V3SqrDistance2D(gameObject.transform.position, owner.transform.position);
					if (!(num3 > num2) && num3 > num)
					{
						result = gameObject;
						num = num3;
					}
				}
			}
			return result;
		}
		case SpellCastData.CastTarget.OwnAnimalCompanion:
			return GameUtilities.FindAnimalCompanion(owner);
		default:
			return closestTarget;
		}
	}

	private static void PopulatePotentialTargets(GameObject owner, SpellCastData instruction, float searchRange)
	{
		bool mustBeFowVisible = PartyHelper.IsPartyMember(owner);
		GameUtilities.GetEnemiesInRange(owner, GameUtilities.FindActiveAIController(owner), GetMaxRangeForInstructionBasedOnMobility(owner, instruction, 20f), s_potentialTargets, mustBeFowVisible);
		if (s_potentialTargets.Count == 0)
		{
			GameUtilities.GetEnemiesInRange(owner, GameUtilities.FindActiveAIController(owner), GetMaxRangeForInstructionBasedOnMobility(owner, instruction, searchRange), s_potentialTargets, mustBeFowVisible);
		}
	}

	private GameObject GetTargetFromPreference(GameObject owner, SpellCastData instruction, List<GameObject> potentialTargets)
	{
		GameObject targetFromPreference = GetTargetFromPreference(owner, instruction, instruction.TargetPreference, potentialTargets);
		if (targetFromPreference != null)
		{
			return targetFromPreference;
		}
		if (instruction != null && instruction.TargetPreference != null && instruction.TargetPreference.PreferenceType == TargetPreference.TargetPreferenceType.BehindAttacker)
		{
			return null;
		}
		if (instruction.IgnoreFallbackToDefaultTargetPreference)
		{
			return null;
		}
		return GetTargetFromPreference(owner, instruction, AIController.FilteredDefaultTargetPreference, potentialTargets);
	}

	private GameObject GetTargetFromPreference(GameObject owner, SpellCastData instruction, TargetPreference preference, List<GameObject> potentialTargets)
	{
		if (preference == null)
		{
			return null;
		}
		AttackBase attackBase = null;
		AIController aIController = GameUtilities.FindActiveAIController(owner);
		float searchRange = GetSearchRange(aIController);
		attackBase = ((instruction.CastInstruction != 0 && instruction.CastInstruction != SpellCastData.Instruction.CastAtWaypoint) ? GetLongestWeaponAttack(aIController, owner) : instruction.Spell.GetComponent<AttackBase>());
		if (attackBase == null)
		{
			return null;
		}
		float num = attackBase.TotalAttackDistance;
		if (aIController != null)
		{
			num += aIController.Mover.Radius;
		}
		float num2 = 0f;
		float num3 = num2;
		GameObject result = null;
		switch (preference.PreferenceType)
		{
		case TargetPreference.TargetPreferenceType.None:
		case TargetPreference.TargetPreferenceType.LowestStamina:
		case TargetPreference.TargetPreferenceType.LowestDamageThreshold:
		case TargetPreference.TargetPreferenceType.LowestDefense:
		case TargetPreference.TargetPreferenceType.SneakAttackVulnerable:
		case TargetPreference.TargetPreferenceType.LowNumberOfEngagers:
			num3 = float.MaxValue;
			break;
		}
		bool flag = false;
		if (aIController != null)
		{
			flag = aIController.IsEngaged();
		}
		for (int i = 0; i < potentialTargets.Count; i++)
		{
			GameObject gameObject = potentialTargets[i];
			if (gameObject == null)
			{
				continue;
			}
			AIController aIController2 = GameUtilities.FindActiveAIController(gameObject);
			if (aIController2 == null)
			{
				continue;
			}
			float num4 = GameUtilities.V3SqrDistance2D(owner.transform.position, gameObject.transform.position);
			float num5 = num + aIController2.Mover.Radius;
			num5 *= num5;
			if (num4 > num5 && (aIController2.Mover.Frozen || preference.AllowedMovementToTarget == TargetPreference.AllowedMovementToTargetType.WillNotMove || (preference.AllowedMovementToTarget == TargetPreference.AllowedMovementToTargetType.MoveWithinRange && flag && !aIController2.IsEngaging(owner))))
			{
				continue;
			}
			num2 = 0f;
			if (preference.PreferenceType == TargetPreference.TargetPreferenceType.None)
			{
				Health component = gameObject.GetComponent<Health>();
				num2 += Mathf.Sqrt(num4) / searchRange * 2f;
				num2 += component.CurrentStamina / component.MaxStamina * 0.5f;
				if (aIController2.HasEngaged(gameObject))
				{
					num2 -= 3f;
				}
				else if (aIController2.IsEngagedBy(owner))
				{
					num2 -= 2.4f;
				}
				if (num2 < num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.HighestStamina)
			{
				Health component2 = gameObject.GetComponent<Health>();
				num2 += (1f - Mathf.Sqrt(num4) / searchRange) * 0.6f;
				num2 += component2.CurrentStamina / component2.MaxStamina * 1.75f;
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.LowestStamina)
			{
				Health component3 = gameObject.GetComponent<Health>();
				num2 += Mathf.Sqrt(num4) / searchRange * 0.6f;
				num2 += component3.CurrentStamina / component3.MaxStamina * 1.75f;
				if (num2 < num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.LowestDamageThreshold)
			{
				num2 = gameObject.GetComponent<CharacterStats>().CalcDT(preference.DamageType, isVeilPiercing: false);
				if (num2 < num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.HighestDefense)
			{
				num2 = gameObject.GetComponent<CharacterStats>().CalculateDefense(preference.DefenseType);
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.LowestDefense)
			{
				num2 = gameObject.GetComponent<CharacterStats>().CalculateDefense(preference.DefenseType);
				if (num2 < num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.HighestDamageInflictor)
			{
				num2 = 1f - Mathf.Sqrt(num4) / searchRange;
				if (gameObject == aIController.HighestDamageInflictor)
				{
					num2 += 5f;
				}
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.CurrentEngager)
			{
				if (aIController.IsEngagedBy(gameObject))
				{
					num2 = 1f;
					if (aIController.CurrentTarget == gameObject)
					{
						num2 += 1f;
					}
					if (num2 > num3)
					{
						result = gameObject;
						num3 = num2;
					}
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.SneakAttackVulnerable)
			{
				StatusEffect sneakAttackAffliction = FlankingAbility.GetSneakAttackAffliction(gameObject);
				if (sneakAttackAffliction != null && sneakAttackAffliction.TimeLeft > 2f)
				{
					num2 = Mathf.Sqrt(num4) / searchRange;
					if (num2 < num3)
					{
						result = gameObject;
						num3 = num2;
					}
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.LowNumberOfEngagers)
			{
				num2 = Mathf.Sqrt(num4) / searchRange;
				num2 += (float)aIController2.EngagedBy.Count * 5f;
				if (aIController.IsEngaging(gameObject))
				{
					num2 -= 5f;
				}
				if (num2 < num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.Spellcasters)
			{
				CharacterStats component4 = gameObject.GetComponent<CharacterStats>();
				num2 = (1f - Mathf.Sqrt(num4) / searchRange) * 2f;
				switch (component4.CharacterClass)
				{
				case CharacterStats.Class.Priest:
				case CharacterStats.Class.Wizard:
				case CharacterStats.Class.Druid:
				case CharacterStats.Class.Cipher:
				case CharacterStats.Class.Chanter:
					num2 += 6f;
					num2 += OEIRandom.Range(0f, 2f);
					break;
				}
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.FastestClass)
			{
				CharacterStats component5 = gameObject.GetComponent<CharacterStats>();
				num2 = 1f - Mathf.Sqrt(num4) / searchRange;
				switch (component5.CharacterClass)
				{
				case CharacterStats.Class.Rogue:
					num2 += 10f;
					break;
				case CharacterStats.Class.Monk:
					num2 += 9f;
					break;
				case CharacterStats.Class.Barbarian:
					num2 += 8f;
					break;
				case CharacterStats.Class.Ranger:
					num2 += 7f;
					break;
				case CharacterStats.Class.Paladin:
					num2 += 6f;
					break;
				case CharacterStats.Class.Fighter:
					num2 += 5f;
					break;
				}
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.AfflictedBy)
			{
				Affliction affliction = null;
				Affliction affliction2 = null;
				switch (preference.AfflictedBy)
				{
				case TargetPreference.AfflictedByType.FrightenedOrTerrified:
					affliction = AttackData.Instance.FrightenedAffliction;
					affliction2 = AttackData.Instance.TerrifiedAffliction;
					break;
				case TargetPreference.AfflictedByType.SickenedOrWeakened:
					affliction = AttackData.Instance.SickenedAffliction;
					affliction2 = AttackData.Instance.WeakenedAffliction;
					break;
				case TargetPreference.AfflictedByType.HobbledOrStuck:
					affliction = AttackData.Instance.HobbledAffliction;
					affliction2 = AttackData.Instance.StuckAffliction;
					break;
				case TargetPreference.AfflictedByType.DazedOrConfused:
					affliction = AttackData.Instance.DazedAffliction;
					affliction2 = AttackData.Instance.ConfusedAffliction;
					break;
				case TargetPreference.AfflictedByType.ParalyzedOrPetrified:
					affliction = AfflictionData.Instance.ParalyzedPrefab;
					affliction2 = AfflictionData.Instance.PetrifiedPrefab;
					break;
				case TargetPreference.AfflictedByType.CharmedOrDominated:
					affliction = AfflictionData.Instance.CharmedPrefab;
					affliction2 = AfflictionData.Instance.DominatedPrefab;
					break;
				case TargetPreference.AfflictedByType.ProneOrUnconscious:
					affliction = AfflictionData.Instance.PronePrefab;
					affliction2 = AfflictionData.Instance.UnconsciousPrefab;
					break;
				}
				if (affliction == null || affliction2 == null)
				{
					continue;
				}
				CharacterStats component6 = gameObject.GetComponent<CharacterStats>();
				if (!(component6 != null))
				{
					continue;
				}
				StatusEffect statusEffectFromAffliction = component6.GetStatusEffectFromAffliction(affliction);
				if (statusEffectFromAffliction != null && statusEffectFromAffliction.TimeLeft > 2f)
				{
					num2 = 1f - Mathf.Sqrt(num4) / searchRange;
				}
				else
				{
					statusEffectFromAffliction = component6.GetStatusEffectFromAffliction(affliction2);
					if (statusEffectFromAffliction != null && statusEffectFromAffliction.TimeLeft > 2f)
					{
						num2 = 1f - Mathf.Sqrt(num4) / searchRange;
					}
				}
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.BehindAttacker)
			{
				float num6 = 3f;
				if (aIController != null)
				{
					num6 += aIController.Mover.Radius;
				}
				num6 *= num6;
				if (!(num4 < num6))
				{
					continue;
				}
				Vector2 lhs = GameUtilities.V3ToV2(owner.transform.forward);
				Vector2 rhs = GameUtilities.V3Subtract2D(gameObject.transform.position, owner.transform.position);
				if (rhs.sqrMagnitude > float.Epsilon)
				{
					rhs.Normalize();
					float num7 = Vector2.Dot(lhs, rhs);
					if (num7 < -0.5f && num7 < num3)
					{
						result = gameObject;
						num3 = num7;
					}
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.EngagedByAnimalCompanion)
			{
				CharacterStats component7 = owner.GetComponent<CharacterStats>();
				num2 = (1f - Mathf.Sqrt(num4) / searchRange) * 2f;
				if (component7 != null && component7.CharacterClass == CharacterStats.Class.Ranger)
				{
					for (int j = 0; j < aIController.SummonedCreatureList.Count; j++)
					{
						if (aIController.SummonedCreatureList[j] != null)
						{
							AIController aIController3 = GameUtilities.FindActiveAIController(aIController.SummonedCreatureList[j]);
							if (aIController3 != null && aIController3.SummonType == AIController.AISummonType.AnimalCompanion && aIController3.CurrentTarget == gameObject)
							{
								num2 += 6f;
								break;
							}
						}
					}
				}
				if (num2 > num3)
				{
					result = gameObject;
					num3 = num2;
				}
			}
			else if (preference.PreferenceType == TargetPreference.TargetPreferenceType.Engaged && (bool)aIController2 && (float)aIController2.EngagedBy.Count > num3)
			{
				result = gameObject;
				num3 = aIController2.EngagedBy.Count;
			}
		}
		return result;
	}

	private bool GetTargetPosition(GameObject owner, GameObject closestTarget, SpellCastData instruction, out Vector3 targetPosition)
	{
		targetPosition = Vector3.zero;
		float range = 20f;
		switch (instruction.Target)
		{
		case SpellCastData.CastTarget.CloseAlliesCenter:
		{
			m_debugText.AppendLine("  Search Range  = " + range.ToString("###0.00") + " meters.");
			GameObject[] array2 = GameUtilities.FriendsInRange(owner.transform.position, range, owner, includeUnconscious: false);
			if (array2 == null || array2.Length == 0)
			{
				return false;
			}
			m_debugText.AppendLine("  Calculating center position of " + (array2.Length + 1) + " enemies.");
			Vector3 zero2 = Vector3.zero;
			for (int j = 0; j < array2.Length; j++)
			{
				zero2 += array2[j].transform.position;
			}
			zero2 /= (float)array2.Length;
			targetPosition = zero2;
			return true;
		}
		case SpellCastData.CastTarget.CloseEnemiesCenter:
		{
			float range2 = 10f;
			if (closestTarget == null)
			{
				return false;
			}
			if (instruction.Spell != null)
			{
				AttackAOE component = instruction.Spell.GetComponent<AttackAOE>();
				if (component != null)
				{
					range2 = component.BlastRadius;
				}
			}
			m_debugText.AppendLine("  Search Range  = " + range2.ToString("####.00") + " meters.");
			if (closestTarget == null)
			{
				return false;
			}
			GameObject[] array = GameUtilities.FriendsInRange(closestTarget.transform.position, range2, closestTarget, includeUnconscious: false);
			if (array == null || array.Length == 0)
			{
				return false;
			}
			m_debugText.AppendLine("  Calculating center position of " + (array.Length + 1) + " enemies.");
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				zero += array[i].transform.position;
			}
			zero /= (float)array.Length;
			targetPosition = zero;
			return true;
		}
		default:
			return false;
		}
	}

	public void BuildDebugText(ref string text)
	{
		text += m_debugText.ToString();
	}

	protected bool MatchesEquality(ConditionalData conditional, int comparison)
	{
		return MatchesEquality(conditional, conditional.Value, comparison);
	}

	protected bool MatchesEquality(ConditionalData conditional, float value, float comparison)
	{
		bool flag = false;
		flag = conditional.Comparison switch
		{
			SpellCastData.Operator.Equals => value == comparison, 
			SpellCastData.Operator.GreaterThan => comparison > value, 
			SpellCastData.Operator.LessThan => comparison < value, 
			_ => false, 
		};
		m_debugText.Append("     ");
		m_debugText.Append(conditional.Condition.ToString());
		m_debugText.Append(" ");
		m_debugText.Append(comparison);
		m_debugText.Append(" ");
		m_debugText.Append(conditional.Comparison.ToString());
		m_debugText.Append(" ");
		m_debugText.Append(value);
		m_debugText.Append(" = ");
		m_debugText.Append(flag.ToString().ToUpper());
		m_debugText.AppendLine();
		return flag;
	}

	public bool ConditionalIsValid(GameObject owner, SpellCastData data, GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		if (data.Conditionals == null || data.Conditionals.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < data.Conditionals.Length; i++)
		{
			ConditionalData conditionalData = data.Conditionals[i];
			m_debugText.AppendLine("    Testing " + conditionalData.Condition.ToString() + " targeting " + conditionalData.Target);
			GameObject gameObject = null;
			gameObject = ((conditionalData.Target != SpellCastData.ConditionalTargetType.Self && conditionalData.Target != SpellCastData.ConditionalTargetType.AllyOrSelf) ? target : owner);
			switch (conditionalData.Condition)
			{
			case SpellCastData.ConditionType.NumTargetsInAOE:
			{
				if (data.Spell == null)
				{
					return false;
				}
				float num4 = data.Spell.AdjustedFriendlyRadius;
				AttackBase component10 = data.Spell.GetComponent<AttackBase>();
				if (component10 is AttackAOE)
				{
					num4 = (component10 as AttackAOE).AdjustedBlastRadius;
				}
				int num5 = 0;
				if (conditionalData.Target == SpellCastData.ConditionalTargetType.Ally || conditionalData.Target == SpellCastData.ConditionalTargetType.AllyOrSelf)
				{
					GameObject[] array2 = GameUtilities.FriendsInRange(target.transform.position, num4, owner, includeUnconscious: false);
					if (array2 != null)
					{
						num5 = array2.Length;
					}
				}
				else if (conditionalData.Target == SpellCastData.ConditionalTargetType.Enemy)
				{
					GameUtilities.GetEnemiesInRange(owner, GameUtilities.FindActiveAIController(target), num4, s_potentialTargets);
					if (s_potentialTargets.Count > 0)
					{
						num5 = s_potentialTargets.Count;
					}
					s_potentialTargets.Clear();
				}
				if (conditionalData.Target == SpellCastData.ConditionalTargetType.AllyOrSelf && Vector3.Distance(owner.transform.position, target.transform.position) <= num4)
				{
					num5++;
				}
				if (!MatchesEquality(conditionalData, num5))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.MoreThanOneTargetInAOEWithStaminaPercentage:
				if (!NumTargetsInAOEWithStaminaLessThan(owner, data, target, conditionalData, 1))
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.MoreThanTwoTargetsInAOEWithStaminaPercentage:
				if (!NumTargetsInAOEWithStaminaLessThan(owner, data, target, conditionalData, 2))
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.HealthPercentage:
			{
				Health component8 = gameObject.GetComponent<Health>();
				if ((bool)component8)
				{
					if (!MatchesEquality(conditionalData, (int)(component8.HealthPercentage * 100f)))
					{
						return false;
					}
					break;
				}
				return false;
			}
			case SpellCastData.ConditionType.StaminaPercentage:
			{
				Health component9 = gameObject.GetComponent<Health>();
				if ((bool)component9)
				{
					if (!MatchesEquality(conditionalData, (int)(component9.StaminaPercentage * 100f)))
					{
						return false;
					}
					break;
				}
				return false;
			}
			case SpellCastData.ConditionType.InAttackRange:
			{
				float comparison = Vector3.Distance(owner.transform.position, target.transform.position);
				AttackBase attackBase = ((data.Spell != null) ? data.Spell.GetComponent<AttackBase>() : null);
				if (attackBase == null)
				{
					attackBase = owner.GetComponent<Equipment>().PrimaryAttack;
				}
				if (attackBase == null || !MatchesEquality(conditionalData, attackBase.TotalAttackDistance, comparison))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.Distance:
			{
				float comparison3 = GameUtilities.V3Distance2D(owner.transform.position, target.transform.position);
				if (!MatchesEquality(conditionalData, conditionalData.Value, comparison3))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.Engaged:
			{
				int comparison2 = 0;
				AIController aIController4 = GameUtilities.FindActiveAIController(gameObject);
				if (aIController4 != null)
				{
					comparison2 = aIController4.EngagedBy.Count;
				}
				if (!MatchesEquality(conditionalData, comparison2))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.Engaging:
			{
				int comparison4 = 0;
				AIController aIController7 = GameUtilities.FindActiveAIController(gameObject);
				if (aIController7 != null)
				{
					comparison4 = aIController7.EngagedEnemies.Count;
				}
				if (!MatchesEquality(conditionalData, comparison4))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.HasHostileEffect:
			{
				CharacterStats component5 = gameObject.GetComponent<CharacterStats>();
				if (component5 == null)
				{
					return false;
				}
				if (!MatchesEquality(conditionalData, component5.CountHostileEffects()))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.HasHostileEffectWithDuration:
			{
				CharacterStats component = gameObject.GetComponent<CharacterStats>();
				if (component == null)
				{
					return false;
				}
				s_hostileStatusEffects.Clear();
				component.GetHostileStatusEffects(s_hostileStatusEffects);
				if (s_hostileStatusEffects.Count == 0)
				{
					return false;
				}
				for (int j = 0; j < s_hostileStatusEffects.Count; j++)
				{
					if (MatchesEquality(conditionalData, conditionalData.Value, s_hostileStatusEffects[j].Duration))
					{
						return true;
					}
				}
				return false;
			}
			case SpellCastData.ConditionType.Stunned:
			{
				CharacterStats component6 = gameObject.GetComponent<CharacterStats>();
				if (component6 == null)
				{
					return false;
				}
				bool flag = component6.HasStatusEffectOfType(StatusEffect.ModifiedStat.Stunned);
				m_debugText.AppendLine("     Stunned is " + flag.ToString().ToUpper());
				if (!flag)
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.MoveRateEffect:
			{
				CharacterStats component3 = gameObject.GetComponent<CharacterStats>();
				if (component3 == null)
				{
					return false;
				}
				float num2 = 0f;
				foreach (StatusEffect item in component3.FindStatusEffectsOfType(StatusEffect.ModifiedStat.MovementRate))
				{
					num2 += item.ParamsValue();
				}
				if (!MatchesEquality(conditionalData, conditionalData.Value, num2))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.AllyCount:
			{
				int num = 0;
				GameObject[] array = GameUtilities.FriendsInRange(gameObject.transform.position, 15f, gameObject, includeUnconscious: false);
				if (array != null)
				{
					for (int k = 0; k < array.Length; k++)
					{
						AIController aIController = GameUtilities.FindActiveAIController(array[k]);
						if (aIController is PartyMemberAI || (aIController != null && aIController.InCombat))
						{
							num++;
						}
					}
				}
				if (!MatchesEquality(conditionalData, num))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.EnemyCount:
			{
				int num3 = 0;
				if (conditionalData.Target == SpellCastData.ConditionalTargetType.Self || conditionalData.Target == SpellCastData.ConditionalTargetType.AllyOrSelf)
				{
					GameUtilities.GetEnemiesInRange(owner, GameUtilities.FindActiveAIController(target), 15f, s_potentialTargets);
				}
				else
				{
					GameUtilities.GetEnemiesInRange(target, GameUtilities.FindActiveAIController(owner), 15f, s_potentialTargets);
				}
				for (int l = 0; l < s_potentialTargets.Count; l++)
				{
					AIController aIController2 = GameUtilities.FindActiveAIController(s_potentialTargets[l]);
					if (aIController2 is PartyMemberAI || (aIController2 != null && aIController2.InCombat))
					{
						num3++;
					}
				}
				s_potentialTargets.Clear();
				if (!MatchesEquality(conditionalData, num3))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.DifficultyIsEasy:
				if (GameState.Instance != null && GameState.Instance.Difficulty != 0 && GameState.Instance.Difficulty != GameDifficulty.StoryTime)
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.DifficultyIsNormal:
				if (GameState.Instance != null && GameState.Instance.Difficulty != GameDifficulty.Normal)
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.DifficultyIsHard:
				if (GameState.Instance != null && GameState.Instance.Difficulty != GameDifficulty.Hard && GameState.Instance.Difficulty != GameDifficulty.PathOfTheDamned)
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.DifficultyIsNormalOrHard:
				if (GameState.Instance != null && GameState.Instance.Difficulty != GameDifficulty.Hard && GameState.Instance.Difficulty != GameDifficulty.PathOfTheDamned && GameState.Instance.Difficulty != GameDifficulty.Normal)
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.DifficultyIsEasyOrNormal:
				if (GameState.Instance != null && GameState.Instance.Difficulty != 0 && GameState.Instance.Difficulty != GameDifficulty.Normal && GameState.Instance.Difficulty != GameDifficulty.StoryTime)
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.Px1HighLevelScaling:
				if (DifficultyScaling.Instance != null && !DifficultyScaling.Instance.IsAnyScalerActive(DifficultyScaling.Scaler.PX1_HIGH_LEVEL))
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.EngagedByAnimalCompanion:
			{
				CharacterStats component11 = owner.GetComponent<CharacterStats>();
				AIController aIController5 = GameUtilities.FindActiveAIController(owner);
				if (component11 == null || component11.CharacterClass != CharacterStats.Class.Ranger)
				{
					return false;
				}
				bool flag2 = false;
				for (int m = 0; m < aIController5.SummonedCreatureList.Count; m++)
				{
					if (aIController5.SummonedCreatureList[m] != null)
					{
						AIController aIController6 = GameUtilities.FindActiveAIController(aIController5.SummonedCreatureList[m]);
						if (aIController6 != null && aIController6.SummonType == AIController.AISummonType.AnimalCompanion && aIController6.EngagedEnemies.Contains(target))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.HasSummonedWeapon:
			{
				Equipment component7 = gameObject.GetComponent<Equipment>();
				if (component7 == null)
				{
					return false;
				}
				bool hasSummonedWeapon = component7.HasSummonedWeapon;
				m_debugText.AppendLine("     Has summoned weapon is " + hasSummonedWeapon.ToString().ToUpper());
				if (hasSummonedWeapon)
				{
					if (conditionalData.Value == 0)
					{
						return false;
					}
				}
				else if (conditionalData.Value != 0)
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.SummonCount:
			{
				AIController aIController3 = GameUtilities.FindActiveAIController(gameObject);
				if (aIController3 == null)
				{
					return false;
				}
				if (!MatchesEquality(conditionalData, aIController3.SummonedCreatureList.Count))
				{
					return false;
				}
				break;
			}
			case SpellCastData.ConditionType.IsImmuneToAnyAfflictionFromAbility:
			{
				CharacterStats component4 = gameObject.GetComponent<CharacterStats>();
				if ((bool)component4 && (bool)data.Spell && (bool)data.Spell.Attack)
				{
					return data.Spell.Attack.IsCharacterImmuneToAnyAffliction(component4) != (conditionalData.Value == 0);
				}
				break;
			}
			case SpellCastData.ConditionType.ProneOrUnconscious:
			{
				CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
				if ((bool)component2)
				{
					return (component2.HasStatusEffectFromAffliction(AfflictionData.Instance.UnconsciousPrefab) || component2.HasStatusEffectFromAffliction(AfflictionData.Instance.PronePrefab)) != (conditionalData.Value == 0);
				}
				break;
			}
			case SpellCastData.ConditionType.TimeInCombat:
				if (!MatchesEquality(conditionalData, Mathf.FloorToInt(GameState.InCombatDuration)))
				{
					return false;
				}
				break;
			case SpellCastData.ConditionType.DifficultyIsStoryTime:
				if ((bool)GameState.Instance && !GameState.Instance.IsDifficultyStoryTime)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	private bool NumTargetsInAOEWithStaminaLessThan(GameObject owner, SpellCastData data, GameObject target, ConditionalData conditional, int targetsNeededWithinRange)
	{
		if (data.Spell == null)
		{
			return false;
		}
		float num = data.Spell.AdjustedFriendlyRadius;
		AttackBase component = data.Spell.GetComponent<AttackBase>();
		if (component is AttackAOE)
		{
			num = (component as AttackAOE).AdjustedBlastRadius;
		}
		int num2 = 0;
		if (conditional.Target == SpellCastData.ConditionalTargetType.Ally || conditional.Target == SpellCastData.ConditionalTargetType.AllyOrSelf)
		{
			GameObject[] array = GameUtilities.FriendsInRange(target.transform.position, num, owner, includeUnconscious: false);
			if (array != null)
			{
				GameObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Health component2 = array2[i].GetComponent<Health>();
					if ((bool)component2 && MatchesEquality(conditional, (int)(component2.StaminaPercentage * 100f)))
					{
						num2++;
					}
				}
			}
		}
		else if (conditional.Target == SpellCastData.ConditionalTargetType.Enemy)
		{
			GameUtilities.GetEnemiesInRange(owner, GameUtilities.FindActiveAIController(target), num, s_potentialTargets);
			if (s_potentialTargets.Count > 0)
			{
				foreach (GameObject s_potentialTarget in s_potentialTargets)
				{
					Health component3 = s_potentialTarget.GetComponent<Health>();
					if ((bool)component3 && MatchesEquality(conditional, (int)(component3.StaminaPercentage * 100f)))
					{
						num2++;
					}
				}
			}
			s_potentialTargets.Clear();
		}
		if (conditional.Target == SpellCastData.ConditionalTargetType.AllyOrSelf && Vector3.Distance(owner.transform.position, target.transform.position) <= num)
		{
			Health component4 = owner.GetComponent<Health>();
			if ((bool)component4 && MatchesEquality(conditional, (int)(component4.StaminaPercentage * 100f)))
			{
				num2++;
			}
		}
		if (num2 < targetsNeededWithinRange)
		{
			return false;
		}
		return true;
	}

	public override string GetDebugText()
	{
		return m_debugText.ToString();
	}
}

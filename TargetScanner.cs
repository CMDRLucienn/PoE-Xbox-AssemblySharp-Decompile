using System.Collections.Generic;
using AI.Plan;
using UnityEngine;

public class TargetScanner
{
	protected AttackBase m_attackToUse;

	protected bool m_perceptionTriggered;

	protected bool m_scriptEventSearched;

	protected ScriptEvent m_scriptEvent;

	private static List<GameObject> s_potentialTargets = new List<GameObject>();

	protected const float DISTANCE_WEIGHT = 2f;

	protected const float HEALTH_WEIGHT = 0.5f;

	protected const float CURRENT_TARGET_WEIGHT = 3f;

	protected const float PREFERS_MELEE_RANGE = 8f;

	protected const float PREFERS_RANGED_RANGE = 1f;

	private bool m_prevPerceptState;

	public AttackBase AttackToUse
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

	public virtual void Reset()
	{
		m_attackToUse = null;
		m_scriptEvent = null;
		m_perceptionTriggered = false;
		m_scriptEventSearched = false;
	}

	public void ClearPerceptionState()
	{
		m_prevPerceptState = false;
	}

	public virtual bool ScanForTarget(GameObject owner, AIController aiController, float overrideRange, bool ignoreIfCurrentTarget)
	{
		UpdatePerception(owner);
		GetPotentialTargets(owner, aiController, overrideRange, s_potentialTargets);
		if (s_potentialTargets.Count > 0)
		{
			GameObject gameObject = ScanForTargetToAttack(s_potentialTargets, owner, aiController);
			if (gameObject != null)
			{
				if (ignoreIfCurrentTarget && gameObject == aiController.CurrentTarget)
				{
					s_potentialTargets.Clear();
					return false;
				}
				Equipment component = owner.GetComponent<Equipment>();
				if (component != null && component.IsWeaponSetValid(1) && PartyMemberAI.IsInPartyList(owner.GetComponent<PartyMemberAI>()))
				{
					AIPackageController aIPackageController = aiController as AIPackageController;
					if (aIPackageController == null || aIPackageController.InstructionSet == null || aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.UsePrimary)
					{
						component.SelectWeaponSet(1, enforceRecoveryPenalty: false);
					}
				}
				ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
				aiController.StateManager.PushState(approachTarget);
				approachTarget.TargetScanner = this;
				approachTarget.Target = gameObject;
				AudioBank component2 = owner.GetComponent<AudioBank>();
				if ((bool)component2)
				{
					component2.PlayFrom("Combat");
				}
				GameEventArgs obj = new GameEventArgs
				{
					Type = GameEventType.RequestHelp,
					GameObjectData = new GameObject[3]
				};
				obj.GameObjectData[0] = owner;
				obj.GameObjectData[1] = gameObject;
				obj.GameObjectData[2] = owner;
				AIController.BroadcastToAllies(obj, owner, aiController.ShoutRange);
				s_potentialTargets.Clear();
				return true;
			}
			AIState currentState = aiController.StateManager.CurrentState;
			gameObject = ScanForTargetToInvestigate(s_potentialTargets, owner, aiController);
			s_potentialTargets.Clear();
			if (gameObject != null && !(currentState is Investigate))
			{
				Investigate investigate = AIStateManager.StatePool.Allocate<Investigate>();
				aiController.StateManager.PushState(investigate, clearStack: true);
				investigate.TargetPos = gameObject.transform.position;
				investigate.Attack = m_attackToUse;
				investigate.TargetScanner = this;
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.StealthSuspicious);
				return true;
			}
		}
		return false;
	}

	protected GameObject ScanForTargetToAttack(List<GameObject> potentialTargets, GameObject owner, AIController aiController)
	{
		float num = float.MaxValue;
		GameObject result = null;
		Faction component = owner.GetComponent<Faction>();
		if (component == null)
		{
			Debug.LogError(owner.name + " doesn't have a faction.", owner);
			return null;
		}
		float num2 = aiController.PerceptionDistance;
		if (aiController.InCombat)
		{
			num2 += 6f;
		}
		Transform transform = owner.transform;
		for (int i = 0; i < potentialTargets.Count; i++)
		{
			GameObject gameObject = potentialTargets[i];
			if (!component.IsHostile(gameObject))
			{
				continue;
			}
			Health component2 = gameObject.GetComponent<Health>();
			if (component2 == null || !component2.Targetable)
			{
				continue;
			}
			AIController component3 = gameObject.GetComponent<AIController>();
			if (component3 == null)
			{
				continue;
			}
			CharacterStats component4 = gameObject.GetComponent<CharacterStats>();
			if (component4 == null)
			{
				continue;
			}
			float num3 = GameUtilities.V3Distance2D(transform.position, gameObject.transform.position);
			float num4 = num2;
			if (component4.NoiseLevelRadius > num3)
			{
				result = gameObject;
				Stealth.SetInStealthMode(gameObject, inStealth: false);
			}
			if (!Stealth.IsInStealthMode(gameObject) && !(num3 > num4) && GameUtilities.LineofSight(transform.position, component3.transform.position, 1f, includeDynamics: false))
			{
				float num5 = 0f;
				num5 += num3 / num4 * 2f;
				num5 += component2.CurrentStamina / component2.MaxStamina * 0.5f;
				num5 += (aiController.HasEngaged(gameObject) ? (-3f) : 0f);
				if (num5 < num && !Stealth.IsInStealthMode(gameObject))
				{
					num = num5;
					result = gameObject;
				}
			}
		}
		return result;
	}

	protected GameObject ScanForTargetToInvestigate(List<GameObject> potentialTargets, GameObject owner, AIController aiController)
	{
		float num = float.MinValue;
		GameObject result = null;
		Faction component = owner.GetComponent<Faction>();
		if (component == null)
		{
			Debug.LogError(owner.name + " doesn't have a faction.", owner);
			return null;
		}
		for (int i = 0; i < potentialTargets.Count; i++)
		{
			GameObject gameObject = potentialTargets[i];
			if (gameObject == null)
			{
				continue;
			}
			Health component2 = gameObject.GetComponent<Health>();
			if (component2 == null || !component2.Targetable || gameObject.GetComponent<CharacterStats>() == null)
			{
				continue;
			}
			Stealth stealthComponent = Stealth.GetStealthComponent(gameObject.gameObject);
			if (stealthComponent == null || gameObject.GetComponent<Mover>() == null || !Stealth.IsInStealthMode(gameObject))
			{
				continue;
			}
			float num2 = GameUtilities.V3SqrDistance2D(owner.transform.position, gameObject.transform.position);
			float stealthedCharacterSuspicionDistance = aiController.StealthedCharacterSuspicionDistance;
			if (num2 < stealthedCharacterSuspicionDistance * stealthedCharacterSuspicionDistance && stealthComponent.GetSuspicion(owner) > 100f && num < stealthComponent.GetSuspicion(owner))
			{
				if (component.IsHostile(gameObject))
				{
					result = gameObject;
				}
				num = stealthComponent.GetSuspicion(owner);
			}
		}
		return result;
	}

	public void GetPotentialTargets(GameObject owner, AIController aiController, float overrideRange, List<GameObject> potentialTargets)
	{
		GetPotentialTargets(owner, aiController, overrideRange, potentialTargets, mustBeStealthed: false);
	}

	public void GetPotentialTargets(GameObject owner, AIController aiController, float overrideRange, List<GameObject> potentialTargets, bool mustBeStealthed)
	{
		Faction component = owner.GetComponent<Faction>();
		if (component == null)
		{
			Debug.LogError(owner.name + " doesn't have a faction.", owner);
			return;
		}
		bool flag = owner.GetComponent<PartyMemberAI>() != null && component.IsInPlayerFaction;
		float num = aiController.PerceptionDistance * 2f;
		if (overrideRange > 0f)
		{
			num = overrideRange;
		}
		float num2 = num * num;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if ((mustBeStealthed && !Stealth.IsInStealthMode(faction.gameObject)) || (!faction.IsInPlayerFaction && component.GetRelationship(faction) != Faction.Relationship.Hostile))
			{
				continue;
			}
			AIController component2 = faction.GetComponent<AIController>();
			if (component2 == null || (flag && (bool)faction.GetComponent<PartyMemberAI>()) || component.IsFriendly(faction) || component2.IsPet || component2.IsInvisible)
			{
				continue;
			}
			float num3 = GameUtilities.V3SqrDistance2D(aiController.RetreatPosition, faction.gameObject.transform.position);
			if (aiController.IgnoreKiteTargets && (num3 >= aiController.KiteDistanceFromStartPositionSq || (aiController.Mover != null && component2.Mover != null && aiController.Mover.DesiredSpeed <= component2.Mover.DesiredSpeed)))
			{
				continue;
			}
			float num4 = GameUtilities.V3SqrDistance2D(owner.transform.position, faction.gameObject.transform.position);
			if (aiController.IsTethered() && num3 > aiController.GetTetherDistanceSq() && num4 < 9f)
			{
				continue;
			}
			CharacterStats component3 = faction.GetComponent<CharacterStats>();
			if (!(component3 == null))
			{
				float num5 = component3.NoiseLevelRadius * 2f;
				num5 *= num5;
				if ((num4 < num2 || num4 < num5) && GameUtilities.LineofSight(owner.transform.position, component2.gameObject, 1f))
				{
					potentialTargets.Add(component2.gameObject);
				}
			}
		}
	}

	public virtual void SelectAttack(GameObject owner, GameObject originalTarget, AIController aiController, bool isForceAttack, out GameObject finalTarget)
	{
		finalTarget = originalTarget;
		SelectWeaponSetBasedOnTargetDistance(aiController, owner, finalTarget);
		m_attackToUse = GetNextAttackFromEquipment(owner);
	}

	protected AttackBase GetNextAttackFromEquipment(GameObject owner)
	{
		AttackBase attackToUse = m_attackToUse;
		Equipment component = owner.GetComponent<Equipment>();
		AttackBase primaryAttack = component.PrimaryAttack;
		AttackBase secondaryAttack = component.SecondaryAttack;
		if (primaryAttack == null && secondaryAttack == null)
		{
			return null;
		}
		if (primaryAttack != null && secondaryAttack == null)
		{
			return primaryAttack;
		}
		if (primaryAttack == null && secondaryAttack != null)
		{
			return secondaryAttack;
		}
		if (attackToUse != primaryAttack)
		{
			return primaryAttack;
		}
		return secondaryAttack;
	}

	public void SelectWeaponSetBasedOnTargetDistance(AIController aiController, GameObject owner, GameObject target)
	{
		AIPackageController aIPackageController = aiController as AIPackageController;
		if (aIPackageController == null || aIPackageController.InstructionSet == null || aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.UsePrimary)
		{
			return;
		}
		Equipment component = owner.GetComponent<Equipment>();
		if (component == null || !component.IsWeaponSetValid(0) || !component.IsWeaponSetValid(1))
		{
			return;
		}
		int num = component.SelectedWeaponSetSerialized;
		int num2 = 0;
		AttackBase attackBase = null;
		AttackBase attackBase2 = null;
		AttackBase attackBase3 = null;
		AttackBase attackBase4 = null;
		if (component.IsWeaponSetValid(2))
		{
			if (num == 0)
			{
				component.SelectWeaponSet(1, enforceRecoveryPenalty: false);
				num = 1;
			}
			if (num == 1)
			{
				attackBase = component.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
				attackBase2 = component.GetPrimaryWeaponFromWeaponSet(2).GetComponent<AttackBase>();
				num2 = 2;
			}
			else
			{
				attackBase = component.GetPrimaryWeaponFromWeaponSet(2).GetComponent<AttackBase>();
				attackBase2 = component.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
				num2 = 1;
			}
		}
		else if (num == 0)
		{
			attackBase = component.GetPrimaryWeaponFromWeaponSet(0).GetComponent<AttackBase>();
			attackBase2 = component.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
			num2 = 1;
		}
		else
		{
			attackBase = component.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
			attackBase2 = component.GetPrimaryWeaponFromWeaponSet(0).GetComponent<AttackBase>();
			num2 = 0;
		}
		if (attackBase == null || attackBase2 == null)
		{
			return;
		}
		if (attackBase is AttackMelee)
		{
			attackBase3 = attackBase;
			if (attackBase2 is AttackRanged || attackBase2 is AttackFirearm)
			{
				attackBase4 = attackBase2;
			}
		}
		else if (attackBase is AttackRanged || attackBase is AttackFirearm)
		{
			attackBase4 = attackBase;
			if (attackBase2 is AttackMelee)
			{
				attackBase3 = attackBase2;
			}
		}
		if (attackBase3 == null || attackBase4 == null)
		{
			return;
		}
		float num3 = GameUtilities.ObjectDistance2D(owner, target);
		if (aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.PrefersMelee)
		{
			if (num3 > 8f)
			{
				if (attackBase != attackBase4)
				{
					component.SelectWeaponSet(num2, enforceRecoveryPenalty: true);
				}
			}
			else if (attackBase != attackBase3)
			{
				component.SelectWeaponSet(num2, enforceRecoveryPenalty: false);
			}
		}
		else if (aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.PrefersRanged)
		{
			bool flag = false;
			AIController aIController = GameUtilities.FindActiveAIController(target);
			if (aIController != null)
			{
				flag = aIController.IsMoving();
			}
			if (num3 > 1f || flag)
			{
				if (attackBase != attackBase4)
				{
					component.SelectWeaponSet(num2, enforceRecoveryPenalty: true);
				}
			}
			else if (attackBase != attackBase3)
			{
				component.SelectWeaponSet(num2, enforceRecoveryPenalty: false);
			}
		}
		else
		{
			if (aIPackageController.InstructionSet.WeaponPreference != SpellList.WeaponPreferenceType.PrefersRangedIfWeaponLoaded)
			{
				return;
			}
			AttackFirearm attackFirearm = attackBase4 as AttackFirearm;
			if (attackFirearm == null || !attackFirearm.RequiresReload)
			{
				if (attackBase != attackBase4)
				{
					component.SelectWeaponSet(num2, enforceRecoveryPenalty: true);
				}
			}
			else if (attackBase != attackBase3)
			{
				component.SelectWeaponSet(num2, enforceRecoveryPenalty: false);
			}
		}
	}

	public AttackBase GetLongestWeaponAttack(AIController aiController, GameObject owner)
	{
		AIPackageController aIPackageController = aiController as AIPackageController;
		if (aIPackageController == null || aIPackageController.InstructionSet == null)
		{
			return GetNextAttackFromEquipment(owner);
		}
		if (aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.UsePrimary)
		{
			return GetNextAttackFromEquipment(owner);
		}
		Equipment component = owner.GetComponent<Equipment>();
		if (component == null)
		{
			return GetNextAttackFromEquipment(owner);
		}
		AttackBase attackBase = null;
		float num = -1f;
		if (component.IsWeaponSetValid(0))
		{
			AttackBase component2 = component.GetPrimaryWeaponFromWeaponSet(0).GetComponent<AttackBase>();
			if (component2 != null)
			{
				attackBase = component2;
				num = component2.AttackDistance;
			}
		}
		if (component.IsWeaponSetValid(1))
		{
			AttackBase component3 = component.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
			if (component3 != null && component3.AttackDistance > num)
			{
				attackBase = component3;
				num = component3.AttackDistance;
			}
		}
		if (attackBase != null)
		{
			return attackBase;
		}
		return GetNextAttackFromEquipment(owner);
	}

	public virtual bool HasAvailableAttack(GameObject owner)
	{
		return true;
	}

	public void UpdatePerception(GameObject owner)
	{
		if (!m_scriptEventSearched)
		{
			m_scriptEvent = owner.GetComponent<ScriptEvent>();
			if (m_scriptEvent != null && !m_scriptEvent.HasScriptType(ScriptEvent.ScriptEvents.OnPerceptionPerLoad) && !m_scriptEvent.HasScriptType(ScriptEvent.ScriptEvents.OnPerception))
			{
				m_scriptEvent = null;
			}
			m_scriptEventSearched = true;
		}
		if (!(m_scriptEvent != null))
		{
			return;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			Stealth stealthComponent = Stealth.GetStealthComponent(onlyPrimaryPartyMember.gameObject);
			if ((bool)stealthComponent && stealthComponent.IsInStealthMode())
			{
				if (stealthComponent.GetSuspicion(owner) >= 200f)
				{
					ExecuteAllOnPercept();
					return;
				}
				continue;
			}
			float perceptionDistance = onlyPrimaryPartyMember.PerceptionDistance;
			if (GameUtilities.V3SqrDistance2D(owner.transform.position, onlyPrimaryPartyMember.gameObject.transform.position) < perceptionDistance * perceptionDistance && GameUtilities.LineofSight(owner.transform.position, onlyPrimaryPartyMember.gameObject, 1f))
			{
				ExecuteAllOnPercept();
				return;
			}
		}
		m_prevPerceptState = false;
	}

	public void ExecuteAllOnPercept()
	{
		if (!m_prevPerceptState && m_scriptEvent != null)
		{
			m_prevPerceptState = true;
			m_scriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPerception);
			OnPerceptionOncePerLoad();
		}
	}

	private void OnPerceptionOncePerLoad()
	{
		if (!m_perceptionTriggered)
		{
			m_scriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPerceptionPerLoad);
			m_perceptionTriggered = true;
		}
	}

	public virtual string GetDebugText()
	{
		return string.Empty;
	}
}

using AI.Achievement;
using UnityEngine;

namespace AI.Plan;

public class ApproachTarget : GameAIState
{
	private enum AttackState
	{
		NoSet,
		NotIssued,
		Issued
	}

	protected AttackBase m_attackToUse;

	protected AttackBase m_currentAttack;

	protected TargetScanner m_targetScanner;

	protected GameObject m_target;

	protected Mover m_targetMover;

	protected bool m_isForceAttack;

	private bool m_hasBeenUpdated;

	private AttackState m_attackState;

	private Faction m_targetFaction;

	private Team m_targetTeam;

	private Team m_originalTeam;

	protected GameObject m_originalTarget;

	private bool m_inCombat;

	public override bool InCombat
	{
		get
		{
			if (m_inCombat)
			{
				return true;
			}
			if (m_target != null && m_target.GetComponent<PartyMemberAI>() != null)
			{
				return true;
			}
			return false;
		}
	}

	public override AttackBase CurrentAttack => m_currentAttack;

	public AttackBase Attack
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

	public GameObject Target
	{
		get
		{
			return m_target;
		}
		set
		{
			m_target = value;
		}
	}

	public bool IsForceAttack
	{
		get
		{
			return m_isForceAttack;
		}
		set
		{
			m_isForceAttack = value;
		}
	}

	public TargetScanner TargetScanner
	{
		get
		{
			return m_targetScanner;
		}
		set
		{
			m_targetScanner = value;
		}
	}

	public override GameObject CurrentTarget => m_target;

	public override void Reset()
	{
		base.Reset();
		m_attackToUse = null;
		m_currentAttack = null;
		m_targetScanner = null;
		m_target = null;
		m_isForceAttack = false;
		m_hasBeenUpdated = false;
		m_attackState = AttackState.NoSet;
		m_originalTarget = null;
		m_targetMover = null;
		m_targetFaction = null;
		m_targetTeam = null;
		m_originalTeam = null;
		m_inCombat = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_ai.UpdateMustDieForCombatToEnd();
		m_hasBeenUpdated = false;
		m_inCombat = false;
		StopMover();
		if (m_originalTarget == null)
		{
			m_originalTarget = m_target;
		}
		Equipment component = m_owner.GetComponent<Equipment>();
		if (component != null && component.IsWeaponSetValid(1) && !PartyMemberAI.IsInPartyList(m_ai as PartyMemberAI))
		{
			bool enforceRecoveryPenalty = true;
			if (component.CurrentItems != null)
			{
				enforceRecoveryPenalty = component.CurrentItems.PrimaryWeapon != null;
			}
			AIPackageController aIPackageController = m_ai as AIPackageController;
			if (aIPackageController == null || aIPackageController.InstructionSet == null || aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.UsePrimary)
			{
				component.SelectWeaponSet(1, enforceRecoveryPenalty);
			}
		}
		if (m_isForceAttack && m_target != null && !m_ai.IsTargetable(m_target))
		{
			m_isForceAttack = false;
		}
		if (m_attackToUse != null && m_attackToUse.HasForcedTarget)
		{
			base.Manager.PopCurrentState();
			Attack attack = PushState<Attack>();
			attack.Parameters.Attack = m_attackToUse;
			attack.Parameters.TargetObject = m_attackToUse.ForcedTarget;
			attack.Parameters.ShouldAttackObject = attack.Parameters.TargetObject != null;
		}
		if (m_attackToUse == null || m_attackState == AttackState.Issued)
		{
			if (m_targetScanner != null && (m_attackState == AttackState.NoSet || m_attackState == AttackState.Issued))
			{
				GameObject finalTarget = null;
				m_targetScanner.SelectAttack(m_owner, m_target, m_ai, m_isForceAttack, out finalTarget);
				if (finalTarget != null && m_targetScanner.AttackToUse != null)
				{
					m_target = finalTarget;
					m_currentAttack = m_targetScanner.AttackToUse;
					m_attackState = AttackState.NotIssued;
				}
				else
				{
					m_currentAttack = null;
					m_attackState = AttackState.NoSet;
				}
			}
		}
		else
		{
			m_currentAttack = m_attackToUse;
		}
		if (m_target != null)
		{
			m_targetMover = m_target.GetComponent<Mover>();
		}
		if (m_targetFaction == null && m_target != null)
		{
			m_targetFaction = m_target.GetComponent<Faction>();
			if (m_targetFaction != null)
			{
				m_targetTeam = m_targetFaction.CurrentTeamInstance;
			}
		}
		Faction component2 = m_owner.GetComponent<Faction>();
		if (component2 != null)
		{
			m_originalTeam = component2.CurrentTeamInstance;
		}
	}

	public override void OnExit()
	{
		m_inCombat = false;
		base.OnExit();
	}

	public override void Update()
	{
		if (m_ai == null)
		{
			Debug.LogError("AI update run without OnEnter being run first!");
			return;
		}
		base.Update();
		m_hasBeenUpdated = true;
		if (m_ai.BeingKited())
		{
			m_ai.StateManager.PopCurrentState();
			m_ai.StopKiting();
			return;
		}
		if (m_isForceAttack && m_target != null && !m_ai.IsTargetable(m_target))
		{
			m_isForceAttack = false;
		}
		if (m_currentAttack == null)
		{
			if (m_targetScanner == null)
			{
				base.Manager.PopCurrentState();
				return;
			}
			if (!m_targetScanner.HasAvailableAttack(m_owner))
			{
				m_ai.FaceTarget(m_targetScanner.AttackToUse);
				return;
			}
			GameObject finalTarget = null;
			m_targetScanner.SelectAttack(m_owner, m_target, m_ai, m_isForceAttack, out finalTarget);
			if (!(finalTarget != null) || !(m_targetScanner.AttackToUse != null))
			{
				base.Manager.PopCurrentState();
				return;
			}
			m_target = finalTarget;
			m_currentAttack = m_targetScanner.AttackToUse;
			m_attackState = AttackState.NotIssued;
			m_targetMover = finalTarget.GetComponent<Mover>();
			Faction component = m_target.GetComponent<Faction>();
			if (component != null)
			{
				m_targetTeam = component.CurrentTeamInstance;
			}
		}
		if (m_target == null || !m_ai.IsTargetable(m_target) || m_currentAttack == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		AttackFirearm attackFirearm = m_currentAttack as AttackFirearm;
		if (attackFirearm != null && attackFirearm.BaseIsReady() && attackFirearm.RequiresReload)
		{
			PushState<ReloadWeapon>().Setup(attackFirearm);
			return;
		}
		m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
		if (m_targetTeam != null && m_targetFaction.CurrentTeamInstance != m_targetTeam)
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_originalTeam != null)
		{
			Faction component2 = m_owner.GetComponent<Faction>();
			if (component2 != null && component2.CurrentTeamInstance != m_originalTeam)
			{
				base.Manager.PopCurrentState();
				return;
			}
		}
		float magnitude = (m_target.transform.position - m_owner.transform.position).magnitude;
		float num = m_ai.Mover.Radius + m_targetMover.Radius;
		magnitude -= num;
		float num2 = m_currentAttack.TotalAttackDistance;
		if (magnitude < num2 && magnitude > 11f && m_currentAttack is AttackRanged && (bool)FogOfWar.Instance && !FogOfWar.Instance.RangedAttackerVisible(m_ai.transform.position))
		{
			num2 = 11f;
		}
		if (magnitude > num2 || (m_currentAttack is AttackRanged && !LineOfSightToTarget(m_target)))
		{
			if (m_ai.EngagedBy.Count > 0 && !m_ai.EngagedBy.Contains(m_target))
			{
				foreach (GameObject item in m_ai.EngagedBy)
				{
					if (item != null)
					{
						m_target = item;
						m_targetMover = m_target.GetComponent<Mover>();
						return;
					}
				}
			}
			m_ai.CancelAllEngagements();
			if (m_currentAttack is AttackMelee)
			{
				AIPackageController aIPackageController = m_ai as AIPackageController;
				if (aIPackageController != null && aIPackageController.InstructionSet != null && aIPackageController.InstructionSet.WeaponPreference == SpellList.WeaponPreferenceType.PrefersRanged)
				{
					Equipment component3 = m_owner.GetComponent<Equipment>();
					if (component3 != null)
					{
						if (component3.IsWeaponSetValid(0))
						{
							AttackBase component4 = component3.GetPrimaryWeaponFromWeaponSet(0).GetComponent<AttackBase>();
							if (component4 is AttackRanged || component4 is AttackFirearm)
							{
								m_ai.StateManager.PopCurrentState();
								return;
							}
						}
						if (component3.IsWeaponSetValid(1))
						{
							AttackBase component5 = component3.GetPrimaryWeaponFromWeaponSet(1).GetComponent<AttackBase>();
							if (component5 is AttackRanged || component5 is AttackFirearm)
							{
								m_ai.StateManager.PopCurrentState();
								return;
							}
						}
						if (component3.IsWeaponSetValid(2))
						{
							AttackBase component6 = component3.GetPrimaryWeaponFromWeaponSet(2).GetComponent<AttackBase>();
							if (component6 is AttackRanged || component6 is AttackFirearm)
							{
								m_ai.StateManager.PopCurrentState();
								return;
							}
						}
					}
				}
			}
			if (m_ai.Mover.GetRunSpeed() <= 0f || m_ai.Mover.Frozen)
			{
				m_ai.StateManager.PopCurrentState();
				return;
			}
			PathToPosition pathToPosition = PushState<PathToPosition>();
			pathToPosition.Parameters.Target = m_target;
			pathToPosition.Parameters.StopOnLOS = true;
			pathToPosition.Parameters.Range = num2;
			pathToPosition.Parameters.MovementType = AnimationController.MovementType.Run;
			pathToPosition.Parameters.TargetScanner = m_targetScanner;
			pathToPosition.ParentState = this;
			bool flag = m_currentAttack is AttackRanged;
			AttackMelee attackMelee = m_currentAttack as AttackMelee;
			if (flag)
			{
				pathToPosition.Parameters.Range -= 0.8f;
			}
			else
			{
				pathToPosition.Parameters.Range -= 0.2f;
			}
			pathToPosition.Parameters.GetAsCloseAsPossible = !flag && (attackMelee == null || attackMelee.TotalAttackDistance < 0.01f);
			pathToPosition.Parameters.DesiresMaxRange = flag;
			return;
		}
		m_inCombat = true;
		if (!m_currentAttack.IsReady())
		{
			m_ai.FaceTarget(m_currentAttack);
			return;
		}
		AttackBase attackBase = m_currentAttack;
		StatusEffect[] array = null;
		GenericAbility component7 = m_currentAttack.gameObject.GetComponent<GenericAbility>();
		if ((bool)component7)
		{
			if (component7.UsePrimaryAttack)
			{
				Equipment component8 = m_owner.GetComponent<Equipment>();
				if (component8 != null && m_stats != null)
				{
					AttackBase primaryAttack = component8.PrimaryAttack;
					if (primaryAttack != null)
					{
						if (!primaryAttack.IsReady())
						{
							attackFirearm = primaryAttack as AttackFirearm;
							if (attackFirearm != null && attackFirearm.RequiresReload)
							{
								PushState<ReloadWeapon>().Setup(attackFirearm);
							}
							m_ai.FaceTarget(primaryAttack);
							return;
						}
						attackBase = primaryAttack;
						array = new StatusEffect[1];
						StatusEffectParams statusEffectParams = new StatusEffectParams();
						statusEffectParams.IsHostile = false;
						statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.ApplyAttackEffects;
						statusEffectParams.AttackPrefab = m_currentAttack;
						statusEffectParams.OneHitUse = true;
						GenericAbility.AbilityType abType = GenericAbility.AbilityType.Ability;
						if (component7 is GenericSpell)
						{
							abType = GenericAbility.AbilityType.Spell;
						}
						array[0] = StatusEffect.Create(m_owner, statusEffectParams, abType, null, deleteOnClear: true);
					}
				}
			}
			else
			{
				component7.Activate(m_owner);
			}
		}
		bool flag2 = false;
		if (!IsHostile(m_target))
		{
			if (m_originalTarget == null || !m_ai.IsTargetable(m_originalTarget))
			{
				base.Manager.PopCurrentState();
			}
			else
			{
				flag2 = true;
			}
		}
		Attack attack = PushState<Attack>();
		attack.Parameters.Attack = attackBase;
		attack.Parameters.TargetObject = m_target;
		attack.Parameters.EffectsOnLaunch = array;
		AttackAOE attackAOE = attackBase as AttackAOE;
		if (attackAOE != null && attackAOE.DamageAngleDegrees > 0f && attackAOE.DamageAngleDegrees < 360f)
		{
			Vector3 vector;
			if (m_target != null)
			{
				vector = m_target.transform.position - m_ai.gameObject.transform.position;
				vector.y = 0f;
				vector.Normalize();
			}
			else
			{
				vector = m_ai.gameObject.transform.forward;
			}
			attack.Parameters.TargetObject = null;
			attack.Parameters.Location = m_ai.gameObject.transform.position + vector * 0.1f;
		}
		attack.Parameters.ShouldAttackObject = attack.Parameters.TargetObject != null;
		m_attackState = AttackState.Issued;
		if (flag2)
		{
			m_attackToUse = null;
			m_target = m_originalTarget;
		}
	}

	public override string GetDebugText()
	{
		string text = ":";
		if (m_target != null)
		{
			text = text + " Current Target: " + m_target.ToString();
		}
		if (m_currentAttack != null)
		{
			text = text + " Current Attack: " + m_currentAttack.ToString();
		}
		if (m_targetScanner != null)
		{
			string debugText = m_targetScanner.GetDebugText();
			if (!string.IsNullOrEmpty(debugText))
			{
				text = text + " State: " + debugText;
			}
		}
		return text;
	}

	public override bool AllowBlockedMovement()
	{
		return m_hasBeenUpdated;
	}
}

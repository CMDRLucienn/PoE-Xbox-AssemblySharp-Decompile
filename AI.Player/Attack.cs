using AI.Achievement;
using UnityEngine;

namespace AI.Player;

public class Attack : PlayerState
{
	private AttackBase m_attackToUse;

	private AttackBase m_weaponAttack;

	private GameObject m_target;

	private Team m_targetTeam;

	private Mover m_targetMover;

	private StatusEffect[] m_effectsOnLaunch;

	private GenericAbility m_ability;

	private bool m_inCombat;

	private bool m_switchHands;

	private bool m_isStealthAttack;

	private bool m_queueWeaponSetChange;

	private bool m_isAutoAttack;

	private bool m_issuedSelfCast;

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

	public bool IsAutoAttack
	{
		get
		{
			return m_isAutoAttack;
		}
		set
		{
			m_isAutoAttack = value;
		}
	}

	public AttackBase WeaponAttack
	{
		get
		{
			return m_weaponAttack;
		}
		set
		{
			m_weaponAttack = value;
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

	public Mover TargetMover
	{
		get
		{
			return m_targetMover;
		}
		set
		{
			m_targetMover = value;
		}
	}

	public Team TargetTeam
	{
		get
		{
			return m_targetTeam;
		}
		set
		{
			m_targetTeam = value;
		}
	}

	public StatusEffect[] EffectsOnLaunch
	{
		get
		{
			return m_effectsOnLaunch;
		}
		set
		{
			m_effectsOnLaunch = value;
		}
	}

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

	public override AttackBase CurrentAttack => m_attackToUse;

	public override bool InCombat => m_inCombat;

	public override GameObject CurrentTarget => m_target;

	public override void Reset()
	{
		base.Reset();
		m_attackToUse = null;
		m_weaponAttack = null;
		m_target = null;
		m_targetTeam = null;
		m_targetMover = null;
		m_effectsOnLaunch = null;
		m_ability = null;
		m_inCombat = false;
		m_switchHands = false;
		m_isStealthAttack = false;
		m_queueWeaponSetChange = false;
		m_isAutoAttack = false;
		m_issuedSelfCast = false;
	}

	public void QueueWeaponSetChange()
	{
		m_queueWeaponSetChange = true;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_inCombat = false;
		m_partyMemberAI.DestinationCircleState = null;
		m_partyMemberAI.HideDestination();
		if (Stealth.IsInStealthMode(m_owner))
		{
			m_isStealthAttack = true;
		}
		if (m_target != null)
		{
			m_targetMover = m_target.GetComponent<Mover>();
			if (m_targetTeam == null)
			{
				Faction component = m_target.GetComponent<Faction>();
				if (component != null)
				{
					m_targetTeam = component.CurrentTeamInstance;
				}
			}
		}
		m_animation.ClearReactions();
		m_animation.ClearActions();
		m_animation.DesiredAction.Reset();
		m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
		m_animation.Loop = true;
	}

	public override void OnExit()
	{
		m_inCombat = false;
		PartyMemberAI partyMemberAI = m_ai as PartyMemberAI;
		if (partyMemberAI != null && !m_isAutoAttack)
		{
			partyMemberAI.QueuedAbility = null;
		}
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
		if (m_queueWeaponSetChange)
		{
			m_attackToUse = null;
			m_weaponAttack = null;
			m_targetTeam = null;
			m_targetMover = null;
			m_inCombat = false;
			m_switchHands = false;
			m_isStealthAttack = false;
			m_queueWeaponSetChange = false;
			OnEnter();
			return;
		}
		AIController.AggressionType autoAttackAggression = m_ai.GetAutoAttackAggression();
		if (!CanAttackTarget() && (autoAttackAggression == AIController.AggressionType.Passive || !m_partyMemberAI.AutoPickNearbyEnemy(this)))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_attackToUse == null || m_switchHands)
		{
			SetupAttack();
			m_switchHands = false;
			if (m_issuedSelfCast)
			{
				m_issuedSelfCast = false;
				return;
			}
			if (m_ai.StateManager.CurrentState is Ability)
			{
				return;
			}
		}
		m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
		AttackFirearm attackFirearm = m_attackToUse as AttackFirearm;
		if (attackFirearm != null && attackFirearm.BaseIsReady() && attackFirearm.RequiresReload)
		{
			PushState<ReloadWeapon>().AttackToUse = attackFirearm;
			return;
		}
		if (OutOfCharges(m_attackToUse))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_isAutoAttack && !GameState.InCombat)
		{
			base.Manager.PopCurrentState();
			return;
		}
		float magnitude = (m_target.transform.position - m_owner.transform.position).magnitude;
		float num = m_ai.Mover.Radius + m_targetMover.Radius;
		magnitude -= num;
		if (m_attackToUse == null)
		{
			return;
		}
		if (magnitude > m_attackToUse.TotalAttackDistance || (m_attackToUse is AttackRanged && !LineOfSightToTarget(m_target)))
		{
			if (m_isAutoAttack && m_ai.EngagedBy.Count > 0)
			{
				if (autoAttackAggression == AIController.AggressionType.Passive || !m_partyMemberAI.AutoPickNearbyEnemy(this))
				{
					base.Manager.PopCurrentState();
				}
				return;
			}
			if (m_target == null)
			{
				base.Manager.PopCurrentState();
				return;
			}
			float num2 = m_attackToUse.TotalAttackDistance;
			if (num2 > 4f)
			{
				num2 -= 1f;
			}
			else if (num2 > 1f)
			{
				num2 -= 0.5f;
			}
			else if (num2 > 0.4f)
			{
				num2 -= 0.1f;
			}
			PathToPosition pathToPosition = PushState<PathToPosition>();
			pathToPosition.Parameters.Target = m_target;
			pathToPosition.Parameters.StopOnLOS = true;
			pathToPosition.Parameters.Range = num2;
			pathToPosition.ParentState = this;
			if (m_attackToUse is AttackMelee && num2 > 1f && num2 < 2f)
			{
				pathToPosition.Parameters.IgnoreObstaclesWithinRange = true;
			}
			pathToPosition.Parameters.PopOnEnterIfTargetInvalid = true;
			bool flag = m_attackToUse is AttackRanged;
			AttackMelee attackMelee = m_attackToUse as AttackMelee;
			pathToPosition.Parameters.GetAsCloseAsPossible = !flag && (attackMelee == null || attackMelee.TotalAttackDistance < 0.01f);
			pathToPosition.Parameters.DesiresMaxRange = flag;
			return;
		}
		if (m_target != null && !m_attackToUse.HasForcedTarget && !m_attackToUse.IsValidPrimaryTarget(m_target))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_target != null)
		{
			Faction component = m_target.GetComponent<Faction>();
			if (component != null && (m_faction.IsHostile(component) || component.IsHostile(m_faction)))
			{
				m_inCombat = GameState.InCombat;
			}
		}
		else
		{
			m_inCombat = GameState.InCombat;
		}
		if (!m_attackToUse.IsReady())
		{
			m_ai.FaceTarget(m_attackToUse);
			return;
		}
		if ((bool)m_attackToUse.ForcedTarget && !m_isAutoAttack)
		{
			base.Manager.PopCurrentState();
		}
		if (m_isStealthAttack)
		{
			m_attackToUse.IsStealthAttack = m_isStealthAttack;
		}
		bool flag2 = false;
		CharacterStats component2 = ComponentUtils.GetComponent<CharacterStats>(m_owner);
		if ((bool)component2)
		{
			flag2 = component2.IsInvisible;
		}
		m_isStealthAttack = Stealth.IsInStealthMode(m_owner) || flag2;
		AI.Achievement.Attack attack = PushState<AI.Achievement.Attack>();
		attack.Parameters.Attack = m_attackToUse;
		attack.Parameters.TargetObject = m_target;
		attack.Parameters.EffectsOnLaunch = m_effectsOnLaunch;
		attack.Parameters.WeaponAttack = m_weaponAttack;
		attack.Parameters.Ability = GetAbility();
		attack.Parameters.ShouldAttackObject = attack.Parameters.TargetObject != null;
		m_switchHands = true;
	}

	public bool CanAttackTarget()
	{
		if (m_target == null || !m_ai.IsTargetable(m_target, m_attackToUse))
		{
			return false;
		}
		Faction component = m_target.GetComponent<Faction>();
		if (component != null)
		{
			return component.CurrentTeamInstance == m_targetTeam;
		}
		return true;
	}

	private GenericAbility GetAbility()
	{
		if (m_ability != null)
		{
			return m_ability;
		}
		if (m_attackToUse != null)
		{
			return m_attackToUse.gameObject.GetComponent<GenericAbility>();
		}
		return null;
	}

	protected virtual bool SetupAttack()
	{
		if (m_partyMemberAI.UseInstructionSet && m_partyMemberAI.InstructionSet != null)
		{
			GameObject finalTarget = null;
			m_partyMemberAI.CasterTargetScanner.SelectAttack(m_owner, m_target, m_partyMemberAI, isForceAttack: false, out finalTarget);
			GenericAbility selectedAbility = m_partyMemberAI.CasterTargetScanner.SelectedAbility;
			bool flag = true;
			if (!IsAutoAttack && finalTarget != m_target)
			{
				if (finalTarget == m_owner)
				{
					m_issuedSelfCast = true;
				}
				else
				{
					flag = false;
				}
			}
			if (flag && finalTarget != null && (m_partyMemberAI.CasterTargetScanner.AttackToUse != null || (selectedAbility != null && (selectedAbility.UsePrimaryAttack || selectedAbility.UseFullAttack))))
			{
				if (selectedAbility != null && (selectedAbility.UsePrimaryAttack || selectedAbility.UseFullAttack))
				{
					Equipment component = m_owner.GetComponent<Equipment>();
					if (component != null)
					{
						AttackBase primaryAttack = component.PrimaryAttack;
						if (primaryAttack != null)
						{
							GenericAbility component2 = selectedAbility.gameObject.GetComponent<GenericAbility>();
							AttackBase component3 = selectedAbility.gameObject.GetComponent<AttackBase>();
							if (selectedAbility.Attack != null)
							{
								StatusEffect[] array = new StatusEffect[1];
								StatusEffectParams statusEffectParams = new StatusEffectParams();
								statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.ApplyAttackEffects;
								statusEffectParams.AttackPrefab = selectedAbility.Attack;
								statusEffectParams.OneHitUse = true;
								statusEffectParams.IsHostile = false;
								GenericAbility.AbilityType abType = GenericAbility.AbilityType.Ability;
								if (selectedAbility is GenericSpell)
								{
									abType = GenericAbility.AbilityType.Spell;
								}
								array[0] = StatusEffect.Create(m_owner, selectedAbility, statusEffectParams, abType, null, deleteOnClear: true);
								if (selectedAbility.Attack.UseAttackVariationOnFullOrPrimary && (selectedAbility.UsePrimaryAttack || selectedAbility.UseFullAttack))
								{
									UpdateAttack(selectedAbility, primaryAttack, finalTarget, selectedAbility.UseFullAttack, component2, array, component3, selectedAbility.Attack.AttackVariation);
								}
								else
								{
									UpdateAttack(selectedAbility, primaryAttack, finalTarget, selectedAbility.UseFullAttack, component2, array, component3, -1);
								}
								return true;
							}
							UpdateAttack(selectedAbility, primaryAttack, finalTarget, selectedAbility.UseFullAttack, component2, null, component3, -1);
							return true;
						}
					}
				}
				m_attackToUse = m_partyMemberAI.CasterTargetScanner.AttackToUse;
				m_target = finalTarget;
				m_targetMover = finalTarget.GetComponent<Mover>();
				if (m_target != null)
				{
					Faction component4 = m_target.GetComponent<Faction>();
					if (component4 != null)
					{
						m_targetTeam = component4.CurrentTeamInstance;
					}
				}
				return true;
			}
		}
		AttackBase primaryAttack2 = m_partyMemberAI.GetPrimaryAttack();
		AttackBase secondaryAttack = m_partyMemberAI.GetSecondaryAttack();
		AttackMelee attackMelee = primaryAttack2 as AttackMelee;
		AttackMelee attackMelee2 = secondaryAttack as AttackMelee;
		if (m_attackToUse != null && m_attackToUse != primaryAttack2 && m_attackToUse != secondaryAttack)
		{
			return true;
		}
		if (primaryAttack2 == null && secondaryAttack == null)
		{
			return false;
		}
		if ((primaryAttack2 != null && secondaryAttack == null) || (attackMelee != null && !attackMelee.Unarmed && attackMelee2 != null && attackMelee2.Unarmed))
		{
			m_attackToUse = primaryAttack2;
		}
		else if (primaryAttack2 == null && secondaryAttack != null)
		{
			m_attackToUse = secondaryAttack;
		}
		else if (m_attackToUse != primaryAttack2)
		{
			m_attackToUse = primaryAttack2;
		}
		else
		{
			m_attackToUse = secondaryAttack;
		}
		return m_attackToUse != null;
	}

	private void UpdateAttack(GenericAbility ability, AttackBase attack, GameObject target, bool useFullAttack, GenericAbility weaponAbility, StatusEffect[] effectsOnLaunch, AttackBase weaponAttack, int animVariation)
	{
		if (!m_issuedSelfCast)
		{
			m_ai.StateManager.PopCurrentState();
		}
		TargetedAttack targetedAttack = AIStateManager.StatePool.Allocate<TargetedAttack>();
		m_ai.StateManager.PushState(targetedAttack);
		targetedAttack.AttackToUse = attack;
		targetedAttack.WeaponAttack = weaponAttack;
		targetedAttack.Target = target;
		targetedAttack.FullAttack = useFullAttack;
		targetedAttack.StatusEffects = effectsOnLaunch;
		targetedAttack.AnimVariation = animVariation;
		targetedAttack.Ability = ability;
		PartyMemberAI partyMemberAI = m_ai as PartyMemberAI;
		if (partyMemberAI != null && !m_isAutoAttack)
		{
			partyMemberAI.QueuedAbility = ability;
		}
	}

	public void ToggleSwitchHands()
	{
		m_switchHands = !m_switchHands;
	}

	public override string GetDebugText()
	{
		string text = ":";
		if (m_target != null)
		{
			text = text + " Target: " + m_target.ToString();
		}
		if (m_attackToUse != null)
		{
			text = text + " Attack: " + m_attackToUse.ToString();
		}
		return text;
	}
}

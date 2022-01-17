using AI.Achievement;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Player;

public class TargetedAttack : PlayerState
{
	private static NavMeshPath s_path = new NavMeshPath();

	private AttackBase m_attackToUse;

	private AttackBase m_weaponAttack;

	private Vector3 m_targetPos = Vector3.zero;

	private Vector3 m_forward = Vector3.zero;

	private GameObject m_target;

	private bool m_performingFullAttack;

	private bool m_emitFromCaster;

	private StatusEffect[] m_statusEffects;

	private int m_animVariation = -1;

	private GenericAbility m_ability;

	private Consumable m_consumable;

	private bool m_inCombat;

	private bool m_isStealthAttack;

	private bool m_isAutoAttack;

	public override AttackBase CurrentAttack => m_attackToUse;

	public override GenericAbility CurrentAbility
	{
		get
		{
			GenericAbility ability = GetAbility();
			if ((bool)ability)
			{
				return ability;
			}
			return base.CurrentAbility;
		}
	}

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

	public Vector3 TargetPos
	{
		get
		{
			return m_targetPos;
		}
		set
		{
			m_targetPos = value;
		}
	}

	public Vector3 Forward
	{
		get
		{
			return m_forward;
		}
		set
		{
			m_forward = value;
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

	public bool FullAttack
	{
		get
		{
			return m_performingFullAttack;
		}
		set
		{
			m_performingFullAttack = value;
		}
	}

	public bool EmitFromCaster
	{
		get
		{
			return m_emitFromCaster;
		}
		set
		{
			m_emitFromCaster = value;
		}
	}

	public StatusEffect[] StatusEffects
	{
		get
		{
			return m_statusEffects;
		}
		set
		{
			m_statusEffects = value;
		}
	}

	public int AnimVariation
	{
		get
		{
			return m_animVariation;
		}
		set
		{
			m_animVariation = value;
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

	public override bool InCombat
	{
		get
		{
			if (m_target != null)
			{
				Faction component = m_owner.GetComponent<Faction>();
				if (component != null && component.IsHostile(m_target))
				{
					return m_inCombat;
				}
			}
			return false;
		}
	}

	public override GameObject CurrentTarget => m_target;

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

	public override void Reset()
	{
		base.Reset();
		m_attackToUse = null;
		m_weaponAttack = null;
		m_targetPos = Vector3.zero;
		m_forward = Vector3.zero;
		m_target = null;
		m_performingFullAttack = false;
		m_emitFromCaster = false;
		m_statusEffects = null;
		m_animVariation = -1;
		m_ability = null;
		m_consumable = null;
		m_inCombat = false;
		m_isStealthAttack = false;
		m_isAutoAttack = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_inCombat = false;
		if (m_attackToUse != null)
		{
			m_attackToUse.BeginTargeting();
		}
		if (m_ability != null)
		{
			m_consumable = m_ability.gameObject.GetComponent<Consumable>();
		}
		if (Stealth.IsInStealthMode(m_owner))
		{
			m_isStealthAttack = true;
		}
		StopMover();
		m_ai.CancelAllEngagements();
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
			return;
		}
		base.Update();
		if (m_isStealthAttack)
		{
			m_attackToUse.IsStealthAttack = m_isStealthAttack;
		}
		m_isStealthAttack = Stealth.IsInStealthMode(m_owner);
		CharacterStats component = m_owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (OutOfCharges(m_attackToUse))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_consumable != null && !m_consumable.CanUse(component))
		{
			base.Manager.PopCurrentState();
			return;
		}
		Vector3 vector = m_targetPos;
		float num = 0f;
		Mover mover = null;
		if (m_target == null && (object)m_target != null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_target != null)
		{
			if (!m_ai.IsTargetable(m_target, m_attackToUse))
			{
				base.Manager.PopCurrentState();
				return;
			}
			vector = m_target.transform.position;
			mover = m_target.GetComponent<Mover>();
			if (mover != null)
			{
				num += mover.Radius;
			}
		}
		AttackFirearm attackFirearm = m_attackToUse as AttackFirearm;
		if (attackFirearm != null)
		{
			if (!attackFirearm.BaseIsReady())
			{
				return;
			}
			if (attackFirearm.RequiresReload)
			{
				PushState<ReloadWeapon>().AttackToUse = attackFirearm;
				return;
			}
		}
		Mover mover2 = m_ai.Mover;
		Transform transform = m_owner.transform;
		if (mover2 != null)
		{
			num += mover2.Radius;
		}
		float num2 = m_attackToUse.TotalAttackDistance + num;
		float num3 = GameUtilities.V3SqrDistance2D(transform.position, vector);
		if (m_attackToUse.PathsToPos && (num3 > num2 * num2 || !GameUtilities.LineofSight(transform.position, vector, 1f, includeDynamics: false)))
		{
			float num4 = m_attackToUse.TotalAttackDistance;
			if (num4 > 4f)
			{
				num4 -= 1f;
			}
			else if (num4 > 1f)
			{
				num4 -= 0.5f;
			}
			else if (num4 > 0.4f)
			{
				num4 -= 0.1f;
			}
			if (NavMesh.CalculatePath(transform.position, vector, int.MaxValue, s_path) && s_path.status != 0)
			{
				base.Manager.PopCurrentState();
				return;
			}
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			pathToPosition.Parameters.Range = num4;
			pathToPosition.Parameters.Destination = vector;
			pathToPosition.Parameters.LineOfSight = true;
			pathToPosition.ParentState = this;
			if (m_target != null)
			{
				pathToPosition.Parameters.Target = m_target;
			}
			base.Manager.PushState(pathToPosition);
			return;
		}
		m_inCombat = true;
		if (m_forward.sqrMagnitude > float.Epsilon)
		{
			m_ai.FaceDirection(m_forward);
		}
		if (!m_attackToUse.IsReady())
		{
			return;
		}
		if (m_emitFromCaster)
		{
			vector = transform.position;
			if (m_target != null)
			{
				m_forward = m_target.transform.position - transform.position;
				m_forward.y = 0f;
				m_forward.Normalize();
			}
		}
		if (m_forward.sqrMagnitude > float.Epsilon)
		{
			transform.rotation = Quaternion.LookRotation(m_forward);
		}
		m_partyMemberAI.StateManager.PopCurrentState();
		if (m_partyMemberAI.StateManager.IsExecutingDefaultState() && m_target != null)
		{
			Faction component2 = m_owner.GetComponent<Faction>();
			Faction component3 = m_target.GetComponent<Faction>();
			if ((component3 != null && component3.IsHostile(component2)) || (component2 != null && component2.IsHostile(component3)))
			{
				AttackBase primaryAttack = m_partyMemberAI.GetPrimaryAttack();
				bool flag = primaryAttack is AttackRanged;
				if (!flag)
				{
					AttackMelee attackMelee = primaryAttack as AttackMelee;
					if (attackMelee != null)
					{
						float num5 = attackMelee.TotalAttackDistance + num;
						flag = num3 <= num5 * num5;
					}
				}
				else
				{
					float num6 = primaryAttack.TotalAttackDistance + num;
					if (num3 > num6 * num6)
					{
						flag = false;
					}
				}
				if (flag)
				{
					Attack attack = PushState<Attack>();
					attack.IsAutoAttack = true;
					attack.Target = m_target;
				}
			}
		}
		if (m_target != null && !m_attackToUse.IsValidPrimaryTarget(m_target))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_performingFullAttack)
		{
			AttackBase primaryAttack2 = m_partyMemberAI.GetPrimaryAttack();
			AttackBase secondaryAttack = m_partyMemberAI.GetSecondaryAttack();
			if (primaryAttack2 != null)
			{
				AI.Achievement.Attack attack2 = PushState<AI.Achievement.Attack>();
				attack2.Parameters.TargetObject = m_target;
				attack2.Parameters.Location = vector;
				attack2.Parameters.SecondaryAttack = secondaryAttack;
				attack2.Parameters.EffectsOnLaunch = m_statusEffects;
				attack2.Parameters.WeaponAttack = m_weaponAttack;
				attack2.Parameters.AnimVariation = m_animVariation;
				attack2.Parameters.Ability = GetAbility();
				if (m_emitFromCaster)
				{
					attack2.Parameters.TargetObject = null;
				}
				if (secondaryAttack != null)
				{
					attack2.Parameters.Attack = secondaryAttack;
					attack2.Parameters.SecondaryAttack = primaryAttack2;
				}
				else
				{
					attack2.Parameters.Attack = primaryAttack2;
					attack2.Parameters.SecondaryAttack = null;
				}
				attack2.Parameters.ShouldAttackObject = attack2.Parameters.TargetObject != null;
			}
		}
		else
		{
			SetupAttack();
			AI.Achievement.Attack attack3 = PushState<AI.Achievement.Attack>();
			attack3.Parameters.Attack = m_attackToUse;
			attack3.Parameters.TargetObject = m_target;
			attack3.Parameters.Location = vector;
			attack3.Parameters.EffectsOnLaunch = m_statusEffects;
			attack3.Parameters.WeaponAttack = m_weaponAttack;
			attack3.Parameters.AnimVariation = m_animVariation;
			attack3.Parameters.Ability = GetAbility();
			if (m_emitFromCaster)
			{
				attack3.Parameters.TargetObject = null;
			}
			attack3.Parameters.ShouldAttackObject = attack3.Parameters.TargetObject != null;
		}
		m_attackToUse.TargetingStopped();
	}

	private GenericAbility GetAbility()
	{
		if (m_ability != null)
		{
			return m_ability;
		}
		if (!m_performingFullAttack && m_attackToUse != null)
		{
			return m_attackToUse.gameObject.GetComponent<GenericAbility>();
		}
		return null;
	}

	protected virtual bool SetupAttack()
	{
		if (m_attackToUse != null)
		{
			return true;
		}
		AttackBase primaryAttack = m_partyMemberAI.GetPrimaryAttack();
		AttackBase secondaryAttack = m_partyMemberAI.GetSecondaryAttack();
		if (m_attackToUse != primaryAttack && m_attackToUse != secondaryAttack)
		{
			return true;
		}
		if (primaryAttack != null && secondaryAttack == null)
		{
			m_attackToUse = primaryAttack;
		}
		else if (primaryAttack == null && secondaryAttack != null)
		{
			m_attackToUse = secondaryAttack;
		}
		else if (primaryAttack.IsReady())
		{
			m_attackToUse = primaryAttack;
		}
		else if (secondaryAttack.IsReady())
		{
			m_attackToUse = secondaryAttack;
		}
		else
		{
			m_attackToUse = primaryAttack;
		}
		return true;
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

	public override bool IsTargetingPosition()
	{
		return m_target == null;
	}

	public override Vector3 GetTargetedPosition()
	{
		if (m_forward.sqrMagnitude > float.Epsilon)
		{
			return m_owner.transform.position + m_forward;
		}
		return m_targetPos;
	}
}

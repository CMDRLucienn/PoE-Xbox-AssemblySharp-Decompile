using AI.Player;
using UnityEngine;

namespace AI.Achievement;

public class Attack : GameAIState
{
	public class Params
	{
		public GameObject TargetObject;

		public Vector3 Location = Vector3.zero;

		public AttackBase Attack;

		public AttackBase SecondaryAttack;

		public AttackBase WeaponAttack;

		public StatusEffect[] EffectsOnLaunch;

		public int AnimVariation = -1;

		public GenericAbility Ability;

		public bool ForceLoop;

		public bool CheckAnimErrors = true;

		public bool Invulnerable;

		public bool ShouldAttackObject;
	}

	public const float OUTTRO_LENGTH = 0.733f;

	public const float MIN_REACH_WEAPON_LENGTH = 0.01f;

	private Params m_params = new Params();

	private bool m_attackLaunched;

	private bool m_relaunchAttack;

	private bool m_secondaryAttackTriggered;

	private Vector3 m_targetPosition = Vector3.zero;

	private bool m_waitForTransition;

	private bool m_attackInterrupted;

	private bool m_inCombat;

	private float m_totalTime;

	private float m_animDuration;

	private bool m_animStarted;

	private AttackBase m_currentAttack;

	private bool m_abilityTriggered;

	private bool m_attackCompleteCalled;

	private bool m_inWeaponChange;

	private AnimationController.ActionType m_attackAction;

	public bool InWeaponChange
	{
		get
		{
			return m_inWeaponChange;
		}
		set
		{
			m_inWeaponChange = value;
		}
	}

	public override bool CanCancel
	{
		get
		{
			if (m_inWeaponChange)
			{
				return true;
			}
			if (CanUserInterrupt())
			{
				return true;
			}
			return false;
		}
	}

	public override bool InCombat
	{
		get
		{
			GameObject currentTarget = CurrentTarget;
			if (currentTarget != null && (currentTarget == m_owner || IsHostile(currentTarget)))
			{
				return m_inCombat;
			}
			return false;
		}
	}

	public override int Priority
	{
		get
		{
			if (m_params.Invulnerable)
			{
				return 12;
			}
			if (CanUserInterrupt())
			{
				return 0;
			}
			return 2;
		}
	}

	public Params Parameters
	{
		get
		{
			return m_params;
		}
		set
		{
			m_params = value;
		}
	}

	public override GameObject CurrentTarget => m_params.TargetObject;

	public override AttackBase CurrentAttack => m_params.Attack;

	public override bool IsPerformingSecondPartOfFullAttack => m_secondaryAttackTriggered;

	public override void Reset()
	{
		base.Reset();
		m_params.TargetObject = null;
		m_params.Location = Vector3.zero;
		m_params.Attack = null;
		m_params.SecondaryAttack = null;
		m_params.WeaponAttack = null;
		m_params.EffectsOnLaunch = null;
		m_params.AnimVariation = -1;
		m_params.Ability = null;
		m_params.ForceLoop = false;
		m_params.CheckAnimErrors = true;
		m_params.Invulnerable = false;
		m_params.ShouldAttackObject = false;
		m_attackLaunched = false;
		m_relaunchAttack = false;
		m_secondaryAttackTriggered = false;
		m_targetPosition = Vector3.zero;
		m_waitForTransition = false;
		m_attackInterrupted = false;
		m_inCombat = false;
		m_totalTime = 0f;
		m_animDuration = 0f;
		m_animStarted = false;
		m_currentAttack = null;
		m_abilityTriggered = false;
		m_attackCompleteCalled = false;
		m_inWeaponChange = false;
		m_attackAction = AnimationController.ActionType.None;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_inCombat = false;
		if (m_relaunchAttack)
		{
			m_attackLaunched = false;
			m_relaunchAttack = false;
			m_attackInterrupted = false;
		}
		if (m_attackLaunched)
		{
			AttackComplete();
		}
		else if ((bool)m_params.Attack)
		{
			if ((bool)m_params.TargetObject)
			{
				m_targetPosition = m_params.TargetObject.transform.position;
			}
			else
			{
				m_targetPosition = m_params.Location;
			}
			m_currentAttack = m_params.Attack;
			if (m_currentAttack != null)
			{
				m_currentAttack.SkipAnimation = false;
			}
			m_totalTime = 0f;
			m_animDuration = 0f;
			m_animStarted = false;
			m_animation.ClearReactions();
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
			m_animation.ClearInterrupt();
			m_animation.SetReaction(AnimationController.ReactionType.None);
			m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
			StopMover();
		}
		else
		{
			base.Manager.PopCurrentState();
		}
	}

	public override void OnExit()
	{
		if (m_attackLaunched && !m_attackCompleteCalled)
		{
			InterruptAttack();
		}
		m_animation.ClearActions();
		if (m_currentAttack != null)
		{
			AnimationController component = m_currentAttack.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.ClearActions();
			}
		}
		DeactivateAbility();
		m_inCombat = false;
		base.OnExit();
	}

	public override void OnCancel()
	{
		if (m_currentAttack != null)
		{
			if (m_params.Ability != null && m_attackLaunched && !m_attackCompleteCalled && m_params.Ability.Applied && m_currentAttack.CanCancel)
			{
				m_params.Ability.RestoreCooldown();
			}
			if (m_currentAttack.CanCancel)
			{
				m_relaunchAttack = true;
			}
			m_currentAttack.Cancel();
			m_attackInterrupted = true;
			m_animation.Loop = false;
			m_animation.Instant = false;
			DeactivateStatusEffects();
			if (!m_attackLaunched && m_ai.StateManager.QueuedState is AI.Player.Attack attack)
			{
				attack.ToggleSwitchHands();
			}
		}
		DeactivateAbility();
		InterruptAudio();
	}

	public override void OnAbort()
	{
		InterruptAttack();
		m_animation.ClearActions();
		if (m_currentAttack != null)
		{
			AnimationController component = m_currentAttack.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.ClearActions();
			}
			m_currentAttack.NotifyAttackComplete(0f);
		}
		base.OnAbort();
	}

	public override void OnEvent(GameEventArgs args)
	{
		base.OnEvent(args);
		GameEventType type = args.Type;
		if ((uint)(type - 8) <= 1u)
		{
			InterruptAttack();
		}
	}

	public override void Interrupt()
	{
		InterruptAttack();
	}

	public void InterruptAttack()
	{
		if (m_currentAttack != null)
		{
			m_currentAttack.Interrupt();
			m_animation.Interrupt();
			m_animation.Instant = false;
			m_attackInterrupted = true;
			DeactivateStatusEffects();
		}
		DeactivateAbility();
		InterruptAudio();
	}

	private void InterruptAudio()
	{
		PartyMemberAI partyMemberAI = m_ai as PartyMemberAI;
		if (partyMemberAI != null && partyMemberAI.SoundSet != null)
		{
			partyMemberAI.SoundSet.InterruptAudio();
		}
	}

	protected void DeactivateStatusEffects()
	{
		if (m_params != null && m_params.EffectsOnLaunch != null)
		{
			StatusEffect[] effectsOnLaunch = m_params.EffectsOnLaunch;
			for (int i = 0; i < effectsOnLaunch.Length; i++)
			{
				effectsOnLaunch[i]?.DeactivateAttackPrefabAbility(base.Owner);
			}
		}
	}

	private void DeactivateAbility()
	{
		if (m_params.Ability != null && m_abilityTriggered && !m_params.Ability.Modal)
		{
			if (m_params.Ability.UseFullAttack || m_params.Ability.UsePrimaryAttack)
			{
				m_params.Ability.Deactivate(base.Owner);
			}
			m_abilityTriggered = false;
		}
	}

	public override void Update()
	{
		base.Update();
		if (m_attackInterrupted)
		{
			if (!m_animation.IsPerformingAction(AnimationController.ActionType.Attack, GetAnimVariation()))
			{
				base.Manager.PopCurrentState();
			}
			return;
		}
		if (m_ai.EngagementsEnabled())
		{
			m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
		}
		if (!m_attackLaunched)
		{
			if (m_params.TargetObject == null && m_params.ShouldAttackObject)
			{
				m_ai.StateManager.PopCurrentState();
			}
			else if (m_params.TargetObject != null && !m_currentAttack.HasForcedTarget && !m_currentAttack.IsValidPrimaryTarget(m_params.TargetObject))
			{
				base.Manager.PopCurrentState();
			}
			else
			{
				if (m_currentAttack.RecoveryTimer > 0f)
				{
					return;
				}
				if (!m_currentAttack.IsReady())
				{
					base.Manager.PopCurrentState();
					return;
				}
				Transform transform = m_owner.transform;
				Vector3 vector = m_targetPosition - transform.position;
				vector.y = 0f;
				if (vector.sqrMagnitude < float.Epsilon)
				{
					vector = transform.forward;
				}
				if (m_currentAttack.TargetAngle > 0f)
				{
					vector = Quaternion.Euler(0f, m_currentAttack.TargetAngle, 0f) * vector;
				}
				vector.Normalize();
				if (Vector3.Dot(transform.forward, vector) > 0.95f)
				{
					if (m_currentAttack.AbilityOrigin != null)
					{
						m_currentAttack.AbilityOrigin.PlayVocalization();
					}
					if (m_params.EffectsOnLaunch != null)
					{
						CharacterStats component = m_owner.GetComponent<CharacterStats>();
						if (component != null)
						{
							StatusEffect[] effectsOnLaunch = m_params.EffectsOnLaunch;
							foreach (StatusEffect statusEffect in effectsOnLaunch)
							{
								if (statusEffect != null)
								{
									component.ApplyStatusEffectImmediate(statusEffect);
								}
							}
						}
					}
					if (m_params.WeaponAttack != null)
					{
						m_params.WeaponAttack.LaunchOnStartVisualEffect();
					}
					if (m_params.Ability != null)
					{
						if (!m_abilityTriggered)
						{
							if (m_params.Ability.UseFullAttack || m_params.Ability.UsePrimaryAttack)
							{
								m_params.Ability.Activate(base.Owner);
							}
							m_abilityTriggered = true;
						}
						else if (m_secondaryAttackTriggered && m_params.Ability.UseFullAttack)
						{
							m_params.Ability.ActivateOneHitStatusEffects();
						}
					}
					if (m_params.SecondaryAttack != null && m_secondaryAttackTriggered && m_params.AnimVariation >= 0)
					{
						m_currentAttack.SkipAnimation = true;
					}
					m_currentAttack.TriggeringAbility = Parameters.Ability;
					if (m_params.TargetObject != null && m_currentAttack.RequiresHitObject)
					{
						m_currentAttack.Launch(m_params.TargetObject, m_params.AnimVariation);
					}
					else
					{
						m_currentAttack.Launch(m_targetPosition, null, m_params.AnimVariation);
					}
					if (m_currentAttack.SkipAnimation)
					{
						AttackComplete();
					}
					m_attackLaunched = true;
					m_totalTime = 0f;
					m_animDuration = 0f;
					m_animStarted = false;
					m_animation.Loop = true;
					m_attackAction = m_animation.DesiredAction.m_actionType;
				}
				else
				{
					Vector3 worldPosition = transform.position + vector;
					worldPosition.y = transform.position.y;
					transform.LookAt(worldPosition);
				}
			}
			return;
		}
		if (m_currentAttack != null)
		{
			if (m_currentAttack.CanCancel || CurrentTarget == m_owner)
			{
				m_inCombat = GameState.InCombat;
			}
			else
			{
				m_inCombat = true;
			}
		}
		if (!m_animStarted)
		{
			if (m_animation.Idle || !m_animation.IsPerformingAction(m_animation.DesiredAction.m_actionType, m_animation.DesiredAction.m_variation))
			{
				if (m_animation.DesiredAction.m_actionType != m_attackAction || m_attackAction == AnimationController.ActionType.None)
				{
					m_ai.StateManager.PopCurrentState();
				}
				return;
			}
			m_animStarted = true;
			if (m_currentAttack.AttackSpeed == AttackBase.AttackSpeedType.Instant || m_currentAttack.gameObject.GetComponent<GenericSpell>() == null)
			{
				m_animDuration = m_animation.CurrentAvatar.GetCurrentAnimatorStateInfo(0).length;
			}
			else if (m_currentAttack.AttackSpeed == AttackBase.AttackSpeedType.Short)
			{
				m_animDuration = AttackData.Instance.ShortAttackSpeed;
			}
			else if (m_currentAttack.AttackSpeed == AttackBase.AttackSpeedType.Long)
			{
				m_animDuration = AttackData.Instance.LongAttackSpeed;
			}
			if (m_animation.TimeScale > float.Epsilon)
			{
				m_animDuration /= m_animation.TimeScale;
			}
		}
		if (m_params.Invulnerable)
		{
			return;
		}
		m_totalTime += Time.deltaTime;
		if (m_totalTime + 0.733f >= m_currentAttack.AttackSpeedTime * (1f / m_animation.TimeScale) && m_animation.Loop && !m_params.ForceLoop)
		{
			m_animation.Loop = false;
		}
		else if (m_params.CheckAnimErrors && m_totalTime - 0.733f >= Mathf.Max(0.733f, m_currentAttack.AttackSpeedTime) * 10f)
		{
			UIDebug.Instance.LogOnScreenWarning("Failed to play attack " + m_currentAttack.name + " with variation " + m_currentAttack.AttackVariation.ToString("00") + ".", UIDebug.Department.Animation, 10f);
			Debug.LogError(m_currentAttack.name + " is stuck! This is attack" + m_currentAttack.AttackVariation.ToString("00") + " aborting animation!");
			m_animation.ClearActions();
			m_animation.Interrupt();
			m_totalTime = 1f;
			m_animDuration = m_totalTime;
			AnimationController component2 = m_currentAttack.GetComponent<AnimationController>();
			if ((bool)component2)
			{
				component2.ClearActions();
			}
			AttackComplete();
			return;
		}
		m_ai.FaceTarget(m_currentAttack);
		if (m_animation.DesiredAction.m_actionType == AnimationController.ActionType.None || (m_animation.CurrentAction.m_actionType != 0 && m_animation.CurrentAction.m_actionType != AnimationController.ActionType.Pending && !m_animation.IsPerformingAction(m_animation.DesiredAction.m_actionType, m_animation.DesiredAction.m_variation)))
		{
			if (!m_animation.Idle)
			{
				m_animation.DesiredAction.Reset();
				m_waitForTransition = true;
				if (m_currentAttack != null)
				{
					AnimationController component3 = m_currentAttack.GetComponent<AnimationController>();
					if ((bool)component3)
					{
						component3.DesiredAction.Reset();
					}
				}
			}
			else
			{
				AttackComplete();
			}
		}
		else if (m_waitForTransition && m_animation.Idle)
		{
			AttackComplete();
		}
	}

	private void AttackComplete()
	{
		if (m_params.SecondaryAttack != null && !m_secondaryAttackTriggered)
		{
			if (m_currentAttack != null)
			{
				m_currentAttack.NotifyAttackComplete(0f);
			}
			m_attackLaunched = false;
			m_secondaryAttackTriggered = true;
			m_currentAttack = m_params.SecondaryAttack;
			m_waitForTransition = false;
			return;
		}
		m_attackCompleteCalled = true;
		DeactivateAbility();
		m_animation.Instant = false;
		m_animation.DesiredAction.Reset();
		if (m_currentAttack != null)
		{
			float totalTime = m_animDuration;
			if (m_attackInterrupted && !m_currentAttack.ImpactFrameHit)
			{
				totalTime = 0f;
			}
			if (GameState.InCombat && m_ai is PartyMemberAI)
			{
				GenericAbility genericAbility = m_currentAttack.TriggeringAbility ?? m_currentAttack.LastTriggeringAbility ?? m_currentAttack.AbilityOrigin;
				if ((bool)genericAbility && !genericAbility.Passive)
				{
					Equipment equipment = (m_ai ? m_ai.GetComponent<Equipment>() : null);
					if (!equipment || equipment.CurrentItems == null || !equipment.CurrentItems.SecondaryWeapon || equipment.CurrentItems.SecondaryWeapon.FindItemModSecondaryAttack() != m_currentAttack)
					{
						GameState.AutoPause(AutoPauseOptions.PauseEvent.PartyMemberCastFinished, m_ai.gameObject, m_ai.gameObject);
					}
				}
			}
			m_currentAttack.NotifyAttackComplete(totalTime);
		}
		m_animation.Loop = false;
		if (m_currentAttack != null)
		{
			AnimationController component = m_currentAttack.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.DesiredAction.Reset();
			}
		}
		base.Manager.PopCurrentState();
	}

	public void ForceAnimToEnd()
	{
		m_totalTime = 0.733f + m_currentAttack.AttackSpeedTime;
		m_animDuration = m_totalTime;
	}

	private void ResetAnimation()
	{
		m_animation.DesiredAction.Reset();
		m_animation.Loop = false;
		AnimationController component = m_currentAttack.GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.DesiredAction.Reset();
		}
	}

	private int GetAnimVariation()
	{
		if (m_params.SecondaryAttack != null && m_params.AnimVariation >= 0)
		{
			return m_params.AnimVariation;
		}
		if (m_currentAttack != null)
		{
			return m_currentAttack.AttackVariation;
		}
		return -1;
	}

	public bool CanUserInterrupt()
	{
		if (!m_attackLaunched || m_currentAttack == null)
		{
			return true;
		}
		return m_currentAttack.CanCancel;
	}

	public override string GetDebugText()
	{
		string text = ":";
		if (m_params.TargetObject != null)
		{
			text = text + " Target: " + m_params.TargetObject.ToString();
		}
		if (m_params.Attack != null)
		{
			text = text + " Attack: " + m_params.Attack.ToString();
		}
		return text;
	}
}

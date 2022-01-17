using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : IGameEventListener
{
	protected AIStateManager m_parent;

	protected AnimationController m_animation;

	protected AIController m_ai;

	protected GameObject m_owner;

	public GameObject Owner => m_owner;

	public virtual bool CanCancel => true;

	public AIStateManager Manager => m_parent;

	public virtual AttackBase CurrentAttack => null;

	public virtual GenericAbility CurrentAbility
	{
		get
		{
			if ((bool)CurrentAttack)
			{
				return CurrentAttack.AbilityOrigin;
			}
			return null;
		}
	}

	public virtual bool InCombat => false;

	public virtual int Priority => -1;

	public virtual GameObject CurrentTarget => null;

	public virtual bool UseQueuedTarget => false;

	public virtual bool AllowsQueueing => true;

	public virtual bool CanBeQueuedIfLowerPriority => true;

	public virtual bool IsPerformingSecondPartOfFullAttack => false;

	public virtual AIState ParentState
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual void Reset()
	{
		m_parent = null;
		m_animation = null;
		m_ai = null;
		m_owner = null;
	}

	public void Init(AIStateManager parent, GameObject owner)
	{
		m_parent = parent;
		m_owner = owner;
		m_animation = m_owner.GetComponent<AnimationController>();
		m_ai = parent.AIController;
	}

	public void StopMover()
	{
		if (m_ai != null)
		{
			Mover mover = m_ai.Mover;
			if (mover != null)
			{
				mover.Stop();
				mover.enabled = false;
			}
		}
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnCancel()
	{
	}

	public virtual void OnAbort()
	{
	}

	public virtual void Interrupt()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void OnGUI()
	{
	}

	public virtual string GetDebugText()
	{
		return string.Empty;
	}

	public void BaseEnter()
	{
		OnEnter();
	}

	public void BaseExit()
	{
		OnExit();
	}

	public void BaseCancel()
	{
		if (CanCancel)
		{
			OnCancel();
		}
	}

	public void BaseAbort()
	{
		OnAbort();
	}

	public void BaseUpdate()
	{
		Update();
	}

	public virtual void UpdateWhenPaused()
	{
		if (TurnWhilePaused() && m_ai != null)
		{
			m_ai.FaceTarget(CurrentAttack);
		}
	}

	public virtual bool IsTargetingPosition()
	{
		return false;
	}

	public virtual Vector3 GetTargetedPosition()
	{
		return Vector3.zero;
	}

	public virtual bool IsPathingObstacle()
	{
		return true;
	}

	public virtual bool AllowBlockedMovement()
	{
		return true;
	}

	public virtual bool CanBeNudgedBy(Mover pather)
	{
		return false;
	}

	public virtual bool IsMoving()
	{
		return false;
	}

	public virtual bool IsPathBlocked()
	{
		return false;
	}

	public virtual bool PerformsSoftSteering()
	{
		return false;
	}

	public virtual bool IsIdling()
	{
		return false;
	}

	public virtual bool TurnWhilePaused()
	{
		return true;
	}

	public virtual bool AllowEngagementUpdate()
	{
		return true;
	}

	public void Free()
	{
		Reset();
		AIStateManager.StatePool.Free(this);
	}

	public T PushState<T>() where T : AIState, new()
	{
		return PushState<T>(clearStack: false);
	}

	public T PushState<T>(bool clearStack) where T : AIState, new()
	{
		T val = AIStateManager.StatePool.Allocate<T>();
		Manager.PushState(val, clearStack);
		return val;
	}

	public Vector3 MeToEnemy()
	{
		GameObject currentTarget = m_ai.CurrentTarget;
		if (currentTarget == null)
		{
			return m_owner.transform.forward;
		}
		Vector3 result = currentTarget.transform.position - m_owner.transform.position;
		result.Normalize();
		return result;
	}

	public Vector3 EnemyToMe()
	{
		GameObject currentTarget = m_ai.CurrentTarget;
		if (currentTarget == null)
		{
			return m_owner.transform.forward;
		}
		Vector3 result = m_owner.transform.position - currentTarget.transform.position;
		result.Normalize();
		return result;
	}

	public bool LineOfSightToTarget(GameObject target)
	{
		return GameUtilities.LineofSight(m_owner.transform.position, target.transform.position, 1f, includeDynamics: false);
	}

	public List<GameObject> FriendsInRange(float range, float maxAngleDeg)
	{
		float num = range * range;
		List<GameObject> list = new List<GameObject>();
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (activeFactionComponent.gameObject == m_owner || activeFactionComponent.IsHostile(m_owner))
			{
				continue;
			}
			Vector3 to = activeFactionComponent.transform.position - m_owner.transform.position;
			if (!(to.sqrMagnitude > num))
			{
				to.Normalize();
				if (!(Vector3.Angle(m_owner.transform.forward, to) > maxAngleDeg))
				{
					list.Add(activeFactionComponent.gameObject);
				}
			}
		}
		return list;
	}

	protected virtual AttackBase PickAutoAttack()
	{
		Equipment component = m_owner.GetComponent<Equipment>();
		AttackBase attackBase = null;
		AttackBase attackBase2 = null;
		if (component.CurrentItems.PrimaryWeapon != null)
		{
			AttackBase[] components = component.CurrentItems.PrimaryWeapon.GetComponents<AttackBase>();
			if (components.Length != 0)
			{
				attackBase = components[0];
			}
		}
		if (component.CurrentItems.SecondaryWeapon != null)
		{
			AttackBase[] components2 = component.CurrentItems.SecondaryWeapon.GetComponents<AttackBase>();
			if (components2.Length != 0)
			{
				attackBase2 = components2[0];
			}
		}
		if (attackBase == null && attackBase2 == null)
		{
			return null;
		}
		if (attackBase != null && attackBase2 == null)
		{
			return attackBase;
		}
		if (attackBase == null && attackBase2 != null)
		{
			return attackBase2;
		}
		if (attackBase.IsReady())
		{
			return attackBase;
		}
		if (attackBase2.IsReady())
		{
			return attackBase2;
		}
		return attackBase;
	}

	public void InterruptAnimationForCutscene()
	{
		if (m_animation != null)
		{
			m_animation.SetReaction(AnimationController.ReactionType.None);
			m_animation.ClearReactions();
			if (!m_animation.Idle)
			{
				m_animation.Interrupt();
			}
			m_animation.Loop = false;
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
		}
	}

	protected void InitFidgetController(FidgetController fidget, TargetScanner targetScanner)
	{
		fidget.Enabled = false;
		if (m_animation == null)
		{
			m_animation = m_owner.GetComponent<AnimationController>();
		}
		if (m_animation != null && m_animation.FidgetCount > 0)
		{
			fidget.Init(this, m_animation, targetScanner);
			fidget.Enabled = true;
		}
	}

	public virtual void OnEvent(GameEventArgs args)
	{
	}
}

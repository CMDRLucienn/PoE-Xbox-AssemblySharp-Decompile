using UnityEngine;
using UnityEngine.AI;

namespace AI.Achievement;

public class PushedBack : GameAIState
{
	private const float DURATION = 2f;

	private const float MAX_SPEED = 30f;

	private float m_timer;

	private bool m_pushStarted;

	private bool m_reachedGoal;

	private Vector3 m_start = Vector3.zero;

	private Vector3 m_end = Vector3.zero;

	private Vector3 m_moveDir = Vector3.zero;

	private float m_speed;

	private Quaternion m_facing = Quaternion.identity;

	private bool m_inCombat = true;

	private KnockedDown m_knockedDownState;

	public bool InCombatOverride
	{
		get
		{
			return m_inCombat;
		}
		set
		{
			m_inCombat = value;
		}
	}

	public override bool InCombat => m_inCombat;

	public override int Priority => 8;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_timer = 0f;
		m_pushStarted = false;
		m_reachedGoal = false;
		m_start = Vector3.zero;
		m_end = Vector3.zero;
		m_moveDir = Vector3.zero;
		m_speed = 0f;
		m_facing = Quaternion.identity;
		m_inCombat = true;
		m_knockedDownState = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_pushStarted)
		{
			base.Manager.PopCurrentState();
			return;
		}
		m_pushStarted = true;
		m_timer = 2f;
		StopMover();
		m_ai.CancelAllEngagements();
		m_animation.ClearReactions();
		m_animation.DesiredAction.Reset();
		m_animation.SetReaction(AnimationController.ReactionType.Hit);
	}

	public void InitPush(GameObject attacker, Vector3 position, Vector3 direction, float distance, float speed, bool lockOrientation, bool orientBackwards)
	{
		m_start = position;
		m_end = m_start + direction * distance;
		if (speed < 30f)
		{
			m_speed = speed;
		}
		else
		{
			m_speed = 30f;
		}
		if (NavMesh.Raycast(m_start, m_end, out var hit, int.MaxValue))
		{
			m_end = hit.position;
		}
		m_moveDir = m_end - m_start;
		m_moveDir.Normalize();
		if (orientBackwards)
		{
			Vector3 forward = m_start - m_end;
			forward.y = 0f;
			forward.Normalize();
			m_facing = Quaternion.LookRotation(forward);
		}
		else if (!lockOrientation)
		{
			Vector3 forward2 = direction;
			forward2.y = 0f;
			forward2.Normalize();
			m_facing = Quaternion.LookRotation(forward2);
		}
		if (m_ai is PartyMemberAI && attacker != null)
		{
			PartyMemberAI component = attacker.GetComponent<PartyMemberAI>();
			if (component != null && component.gameObject.activeInHierarchy)
			{
				InCombatOverride = false;
			}
		}
	}

	public override void OnExit()
	{
	}

	public override void OnAbort()
	{
	}

	public override void Update()
	{
		if (m_timer > 0f)
		{
			Transform transform = m_owner.transform;
			m_timer -= Time.deltaTime;
			transform.rotation = m_facing;
			if (m_timer <= 0f)
			{
				m_animation.SetReaction(AnimationController.ReactionType.None);
			}
			else if (!m_reachedGoal)
			{
				Vector3 vector = transform.position + m_moveDir * m_speed * Time.deltaTime;
				Vector3 normalized = (m_end - vector).normalized;
				normalized.Normalize();
				if (Vector3.Dot(m_moveDir, normalized) < 0f)
				{
					m_reachedGoal = true;
					vector = m_end;
					m_speed = 0f;
					m_timer = 0f;
				}
				if (m_ai.Mover != null && !m_ai.Mover.PositionOverlaps(vector, m_ai.Mover, onlyCheckPathingObstacles: true))
				{
					transform.position = vector;
				}
			}
		}
		else if (m_animation.CurrentReaction != AnimationController.ReactionType.Pending && !m_animation.IsPerformingReaction(AnimationController.ReactionType.Hit))
		{
			m_animation.ClearReactions();
			base.Manager.PopCurrentState();
			if (m_knockedDownState != null)
			{
				base.Manager.PushState(m_knockedDownState);
			}
		}
	}

	public void SetKnockedDownState(KnockedDown knockedDown)
	{
		m_knockedDownState = knockedDown;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}

	public override bool AllowEngagementUpdate()
	{
		return false;
	}
}

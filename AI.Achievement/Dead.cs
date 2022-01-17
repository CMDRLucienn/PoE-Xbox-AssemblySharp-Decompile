using UnityEngine;

namespace AI.Achievement;

public class Dead : AIState
{
	public const float DECAY_TIME = 10f;

	private float m_decayTimer;

	private Health m_health;

	private float m_transitionDelay;

	public override int Priority => 11;

	public override bool AllowsQueueing => false;

	public override void Reset()
	{
		base.Reset();
		m_health = null;
		m_transitionDelay = 0f;
	}

	public override void OnEnter()
	{
		StopMover();
		m_ai.CancelAllEngagements();
		m_health = m_ai.GetComponent<Health>();
		Equipment component = m_ai.GetComponent<Equipment>();
		if ((bool)component && m_health.ShouldDecay)
		{
			Animator animator = null;
			if ((bool)component.PrimaryAttack)
			{
				animator = component.PrimaryAttack.GetComponent<Animator>();
			}
			if (animator != null && animator.gameObject != m_owner)
			{
				animator.enabled = false;
			}
			if ((bool)component.SecondaryAttack)
			{
				animator = component.SecondaryAttack.GetComponent<Animator>();
			}
			if (animator != null && animator.gameObject != m_owner)
			{
				animator.enabled = false;
			}
		}
		if (m_health.GetComponent<Collider>() != null)
		{
			m_health.GetComponent<Collider>().enabled = false;
		}
		if (m_health.GetComponent<Rigidbody>() != null)
		{
			m_health.GetComponent<Rigidbody>().detectCollisions = false;
		}
		AudioBank component2 = m_ai.GetComponent<AudioBank>();
		if (component2 != null)
		{
			component2.PlayFrom("Death");
		}
		if (m_health.ShouldDecay)
		{
			m_decayTimer = 10f;
			if (m_health.ShouldGib)
			{
				m_decayTimer = 0f;
			}
		}
		m_animation.SetReaction(AnimationController.ReactionType.Dead);
		m_transitionDelay = 0.075f;
	}

	public override void OnExit()
	{
	}

	public override void Update()
	{
		if (m_transitionDelay <= 0f && !m_animation.IsPerformingReaction(AnimationController.ReactionType.Dead))
		{
			m_animation.SetReaction(AnimationController.ReactionType.Dead);
			m_transitionDelay = 0.075f;
		}
		if (m_transitionDelay > 0f)
		{
			m_transitionDelay -= Time.deltaTime;
		}
		if (m_decayTimer > 0f)
		{
			m_decayTimer -= Time.deltaTime;
		}
		if (m_health.Dead && m_health.ShouldDecay && m_decayTimer <= 0f)
		{
			if ((bool)m_health && m_health.ShouldGib)
			{
				GameUtilities.Destroy(m_health.gameObject);
			}
			base.Manager.AbortStateStack();
		}
	}

	public override bool IsPathingObstacle()
	{
		return false;
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

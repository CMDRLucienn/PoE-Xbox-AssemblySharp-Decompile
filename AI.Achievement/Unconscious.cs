using UnityEngine;

namespace AI.Achievement;

public class Unconscious : GameAIState
{
	private const float UNCONSCIOUS_TIME = 3f;

	private float m_timer;

	private Health m_health;

	private float m_transitionDelay;

	public override int Priority => 10;

	public override bool AllowsQueueing => false;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override void Reset()
	{
		base.Reset();
		m_health = null;
		m_transitionDelay = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
		m_ai.CancelAllEngagements();
		m_health = m_ai.GetComponent<Health>();
		if (m_health.GetComponent<Collider>() != null)
		{
			m_health.GetComponent<Collider>().enabled = false;
		}
		if (m_health.GetComponent<Rigidbody>() != null)
		{
			m_health.GetComponent<Rigidbody>().detectCollisions = false;
		}
		AudioBank component = m_ai.GetComponent<AudioBank>();
		if (component != null)
		{
			component.PlayFrom("Death");
		}
		m_animation.SetReaction(AnimationController.ReactionType.Dead);
		m_transitionDelay = 0.075f;
		m_timer = 3f;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_animation.SetReaction(AnimationController.ReactionType.None);
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
		if (m_timer > 0f)
		{
			m_timer -= Time.deltaTime;
		}
		if (!(m_timer <= 0f) || GameState.InCombat || IsEnemyNearby())
		{
			return;
		}
		TriggerRevive();
		SharedStats component = m_ai.GetComponent<SharedStats>();
		if ((bool)component && component.SharedCharacter != null)
		{
			AIController component2 = component.SharedCharacter.GetComponent<AIController>();
			if ((bool)component2 && component2.StateManager.CurrentState is Unconscious unconscious)
			{
				unconscious.TriggerRevive();
			}
		}
	}

	private bool IsEnemyNearby()
	{
		Faction component = m_owner.GetComponent<Faction>();
		if (component == null)
		{
			return false;
		}
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (activeFactionComponent.IsInPlayerFaction || !activeFactionComponent.IsHostile(component))
			{
				continue;
			}
			AIController aIController = activeFactionComponent.GetComponent<AIController>();
			if (aIController == null)
			{
				continue;
			}
			if (!aIController.enabled)
			{
				aIController = null;
				AIController[] components = activeFactionComponent.GetComponents<AIController>();
				foreach (AIController aIController2 in components)
				{
					if (aIController2.enabled && aIController2.gameObject.activeInHierarchy)
					{
						aIController = aIController2;
						break;
					}
				}
				if (aIController == null)
				{
					continue;
				}
			}
			if (!aIController.IsPet)
			{
				float perceptionDistance = aIController.PerceptionDistance;
				if (GameUtilities.V3SqrDistance2D(m_owner.transform.position, activeFactionComponent.gameObject.transform.position) <= perceptionDistance * perceptionDistance && GameUtilities.LineofSight(m_owner.transform.position, aIController.gameObject, 1f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TriggerRevive()
	{
		base.Manager.PopCurrentState();
		m_health.OnRevive();
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

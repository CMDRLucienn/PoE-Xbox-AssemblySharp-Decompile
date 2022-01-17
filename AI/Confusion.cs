using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;
using UnityEngine.AI;

namespace AI;

public class Confusion : MonoBehaviour
{
	private const float TRANSITION_DURATION = 6f;

	private float m_duration;

	private AIController m_aiController;

	private float m_totalElapsedTime;

	private float m_transitionTimer;

	private bool m_initialized;

	private static List<GameObject> s_potentialTargets = new List<GameObject>();

	public float Duration
	{
		get
		{
			return m_duration;
		}
		set
		{
			m_duration = value;
		}
	}

	public AIController AIController
	{
		get
		{
			return m_aiController;
		}
		set
		{
			m_aiController = value;
		}
	}

	public float TimeRemaining
	{
		get
		{
			if (m_totalElapsedTime < m_duration)
			{
				return m_duration - m_totalElapsedTime;
			}
			return 0f;
		}
	}

	public void Start()
	{
		m_totalElapsedTime = 0f;
		m_transitionTimer = 0f;
		m_initialized = false;
	}

	public void Update()
	{
		if (GameState.Paused)
		{
			return;
		}
		if (m_aiController != null)
		{
			Health component = m_aiController.gameObject.GetComponent<Health>();
			if (component != null && (component.Dead || component.Unconscious))
			{
				return;
			}
		}
		if (m_aiController != null && m_transitionTimer <= 0f)
		{
			m_aiController.SafePopAllStates();
			m_aiController.RestoreFactionBeforeConfusion();
			if (m_initialized && !IsEnemyNearby())
			{
				m_totalElapsedTime = m_duration;
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Confused;
				gameEventArgs.IntData = new int[1];
				gameEventArgs.IntData[0] = 0;
				m_aiController.OnEvent(gameEventArgs);
				return;
			}
			m_initialized = true;
			switch (OEIRandom.Range(0, 3))
			{
			case 0:
				m_aiController.CancelAllEngagements();
				if (!SwapTeam())
				{
					Wait wait3 = AIStateManager.StatePool.Allocate<Wait>();
					wait3.Duration = 6f;
					wait3.InfiniteWait = false;
					m_aiController.StateManager.PushState(wait3);
				}
				break;
			case 1:
			{
				SwapTeam();
				PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				insideUnitCircle.Normalize();
				Vector3 vector = GameUtilities.V2ToV3(insideUnitCircle) * 6f;
				vector += m_aiController.gameObject.transform.position;
				if (NavMesh.Raycast(m_aiController.gameObject.transform.position, vector, out var hit, int.MaxValue))
				{
					vector = hit.position;
				}
				pathToPosition.Parameters.Destination = vector;
				pathToPosition.Parameters.Range = 1f;
				m_aiController.StateManager.PushState(pathToPosition);
				m_aiController.CancelAllEngagements();
				Wait wait2 = AIStateManager.StatePool.Allocate<Wait>();
				wait2.Duration = 6f;
				wait2.InfiniteWait = false;
				m_aiController.StateManager.QueueState(wait2);
				break;
			}
			case 2:
			{
				m_aiController.CancelAllEngagements();
				SwapTeam();
				Wait wait = AIStateManager.StatePool.Allocate<Wait>();
				wait.Duration = 6f;
				wait.InfiniteWait = false;
				m_aiController.StateManager.PushState(wait);
				break;
			}
			}
			m_transitionTimer += 6f;
		}
		m_totalElapsedTime += Time.deltaTime;
		m_transitionTimer -= Time.deltaTime;
	}

	private bool SwapTeam()
	{
		TargetScanner targetScanner = m_aiController.GetTargetScanner();
		if (targetScanner != null)
		{
			s_potentialTargets.Clear();
			targetScanner.GetPotentialTargets(m_aiController.StateManager.CurrentState.Owner, m_aiController, -1f, s_potentialTargets);
			if (s_potentialTargets.Count > 0)
			{
				m_aiController.ChangeFactionBecauseOfConfusion(s_potentialTargets[0]);
				s_potentialTargets.Clear();
				return true;
			}
			return false;
		}
		if (m_aiController is PartyMemberAI)
		{
			Vector3 position = m_aiController.StateManager.Owner.transform.position;
			foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
			{
				if (!(GameUtilities.V3SqrDistance2D(position, activeFactionComponent.transform.position) > 400f) && activeFactionComponent.IsHostile(m_aiController.StateManager.Owner))
				{
					Health component = activeFactionComponent.GetComponent<Health>();
					if (!(component == null) && !component.Dead && component.gameObject.activeInHierarchy)
					{
						m_aiController.ChangeFactionBecauseOfConfusion(activeFactionComponent.gameObject);
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsEnemyNearby()
	{
		Faction component = m_aiController.gameObject.GetComponent<Faction>();
		if (component == null)
		{
			return false;
		}
		bool isInPlayerFaction = component.IsInPlayerFaction;
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if ((isInPlayerFaction && activeFactionComponent.IsInPlayerFaction) || !activeFactionComponent.IsHostile(component))
			{
				continue;
			}
			AIController component2 = activeFactionComponent.GetComponent<AIController>();
			if (!(component2 == null) && !component2.IsPet)
			{
				float perceptionDistance = component2.PerceptionDistance;
				if (GameUtilities.V3SqrDistance2D(m_aiController.transform.position, activeFactionComponent.gameObject.transform.position) <= perceptionDistance * perceptionDistance && GameUtilities.LineofSight(m_aiController.transform.position, component2.gameObject, 1f))
				{
					return true;
				}
			}
		}
		return false;
	}
}

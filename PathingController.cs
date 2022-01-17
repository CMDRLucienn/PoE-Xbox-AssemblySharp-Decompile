using AI.Achievement;
using UnityEngine;
using UnityEngine.AI;

public class PathingController
{
	private AIState m_state;

	private PathToPosition.Params m_params;

	private Mover m_mover;

	private Vector3 m_prevTargetPos;

	private Vector3 m_closestDesiredPos;

	private Vector3 m_prevPos;

	private float m_distanceToGoalSq = float.MaxValue;

	private AnimationController.MovementType m_currentMovementType = AnimationController.MovementType.Run;

	private const float ADJACENT_DISTANCE = 0.1f;

	public Mover Mover => m_mover;

	public bool Enabled
	{
		get
		{
			if (m_mover != null)
			{
				return m_mover.enabled;
			}
			return false;
		}
	}

	public void Reset()
	{
		m_state = null;
		m_params = null;
		m_mover = null;
		m_prevTargetPos = Vector3.zero;
		m_closestDesiredPos = Vector3.zero;
		m_prevPos = Vector3.zero;
		m_distanceToGoalSq = float.MaxValue;
		m_currentMovementType = AnimationController.MovementType.Run;
	}

	public void Init(AIState state, PathToPosition.Params pathParams, bool startPathing)
	{
		m_state = state;
		m_params = pathParams;
		m_mover = m_state.Owner.GetComponent<Mover>();
		if (m_mover == null)
		{
			Debug.LogError("PathToPosition's owner doesn't have a Mover component! Owner is " + m_state.Owner.name);
			return;
		}
		m_currentMovementType = m_params.MovementType;
		if (m_params.MovementType == AnimationController.MovementType.Walk)
		{
			m_mover.UseWalkSpeed();
		}
		else
		{
			m_mover.UseRunSpeed();
		}
		if (m_params.Target != null)
		{
			Mover component = m_params.Target.GetComponent<Mover>();
			if (component != null)
			{
				m_params.Range = m_mover.Radius + component.Radius + m_params.Range;
			}
		}
		Vector3 vector = GetDestination();
		if (m_params.Range > 0f && NavMesh.SamplePosition(vector, out var hit, m_params.Range * 2f, int.MaxValue))
		{
			if (m_params.Target == null)
			{
				m_params.Destination = hit.position;
				vector = m_params.Destination;
			}
			else
			{
				m_prevTargetPos = hit.position;
				vector = m_prevTargetPos;
			}
		}
		if (startPathing)
		{
			GeneratePath(vector);
		}
	}

	public void UpdateStealth()
	{
		if (Stealth.IsInStealthMode(m_mover.gameObject))
		{
			if (m_currentMovementType != AnimationController.MovementType.Walk)
			{
				m_currentMovementType = AnimationController.MovementType.Walk;
				m_mover.UseWalkSpeed();
			}
		}
		else if (m_params.MovementType != AnimationController.MovementType.Walk && m_currentMovementType != AnimationController.MovementType.Run)
		{
			m_currentMovementType = AnimationController.MovementType.Run;
			m_mover.UseRunSpeed();
		}
	}

	public void Update()
	{
		if (m_params == null)
		{
			return;
		}
		Vector3 vector = GetDestination();
		if (m_params.Target != null && m_mover.BlockedByDoor == null && GameUtilities.V3Subtract2D(vector, m_prevTargetPos).sqrMagnitude > 0.01f && NavMesh.SamplePosition(vector, out var hit, m_params.Range * 2f, int.MaxValue) && GeneratePath(hit.position))
		{
			m_prevTargetPos = hit.position;
			vector = m_prevTargetPos;
			m_distanceToGoalSq = float.MaxValue;
		}
		UpdateClosestDesiredLocation();
		if (m_mover == null)
		{
			return;
		}
		if ((m_mover.Blocked || !m_mover.HasGoal) && m_params.OpenBlockingDoors && m_mover.BlockedByDoor != null)
		{
			float num = m_mover.BlockedByDoor.UseRadius + 2f;
			if (GameUtilities.V3SqrDistance2D(m_mover.BlockedByDoor.transform.position, m_mover.transform.position) <= num * num)
			{
				if (m_mover.BlockedByDoor.CurrentState == OCL.State.Closed)
				{
					m_mover.BlockedByDoor.Use(m_state.Owner);
				}
				m_mover.SetBlockFlag(Mover.BlockFlag.None);
				m_mover.BlockedByDoor = null;
				m_mover.BlockedBy = null;
				GeneratePath(vector);
				m_distanceToGoalSq = float.MaxValue;
			}
		}
		else if (ReachedDestination())
		{
			if (m_params.GetAsCloseAsPossible && !m_mover.IsPartialPath() && !m_mover.PositionOverlaps(m_closestDesiredPos, m_mover))
			{
				m_mover.transform.position = m_closestDesiredPos;
			}
			if (m_mover.IsPartialPath() && m_mover.BlockedByDoor == null && !(m_mover.AIController is PartyMemberAI))
			{
				m_mover.AIController.StateManager.PopAllStates();
			}
			m_mover.Stop();
		}
	}

	public void UpdatePreviousPosition()
	{
		if (!GameState.Paused)
		{
			m_prevPos = m_mover.transform.position;
		}
	}

	private void UpdateClosestDesiredLocation()
	{
		Vector3 destination = GetDestination();
		if (m_params.GetAsCloseAsPossible)
		{
			float num = 0f;
			if (m_params.Target != null)
			{
				Mover component = m_params.Target.GetComponent<Mover>();
				if (component != null)
				{
					num = component.Radius;
				}
			}
			float num2 = m_mover.Radius + num + 0.1f;
			Vector3 vector = m_state.Owner.transform.position - destination;
			vector.Normalize();
			m_closestDesiredPos = vector * num2 + destination;
		}
		else
		{
			m_closestDesiredPos = destination;
		}
	}

	public bool ReachedDestination()
	{
		if (m_mover == null)
		{
			return true;
		}
		if (m_mover.ReachedGoal && !m_params.GetAsCloseAsPossible)
		{
			return true;
		}
		Vector3 vector = GetDestination();
		if (m_mover.IsPartialPath())
		{
			vector = ((m_mover.Route.Length == 0) ? m_mover.transform.position : m_mover.Route[m_mover.Route.Length - 1]);
		}
		float num = GameUtilities.V3SqrDistance2D(vector, m_state.Owner.transform.position);
		if (num <= m_params.Range * m_params.Range)
		{
			if ((m_params.StopOnLOS || m_params.LineOfSight) && !GameUtilities.LineofSight(m_state.Owner.transform.position, vector, 1f, includeDynamics: false))
			{
				return false;
			}
			if (m_params.GetAsCloseAsPossible)
			{
				if (m_mover.IsPartialPath())
				{
					return true;
				}
				if (m_distanceToGoalSq < num)
				{
					return true;
				}
				m_distanceToGoalSq = num;
				float num2 = 0f;
				Mover mover = null;
				if (m_params.Target != null)
				{
					mover = m_params.Target.GetComponent<Mover>();
					if (mover != null)
					{
						num2 = mover.Radius;
					}
				}
				float num3 = m_mover.Radius + num2 + 0.1f + 0.1f;
				if (num > num3 * num3 && GameUtilities.V3SqrDistance2D(m_prevPos, m_state.Owner.transform.position) > float.Epsilon && !Mover.PositionOccupied(m_closestDesiredPos, m_mover))
				{
					return false;
				}
			}
			return true;
		}
		if (m_mover.ReachedGoal)
		{
			m_mover.ReachedGoal = false;
			GeneratePath(vector);
		}
		return false;
	}

	private bool GeneratePath(Vector3 destination)
	{
		bool flag = m_mover.PathToDestination(destination, m_params.Range);
		m_distanceToGoalSq = float.MaxValue;
		if (flag && m_params.DesiresMaxRange && m_mover.Route.Length > 2)
		{
			Vector3 vector = m_mover.Route[m_mover.Route.Length - 1];
			Vector2 vector2 = GameUtilities.V3Subtract2D(m_mover.Route[m_mover.Route.Length - 2], vector);
			vector2.Normalize();
			float num = m_params.Range - 0.5f;
			Vector3 vector3 = GameUtilities.V2ToV3(vector2 * num) + vector;
			if (NavMesh.Raycast(m_params.Target.gameObject.transform.position, vector3, out var hit, int.MaxValue))
			{
				vector3 = hit.position;
			}
			flag = m_mover.PathToDestination(vector3, m_mover.Radius);
			if (flag && m_mover.Route.Length == 2)
			{
				float num2 = Vector2.Dot(vector2, GameUtilities.V3Subtract2D(m_mover.transform.position, vector));
				if (num2 < num)
				{
					vector3 = GameUtilities.V2ToV3(vector2 * num2) + vector;
					flag = m_mover.PathToDestination(vector3, m_mover.Radius);
				}
			}
		}
		return flag;
	}

	public bool HasDestination()
	{
		return m_params != null;
	}

	public Vector3 GetDestination()
	{
		if (m_params.Target == null)
		{
			return m_params.Destination;
		}
		return m_params.Target.gameObject.transform.position;
	}

	public void Stop()
	{
		if ((bool)m_mover && m_mover.enabled)
		{
			m_mover.Stop();
			m_mover.UseRunSpeed();
		}
	}
}

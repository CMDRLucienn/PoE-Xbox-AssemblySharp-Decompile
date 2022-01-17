using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;

namespace AI.Player;

public class UseObject : PlayerState
{
	private Usable m_target;

	private Vector3 m_interactionPoint;

	private float m_usableRadius;

	private float m_arriveRadius;

	public Usable UsableObject
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

	public override void Reset()
	{
		base.Reset();
		m_target = null;
		m_interactionPoint = Vector3.zero;
		m_usableRadius = 0f;
		m_arriveRadius = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_target == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		StopMover();
		if (m_interactionPoint.sqrMagnitude < float.Epsilon)
		{
			m_interactionPoint = m_target.GetClosestInteractionPoint(m_ai.gameObject.transform.position);
		}
		m_usableRadius = m_target.UsableRadius + m_ai.Mover.Radius;
		float num = Mathf.Max(m_usableRadius * 0.15f, 0.01f) + 0.8f;
		if (m_target.ArrivalRadius > num)
		{
			m_arriveRadius = m_target.ArrivalRadius;
		}
		else
		{
			m_arriveRadius = num;
		}
		Mover component = m_target.GetComponent<Mover>();
		if (component != null)
		{
			m_usableRadius += component.Radius;
		}
		m_ai.CancelAllEngagements();
		if (m_target.InteractionObject == null && m_target is Door)
		{
			Vector3 normalized = (m_owner.transform.position - m_target.transform.position).normalized;
			m_interactionPoint = normalized * num + m_target.transform.position;
		}
		else
		{
			if (!(m_target is SceneTransition))
			{
				return;
			}
			PartyMemberAI component2 = m_owner.GetComponent<PartyMemberAI>();
			if ((bool)component2)
			{
				m_interactionPoint = (m_target as SceneTransition).GetMarkerPosition(component2, reverse: false);
				if (component2.Secondary)
				{
					m_interactionPoint = GameUtilities.NearestUnoccupiedLocation(m_interactionPoint, component2.Mover.Radius, 6f, component2.Mover);
				}
			}
			m_arriveRadius = 0.1f;
		}
	}

	private void CancelSceneTransition()
	{
		SceneTransition sceneTransition = m_target as SceneTransition;
		if (sceneTransition != null && sceneTransition.IsObjectOnWaitList(m_owner))
		{
			sceneTransition.CancelTransition();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (!base.Manager.IsStateInStack(this))
		{
			CancelSceneTransition();
		}
	}

	public override void OnCancel()
	{
		base.OnCancel();
		CancelSceneTransition();
	}

	public override void OnAbort()
	{
		base.OnAbort();
		CancelSceneTransition();
	}

	public override void Update()
	{
		if (m_target == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_target is NPCDialogue && m_target.gameObject.GetComponent<Mover>() != null && !m_target.HasInteractionObject)
		{
			m_interactionPoint = m_target.gameObject.transform.position;
		}
		float num = GameUtilities.V3SqrDistance2D(m_interactionPoint, m_owner.transform.position);
		if (num > m_usableRadius * m_usableRadius || !HasLineOfSight())
		{
			float num2 = 0.1f;
			Trap trap = m_target as Trap;
			if (trap == null && num < num2 * num2)
			{
				m_owner.transform.position = m_interactionPoint;
			}
			else
			{
				if (trap != null)
				{
					m_ai.Mover.PathToDestination(m_interactionPoint);
					Vector3 b = Vector3.zero;
					if (m_ai.Mover.Route.Length > 1)
					{
						int num3 = m_ai.Mover.Route.Length - 2;
						bool flag = false;
						while (num3 >= 0)
						{
							if (!trap.IsPointInTrap(m_ai.Mover.Route[num3]))
							{
								flag = true;
								b = m_ai.Mover.Route[num3];
								break;
							}
							num3--;
						}
						if (flag)
						{
							List<Vector3> worldVertices = trap.GetWorldVertices();
							if (worldVertices.Count > 0)
							{
								float num4 = float.MaxValue;
								foreach (Vector3 item in worldVertices)
								{
									float num5 = GameUtilities.V3SqrDistance2D(item, b);
									if (num5 < num4)
									{
										m_interactionPoint = item;
										num4 = num5;
									}
								}
							}
						}
					}
				}
				num = GameUtilities.V3SqrDistance2D(m_interactionPoint, m_owner.transform.position);
				if (num > m_usableRadius * m_usableRadius)
				{
					AIState aIState = null;
					float maxDistance = m_usableRadius + m_ai.Mover.Radius;
					if (m_target is SceneTransition)
					{
						maxDistance = 6f;
					}
					Vector3 destination = GameUtilities.NearestUnoccupiedLocation(m_interactionPoint, m_ai.Mover.Radius, maxDistance, m_ai.Mover);
					bool flag2 = false;
					if (m_target is NPCDialogue && !m_target.HasInteractionObject && m_target.gameObject.GetComponent<Mover>() != null)
					{
						flag2 = true;
						PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
						pathToPosition.Parameters.Destination = destination;
						pathToPosition.Parameters.Range = m_arriveRadius;
						pathToPosition.ParentState = this;
						pathToPosition.Parameters.Target = m_target.gameObject;
						pathToPosition.Parameters.Range = m_arriveRadius * 0.75f;
						aIState = pathToPosition;
					}
					if (!flag2)
					{
						Move move = AIStateManager.StatePool.Allocate<Move>();
						move.Destination = destination;
						move.Range = m_arriveRadius;
						move.ShowDestinationCircle = false;
						move.ParentState = this;
						aIState = move;
					}
					if (m_ai.Mover.IsPartialPath())
					{
						AIStateManager.StatePool.Free(aIState);
						m_ai.StateManager.PopCurrentState();
					}
					else
					{
						base.Manager.PushState(aIState);
					}
					return;
				}
			}
		}
		if (m_target.IsUsable)
		{
			Door door = m_target as Door;
			if (door != null && door.CurrentState == OCL.State.Open && door.IsAnyMoverIntersectingNavMeshObstacle())
			{
				base.Manager.PopCurrentState();
				return;
			}
			Trap trap2 = m_target as Trap;
			if (trap2 == null && m_target is Container)
			{
				trap2 = m_target.gameObject.GetComponent<Trap>();
			}
			if (trap2 != null)
			{
				base.Manager.PopCurrentState();
				m_target.Use(m_owner);
				return;
			}
			bool flag3 = m_target.Use(m_owner);
			bool flag4 = false;
			if (m_target is SceneTransition && flag3)
			{
				base.Manager.PopCurrentState();
				PushState<WaitForSceneTransition>().TransitionObject = m_target as SceneTransition;
				flag4 = true;
			}
			else if (m_target is NPCDialogue && flag3)
			{
				AIController component = m_target.gameObject.GetComponent<AIController>();
				if (component != null && component.IsMoving())
				{
					Vector3 forwardFacing = m_owner.transform.position - m_target.gameObject.transform.position;
					forwardFacing.y = 0f;
					forwardFacing.Normalize();
					AI.Achievement.Wait wait = AIStateManager.StatePool.Allocate<AI.Achievement.Wait>();
					wait.ForwardFacing = forwardFacing;
					wait.Duration = 3f;
					component.StateManager.PushState(wait);
				}
			}
			ScriptEvent component2 = m_target.GetComponent<ScriptEvent>();
			if ((bool)component2)
			{
				SpecialCharacterInstanceID.Add(m_owner, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
				component2.ExecuteScript(ScriptEvent.ScriptEvents.OnUsed);
			}
			if (flag4)
			{
				return;
			}
		}
		base.Manager.PopState(this);
	}

	private bool HasLineOfSight()
	{
		if (m_target.GetComponent<Examinable>() != null)
		{
			return GameUtilities.LineofSight(m_owner.transform.position, m_interactionPoint, 1f, includeDynamics: false);
		}
		return true;
	}

	public override string GetDebugText()
	{
		if (m_target != null)
		{
			return ": Use Object: " + m_target.ToString();
		}
		return string.Empty;
	}
}

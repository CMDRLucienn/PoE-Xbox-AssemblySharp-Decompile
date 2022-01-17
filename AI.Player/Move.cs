using AI.Achievement;
using UnityEngine;

namespace AI.Player;

public class Move : PlayerState
{
	protected PathToPosition.Params m_params = new PathToPosition.Params();

	private PathingController m_pathingController = new PathingController();

	private bool m_showDestinationCircle;

	private bool m_hasBeenUpdated;

	private AIState m_parentState;

	private bool m_frozen;

	public Vector3 Destination
	{
		get
		{
			return m_params.Destination;
		}
		set
		{
			m_params.Destination = value;
		}
	}

	public float Range
	{
		get
		{
			return m_params.Range;
		}
		set
		{
			m_params.Range = value;
		}
	}

	public bool ShowDestinationCircle
	{
		get
		{
			return m_showDestinationCircle;
		}
		set
		{
			m_showDestinationCircle = value;
		}
	}

	public override bool InCombat => false;

	public override AIState ParentState
	{
		get
		{
			return m_parentState;
		}
		set
		{
			m_parentState = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_params.Reset();
		m_pathingController.Reset();
		m_showDestinationCircle = false;
		m_hasBeenUpdated = false;
		m_parentState = null;
		m_frozen = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_hasBeenUpdated = false;
		if (m_partyMemberAI == null)
		{
			Debug.LogError("AI is using a player state without a PartyMemberAI component!", m_owner);
		}
		Mover mover = m_ai.Mover;
		if (mover != null)
		{
			mover.ClearForcedTurnDirection();
			mover.ReachedGoal = false;
			mover.IgnoreAttackBlocking = false;
			mover.enabled = true;
			m_params.OpenBlockingDoors = false;
			m_frozen = mover.Frozen;
		}
		m_pathingController.Init(this, m_params, startPathing: true);
		m_ai.CancelAllEngagements();
		if (m_showDestinationCircle)
		{
			m_partyMemberAI.DestinationCircleState = this;
			m_partyMemberAI.DestinationCirclePosition = GetDestination();
		}
		if (mover.IsPartialPath() && GameUtilities.V3SqrDistance2D(mover.transform.position, mover.Route[mover.Route.Length - 1]) < mover.ArrivalDistance * mover.ArrivalDistance)
		{
			base.Manager.PopCurrentState();
			mover.Stop();
		}
	}

	public override void OnAbort()
	{
		base.OnAbort();
		m_pathingController.Stop();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_pathingController.Stop();
	}

	public override void Update()
	{
		if (m_partyMemberAI == null)
		{
			Debug.LogError("AI is using a player state without a PartyMemberAI component!", m_owner);
		}
		base.Update();
		m_hasBeenUpdated = true;
		if (m_frozen)
		{
			if (!m_ai.Mover.Frozen)
			{
				m_pathingController.Init(this, m_params, startPathing: true);
				m_frozen = false;
			}
		}
		else
		{
			m_frozen = m_ai.Mover.Frozen;
		}
		m_pathingController.UpdateStealth();
		m_pathingController.Update();
		if (m_pathingController.ReachedDestination())
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_ai.Mover.IsPartialPath() && GameUtilities.V3SqrDistance2D(m_ai.Mover.transform.position, m_ai.Mover.Route[m_ai.Mover.Route.Length - 1]) < m_ai.Mover.ArrivalDistance * m_ai.Mover.ArrivalDistance)
		{
			Vector3 destination = GameUtilities.NearestUnoccupiedLocation(m_ai.Mover.transform.position, m_ai.Mover.Radius, 6f, m_ai.Mover);
			m_params.Destination = destination;
			m_pathingController.Init(this, m_params, startPathing: true);
		}
		m_ai.UpdateEngagement(base.Owner, m_partyMemberAI.GetPrimaryAttack());
	}

	public bool HasDestination()
	{
		return m_pathingController.HasDestination();
	}

	public Vector3 GetDestination()
	{
		return m_pathingController.GetDestination();
	}

	public void SetDestinationWithRange(Vector3 position, float range, bool stopAtLOS)
	{
		m_params.Destination = position;
		m_params.Range = range;
		m_params.StopOnLOS = stopAtLOS;
	}

	public override string GetDebugText()
	{
		if (m_params.Target != null)
		{
			return ": Target: " + m_params.Target.ToString();
		}
		return ": Destination: (" + m_params.Destination.x + ", " + m_params.Destination.z + ")";
	}

	public override bool IsMoving()
	{
		return m_ai.Mover.Speed > float.Epsilon;
	}

	public override bool AllowBlockedMovement()
	{
		return m_hasBeenUpdated;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}

	public override void OnEvent(GameEventArgs args)
	{
		base.OnEvent(args);
		switch (args.Type)
		{
		case GameEventType.MeleeEngageBroken:
			m_ai.EnemyBreaksEngagement(args.GameObjectData[0]);
			break;
		case GameEventType.MeleeEngagementForceBreak:
			m_ai.CancelEngagement(args.GameObjectData[0]);
			m_ai.EnemyBreaksEngagement(args.GameObjectData[0]);
			break;
		}
	}
}

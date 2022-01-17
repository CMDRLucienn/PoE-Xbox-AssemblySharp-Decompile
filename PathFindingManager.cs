using System;
using System.Collections.Generic;
using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;
using UnityEngine.AI;

public class PathFindingManager : MonoBehaviour
{
	private class Obstacle
	{
		public Mover Mover { get; private set; }

		public SteeringAvoidance SteeringAvoidance { get; private set; }

		public Vector2 Position { get; private set; }

		public float Radius { get; private set; }

		public float RadiusSq { get; private set; }

		public float PatherRadius { get; private set; }

		public float ObstacleRadius { get; private set; }

		public void Set(Mover obstacle, SteeringAvoidance steeringAvoidance, Vector2 position, float patherRadius, float obstacleRadius)
		{
			Mover = obstacle;
			SteeringAvoidance = steeringAvoidance;
			Position = position;
			PatherRadius = patherRadius;
			ObstacleRadius = obstacleRadius;
			Radius = PatherRadius + ObstacleRadius;
			RadiusSq = Radius * Radius;
		}
	}

	private class Vertex
	{
		public float PathingDistance;

		public Vertex FromVertex;

		public float Penalty;

		public bool IsOpen;

		public bool IsClosed;

		public Vector2 Position { get; private set; }

		public List<Link> Links { get; private set; }

		public int WaypointIndex { get; private set; }

		public Vertex()
		{
			Links = new List<Link>();
		}

		public void Set(Vector2 position, float penalty, int waypointIndex)
		{
			Position = position;
			Penalty = penalty;
			WaypointIndex = waypointIndex;
			Links.Clear();
			PathingDistance = 0f;
			FromVertex = null;
			IsOpen = false;
			IsClosed = false;
		}
	}

	private class Link
	{
		public Vertex Vertex1 { get; private set; }

		public Vertex Vertex2 { get; private set; }

		public float Distance { get; private set; }

		public float DistanceSq { get; private set; }

		public void Set(Vertex vertex1, Vertex vertex2)
		{
			Vertex1 = vertex1;
			Vertex2 = vertex2;
			DistanceSq = (vertex2.Position - vertex1.Position).sqrMagnitude;
			Distance = Mathf.Sqrt(DistanceSq);
			if (vertex2.Penalty > 0f)
			{
				Distance += vertex2.Penalty;
				DistanceSq = Distance * Distance;
			}
		}
	}

	private class Node
	{
		public const int INVALID_INDEX = -1;

		public List<Vertex> Vertices { get; private set; }

		public int WaypointIndex { get; set; }

		public Node()
		{
			Vertices = new List<Vertex>();
		}

		public void Clear()
		{
			Vertices.Clear();
			WaypointIndex = -1;
		}
	}

	private enum SteeringAvoidance
	{
		Invalid = -1,
		HeadOn,
		Towards,
		Perpendicular,
		Away,
		Parallel,
		Count
	}

	private class SteeringAvoidanceData
	{
		public float Angle;

		public float BlockDistance;

		public float SteerDistance;

		public float StopDistance;

		public float SlowDownDistance;

		public float SlowDownAngle;
	}

	private const float CHECK_SPACE_RESERVED_DISTANCE_SQ = 9f;

	private const float CHECK_SPACE_RESERVED_NOT_LAST_CORNER_DISTANCE_SQ = 1f;

	private const float CHECK_NUDGE_DISTANCE_SQ = 2.25f;

	private const float STATIONARY_TARGET_RANGE_SQ = 64f;

	private const float STATIONARY_TARGET_SEARCH_OFFSET = 3.5f;

	private const float MOVING_PENALTY = 2f;

	private const float MOVING_RIGHT_PENALTY = 1f;

	private const float STOP_BUFFER = 0.1f;

	private const float BLOCKED_RANGE = 0.5f;

	private const float STOP_BLOCKED_ATTACKER_RANGE = 0.25f;

	private const float STOP_BLOCKED_ATTACKER_ANGLE = 0f;

	private const int MAX_PATH_NODES = 40;

	private const int MAX_OBSTACLE_CLUSTERS = 50;

	private static Obstacle[] s_obstacles;

	private static Obstacle[] s_slowDownObstacles;

	private static Node[] s_nodes;

	private static Vertex[] s_vertices;

	private static Link[] s_links;

	private static int s_nodeCount = 0;

	private static SteeringAvoidanceData[] s_steeringAvoidance;

	private static Mover.ObstacleCluster[] s_obstacleClusters;

	private static bool m_recalculatePathsRequested = false;

	private static bool m_recalculatePathsQueued = false;

	private static List<Mover> s_lateUpdateObstacles = new List<Mover>();

	private static List<Mover> s_lateUpdatePathingMovers = new List<Mover>();

	private static List<Obstacle> s_slowDownForMovingObstacles = new List<Obstacle>();

	private static List<Mover> s_stopBlockedAttackerEngaged = new List<Mover>();

	private static List<Obstacle> s_stopPathForMoverObstacles = new List<Obstacle>();

	public static PathFindingManager Instance { get; private set; }

	public Mover.ObstacleCluster[] ObstacleClusters => s_obstacleClusters;

	public static void RequestRecalculatePaths()
	{
		m_recalculatePathsRequested = true;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'PathFindingManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		s_obstacles = new Obstacle[40];
		s_slowDownObstacles = new Obstacle[40];
		s_nodes = new Node[40];
		s_vertices = new Vertex[320];
		s_links = new Link[640];
		s_steeringAvoidance = new SteeringAvoidanceData[5];
		s_obstacleClusters = new Mover.ObstacleCluster[50];
		for (int i = 0; i < s_obstacles.Length; i++)
		{
			s_obstacles[i] = new Obstacle();
		}
		for (int j = 0; j < s_slowDownObstacles.Length; j++)
		{
			s_slowDownObstacles[j] = new Obstacle();
		}
		for (int k = 0; k < s_nodes.Length; k++)
		{
			s_nodes[k] = new Node();
		}
		for (int l = 0; l < s_vertices.Length; l++)
		{
			s_vertices[l] = new Vertex();
		}
		for (int m = 0; m < s_links.Length; m++)
		{
			s_links[m] = new Link();
		}
		for (int n = 0; n < 5; n++)
		{
			s_steeringAvoidance[n] = new SteeringAvoidanceData();
		}
		for (int num = 0; num < s_obstacleClusters.Length; num++)
		{
			s_obstacleClusters[num] = new Mover.ObstacleCluster();
			s_obstacleClusters[num].Init();
		}
		float blockDistance = 8f;
		float blockDistance2 = 2f;
		float blockDistance3 = 1f;
		float blockDistance4 = 0.75f;
		float blockDistance5 = 0.15f;
		float steerDistance = 7f;
		float steerDistance2 = 5f;
		float steerDistance3 = 0.5f;
		float steerDistance4 = 0.05f;
		float steerDistance5 = 0.05f;
		float stopDistance = 0.5f;
		float stopDistance2 = 0.15f;
		float stopDistance3 = 0.15f;
		float slowDownDistance = 1.2f;
		float slowDownDistance2 = 0.5f;
		float slowDownDistance3 = 0.5f;
		float slowDownAngle = 90f;
		float slowDownAngle2 = 90f;
		s_steeringAvoidance[0].Angle = 165f;
		s_steeringAvoidance[0].BlockDistance = blockDistance;
		s_steeringAvoidance[0].SteerDistance = steerDistance;
		s_steeringAvoidance[0].StopDistance = 0f;
		s_steeringAvoidance[0].SlowDownDistance = 0f;
		s_steeringAvoidance[1].Angle = 120f;
		s_steeringAvoidance[1].BlockDistance = blockDistance2;
		s_steeringAvoidance[1].SteerDistance = steerDistance2;
		s_steeringAvoidance[1].StopDistance = 0f;
		s_steeringAvoidance[1].SlowDownDistance = 0f;
		s_steeringAvoidance[2].Angle = 60f;
		s_steeringAvoidance[2].BlockDistance = blockDistance3;
		s_steeringAvoidance[2].SteerDistance = steerDistance3;
		s_steeringAvoidance[2].StopDistance = stopDistance;
		s_steeringAvoidance[2].SlowDownDistance = slowDownDistance;
		s_steeringAvoidance[3].Angle = 20f;
		s_steeringAvoidance[3].BlockDistance = blockDistance4;
		s_steeringAvoidance[3].SteerDistance = steerDistance4;
		s_steeringAvoidance[3].StopDistance = stopDistance2;
		s_steeringAvoidance[3].SlowDownDistance = slowDownDistance2;
		s_steeringAvoidance[3].SlowDownAngle = slowDownAngle2;
		s_steeringAvoidance[4].Angle = -0.0001f;
		s_steeringAvoidance[4].BlockDistance = blockDistance5;
		s_steeringAvoidance[4].SteerDistance = steerDistance5;
		s_steeringAvoidance[4].StopDistance = stopDistance3;
		s_steeringAvoidance[4].SlowDownDistance = slowDownDistance3;
		s_steeringAvoidance[4].SlowDownAngle = slowDownAngle;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_recalculatePathsQueued)
		{
			m_recalculatePathsQueued = false;
			Mover.RecalculateAllPaths();
		}
	}

	private void LateUpdate()
	{
		if (m_recalculatePathsRequested)
		{
			m_recalculatePathsRequested = false;
			m_recalculatePathsQueued = true;
		}
		List<Mover> movers = Mover.Movers;
		if (movers.Count <= 0)
		{
			return;
		}
		s_lateUpdateObstacles.Clear();
		s_lateUpdatePathingMovers.Clear();
		for (int i = 0; i < movers.Count; i++)
		{
			Mover mover = movers[i];
			if (mover.IsPathingObstacle())
			{
				mover.SpeedModifier = 1f;
				s_lateUpdateObstacles.Add(mover);
				if (mover.IsMoving())
				{
					s_lateUpdatePathingMovers.Add(mover);
				}
			}
		}
		if (!GameState.Paused)
		{
			ReconcileOverlappedDestinations(s_lateUpdatePathingMovers, s_lateUpdateObstacles);
		}
		StopBlockedAttackers(s_lateUpdatePathingMovers, s_lateUpdateObstacles);
		if (GameState.InCombat)
		{
			SlowDownForMovingObstacles(s_lateUpdatePathingMovers);
		}
	}

	private void ReconcileOverlappedDestinations(List<Mover> pathingMovers, List<Mover> obstacles)
	{
		if (GameState.s_playerCharacter == null)
		{
			return;
		}
		Faction component = GameState.s_playerCharacter.GetComponent<Faction>();
		for (int i = 0; i < pathingMovers.Count; i++)
		{
			Mover mover = pathingMovers[i];
			if (!mover.IsPathValid() || GameUtilities.V3SqrDistance2D(mover.transform.position, mover.Route[mover.Route.Length - 1]) > 9f)
			{
				continue;
			}
			if (!(mover.AIController is PartyMemberAI))
			{
				Faction component2 = mover.GetComponent<Faction>();
				if (component2 == null || component2.IsHostile(component))
				{
					continue;
				}
			}
			if (mover.IsPathBlocked())
			{
				continue;
			}
			Vector2 vector = GameUtilities.V3ToV2(mover.transform.position);
			Vector2 vector2 = GameUtilities.V3ToV2(mover.Goal);
			if (mover.BlockedByDoor != null)
			{
				vector2 = mover.Route[mover.Route.Length - 1];
			}
			float sqrMagnitude = (vector - vector2).sqrMagnitude;
			if (sqrMagnitude > 9f || (mover.LastWaypointIndex != mover.Route.Length - 1 && sqrMagnitude > 1f))
			{
				continue;
			}
			for (int j = 0; j < obstacles.Count; j++)
			{
				Mover mover2 = obstacles[j];
				if (mover2 == mover)
				{
					continue;
				}
				float num = GameUtilities.V3SqrDistance2D(mover.transform.position, mover2.transform.position);
				if (num > 9f)
				{
					continue;
				}
				if (mover2.IsMoving() || mover2.IsPathBlocked())
				{
					float num2 = mover2.Radius + mover.Radius + 0.2f;
					if (num > num2 * num2 || !(GameUtilities.V3SqrDistance2D(mover.Goal, mover2.Goal) < 0.09f) || GameUtilities.V3SqrDistance2D(mover.Goal, mover.transform.position) > mover.Radius * mover.Radius)
					{
						continue;
					}
				}
				if (!MovePathersGoal(mover2, mover))
				{
					return;
				}
			}
		}
	}

	private bool MoveAwayFromMoversPathingGoal(Mover obstacle, Mover pathingMover)
	{
		Vector2 vector = GameUtilities.V3ToV2(pathingMover.Goal);
		Vector2 vector2 = ((!obstacle.HasGoal || !obstacle.IsBeingNudged) ? GameUtilities.V3ToV2(obstacle.transform.position) : GameUtilities.V3ToV2(obstacle.Goal));
		float num = obstacle.Radius + pathingMover.Radius;
		if ((vector - vector2).sqrMagnitude >= num * num)
		{
			return true;
		}
		Vector2 vector3 = vector2 - vector;
		vector3.Normalize();
		Vector2 vector4 = vector3 * (num + 0.05f) + vector;
		if (!IsPositionOnNavMesh(vector4))
		{
			return false;
		}
		if (Mover.PositionOccupied(vector4, obstacle))
		{
			return false;
		}
		Vector2 to = GameUtilities.V3Subtract2D(pathingMover.Goal, pathingMover.transform.position);
		if (Vector2.Angle(vector3, to) > 90f)
		{
			MovePathersGoal(obstacle, pathingMover);
			return false;
		}
		PushMoveState(obstacle, GameUtilities.V2ToV3(vector4), 0.05f);
		obstacle.PathToDestination(GameUtilities.V2ToV3(vector4), 0.05f);
		obstacle.IsBeingNudged = true;
		obstacle.SnapToFinalDestination = true;
		pathingMover.SnapToFinalDestination = true;
		return true;
	}

	private bool MovePathersGoal(Mover obstacle, Mover pathingMover)
	{
		Vector2 vector = GameUtilities.V3ToV2(obstacle.transform.position);
		Vector2 vector2 = GameUtilities.V3ToV2(pathingMover.FinalCorner);
		float num = obstacle.Radius + pathingMover.Radius;
		if ((vector2 - vector).sqrMagnitude >= num * num)
		{
			return true;
		}
		num += 0.05f;
		Vector2 vector3 = vector2 - vector;
		if (vector3.sqrMagnitude <= 0.01f)
		{
			vector3 = GameUtilities.V3ToV2(pathingMover.transform.position) - vector2;
		}
		vector3.Normalize();
		Vector2 vector4 = vector3 * num + vector;
		Vector3 position = obstacle.transform.position;
		Vector3 targetPosition = GameUtilities.V2ToV3(vector4);
		targetPosition.y = position.y;
		if (NavMesh.Raycast(position, targetPosition, out var hit, int.MaxValue))
		{
			vector3 = GameUtilities.V3ToV2(pathingMover.transform.position) - vector;
			vector3.Normalize();
			vector4 = vector3 * num + vector;
			targetPosition = GameUtilities.V2ToV3(vector4);
			targetPosition.y = position.y;
			if (NavMesh.Raycast(position, targetPosition, out hit, int.MaxValue))
			{
				pathingMover.AIController.StateManager.PopCurrentState();
				return false;
			}
		}
		Vector3 vector5 = GameUtilities.V2ToV3(vector4);
		vector5.y = pathingMover.transform.position.y;
		Mover mover = pathingMover.PositionOverlapsWith(vector5, pathingMover);
		if (mover == null)
		{
			pathingMover.PathToDestination(vector5, 0.05f);
			pathingMover.SnapToFinalDestination = true;
			return true;
		}
		if (GameUtilities.V3SqrDistance2D(obstacle.transform.position, pathingMover.transform.position) <= num * num)
		{
			pathingMover.AIController.StateManager.PopCurrentState();
			return false;
		}
		Vector2 vector6 = GameUtilities.V3ToV2(mover.transform.position);
		vector3 = vector4 - vector6;
		vector3.Normalize();
		float num2 = mover.Radius + pathingMover.Radius + 0.05f;
		vector4 = vector3 * num2 + vector6;
		targetPosition = GameUtilities.V2ToV3(vector4);
		targetPosition.y = position.y;
		if (NavMesh.Raycast(position, targetPosition, out hit, int.MaxValue))
		{
			pathingMover.AIController.StateManager.PopCurrentState();
			return false;
		}
		vector5 = GameUtilities.V2ToV3(vector4);
		vector5.y = pathingMover.transform.position.y;
		if (!pathingMover.PositionOverlaps(vector5, pathingMover))
		{
			pathingMover.PathToDestination(vector5, 0.05f);
			pathingMover.SnapToFinalDestination = true;
			return true;
		}
		return false;
	}

	private void PushMoveState(Mover mover, Vector3 moverGoal, float radius)
	{
		if (mover.AIController != null)
		{
			if (mover.AIController is PartyMemberAI)
			{
				Move move = AIStateManager.StatePool.Allocate<Move>();
				move.Destination = moverGoal;
				move.Range = radius;
				mover.AIController.StateManager.PushState(move, clearStack: false);
			}
			else
			{
				PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
				pathToPosition.Parameters.Destination = moverGoal;
				pathToPosition.Parameters.Range = radius;
				mover.AIController.StateManager.PushState(pathToPosition, clearStack: false);
			}
		}
	}

	private void PushPathBlockedState(Mover mover, Mover blocker, float blockerDistance)
	{
		PushPathBlockedState(mover, blocker, blockerDistance, null);
	}

	private void PushPathBlockedState(Mover mover, Mover blocker, float blockerDistance, List<Mover> obstacles)
	{
		if (!(mover.AIController != null) || Cutscene.CutsceneActive || !mover.AIController.StateManager.CurrentState.AllowBlockedMovement())
		{
			return;
		}
		mover.SaveBlockedRoute();
		if (mover.AIController is PartyMemberAI)
		{
			if (!(mover.AIController.StateManager.CurrentState is AI.Player.WaitForClearPath))
			{
				AI.Player.WaitForClearPath waitForClearPath = AIStateManager.StatePool.Allocate<AI.Player.WaitForClearPath>();
				waitForClearPath.Blocker = blocker;
				waitForClearPath.BlockerDistance = blockerDistance;
				mover.AIController.StateManager.PushState(waitForClearPath, clearStack: false);
				if (obstacles != null && obstacles.Count > 0)
				{
					waitForClearPath.Obstacles.AddRange(obstacles);
				}
			}
		}
		else if (!(mover.AIController.StateManager.CurrentState is AI.Plan.WaitForClearPath))
		{
			AI.Plan.WaitForClearPath waitForClearPath2 = AIStateManager.StatePool.Allocate<AI.Plan.WaitForClearPath>();
			waitForClearPath2.Blocker = blocker;
			waitForClearPath2.BlockerDistance = blockerDistance;
			mover.AIController.StateManager.PushState(waitForClearPath2, clearStack: false);
			if (obstacles != null && obstacles.Count > 0)
			{
				waitForClearPath2.Obstacles.AddRange(obstacles);
			}
		}
	}

	private Vector2 GetOffsetDirection(Vector2 patherPos, Vector2 goalPos, Vector2 stationaryPos)
	{
		Vector2 lhs = stationaryPos - patherPos;
		Vector2 vector = patherPos - goalPos;
		vector.Normalize();
		float num = Vector2.Dot(lhs, vector);
		Vector2 result = patherPos + vector * num - stationaryPos;
		result.Normalize();
		if (num <= float.Epsilon)
		{
			result = new Vector2(0f - result.y, result.x);
		}
		return result;
	}

	private SteeringAvoidance GetSteeringAvoidanceType(Mover pather, Mover obstacle)
	{
		float num = Vector2.Angle(pather.DesiredHeading, obstacle.DesiredHeading);
		for (int i = 0; i < 5; i++)
		{
			if (num >= s_steeringAvoidance[i].Angle)
			{
				return (SteeringAvoidance)i;
			}
		}
		return SteeringAvoidance.Invalid;
	}

	private void SteerAwayFromObstacles(List<Mover> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			SteerAwayFromObstacles(obstacles[i]);
		}
	}

	private void SteerAwayFromObstacles(Mover pathingMover)
	{
		List<Obstacle> list = new List<Obstacle>();
		List<Obstacle> list2 = new List<Obstacle>();
		int num = 0;
		int num2 = 0;
		bool flag = false;
		Vector3 a = pathingMover.transform.position + GameUtilities.V2ToV3(pathingMover.MovementDirection) * 3.5f;
		for (int i = 0; i < Mover.Movers.Count; i++)
		{
			Mover mover = Mover.Movers[i];
			if (mover == pathingMover || !mover.IsPathingObstacle() || GameUtilities.V3SqrDistance2D(a, mover.transform.position) > 64f)
			{
				continue;
			}
			SteeringAvoidance steeringAvoidance = SteeringAvoidance.Invalid;
			if (mover.IsMoving())
			{
				steeringAvoidance = GetSteeringAvoidanceType(pathingMover, mover);
				if (steeringAvoidance == SteeringAvoidance.Invalid)
				{
					continue;
				}
				float num3 = GameUtilities.V3Distance2D(pathingMover.transform.position, mover.transform.position);
				num3 -= pathingMover.Radius + mover.Radius;
				float slowDownDistance = s_steeringAvoidance[(int)steeringAvoidance].SlowDownDistance;
				if (slowDownDistance > 0f && num3 <= slowDownDistance)
				{
					list2.Add(s_slowDownObstacles[num2]);
					s_slowDownObstacles[num2].Set(mover, steeringAvoidance, GameUtilities.V3ToV2(mover.transform.position), pathingMover.Radius, mover.Radius);
					num++;
				}
				if (num3 > s_steeringAvoidance[(int)steeringAvoidance].SteerDistance)
				{
					continue;
				}
				flag = true;
			}
			if (!mover.OverlapsPosition(pathingMover.Goal))
			{
				list.Add(s_obstacles[num]);
				s_obstacles[num].Set(mover, steeringAvoidance, GameUtilities.V3ToV2(mover.transform.position), pathingMover.Radius, mover.Radius);
				num++;
			}
		}
		if (list.Count <= 0)
		{
			StopCombatSteering(pathingMover);
			AdjustSpeed(pathingMover, list2);
			return;
		}
		if (flag)
		{
			Mover blockingMover = GetBlockingMover(pathingMover, list, useSlowDownDistance: false);
			if (blockingMover == null)
			{
				StopCombatSteering(pathingMover);
				AdjustSpeed(pathingMover, list2);
				return;
			}
			if (blockingMover.IsMoving() && StopPatherForBlocker(pathingMover, blockingMover, stop: true))
			{
				return;
			}
		}
		else if (HasClearPath(pathingMover, list))
		{
			StopCombatSteering(pathingMover);
			AdjustSpeed(pathingMover, list2);
			return;
		}
		Vector2 vector = GameUtilities.V3ToV2(pathingMover.transform.position);
		Vector2 vector2 = GameUtilities.V3ToV2(pathingMover.Goal);
		ConstructPathingNetwork(vector, pathingMover, list);
		Vector2 intermediateGoal = GetIntermediateGoal(vector2, pathingMover, list);
		if ((intermediateGoal - vector2).sqrMagnitude <= float.Epsilon)
		{
			if (!pathingMover.IsPathBlocked())
			{
				Mover blockingMover2 = GetBlockingMover(pathingMover, list, useSlowDownDistance: false);
				float num4 = 0.5f + blockingMover2.Radius + pathingMover.Radius + 0.2f;
				if (blockingMover2 != null && GameUtilities.V3SqrDistance2D(blockingMover2.transform.position, pathingMover.transform.position) > num4 * num4)
				{
					PushMoveState(pathingMover, blockingMover2.transform.position, num4 - 0.1f);
				}
				else
				{
					PushPathBlockedState(pathingMover, null, 0.5f);
				}
			}
			return;
		}
		AdjustSpeed(pathingMover, list2);
		if (pathingMover.IsPathBlocked())
		{
			pathingMover.AIController.StateManager.PopCurrentState();
		}
		float sqrMagnitude = (intermediateGoal - vector).sqrMagnitude;
		float arrivalDist = 1f;
		if (sqrMagnitude < 1f)
		{
			arrivalDist = Math.Max(Mathf.Sqrt(sqrMagnitude) * 0.5f, 0.25f);
		}
		pathingMover.CombatSteer(intermediateGoal, arrivalDist);
	}

	private void SlowDownForMovingObstacles(List<Mover> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			SlowDownForMovingObstacles(obstacles[i], obstacles);
		}
	}

	private void SlowDownForMovingObstacles(Mover pather, List<Mover> obstacles)
	{
		s_slowDownForMovingObstacles.Clear();
		int num = 0;
		int num2 = 0;
		Vector3 a = pather.transform.position + GameUtilities.V2ToV3(pather.MovementDirection) * 3.5f;
		for (int i = 0; i < obstacles.Count; i++)
		{
			Mover mover = obstacles[i];
			if (mover == pather || GameUtilities.V3SqrDistance2D(a, mover.transform.position) > 64f)
			{
				continue;
			}
			SteeringAvoidance steeringAvoidanceType = GetSteeringAvoidanceType(pather, mover);
			if (steeringAvoidanceType == SteeringAvoidance.Invalid)
			{
				continue;
			}
			float num3 = GameUtilities.V3Distance2D(pather.transform.position, mover.transform.position);
			num3 -= pather.Radius + mover.Radius;
			float slowDownDistance = s_steeringAvoidance[(int)steeringAvoidanceType].SlowDownDistance;
			if (!(slowDownDistance > 0f) || !(num3 <= slowDownDistance))
			{
				continue;
			}
			float slowDownAngle = s_steeringAvoidance[(int)steeringAvoidanceType].SlowDownAngle;
			if (slowDownAngle > 0f)
			{
				Vector2 to = GameUtilities.V3Subtract2D(mover.transform.position, pather.transform.position);
				if (Vector2.Angle(pather.MovementDirection, to) > slowDownAngle)
				{
					continue;
				}
			}
			s_slowDownForMovingObstacles.Add(s_slowDownObstacles[num2]);
			s_slowDownObstacles[num2].Set(mover, steeringAvoidanceType, GameUtilities.V3ToV2(mover.transform.position), pather.Radius, mover.Radius);
			num++;
		}
		if (s_slowDownForMovingObstacles.Count > 0)
		{
			Mover blockingMover = GetBlockingMover(pather, s_slowDownForMovingObstacles, useSlowDownDistance: false);
			if (blockingMover != null)
			{
				StopPatherForBlocker(pather, blockingMover, stop: false);
				StopPatherForBlocker(pather, blockingMover, stop: true);
			}
		}
	}

	private void StopBlockedAttackers(List<Mover> movers, List<Mover> obstacles)
	{
		for (int i = 0; i < movers.Count; i++)
		{
			Mover mover = movers[i];
			if (mover.IgnoreAttackBlocking)
			{
				continue;
			}
			GameObject currentTarget = mover.AIController.CurrentTarget;
			if (!(currentTarget == null))
			{
				AIController component = currentTarget.GetComponent<AIController>();
				if (!(component == null) && component.EngagedBy.Count > 0)
				{
					StopBlockedAttacker(mover, obstacles, currentTarget);
				}
			}
		}
	}

	private void StopBlockedAttacker(Mover mover, List<Mover> obstacles, GameObject target)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			Mover mover2 = obstacles[i];
			if (mover2.AIController.CurrentTarget != target || mover2 == mover)
			{
				continue;
			}
			Vector2 lhs = GameUtilities.V3Subtract2D(mover2.transform.position, mover.transform.position);
			float num = mover.Radius + mover2.Radius + 0.25f;
			if (lhs.sqrMagnitude > num * num)
			{
				continue;
			}
			lhs.Normalize();
			if (Vector2.Dot(lhs, mover.DesiredHeading) < 0f || (!mover2.AIController.IsPathBlocked() && (!mover2.AIController.IsMoving() || !(Vector2.Dot(mover2.DesiredHeading, mover.DesiredHeading) <= 0f))))
			{
				continue;
			}
			AIController component = target.GetComponent<AIController>();
			if (component != null)
			{
				s_stopBlockedAttackerEngaged.Clear();
				for (int j = 0; j < component.EngagedBy.Count; j++)
				{
					GameObject gameObject = component.EngagedBy[j];
					if (gameObject != null)
					{
						Mover component2 = gameObject.GetComponent<Mover>();
						if (component2 != null)
						{
							s_stopBlockedAttackerEngaged.Add(component2);
						}
					}
				}
			}
			mover.ClearAllTurnDirections();
			PushPathBlockedState(mover, null, 0.5f, s_stopBlockedAttackerEngaged);
			break;
		}
	}

	private static void StopCombatSteering(Mover mover)
	{
		if (mover.IsPathBlocked())
		{
			mover.AIController.StateManager.PopCurrentState();
		}
		mover.StopCombatSteering();
	}

	private static bool IsPositionOnNavMesh(Vector2 position)
	{
		return GameUtilities.IsPositionOnNavMesh(GameUtilities.V2ToV3(position));
	}

	private bool HasClearPath(Mover pather, List<Obstacle> obstacles)
	{
		Vector3[] route = pather.Route;
		int num = route.Length;
		int num2 = 1;
		if (num < 2)
		{
			return false;
		}
		if (pather.LastWaypointIndex > 0)
		{
			num2 = pather.LastWaypointIndex;
		}
		for (int i = num2; i < num; i++)
		{
			Vector2 start = ((i != num2) ? GameUtilities.V3ToV2(route[i - 1]) : GameUtilities.V3ToV2(pather.transform.position));
			Vector2 goal = GameUtilities.V3ToV2(route[i]);
			if (!HasClearPath(start, goal, obstacles, i == num - 1))
			{
				return false;
			}
		}
		return true;
	}

	private bool HasClearPath(Vector2 start, Vector2 goal, List<Obstacle> obstacles, bool isFinalGoal)
	{
		Vector2 startToGoalNormalized = goal - start;
		startToGoalNormalized.Normalize();
		for (int i = 0; i < obstacles.Count; i++)
		{
			Obstacle obstacle = obstacles[i];
			if (!HasClearPath(start, goal, startToGoalNormalized, obstacle, isFinalGoal, useSlowDownDistance: false))
			{
				return false;
			}
		}
		return true;
	}

	private Mover GetBlockingMover(Mover pather, List<Obstacle> obstacles, bool useSlowDownDistance)
	{
		Vector3[] route = pather.Route;
		int num = route.Length;
		int num2 = 1;
		if (num < 2)
		{
			return null;
		}
		if (pather.LastWaypointIndex > 0)
		{
			num2 = pather.LastWaypointIndex;
		}
		for (int i = num2; i < num; i++)
		{
			Vector2 start = ((i != num2) ? GameUtilities.V3ToV2(route[i - 1]) : GameUtilities.V3ToV2(pather.transform.position));
			Vector2 goal = GameUtilities.V3ToV2(route[i]);
			Mover blockingMover = GetBlockingMover(start, goal, obstacles, i == num - 1, useSlowDownDistance);
			if (blockingMover != null)
			{
				return blockingMover;
			}
		}
		return null;
	}

	private Mover GetBlockingMover(Vector2 start, Vector2 goal, List<Obstacle> obstacles, bool isFinalGoal, bool useSlowDownDistance)
	{
		Vector2 startToGoalNormalized = goal - start;
		startToGoalNormalized.Normalize();
		Mover result = null;
		float num = float.MaxValue;
		for (int i = 0; i < obstacles.Count; i++)
		{
			Obstacle obstacle = obstacles[i];
			if (!HasClearPath(start, goal, startToGoalNormalized, obstacle, isFinalGoal, useSlowDownDistance))
			{
				float sqrMagnitude = (obstacle.Position - start).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = obstacle.Mover;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	private bool HasClearPath(Vector2 start, Vector2 goal, Vector2 startToGoalNormalized, Obstacle obstacle, bool isFinalGoal, bool useSlowDownDistance)
	{
		Vector2 rhs = obstacle.Position - start;
		if (obstacle.SteeringAvoidance != SteeringAvoidance.Invalid)
		{
			float num = rhs.magnitude - obstacle.Radius;
			if (useSlowDownDistance)
			{
				if (num > s_steeringAvoidance[(int)obstacle.SteeringAvoidance].SlowDownDistance)
				{
					return true;
				}
			}
			else if (num > s_steeringAvoidance[(int)obstacle.SteeringAvoidance].BlockDistance)
			{
				return true;
			}
		}
		float num2 = Vector2.Dot(startToGoalNormalized, rhs);
		if (num2 < -0.02f)
		{
			return true;
		}
		Vector2 vector = goal - start;
		if ((vector + startToGoalNormalized * obstacle.PatherRadius).sqrMagnitude <= rhs.sqrMagnitude)
		{
			return true;
		}
		float num3 = obstacle.ObstacleRadius + obstacle.PatherRadius;
		if (isFinalGoal)
		{
			float sqrMagnitude = (goal - obstacle.Position).sqrMagnitude;
			if (sqrMagnitude <= obstacle.ObstacleRadius * obstacle.ObstacleRadius)
			{
				return true;
			}
			if (sqrMagnitude <= num3 * num3 && vector.sqrMagnitude < rhs.sqrMagnitude)
			{
				return true;
			}
		}
		Vector2 vector2 = start + startToGoalNormalized * num2;
		if ((obstacle.Position - vector2).sqrMagnitude <= obstacle.RadiusSq - 0.001f)
		{
			return false;
		}
		return true;
	}

	private bool StopPatherForBlocker(Mover pather, Mover blocker, bool stop)
	{
		if (pather.IsBeingNudged || blocker.IsBeingNudged)
		{
			return false;
		}
		SteeringAvoidance steeringAvoidanceType = GetSteeringAvoidanceType(pather, blocker);
		if (steeringAvoidanceType != SteeringAvoidance.Perpendicular && steeringAvoidanceType != SteeringAvoidance.Parallel && steeringAvoidanceType != SteeringAvoidance.Away)
		{
			return false;
		}
		SteeringAvoidanceData steeringAvoidanceData = s_steeringAvoidance[(int)steeringAvoidanceType];
		float num = GameUtilities.V3SqrDistance2D(pather.gameObject.transform.position, blocker.gameObject.transform.position);
		float num2 = pather.Radius + blocker.Radius;
		float blockerDistance = 0f;
		float num3 = 1f;
		if (stop)
		{
			num2 += steeringAvoidanceData.StopDistance;
			blockerDistance = steeringAvoidanceData.StopDistance + 0.1f;
		}
		else
		{
			num2 += steeringAvoidanceData.SlowDownDistance;
		}
		if (num > num2 * num2)
		{
			return false;
		}
		if (!stop)
		{
			float num4 = num2 - pather.Radius - blocker.Radius;
			if (num4 <= 0f)
			{
				Debug.LogError("Invalid slow down distance cannot be the same as stop distance.");
			}
			num3 = (Mathf.Sqrt(num) - pather.Radius - blocker.Radius) / num4;
			num3 += (1f - num3) * 0.4f;
		}
		switch (steeringAvoidanceType)
		{
		case SteeringAvoidance.Perpendicular:
		case SteeringAvoidance.Away:
		{
			Vector2 position = GameUtilities.V3ToV2(blocker.gameObject.transform.position);
			Obstacle obstacle = new Obstacle();
			obstacle.Set(pather, SteeringAvoidance.Invalid, position, blocker.Radius, pather.Radius);
			s_stopPathForMoverObstacles.Clear();
			s_stopPathForMoverObstacles.Add(obstacle);
			if (HasClearPath(blocker, s_stopPathForMoverObstacles))
			{
				if (stop && !CanPatherPassObstacle(pather, blocker))
				{
					if (steeringAvoidanceType != SteeringAvoidance.Away)
					{
						PushPathBlockedState(pather, blocker, blockerDistance);
					}
					else
					{
						pather.SpeedModifier = num3 * 0.5f;
					}
				}
				else
				{
					pather.SpeedModifier = num3;
				}
				return true;
			}
			Vector2 vector = GameUtilities.V3Subtract2D(blocker.gameObject.transform.position, pather.gameObject.transform.position);
			vector.Normalize();
			Vector2 lhs = -vector;
			float num5 = Vector2.Dot(vector, pather.MovementDirection);
			float num6 = Vector2.Dot(lhs, blocker.MovementDirection);
			if (num5 == num6)
			{
				Vector3 lhs2 = GameUtilities.V2ToV3(pather.MovementDirection);
				Vector3 rhs = GameUtilities.V2ToV3(blocker.MovementDirection);
				if (Vector3.Cross(lhs2, rhs).y > 0f)
				{
					if (stop && !CanPatherPassObstacle(pather, blocker))
					{
						PushPathBlockedState(pather, blocker, blockerDistance);
					}
					else
					{
						pather.SpeedModifier = num3;
					}
					return true;
				}
			}
			else if (num6 < num5)
			{
				if (stop && !CanPatherPassObstacle(pather, blocker))
				{
					pather.SpeedModifier = num3 * 0.5f;
				}
				else
				{
					pather.SpeedModifier = num3;
				}
				return true;
			}
			break;
		}
		case SteeringAvoidance.Parallel:
			if (stop && !CanPatherPassObstacle(pather, blocker))
			{
				pather.SpeedModifier = num3 * 0.5f;
			}
			else
			{
				pather.SpeedModifier = num3;
			}
			return true;
		}
		return false;
	}

	private static bool CanPatherPassObstacle(Mover pather, Mover obstacle)
	{
		if (pather.DesiredSpeed < pather.GetRunSpeed() || obstacle.DesiredSpeed >= obstacle.GetRunSpeed())
		{
			return false;
		}
		return true;
	}

	private void AdjustSpeed(Mover pather, List<Obstacle> obstacles)
	{
		if (obstacles.Count > 0)
		{
			Mover blockingMover = GetBlockingMover(pather, obstacles, useSlowDownDistance: true);
			if (blockingMover != null && blockingMover.IsMoving())
			{
				StopPatherForBlocker(pather, blockingMover, stop: false);
			}
		}
	}

	private bool IntersectsObstacle(Vector2 start, Vector2 goal, List<Obstacle> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			if (IntersectsObstacle(start, goal, obstacles[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IntersectsObstacle(Vector2 start, Vector2 goal, Obstacle obstacle)
	{
		Vector2 vector = obstacle.Position - start;
		Vector2 vector2 = goal - start;
		Vector2 rhs = vector;
		Vector2 vector3 = vector2;
		rhs.Normalize();
		vector3.Normalize();
		if (Vector2.Dot(vector3, rhs) > 0f)
		{
			if (vector.magnitude >= vector2.magnitude + obstacle.ObstacleRadius)
			{
				return false;
			}
		}
		else if ((obstacle.Position - goal).magnitude >= vector2.magnitude + obstacle.ObstacleRadius)
		{
			return false;
		}
		float num = Vector2.Dot(vector3, vector);
		Vector2 vector4 = start + vector3 * num;
		if ((obstacle.Position - vector4).sqrMagnitude <= obstacle.RadiusSq - 0.001f)
		{
			return true;
		}
		return false;
	}

	private void ConstructPathingNetwork(Vector2 start, Mover pather, List<Obstacle> obstacles)
	{
		s_vertices[0].Set(start, 0f, 0);
		s_nodes[0].Clear();
		s_nodes[0].Vertices.Add(s_vertices[0]);
		s_nodes[0].WaypointIndex = 0;
		s_nodeCount = 1;
		for (int i = 1; i < pather.Route.Length; i++)
		{
			s_vertices[i].Set(GameUtilities.V3ToV2(pather.Route[i]), 0f, i);
			s_nodes[i].Clear();
			s_nodes[i].Vertices.Add(s_vertices[i]);
			s_nodes[i].WaypointIndex = i;
			s_nodeCount++;
		}
		Vector2 position = s_vertices[s_nodeCount - 1].Position;
		Vector2 from = position - start;
		from.Normalize();
		int num = s_nodeCount;
		int num2 = 0;
		foreach (Obstacle obstacle2 in obstacles)
		{
			Node node = s_nodes[s_nodeCount];
			s_nodeCount++;
			node.Clear();
			float num3 = obstacle2.Radius + 0.05f;
			float num4 = num3 * 0.4142135f;
			Vector2[] obj = new Vector2[8]
			{
				obstacle2.Position + new Vector2(0f - num3, 0f - num4),
				obstacle2.Position + new Vector2(0f - num3, num4),
				obstacle2.Position + new Vector2(0f - num4, 0f - num3),
				obstacle2.Position + new Vector2(0f - num4, num3),
				obstacle2.Position + new Vector2(num4, 0f - num3),
				obstacle2.Position + new Vector2(num4, num3),
				obstacle2.Position + new Vector2(num3, 0f - num4),
				obstacle2.Position + new Vector2(num3, num4)
			};
			float num5 = obstacle2.RadiusSq + 0.001f;
			Vector2[] array = obj;
			foreach (Vector2 vector in array)
			{
				float num6 = 0f;
				if (obstacle2.Mover.IsMoving())
				{
					Vector2 vector2 = vector - obstacle2.Position;
					vector2.Normalize();
					float num7 = Vector2.Dot(obstacle2.Mover.MovementDirection, vector2);
					if (num7 > 0f)
					{
						num6 += num7 * 2f;
					}
					if (Vector2.Angle(from, obstacle2.Mover.MovementDirection) > 160f && Vector2.Dot(vector2, obstacle2.Mover.MovementDirection) <= 0f)
					{
						num6 += 1f;
					}
				}
				Vertex vertex = s_vertices[num];
				vertex.Set(vector, num6, -1);
				if (!PositionOverlapsObstacle(vertex.Position, obstacles))
				{
					node.Vertices.Add(vertex);
					num++;
				}
			}
			for (int k = 0; k < node.Vertices.Count; k++)
			{
				for (int l = k + 1; l < node.Vertices.Count; l++)
				{
					if ((node.Vertices[k].Position - node.Vertices[l].Position).sqrMagnitude <= num5)
					{
						Link link = s_links[num2];
						link.Set(node.Vertices[k], node.Vertices[l]);
						node.Vertices[k].Links.Add(link);
						node.Vertices[l].Links.Add(link);
						num2++;
					}
				}
			}
		}
		for (int m = 0; m < s_nodeCount; m++)
		{
			Node node2 = s_nodes[m];
			List<Obstacle> list = obstacles;
			if (m == pather.Route.Length - 1)
			{
				list = new List<Obstacle>(obstacles);
				Obstacle obstacle = null;
				float num8 = float.MaxValue;
				foreach (Obstacle item in list)
				{
					float sqrMagnitude = (position - item.Position).sqrMagnitude;
					if (sqrMagnitude <= item.ObstacleRadius * item.ObstacleRadius && sqrMagnitude < num8)
					{
						obstacle = item;
						num8 = sqrMagnitude;
					}
				}
				if (obstacle != null)
				{
					list.Remove(obstacle);
				}
			}
			for (int n = m + 1; n < s_nodeCount; n++)
			{
				Node node3 = s_nodes[n];
				if (node2.WaypointIndex != -1 && node3.WaypointIndex != -1 && node2.WaypointIndex != node3.WaypointIndex - 1)
				{
					continue;
				}
				for (int num9 = 0; num9 < node2.Vertices.Count; num9++)
				{
					for (int num10 = 0; num10 < node3.Vertices.Count; num10++)
					{
						if (!NavMesh.Raycast(GameUtilities.V2ToV3(node2.Vertices[num9].Position), GameUtilities.V2ToV3(node3.Vertices[num10].Position), out var _, int.MaxValue) && !IntersectsObstacle(node2.Vertices[num9].Position, node3.Vertices[num10].Position, list))
						{
							Link link = s_links[num2];
							link.Set(node2.Vertices[num9], node3.Vertices[num10]);
							node2.Vertices[num9].Links.Add(link);
							node3.Vertices[num10].Links.Add(link);
							num2++;
						}
					}
				}
			}
		}
	}

	private bool PositionOverlapsObstacle(Vector2 position, List<Obstacle> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			Obstacle obstacle = obstacles[i];
			if ((obstacle.Position - position).sqrMagnitude < obstacle.RadiusSq)
			{
				return true;
			}
		}
		Vector3 position2 = GameUtilities.V2ToV3(position);
		position2.y = 0.1f;
		if (!GameUtilities.IsPositionOnNavMesh(position2))
		{
			return true;
		}
		return false;
	}

	private Vector2 GetIntermediateGoal(Vector2 goal, Mover pather, List<Obstacle> obstacles)
	{
		Vector2 result = goal;
		List<Vertex> list = new List<Vertex>();
		List<Vertex> list2 = new List<Vertex>();
		list.Add(s_nodes[0].Vertices[0]);
		s_nodes[0].Vertices[0].IsOpen = true;
		Vertex vertex = s_nodes[pather.Route.Length - 1].Vertices[0];
		Vertex vertex2 = null;
		while (list.Count > 0)
		{
			Vertex vertex3 = vertex2;
			int num = 0;
			int index = 0;
			foreach (Vertex item in list)
			{
				if (vertex2 == null || item.PathingDistance < vertex2.PathingDistance)
				{
					vertex2 = item;
					index = num;
				}
				num++;
			}
			if (vertex3 == vertex2)
			{
				vertex2 = null;
				continue;
			}
			if (vertex2 == vertex)
			{
				ConstructPath(list2, vertex2);
				break;
			}
			list.RemoveAt(index);
			vertex2.IsOpen = false;
			vertex2.IsClosed = true;
			foreach (Link link in vertex2.Links)
			{
				Vertex vertex4 = link.Vertex2;
				if (link.Vertex2 == vertex2)
				{
					vertex4 = link.Vertex1;
				}
				if (vertex4.IsClosed)
				{
					continue;
				}
				float num2 = vertex2.PathingDistance + link.Distance;
				if (!vertex4.IsOpen || num2 < vertex4.PathingDistance)
				{
					vertex4.FromVertex = vertex2;
					vertex4.PathingDistance = num2;
					if (!vertex4.IsOpen)
					{
						list.Add(vertex4);
						vertex4.IsOpen = true;
					}
				}
			}
		}
		if (list2.Count > 0)
		{
			result = list2[0].Position;
		}
		foreach (Vertex item2 in list2)
		{
			if (!NavMesh.Raycast(GameUtilities.V2ToV3(s_nodes[0].Vertices[0].Position), GameUtilities.V2ToV3(item2.Position), out var _, int.MaxValue) && !IntersectsObstacle(s_nodes[0].Vertices[0].Position, item2.Position, obstacles))
			{
				result = item2.Position;
			}
		}
		return result;
	}

	private void ConstructPath(List<Vertex> finalPath, Vertex currentVertex)
	{
		if (currentVertex != s_nodes[0].Vertices[0])
		{
			finalPath.Insert(0, currentVertex);
			ConstructPath(finalPath, currentVertex.FromVertex);
		}
	}
}

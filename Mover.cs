using System;
using System.Collections.Generic;
using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Navigation/Trenton Mover")]
public class Mover : MonoBehaviour
{
	[Flags]
	public enum BlockFlag
	{
		None = 0,
		Dynamic = 2,
		Edge = 4,
		Combat = 8,
		Error = 0x10
	}

	public enum TurnDirection
	{
		Undecided,
		Left,
		Right
	}

	public class ObstacleCluster
	{
		public List<Mover> Obstacles;

		public void Init()
		{
			Obstacles = new List<Mover>();
		}
	}

	public struct SoftSteeringTimer
	{
		public Mover Mover;

		public float Timer;
	}

	public delegate bool CanBumpDelegate(Mover other, bool staleMate);

	public delegate bool CanClipDelegate(Mover other);

	public delegate bool OnBumpedDelegate(GameObject other, Vector3 force);

	public delegate bool OverrideAvoidance(Mover other, out Vector3 force);

	internal class MoverObstacle
	{
		public Vector3 Center = Vector3.zero;

		public float Radius;

		public Mover Owner;

		public MoverObstacle()
		{
		}

		public MoverObstacle(Vector3 point, float rad)
		{
			Center = point;
			Radius = rad;
		}

		public MoverObstacle(Mover owner, Vector3 point, float rad)
		{
			Center = point;
			Radius = rad;
			Owner = owner;
		}
	}

	private const int MaxSoftSteeringTimers = 4;

	public static bool PopToGround = true;

	public float Radius = 0.5f;

	private float AvoidanceRadius = 1f;

	public float Acceleration = 1024f;

	public float AnimationRunSpeed = 4f;

	public float AnimationWalkSpeed = 2f;

	[Persistent]
	[Tooltip("A character's run speed. If it's negative we will treat it as 0. Do not access directly unless you want the real run speed.")]
	public float RunSpeed = 8f;

	[Persistent]
	[Tooltip("A character's walk speed. If it's negative we will treat it as 0. Do not access directly unless you want the real walk speed.")]
	public float WalkSpeed = 4f;

	[Tooltip("If true, this character can never be moved.")]
	public bool IsImmobile;

	[Tooltip("If true, the character will not try to avoid other characters and will pass through them.")]
	public bool ClipsThroughObstacles;

	private Vector3 m_goal = Vector3.zero;

	private float m_arrivalDist = 0.5f;

	private NavMeshPath m_path;

	private int m_lastCorner;

	private Vector3 m_nextCornerPos = Vector3.zero;

	private Vector3 m_lastPosition = Vector3.zero;

	private Vector3 m_lastReportedPosition = Vector3.zero;

	private AIController m_aiController;

	private Vector2 m_heading = Vector2.zero;

	private Vector2 m_desiredHeading = Vector2.zero;

	private Vector2 m_movementDirection = Vector2.zero;

	private float m_steeringSpeedModifier = 1f;

	private float m_speedModifier = 1f;

	private bool m_snapToFinalDestination;

	private bool m_isBeingNudged;

	private bool m_isCombatSteering;

	private Vector2 m_combatSteeringTarget = Vector2.zero;

	private Vector2 m_combatSteeringDir = Vector2.zero;

	private Vector3[] m_blockedRoute;

	private ObstacleCluster m_obstacleCluster;

	private Vector3 m_steerEnd = Vector3.zero;

	private Vector3 m_steerIntersection = Vector3.zero;

	private TurnDirection m_turnDirection;

	private TurnDirection m_forcedTurnDirection;

	private TurnDirection m_prevTurnDirection;

	private Mover m_forcedTurnObstacle;

	private bool m_ignoreAttackBlocking;

	private Mover m_pivotObstacle;

	private bool m_hasSteeredAroundPivotObstacle;

	private Mover m_prevSoftSteerObstacle;

	private Vector2 m_prevStationarySoftSteerOffset = Vector2.zero;

	private SoftSteeringTimer[] m_softSteeringTimers = new SoftSteeringTimer[4];

	private bool m_forceCombatPathing;

	private bool m_ignoreObstaclesWithinRange;

	private const float MaxSteering = (float)Math.PI * 4f;

	private const float MaxSubtleSteering = (float)Math.PI;

	private const float SubtleSteeringThreshhold = (float)Math.PI / 4f;

	private const float MaxAvoidanceAngle = 1.22173047f;

	private const float AvoidanceCastDistance = 8f;

	private const float AvoidanceSameDirectionAngle = (float)Math.PI / 4f;

	private float CosMaxSteering = Mathf.Cos((float)Math.PI * 4f);

	private float CosMaxSubtleSteering = Mathf.Cos((float)Math.PI);

	private float CosAvoidanceSameDirectionAngle = Mathf.Cos((float)Math.PI / 4f);

	private const float StopWhenBlockedDistance = 0.5f;

	public const float OverlapBuffer = 0.05f;

	private const float NavMeshCastDistance = 2f;

	private const float ReverseDirectionDistance = 0.2f;

	private const float DoorBlockedArrivalDistance = 3f;

	private const float SoftSteeringNudgeDistance = 0.025f;

	private float CosAvoidanceAtAngleLarge = Mathf.Cos(60f);

	private float CosAvoidanceAtAngleMedium = Mathf.Cos(30f);

	private float CosAvoidanceAtAngleSmall = Mathf.Cos(15f);

	private bool m_moveDirectly;

	private float m_desiredSpeed = 8f;

	private float m_currentSpeed;

	private float m_actualSpeed;

	private bool m_reachedLastGoal;

	private bool m_goalUnreachable;

	private GameObject m_blockedBy;

	private Door m_blockedByDoor;

	private bool m_pathBlocked;

	private bool m_frozen;

	private int m_blockFlags;

	private static List<Mover> s_moverList = new List<Mover>();

	private static List<Bounds> s_blockers = new List<Bounds>();

	private static List<Mover> s_potentialObstacles = new List<Mover>();

	private static List<Mover> s_clusterList = new List<Mover>();

	public bool ForceCombatPathing
	{
		get
		{
			return m_forceCombatPathing;
		}
		set
		{
			m_forceCombatPathing = value;
		}
	}

	public bool IgnoreObstaclesWithinRange
	{
		get
		{
			return m_ignoreObstaclesWithinRange;
		}
		set
		{
			m_ignoreObstaclesWithinRange = value;
		}
	}

	public Vector2 DesiredHeading => m_desiredHeading;

	public Vector2 MovementDirection => m_movementDirection;

	public bool IsCombatSteering => m_isCombatSteering;

	public Vector2 CombatSteeringTarget => m_combatSteeringTarget;

	public int LastWaypointIndex => m_lastCorner;

	public Vector3[] Route
	{
		get
		{
			if (HasGoal || m_blockedRoute == null)
			{
				return m_path.corners;
			}
			return m_blockedRoute;
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

	public float SpeedModifier
	{
		get
		{
			return m_speedModifier;
		}
		set
		{
			m_speedModifier = value;
		}
	}

	public float ArrivalDistance => m_arrivalDist;

	public Vector3 Goal => m_goal;

	public Vector3 NextCorner => m_nextCornerPos;

	public Vector3 FinalCorner
	{
		get
		{
			if (m_path != null && m_path.corners.Length != 0)
			{
				return m_path.corners[m_path.corners.Length - 1];
			}
			return m_nextCornerPos;
		}
	}

	public bool HasGoal
	{
		get
		{
			if (m_moveDirectly)
			{
				return true;
			}
			if (m_path != null && m_path.corners != null)
			{
				return m_path.corners.Length != 0;
			}
			return false;
		}
	}

	public bool ReachedGoal
	{
		get
		{
			return m_reachedLastGoal;
		}
		set
		{
			m_reachedLastGoal = value;
		}
	}

	public bool GoalUnreachable => m_goalUnreachable;

	public float Speed
	{
		get
		{
			if (!Frozen)
			{
				return m_currentSpeed;
			}
			return 0f;
		}
	}

	public float AnimationSpeed
	{
		get
		{
			if (Frozen)
			{
				return 0f;
			}
			if (m_currentSpeed > float.Epsilon && (double)m_currentSpeed < (double)m_desiredSpeed * 0.75)
			{
				return m_desiredSpeed * 0.75f;
			}
			return m_currentSpeed;
		}
	}

	public float AnimSpeedMultiplier
	{
		get
		{
			if (m_currentSpeed < float.Epsilon)
			{
				return 1f;
			}
			float num = 1f;
			num = ((!(DesiredSpeed > GetWalkSpeed())) ? (AnimationSpeed / AnimationWalkSpeed) : (AnimationSpeed / AnimationRunSpeed));
			float x = base.gameObject.transform.localScale.x;
			if (x > 0f)
			{
				num /= x;
			}
			return num;
		}
	}

	public float DesiredSpeed => m_desiredSpeed;

	public bool Frozen
	{
		get
		{
			return m_frozen;
		}
		set
		{
			if (value != m_frozen)
			{
				m_frozen = value;
				if (this.OnFrozenChanged != null)
				{
					this.OnFrozenChanged(base.gameObject, m_frozen);
				}
			}
			if (!m_frozen)
			{
				Stop();
			}
		}
	}

	public Vector3 LastPosition => m_lastReportedPosition;

	public bool IsBeingNudged
	{
		get
		{
			return m_isBeingNudged;
		}
		set
		{
			m_isBeingNudged = value;
		}
	}

	public bool SnapToFinalDestination
	{
		get
		{
			return m_snapToFinalDestination;
		}
		set
		{
			m_snapToFinalDestination = value;
		}
	}

	public bool Blocked => m_pathBlocked;

	public GameObject BlockedBy
	{
		get
		{
			return m_blockedBy;
		}
		set
		{
			m_blockedBy = value;
		}
	}

	public Door BlockedByDoor
	{
		get
		{
			return m_blockedByDoor;
		}
		set
		{
			m_blockedByDoor = value;
		}
	}

	public static List<Bounds> Blockers => s_blockers;

	public static List<Mover> Movers => s_moverList;

	public bool IgnoreAttackBlocking
	{
		get
		{
			return m_ignoreAttackBlocking;
		}
		set
		{
			m_ignoreAttackBlocking = value;
		}
	}

	public float BumpDistance { get; set; }

	public event EventHandler OnMovementStarted;

	public event EventHandler OnMovementStopped;

	public event EventHandler OnMovementBlocked;

	public event EventHandler OnMovementUpdated;

	public event EventHandler OnMovementLateUpdated;

	public event Action<GameObject, bool> OnFrozenChanged;

	private void Awake()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component.isKinematic)
		{
			component.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
	}

	private void Start()
	{
		AvoidanceRadius = Mathf.Max(Radius, AvoidanceRadius);
		if (m_path == null)
		{
			m_path = new NavMeshPath();
			m_desiredSpeed = GetRunSpeed();
			m_goal = base.transform.position;
			m_lastPosition = base.transform.position;
			m_lastReportedPosition = m_lastPosition;
		}
	}

	public void OnEnable()
	{
		if (!s_moverList.Contains(this))
		{
			s_moverList.Add(this);
		}
		m_goal = base.transform.position;
		m_lastPosition = base.transform.position;
		m_lastReportedPosition = m_lastPosition;
		m_speedModifier = 1f;
	}

	public void OnDisable()
	{
		if ((base.gameObject == null || !base.gameObject.activeSelf) && s_moverList.Contains(this))
		{
			s_moverList.Remove(this);
		}
		for (int i = 0; i < 4; i++)
		{
			m_softSteeringTimers[i].Mover = null;
			m_softSteeringTimers[i].Timer = 0f;
		}
	}

	public void OnDestroy()
	{
		if (s_moverList.Contains(this))
		{
			s_moverList.Remove(this);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void CopyDataFrom(Mover other)
	{
		Radius = other.Radius;
		Acceleration = other.Acceleration;
		AnimationRunSpeed = other.AnimationRunSpeed;
		AnimationWalkSpeed = other.AnimationWalkSpeed;
		RunSpeed = other.RunSpeed;
		WalkSpeed = other.WalkSpeed;
	}

	public void ClearDesiredHeading()
	{
		m_desiredHeading = GameUtilities.V3ToV2(base.transform.forward);
	}

	public bool IsPathValid()
	{
		if (m_path == null || m_path.corners == null || m_path.corners.Length == 0)
		{
			return false;
		}
		return true;
	}

	private void UpdateHeading(float dt)
	{
		if (m_aiController == null)
		{
			return;
		}
		m_steeringSpeedModifier = 1f;
		if (m_moveDirectly)
		{
			m_desiredHeading = GameUtilities.V3Subtract2D(m_goal, m_lastPosition);
			m_desiredHeading.Normalize();
			return;
		}
		if (m_isCombatSteering)
		{
			m_desiredHeading = m_combatSteeringTarget - GameUtilities.V3ToV2(base.transform.position);
		}
		else if (m_path.corners.Length > 1)
		{
			if (m_lastCorner > 0)
			{
				m_desiredHeading = GameUtilities.V3Subtract2D(m_nextCornerPos, base.transform.position);
			}
			else
			{
				m_desiredHeading = GameUtilities.V3Subtract2D(m_path.corners[1], base.transform.position);
			}
		}
		else
		{
			m_desiredHeading = GameUtilities.V3ToV2(base.transform.forward);
		}
		if (m_desiredHeading.sqrMagnitude > float.Epsilon)
		{
			m_desiredHeading.Normalize();
		}
		else
		{
			m_desiredHeading = GameUtilities.V3ToV2(base.transform.forward);
		}
		bool overlaps = false;
		for (int i = 0; i < 4; i++)
		{
			if (m_softSteeringTimers[i].Mover != null)
			{
				m_softSteeringTimers[i].Timer -= Time.deltaTime;
				if (m_softSteeringTimers[i].Timer <= float.Epsilon)
				{
					m_softSteeringTimers[i].Mover = null;
				}
			}
		}
		if (ClipsThroughObstacles)
		{
			m_heading = m_desiredHeading;
		}
		else if (!m_forceCombatPathing && (!GameState.InCombat || PerformsSoftSteering() || Cutscene.CutsceneActive))
		{
			Mover moverToAvoid = GetMoverToAvoid(out overlaps);
			if (moverToAvoid == null)
			{
				m_heading = m_desiredHeading;
			}
			else
			{
				SteerAroundObstacle(moverToAvoid, dt, overlaps);
			}
		}
		else
		{
			List<Mover> obstaclesToAvoid = GetObstaclesToAvoid(8f);
			if (obstaclesToAvoid.Count > 0)
			{
				CreateObstacleClusters(obstaclesToAvoid);
				if (m_forcedTurnDirection != 0)
				{
					m_turnDirection = m_forcedTurnDirection;
				}
				Mover mover = SteerAroundObstacles(obstaclesToAvoid, base.transform.position, m_desiredHeading, 8f, isInitialCast: true);
				if (mover != null)
				{
					if (m_pivotObstacle == null)
					{
						m_pivotObstacle = mover;
					}
					else if (!mover.m_obstacleCluster.Obstacles.Contains(mover))
					{
						m_pivotObstacle = mover;
						m_hasSteeredAroundPivotObstacle = false;
					}
					else if (mover.m_obstacleCluster.Obstacles.Count > 2)
					{
						if (m_pivotObstacle == mover)
						{
							if (m_hasSteeredAroundPivotObstacle)
							{
								PushPathBlockedState(mover.m_obstacleCluster.Obstacles, forceBlock: true);
								return;
							}
						}
						else
						{
							m_hasSteeredAroundPivotObstacle = true;
						}
					}
					if (mover.m_obstacleCluster.Obstacles.Count > 1 && m_forcedTurnDirection != 0)
					{
						bool flag = false;
						for (int j = 0; j < mover.m_obstacleCluster.Obstacles.Count; j++)
						{
							if (mover.m_obstacleCluster.Obstacles[j] == m_forcedTurnObstacle)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							ClearForcedTurnDirection();
						}
					}
					SteerAroundCluster(mover, m_forcedTurnDirection != TurnDirection.Undecided);
				}
				else
				{
					m_steerIntersection = Vector3.zero;
					m_steerEnd = Vector3.zero;
					ClearForcedTurnDirection();
				}
			}
			else
			{
				ClearForcedTurnDirection();
			}
		}
		Vector2 rhs = GameUtilities.V3ToV2(base.transform.forward);
		float num = Vector2.Dot(m_desiredHeading, rhs);
		float num2 = (float)Math.PI * 4f;
		float num3 = CosMaxSteering;
		if (num > (float)Math.PI / 4f)
		{
			num2 = (float)Math.PI;
			num3 = CosMaxSubtleSteering;
		}
		float num4 = num3 * dt;
		AIState currentState = m_aiController.StateManager.CurrentState;
		if (num >= 1f - num4 || currentState is AI.Plan.WaitForClearPath || currentState is AI.Player.WaitForClearPath)
		{
			m_heading = m_desiredHeading;
		}
		else
		{
			m_heading = GameUtilities.V3ToV2(Vector3.RotateTowards(base.transform.forward, GameUtilities.V2ToV3(m_desiredHeading), num2 * dt, 0f));
		}
	}

	private Mover GetMoverToAvoid(out bool overlaps)
	{
		overlaps = false;
		Mover mover = null;
		float num = float.MaxValue;
		float num2 = 64f;
		float num3 = GameUtilities.V3SqrDistance2D(base.transform.position, m_goal);
		int num4 = 0;
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover2 = s_moverList[i];
			if (mover2 == this || !mover2.gameObject.activeInHierarchy || mover2.ClipsThroughObstacles)
			{
				continue;
			}
			Vector2 lhs = GameUtilities.V3Subtract2D(mover2.transform.position, base.transform.position);
			float sqrMagnitude = lhs.sqrMagnitude;
			float num5 = Radius + mover2.Radius;
			if (sqrMagnitude < num5 * num5)
			{
				num4++;
			}
			if (sqrMagnitude > num2 || sqrMagnitude > num3 || !mover2.IsPathingObstacle())
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < 4; j++)
			{
				if (m_softSteeringTimers[j].Mover == mover2)
				{
					flag = true;
					break;
				}
			}
			if (flag || GameUtilities.V3SqrDistance2D(mover2.transform.position, m_goal) < float.Epsilon)
			{
				continue;
			}
			float magnitude = lhs.magnitude;
			float num6 = Vector2.Dot(lhs, m_desiredHeading);
			if (num6 < float.Epsilon || num6 > num || Mathf.Sqrt(magnitude * magnitude - num6 * num6) > num5)
			{
				continue;
			}
			if (mover2.HasGoal)
			{
				float num7 = 0f - Vector2.Dot(mover2.m_heading, m_desiredHeading);
				if (num7 > 0f)
				{
					if (num7 < CosAvoidanceAtAngleLarge)
					{
						m_steeringSpeedModifier = Mathf.Min(0.25f, m_steeringSpeedModifier);
					}
					else if (num7 < CosAvoidanceAtAngleMedium)
					{
						m_steeringSpeedModifier = Mathf.Min(0.5f, m_steeringSpeedModifier);
					}
					else if (num7 < CosAvoidanceAtAngleSmall)
					{
						m_steeringSpeedModifier = Mathf.Min(0.75f, m_steeringSpeedModifier);
					}
				}
				if (num7 < CosAvoidanceSameDirectionAngle)
				{
					continue;
				}
			}
			mover = mover2;
			num = num6;
		}
		if (m_prevSoftSteerObstacle != null && m_prevSoftSteerObstacle != mover)
		{
			float num8 = GameUtilities.V3SqrDistance2D(base.transform.position, m_prevSoftSteerObstacle.transform.position);
			float num9 = (Radius + m_prevSoftSteerObstacle.Radius) * 2f;
			if (num8 < num9 * num9)
			{
				for (int k = 0; k < 4; k++)
				{
					if (!(m_softSteeringTimers[k].Mover == null))
					{
						continue;
					}
					bool flag2 = true;
					for (int l = 0; l < 4; l++)
					{
						if (m_softSteeringTimers[l].Mover == m_prevSoftSteerObstacle)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						m_softSteeringTimers[k].Mover = m_prevSoftSteerObstacle;
						m_softSteeringTimers[k].Timer = 1f;
						break;
					}
				}
			}
		}
		m_prevSoftSteerObstacle = mover;
		if (num4 > 1)
		{
			overlaps = true;
		}
		else if (num4 == 1)
		{
			for (int m = 0; m < 4; m++)
			{
				if (m_softSteeringTimers[m].Mover != null)
				{
					overlaps = true;
					break;
				}
			}
		}
		return mover;
	}

	private List<Mover> GetObstaclesToAvoid(float range)
	{
		s_potentialObstacles.Clear();
		float num = range * range;
		GameObject currentTarget = m_aiController.CurrentTarget;
		Mover mover = null;
		if (currentTarget != null)
		{
			mover = currentTarget.GetComponent<Mover>();
		}
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover2 = s_moverList[i];
			if (!(mover2 == this) && !(mover2 == mover) && mover2.gameObject.activeInHierarchy && mover2.IsPathingObstacle() && (!mover2.IsMoving() || !(Vector2.Dot(m_desiredHeading, mover2.m_desiredHeading) > 0f) || (!(m_desiredSpeed < GetRunSpeed()) && !(mover2.m_desiredSpeed >= mover2.GetRunSpeed()))) && !(GameUtilities.V3Subtract2D(mover2.transform.position, base.transform.position).sqrMagnitude > num) && (!m_ignoreObstaclesWithinRange || !(GameUtilities.V3Distance2D(mover2.transform.position, m_goal) + mover2.Radius < m_arrivalDist)))
			{
				mover2.m_obstacleCluster = null;
				s_potentialObstacles.Add(mover2);
			}
		}
		return s_potentialObstacles;
	}

	private int CreateObstacleClusters(List<Mover> obstacles)
	{
		int num = 0;
		ObstacleCluster[] obstacleClusters = PathFindingManager.Instance.ObstacleClusters;
		for (int i = 0; i < obstacles.Count; i++)
		{
			Mover mover = obstacles[i];
			if (mover.m_obstacleCluster == null)
			{
				ObstacleCluster obstacleCluster = obstacleClusters[num];
				obstacleCluster.Obstacles.Clear();
				num++;
				AddObstacleToCluster(obstacles, mover, obstacleCluster);
			}
		}
		return num;
	}

	private void AddObstacleToCluster(List<Mover> obstacles, Mover obstacle, ObstacleCluster cluster)
	{
		cluster.Obstacles.Add(obstacle);
		obstacle.m_obstacleCluster = cluster;
		float num = Radius + Radius + obstacle.Radius;
		for (int i = 0; i < obstacles.Count; i++)
		{
			Mover mover = obstacles[i];
			if (mover.m_obstacleCluster == null && GameUtilities.V3Distance2D(obstacle.transform.position, mover.transform.position) < num + mover.Radius)
			{
				AddObstacleToCluster(obstacles, mover, cluster);
			}
		}
	}

	public bool IsPathClear(List<Mover> potentialObstacles, Vector3 startPos)
	{
		float num = Mathf.Cos(1.22173047f);
		Vector2 vector = GameUtilities.V3Subtract2D(m_nextCornerPos, startPos);
		float magnitude = vector.magnitude;
		vector.Normalize();
		for (int i = 0; i < potentialObstacles.Count; i++)
		{
			Mover mover = potentialObstacles[i];
			Vector2 vector2 = GameUtilities.V3Subtract2D(mover.transform.position, startPos);
			float sqrMagnitude = vector2.sqrMagnitude;
			float num2 = magnitude + mover.Radius + 0.05f;
			if (sqrMagnitude > num2 * num2 || GameUtilities.V3SqrDistance2D(mover.transform.position, m_goal) < mover.Radius * mover.Radius)
			{
				continue;
			}
			Vector2 lhs = vector2;
			lhs.Normalize();
			if (!(Vector2.Dot(lhs, vector) < num))
			{
				float num3 = Vector2.Dot(vector, vector2);
				Vector2 vector3 = GameUtilities.V3ToV2(startPos) + vector * num3;
				Vector2 vector4 = GameUtilities.V3ToV2(mover.transform.position) - vector3;
				float num4 = Radius + mover.Radius + 0.05f;
				if (!(vector4.sqrMagnitude > num4 * num4 - 0.001f))
				{
					return false;
				}
			}
		}
		return true;
	}

	private Mover SteerAroundObstacles(List<Mover> potentialObstacles, Vector3 patherPos, Vector2 desiredHeading, float castDistance, bool isInitialCast)
	{
		Mover mover = null;
		float num = float.MaxValue;
		Vector2 vector = Vector2.zero;
		float num2 = Mathf.Cos(1.22173047f);
		float num3 = GameUtilities.V3Distance2D(m_nextCornerPos, base.transform.position);
		for (int i = 0; i < potentialObstacles.Count; i++)
		{
			Mover mover2 = potentialObstacles[i];
			Vector2 vector2 = GameUtilities.V3Subtract2D(mover2.transform.position, patherPos);
			float sqrMagnitude = vector2.sqrMagnitude;
			float num4 = castDistance + mover2.Radius + mover2.Radius;
			num4 *= num4;
			if (sqrMagnitude > num4 || sqrMagnitude > num)
			{
				continue;
			}
			float num5 = num3 + mover2.Radius + 0.05f;
			if (((m_turnDirection == TurnDirection.Undecided || isInitialCast) && sqrMagnitude > num5 * num5) || GameUtilities.V3SqrDistance2D(mover2.transform.position, m_goal) < mover2.Radius * mover2.Radius)
			{
				continue;
			}
			Vector2 vector3 = vector2;
			vector3.Normalize();
			if (!(Vector2.Dot(vector3, desiredHeading) < num2))
			{
				float num6 = Vector2.Dot(desiredHeading, vector2);
				Vector2 vector4 = GameUtilities.V3ToV2(patherPos) + desiredHeading * num6;
				Vector2 vector5 = GameUtilities.V3ToV2(mover2.transform.position) - vector4;
				float num7 = Radius + mover2.Radius + 0.05f;
				if (!(vector5.sqrMagnitude > num7 * num7 - 0.001f))
				{
					float magnitude = vector5.magnitude;
					float num8 = Mathf.Sqrt(num7 * num7 - magnitude * magnitude);
					m_steerIntersection = GameUtilities.V2ToV3(desiredHeading * (num6 - num8)) + patherPos;
					mover = mover2;
					num = sqrMagnitude;
					vector = vector3;
				}
			}
		}
		if (mover == null)
		{
			return null;
		}
		bool flag = true;
		if (m_turnDirection == TurnDirection.Undecided && m_prevTurnDirection != 0 && !mover.IsMoving())
		{
			m_turnDirection = m_prevTurnDirection;
		}
		switch (m_turnDirection)
		{
		case TurnDirection.Left:
			flag = true;
			break;
		case TurnDirection.Right:
			flag = false;
			break;
		case TurnDirection.Undecided:
		{
			Vector3 rhs = GameUtilities.V2ToV3(desiredHeading);
			if (mover.m_obstacleCluster.Obstacles.Count <= 1)
			{
				if (Vector2.Dot(vector, desiredHeading) < 0.9659f && Vector3.Cross(GameUtilities.V2ToV3(vector), rhs).y > 0f)
				{
					flag = false;
				}
				break;
			}
			int num9 = 0;
			int num10 = 0;
			for (int j = 0; j < mover.m_obstacleCluster.Obstacles.Count; j++)
			{
				if (Vector3.Cross(mover.m_obstacleCluster.Obstacles[j].transform.position - patherPos, rhs).y > 0f)
				{
					num9++;
				}
				else
				{
					num10++;
				}
			}
			if (num9 > num10)
			{
				flag = false;
			}
			break;
		}
		}
		Vector2 vector6;
		if (flag)
		{
			vector6 = new Vector2(0f - vector.y, vector.x);
			m_turnDirection = TurnDirection.Left;
		}
		else
		{
			vector6 = new Vector2(vector.y, 0f - vector.x);
			m_turnDirection = TurnDirection.Right;
		}
		vector6.Normalize();
		vector6 *= Radius + mover.Radius + 0.05f;
		Vector2 vector7 = GameUtilities.V3ToV2(mover.transform.position);
		Vector2 vector8 = GameUtilities.V3ToV2(base.transform.position);
		m_steerEnd = GameUtilities.V2ToV3(vector7 + vector6 + -vector * mover.Radius);
		m_desiredHeading = GameUtilities.V3ToV2(m_steerEnd) - vector8;
		m_desiredHeading.Normalize();
		return mover;
	}

	private void SteerAroundCluster(Mover primaryObstacle, bool isRepathing)
	{
		s_clusterList.Clear();
		s_clusterList.AddRange(primaryObstacle.m_obstacleCluster.Obstacles);
		if (!isRepathing)
		{
			s_clusterList.Remove(primaryObstacle);
		}
		Mover mover = null;
		Mover mover2 = null;
		int num = 0;
		do
		{
			if (IsPathClear(primaryObstacle.m_obstacleCluster.Obstacles, m_steerIntersection))
			{
				m_steerEnd = m_nextCornerPos;
				break;
			}
			Vector2 desiredHeading = GameUtilities.V3Subtract2D(m_steerEnd, m_steerIntersection);
			float castDistance = desiredHeading.magnitude + Radius;
			desiredHeading.Normalize();
			mover2 = SteerAroundObstacles(s_clusterList, m_steerIntersection, desiredHeading, castDistance, isInitialCast: false);
			if (mover2 != null)
			{
				s_clusterList.Remove(mover2);
				mover = mover2;
				num++;
			}
		}
		while (mover2 != null);
		Vector2 vector = GameUtilities.V3Subtract2D(m_steerEnd, m_steerIntersection);
		Vector2 vector2 = GameUtilities.V3Subtract2D(m_nextCornerPos, base.transform.position);
		Vector2 vector3 = vector;
		float magnitude = vector.magnitude;
		vector3.Normalize();
		if (magnitude * magnitude > vector2.sqrMagnitude)
		{
			Vector2 rhs = GameUtilities.V3Subtract2D(m_nextCornerPos, m_steerIntersection);
			rhs.Normalize();
			if (Vector2.Dot(vector3, rhs) > 0f)
			{
				magnitude = vector2.magnitude;
			}
		}
		Vector3 vector4 = GameUtilities.V2ToV3(vector3);
		vector4 *= magnitude - 0.01f;
		vector4 += m_steerIntersection;
		NavMeshHit hit;
		bool flag = NavMesh.Raycast(m_steerIntersection, vector4, out hit, int.MaxValue);
		float num2 = GameUtilities.V3SqrDistance2D(hit.position, vector4);
		if (flag)
		{
			Vector3 vector5 = vector4 - base.transform.position;
			Vector3 vector6 = m_nextCornerPos - base.transform.position;
			Vector3 vector7 = Vector3.zero;
			if (m_turnDirection == TurnDirection.Right)
			{
				vector7 = Vector3.Cross(vector5, vector6);
			}
			else if (m_turnDirection == TurnDirection.Left)
			{
				vector7 = Vector3.Cross(vector6, vector5);
			}
			if (vector7.y > 0f)
			{
				flag = false;
			}
		}
		if (flag && num2 > 0.04f)
		{
			if (m_lastCorner == m_path.corners.Length - 1 && GetDistanceToWaypoint() <= m_arrivalDist)
			{
				m_reachedLastGoal = true;
				m_snapToFinalDestination = false;
				Stop();
				return;
			}
			if (!isRepathing)
			{
				if (m_turnDirection == TurnDirection.Left)
				{
					m_turnDirection = TurnDirection.Right;
				}
				else
				{
					m_turnDirection = TurnDirection.Left;
				}
				Vector2 a = -GameUtilities.V3Subtract2D(m_steerEnd, primaryObstacle.transform.position);
				a.x += primaryObstacle.transform.position.x;
				a.y += primaryObstacle.transform.position.z;
				m_steerEnd = GameUtilities.V2ToV3(a);
				SteerAroundCluster(primaryObstacle, isRepathing: true);
				return;
			}
			Vector2 desiredHeading2 = GameUtilities.V3Subtract2D(primaryObstacle.transform.position, base.transform.position);
			float num3 = 0.5f + Radius + primaryObstacle.Radius;
			num3 *= num3;
			if (!(desiredHeading2.sqrMagnitude > num3))
			{
				PushPathBlockedState(primaryObstacle.m_obstacleCluster.Obstacles, forceBlock: false);
				return;
			}
			m_desiredHeading = desiredHeading2;
			m_desiredHeading.Normalize();
		}
		else if (isRepathing)
		{
			m_forcedTurnDirection = m_turnDirection;
			m_forcedTurnObstacle = primaryObstacle;
			m_pivotObstacle = primaryObstacle;
			m_hasSteeredAroundPivotObstacle = false;
		}
		if (!(mover != null) || !(mover != primaryObstacle))
		{
			return;
		}
		if (num >= 3)
		{
			float num4 = Radius + Radius + mover.Radius + primaryObstacle.Radius;
			if (GameUtilities.V3SqrDistance2D(mover.transform.position, primaryObstacle.transform.position) < num4 * num4)
			{
				Vector2 desiredHeading3 = GameUtilities.V3Subtract2D(primaryObstacle.transform.position, base.transform.position);
				float num5 = Radius + primaryObstacle.Radius + 0.2f;
				if (desiredHeading3.sqrMagnitude < num5 * num5)
				{
					PushPathBlockedState(primaryObstacle.m_obstacleCluster.Obstacles, forceBlock: true);
					ClearAllTurnDirections();
				}
				else
				{
					m_desiredHeading = desiredHeading3;
					m_desiredHeading.Normalize();
				}
				return;
			}
		}
		Vector2 desiredHeading4 = GameUtilities.V3Subtract2D(m_steerIntersection, base.transform.position);
		float magnitude2 = desiredHeading4.magnitude;
		s_clusterList.Add(mover);
		desiredHeading4 /= magnitude2;
		SteerAroundObstacles(s_clusterList, base.transform.position, desiredHeading4, magnitude2, isInitialCast: false);
		Vector3 vector8 = m_steerEnd - base.transform.position;
		vector8.Normalize();
		if (vector2.sqrMagnitude < magnitude * magnitude)
		{
			vector8 *= vector2.magnitude;
		}
		else
		{
			vector8 *= magnitude;
		}
		if (NavMesh.Raycast(base.transform.position, vector8 + base.transform.position, out hit, int.MaxValue))
		{
			Vector2 desiredHeading5 = GameUtilities.V3Subtract2D(primaryObstacle.transform.position, base.transform.position);
			float num6 = Radius + primaryObstacle.Radius + 0.2f;
			if (desiredHeading5.sqrMagnitude < num6 * num6)
			{
				if (isRepathing)
				{
					PushPathBlockedState(primaryObstacle.m_obstacleCluster.Obstacles, forceBlock: false);
					return;
				}
				if (m_turnDirection == TurnDirection.Left)
				{
					m_turnDirection = TurnDirection.Right;
				}
				else
				{
					m_turnDirection = TurnDirection.Left;
				}
				Vector2 a2 = -GameUtilities.V3Subtract2D(m_steerEnd, primaryObstacle.transform.position);
				a2.x += primaryObstacle.transform.position.x;
				a2.y += primaryObstacle.transform.position.z;
				m_steerEnd = GameUtilities.V2ToV3(a2);
				SteerAroundCluster(primaryObstacle, isRepathing: true);
			}
			else
			{
				m_desiredHeading = desiredHeading5;
				m_desiredHeading.Normalize();
			}
		}
		else if (isRepathing)
		{
			m_forcedTurnDirection = m_turnDirection;
			m_forcedTurnObstacle = primaryObstacle;
			m_pivotObstacle = primaryObstacle;
			m_hasSteeredAroundPivotObstacle = false;
		}
	}

	public static bool IsPathBlockedByObstacle(Mover pather, Vector3 goal)
	{
		float magnitude = GameUtilities.V3Subtract2D(pather.transform.position, goal).magnitude;
		Vector2 vector = GameUtilities.V3Subtract2D(goal, pather.transform.position);
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (mover == pather || !mover.gameObject.activeInHierarchy)
			{
				continue;
			}
			Vector2 vector2 = GameUtilities.V3Subtract2D(mover.transform.position, pather.transform.position);
			float sqrMagnitude = vector2.sqrMagnitude;
			float num = magnitude + pather.Radius + mover.Radius + 0.05f;
			if (!(sqrMagnitude > num * num))
			{
				Vector2 vector3 = vector2;
				vector3.Normalize();
				float num2 = Vector2.Dot(vector, vector2);
				Vector2 vector4 = GameUtilities.V3ToV2(pather.transform.position) + vector * num2;
				Vector2 vector5 = GameUtilities.V3ToV2(mover.transform.position) - vector4;
				float num3 = pather.Radius + mover.Radius + 0.05f;
				if (vector5.sqrMagnitude < num3 * num3 - 0.001f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SteerAroundObstacle(Mover obstacle, float dt, bool overlaps)
	{
		Vector2 desiredHeading = m_desiredHeading;
		Vector2 vector = GameUtilities.V3ToV2(obstacle.transform.position);
		Vector2 vector2 = GameUtilities.V3ToV2(base.transform.position);
		float num = Vector2.Dot(vector - vector2, m_desiredHeading);
		Vector2 vector3 = vector2 + m_desiredHeading * num;
		if ((vector - vector3).sqrMagnitude > AvoidanceRadius * AvoidanceRadius)
		{
			return;
		}
		bool num2 = obstacle.IsMoving();
		bool flag = false;
		if (!num2 && obstacle == m_prevSoftSteerObstacle && m_prevStationarySoftSteerOffset.sqrMagnitude > float.Epsilon)
		{
			PartyMemberAI component = obstacle.GetComponent<PartyMemberAI>();
			if (component == null || !component.enabled)
			{
				flag = true;
			}
			else
			{
				m_prevStationarySoftSteerOffset = Vector2.zero;
			}
		}
		else
		{
			m_prevStationarySoftSteerOffset = Vector2.zero;
		}
		Vector2 vector4 = ((!(num > float.Epsilon)) ? new Vector2(0f - m_desiredHeading.y, m_desiredHeading.x) : (vector3 - vector));
		if (flag && Vector2.Dot(vector4, m_prevStationarySoftSteerOffset) < float.Epsilon)
		{
			vector4 = -vector4;
		}
		if (!num2)
		{
			m_prevStationarySoftSteerOffset = vector4;
		}
		float num3 = Radius + obstacle.Radius + 0.05f + 0.1f;
		if (overlaps)
		{
			num3 *= 0.4f;
		}
		vector4.Normalize();
		vector4 *= num3;
		m_desiredHeading = vector + vector4 - vector2;
		m_desiredHeading.Normalize();
		float b = m_desiredSpeed * m_speedModifier * m_steeringSpeedModifier;
		b = Mathf.Max(0f, b);
		float num4 = b * dt;
		if (num4 < 0.2f)
		{
			num4 = 0.2f;
		}
		if (!GameUtilities.IsPositionOnNavMesh(base.transform.position + GameUtilities.V2ToV3(m_desiredHeading) * num4))
		{
			m_desiredHeading = desiredHeading;
			AddSoftSteeringTimer(obstacle);
		}
	}

	private void AddSoftSteeringTimer(Mover obstacle)
	{
		for (int i = 0; i < 4; i++)
		{
			if (!(m_softSteeringTimers[i].Mover == null))
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < 4; j++)
			{
				if (m_softSteeringTimers[j].Mover == obstacle)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				m_softSteeringTimers[i].Mover = obstacle;
				m_softSteeringTimers[i].Timer = 1f;
				break;
			}
		}
	}

	private Vector3 SpreadOut(Vector3 targetPos)
	{
		if (m_aiController == null || !IsMoving())
		{
			return targetPos;
		}
		if (m_aiController.IsPet)
		{
			return targetPos;
		}
		float num = float.MaxValue;
		Mover mover = null;
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover2 = s_moverList[i];
			if (mover2 == null || mover2 == this || !mover2.IsMoving())
			{
				continue;
			}
			float num2 = GameUtilities.V3SqrDistance2D(targetPos, mover2.transform.position);
			if (num2 < num)
			{
				float num3 = Radius + mover2.Radius;
				if (num2 < num3 * num3)
				{
					mover = mover2;
					num = num2;
				}
			}
		}
		if (mover != null)
		{
			Vector2 vector = GameUtilities.V3Subtract2D(targetPos, mover.transform.position);
			if (vector.sqrMagnitude > float.Epsilon)
			{
				vector.Normalize();
			}
			else
			{
				vector = Vector2.right;
			}
			targetPos += GameUtilities.V2ToV3(vector * 0.025f);
		}
		return targetPos;
	}

	public bool IsPartialPath()
	{
		if (m_path != null)
		{
			return m_path.status == NavMeshPathStatus.PathPartial;
		}
		return false;
	}

	private Vector3 CalcMoveDirection(out float distanceToCorner)
	{
		if (m_moveDirectly)
		{
			Vector3 result = m_goal - m_lastPosition;
			distanceToCorner = result.magnitude;
			result.Normalize();
			return result;
		}
		Vector3 nextCornerPos = m_nextCornerPos;
		Vector3 vector = nextCornerPos - base.transform.position;
		distanceToCorner = GameUtilities.V3Distance2D(nextCornerPos, base.transform.position);
		Vector3 moveDir = vector;
		if (moveDir.sqrMagnitude > float.Epsilon)
		{
			moveDir.Normalize();
		}
		AdjustForAvoidance(ref moveDir);
		return moveDir;
	}

	private void AdjustForAvoidance(ref Vector3 moveDir)
	{
	}

	public void Push(GameObject attacker, Vector3 direction, float distance, float speed, bool lockOrientation, bool orientBackwards)
	{
		if (!IsImmobile)
		{
			m_aiController.CancelCurrentAttack();
			AIController.BreakAllEngagements(base.gameObject);
			PushedBack pushedBack = AIStateManager.StatePool.Allocate<PushedBack>();
			m_aiController.StateManager.PushState(pushedBack);
			pushedBack.InitPush(attacker, base.transform.position, direction, distance, speed, lockOrientation, orientBackwards);
		}
	}

	private void Update()
	{
		AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
		if (aIController != null)
		{
			m_aiController = aIController;
		}
		UpdateMovement();
		Vector2 vector = (m_isCombatSteering ? m_combatSteeringTarget : ((!m_moveDirectly) ? GameUtilities.V3ToV2(m_nextCornerPos) : GameUtilities.V3ToV2(m_goal)));
		Vector2 movementDirection = vector - GameUtilities.V3ToV2(base.transform.position);
		if (movementDirection.sqrMagnitude > float.Epsilon)
		{
			m_movementDirection = movementDirection;
			m_movementDirection.Normalize();
		}
		if (this.OnMovementUpdated != null)
		{
			this.OnMovementUpdated(this, EventArgs.Empty);
		}
		if (m_blockFlags != 0 && !GameUtilities.IsPositionOnNavMesh(base.transform.position))
		{
			base.transform.position = GameUtilities.NearestUnoccupiedLocation(base.transform.position, Radius, 12f, this);
			if (m_blockedByDoor != null)
			{
				m_reachedLastGoal = true;
			}
		}
	}

	private void UpdateMovement()
	{
		BumpDistance = 0f;
		if (Time.deltaTime < float.Epsilon || m_frozen)
		{
			return;
		}
		m_currentSpeed = Mathf.Max(0f, m_currentSpeed);
		m_prevTurnDirection = m_turnDirection;
		m_turnDirection = TurnDirection.Undecided;
		float num = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);
		if (!m_moveDirectly && (!HasGoal || m_path.status == NavMeshPathStatus.PathInvalid || m_path.corners.Length == 0))
		{
			m_actualSpeed = (base.transform.position - m_lastPosition).magnitude;
			m_actualSpeed /= num;
			m_lastReportedPosition = m_lastPosition;
			m_lastPosition = base.transform.position;
			return;
		}
		UpdateHeading(num);
		Vector3 vector = GameUtilities.V2ToV3(m_desiredHeading);
		float currentSpeed = m_currentSpeed;
		if (m_currentSpeed < m_desiredSpeed)
		{
			m_currentSpeed += m_desiredSpeed * Acceleration * num;
		}
		if (m_currentSpeed > m_desiredSpeed)
		{
			m_currentSpeed = m_desiredSpeed;
		}
		m_currentSpeed = m_currentSpeed * m_speedModifier * m_steeringSpeedModifier;
		m_currentSpeed = Mathf.Max(0f, m_currentSpeed);
		float num2 = (currentSpeed * num + m_currentSpeed * num) * 0.5f;
		Vector3 vector2 = base.transform.position + vector * num2;
		bool flag = false;
		if (m_isCombatSteering)
		{
			if (Vector2.Dot(m_combatSteeringDir, m_combatSteeringTarget - GameUtilities.V3ToV2(vector2)) <= 0f)
			{
				vector2 = GetUnoccupiedPosition(vector2);
				base.transform.position = vector2;
				RecalculatePath();
				return;
			}
		}
		else
		{
			float distanceToWaypoint = GetDistanceToWaypoint();
			float num3 = 0.5f;
			if (m_moveDirectly || m_lastCorner == m_path.corners.Length - 1)
			{
				if (num3 > m_arrivalDist)
				{
					num3 = m_arrivalDist;
				}
				if (m_moveDirectly || m_path.corners.Length > 1)
				{
					Vector3 vector3 = new Vector3(m_nextCornerPos.x, base.transform.position.y, m_nextCornerPos.z);
					Vector3 lhs = vector3 - m_lastPosition;
					if (!m_moveDirectly)
					{
						lhs = m_nextCornerPos - m_path.corners[m_lastCorner - 1];
					}
					lhs.Normalize();
					Vector3 rhs = vector3 - vector2;
					rhs.Normalize();
					if (distanceToWaypoint < Radius * 0.5f && Vector3.Dot(lhs, rhs) < 0f)
					{
						num3 = distanceToWaypoint;
					}
				}
			}
			bool flag2 = distanceToWaypoint <= num3;
			if (!flag2 && m_lastCorner > 0 && m_path.corners.Length > 2 && m_lastCorner < m_path.corners.Length - 1)
			{
				Vector2 lhs2 = GameUtilities.V3Subtract2D(m_path.corners[m_lastCorner - 1], m_path.corners[m_lastCorner]);
				Vector2 rhs2 = GameUtilities.V3Subtract2D(base.transform.position, m_path.corners[m_lastCorner]);
				lhs2.Normalize();
				rhs2.Normalize();
				if (Vector2.Dot(lhs2, rhs2) < 0f)
				{
					flag2 = true;
					if (m_blockedByDoor == null)
					{
						flag = true;
					}
				}
			}
			if (flag2)
			{
				ClearForcedTurnDirection();
				if (UpdateCornerTransition(vector2))
				{
					return;
				}
				if (m_moveDirectly || m_lastCorner == m_path.corners.Length - 1)
				{
					distanceToWaypoint = GetDistanceToWaypoint();
					if (distanceToWaypoint <= num3 && UpdateCornerTransition(vector2))
					{
						return;
					}
				}
			}
		}
		if (m_forceCombatPathing || (GameState.InCombat && IsPathingObstacle() && !Cutscene.CutsceneActive))
		{
			vector2 = GetUnoccupiedPosition(vector2);
		}
		else if (!m_forceCombatPathing && (!GameState.InCombat || PerformsSoftSteering() || Cutscene.CutsceneActive))
		{
			vector2 = SpreadOut(vector2);
		}
		if (m_aiController != null)
		{
			AIState currentState = m_aiController.StateManager.CurrentState;
			if (!(currentState is AI.Plan.WaitForClearPath) && !(currentState is AI.Player.WaitForClearPath))
			{
				base.transform.position = vector2;
			}
		}
		if (m_heading.sqrMagnitude > float.Epsilon)
		{
			base.transform.rotation = Quaternion.LookRotation(GameUtilities.V2ToV3(m_heading));
		}
		Vector3 force = (m_lastPosition - base.transform.position) * 10f;
		Equippable primaryWeapon = GetComponent<Equipment>().CurrentItems.PrimaryWeapon;
		if (primaryWeapon != null && primaryWeapon.GetComponent<Rigidbody>() != null)
		{
			primaryWeapon.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.VelocityChange);
		}
		m_lastReportedPosition = m_lastPosition;
		m_lastPosition = base.transform.position;
		UpdateBlockedByDoor();
		if (flag)
		{
			RecalculatePath();
		}
	}

	private void LateUpdate()
	{
		MoveToGround();
		if (!GameUtilities.IsPositionOnNavMesh(base.transform.position) && NavMesh.FindClosestEdge(base.transform.position, out var hit, -1))
		{
			Vector2 vector = GameUtilities.V3Subtract2D(base.transform.position, hit.position);
			if (vector.sqrMagnitude > float.Epsilon)
			{
				vector.Normalize();
				Vector2 vector2 = GameUtilities.V3ToV2(hit.normal);
				vector2.Normalize();
				if ((vector - vector2).sqrMagnitude > float.Epsilon && Vector2.Dot(vector, vector2) < 0f)
				{
					Vector3 newPos = hit.position;
					newPos.y = base.transform.position.y;
					PushPosToTowardsNextCorner(1, ref newPos);
					PushPosToTowardsNextCorner(2, ref newPos);
					base.transform.position = newPos;
				}
			}
		}
		if (this.OnMovementLateUpdated != null)
		{
			this.OnMovementLateUpdated(this, EventArgs.Empty);
		}
	}

	private void PushPosToTowardsNextCorner(int index, ref Vector3 newPos)
	{
		if (m_lastCorner > 1 && m_lastCorner - index < m_path.corners.Length && GameUtilities.V3Subtract2D(newPos, m_path.corners[m_lastCorner - index]).sqrMagnitude < 0.0009f)
		{
			Vector2 vector = GameUtilities.V3Subtract2D(m_path.corners[m_lastCorner - 1], m_path.corners[m_lastCorner - 2]);
			vector.Normalize();
			vector *= 0.03f;
			newPos.x += vector.x;
			newPos.z += vector.y;
		}
	}

	private float GetDistanceToWaypoint()
	{
		if (m_moveDirectly)
		{
			return GameUtilities.V3Subtract2D(m_goal, m_lastPosition).magnitude;
		}
		return GameUtilities.V3Subtract2D(m_nextCornerPos, base.transform.position).magnitude;
	}

	public float GetRemainingPathDistance()
	{
		if (m_path.corners.Length == 0)
		{
			return 0f;
		}
		float num = GameUtilities.V3Distance2D(base.transform.position, m_path.corners[m_lastCorner]);
		for (int i = m_lastCorner; i < m_path.corners.Length - 1; i++)
		{
			num += GameUtilities.V3Distance2D(m_path.corners[i], m_path.corners[i + 1]);
		}
		return num;
	}

	private bool UpdateCornerTransition(Vector3 targetPos)
	{
		if (m_moveDirectly)
		{
			if (m_snapToFinalDestination && PotentialPositionOccupiedBy(m_goal, this, Radius) == null)
			{
				base.transform.position = GetUnoccupiedPosition(m_goal);
			}
			else
			{
				base.transform.position = targetPos;
			}
			m_reachedLastGoal = true;
			m_snapToFinalDestination = false;
			Stop();
			return true;
		}
		if (m_lastCorner >= m_path.corners.Length - 1)
		{
			m_reachedLastGoal = true;
			m_goalUnreachable = m_path.status == NavMeshPathStatus.PathPartial;
			if (m_goalUnreachable)
			{
				UpdateBlockedByDoor();
			}
			else if (m_snapToFinalDestination && PotentialPositionOccupiedBy(m_goal, this, Radius) == null)
			{
				base.transform.position = m_goal;
			}
			else
			{
				base.transform.position = targetPos;
			}
			m_lastReportedPosition = m_lastPosition;
			m_lastPosition = base.transform.position;
			return true;
		}
		m_lastCorner++;
		m_nextCornerPos = m_path.corners[m_lastCorner];
		UpdateHeading(0f);
		return false;
	}

	private void UpdateBlockedByDoor()
	{
		if (m_path.status != NavMeshPathStatus.PathPartial)
		{
			return;
		}
		Vector3 a = base.transform.position;
		if (m_path.corners.Length != 0)
		{
			a = m_path.corners[m_path.corners.Length - 1];
		}
		Door blockedByDoor = null;
		float num = float.MaxValue;
		for (int i = 0; i < Door.DoorList.Count; i++)
		{
			Door door = Door.DoorList[i];
			float num2 = GameUtilities.V3SqrDistance2D(a, door.transform.position);
			if (!(num2 > 25f) && !(num2 > num))
			{
				blockedByDoor = door;
				num = num2;
			}
		}
		m_blockedByDoor = blockedByDoor;
		SetBlockFlag(BlockFlag.Dynamic);
		m_pathBlocked = true;
	}

	private void PushPathBlockedState(List<Mover> obstacles, bool forceBlock)
	{
		if (!(AIController != null) || Cutscene.CutsceneActive || (!forceBlock && !AIController.StateManager.CurrentState.AllowBlockedMovement()))
		{
			return;
		}
		SaveBlockedRoute();
		m_desiredHeading = GameUtilities.V3Subtract2D(m_nextCornerPos, base.transform.position);
		m_desiredHeading.Normalize();
		m_heading = m_desiredHeading;
		if (AIController is PartyMemberAI)
		{
			if (!(AIController.StateManager.CurrentState is AI.Player.WaitForClearPath))
			{
				AI.Player.WaitForClearPath waitForClearPath = AIStateManager.StatePool.Allocate<AI.Player.WaitForClearPath>();
				waitForClearPath.Obstacles.AddRange(obstacles);
				waitForClearPath.BlockerDistance = 0.5f;
				AIController.StateManager.PushState(waitForClearPath, clearStack: false);
			}
		}
		else if (!(AIController.StateManager.CurrentState is AI.Plan.WaitForClearPath))
		{
			AI.Plan.WaitForClearPath waitForClearPath2 = AIStateManager.StatePool.Allocate<AI.Plan.WaitForClearPath>();
			waitForClearPath2.Obstacles.AddRange(obstacles);
			waitForClearPath2.BlockerDistance = 0.5f;
			AIController.StateManager.PushState(waitForClearPath2, clearStack: false);
		}
	}

	private Vector3 GetUnoccupiedPosition(Vector3 targetPos)
	{
		Mover mover = PotentialPositionOccupiedBy(targetPos, this, Radius);
		if (mover != null)
		{
			Vector2 vector = GameUtilities.V3Subtract2D(targetPos, mover.transform.position);
			vector.Normalize();
			vector *= Radius + mover.Radius + 0.05f;
			Vector3 result = GameUtilities.V2ToV3(GameUtilities.V3ToV2(mover.transform.position) + vector);
			result.y = targetPos.y;
			return result;
		}
		return targetPos;
	}

	public void MoveToGround()
	{
		if (PopToGround && Physics.Raycast(base.transform.position + Vector3.up * 1.8f, layerMask: 1 << LayerMask.NameToLayer("Walkable"), direction: Vector3.down, hitInfo: out var hitInfo, maxDistance: 10f) && hitInfo.rigidbody == null)
		{
			base.transform.position = hitInfo.point;
		}
	}

	private Vector3 CalcAdjustedCorner(Vector3 nextCorner)
	{
		Vector3 vector = nextCorner;
		Vector3 vector2 = base.transform.right;
		if (NavMesh.FindClosestEdge(nextCorner, out var hit, -1) && hit.distance < Radius * 1f)
		{
			vector2 = hit.normal;
			vector = hit.position + vector2 * 0.1f;
			if (NavMesh.FindClosestEdge(vector, out hit, -1) && hit.distance < Radius * 0.25f)
			{
				return nextCorner;
			}
			vector = ((!NavMesh.SamplePosition(vector, out var hit2, 2f, -1)) ? nextCorner : hit2.position);
		}
		if (PositionOrGoalOccupied(vector))
		{
			vector = hit.position + vector2 * Radius;
			vector = ((!NavMesh.SamplePosition(vector, out var hit3, 2f, -1)) ? nextCorner : hit3.position);
		}
		return vector;
	}

	protected void Repath(bool shiftLeft)
	{
		m_isCombatSteering = false;
		if (m_lastCorner > 0 && !m_moveDirectly)
		{
			Vector3 vector = base.transform.position - m_path.corners[m_lastCorner - 1];
			float magnitude = vector.magnitude;
			if (magnitude > 0f)
			{
				vector.Normalize();
				Vector3 vector2 = vector * magnitude * 0.5f;
				vector2 = (m_nextCornerPos = ((!shiftLeft) ? (vector2 + base.transform.right + base.transform.position) : (vector2 - base.transform.right + base.transform.position)));
			}
			else
			{
				m_nextCornerPos = base.transform.right + base.transform.position;
			}
		}
		else
		{
			m_nextCornerPos = base.transform.right + base.transform.position;
		}
		m_nextCornerPos = CalcAdjustedCorner(m_nextCornerPos);
		if (!shiftLeft && (m_nextCornerPos - base.transform.position).sqrMagnitude < 0.001f)
		{
			Repath(shiftLeft: true);
		}
	}

	public static void RecalculateAllPaths()
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (mover.IsMoving())
			{
				mover.RecalculatePath();
			}
		}
	}

	public void RecalculatePath()
	{
		if (m_path != null)
		{
			m_path.ClearCorners();
		}
		if (m_moveDirectly)
		{
			PathDirectly(m_goal, m_arrivalDist);
		}
		else
		{
			PathToDestination(m_goal, m_arrivalDist);
		}
	}

	public void PathToDestination(Vector3 destination)
	{
		PathToDestination(destination, Radius);
	}

	public bool PathToDestination(Vector3 destination, float arrivalDist)
	{
		m_isCombatSteering = false;
		if (m_frozen)
		{
			return false;
		}
		m_blockedByDoor = null;
		m_blockedBy = null;
		m_pathBlocked = false;
		SetBlockFlag(BlockFlag.None);
		Vector3 position = base.transform.position;
		if (HasGoal && GameUtilities.V3SqrDistance2D(destination, m_goal) < 0.01f)
		{
			return true;
		}
		ClearForcedTurnDirection();
		Vector3 vector = destination;
		if (NavMesh.SamplePosition(destination, out var hit, 5f, -1))
		{
			vector = hit.position;
		}
		m_snapToFinalDestination = false;
		float currentSpeed = m_currentSpeed;
		m_path = new NavMeshPath();
		if (NavMesh.CalculatePath(position, vector, int.MaxValue, m_path))
		{
			if (m_path.status == NavMeshPathStatus.PathPartial)
			{
				arrivalDist = 3f;
				if (GameUtilities.V3SqrDistance2D(position, m_path.corners[m_path.corners.Length - 1]) <= arrivalDist * arrivalDist)
				{
					return true;
				}
			}
			m_reachedLastGoal = false;
			m_goal = vector;
			m_arrivalDist = Mathf.Max(arrivalDist, Radius * 0.25f);
			m_lastCorner = 0;
			m_nextCornerPos = m_path.corners[0];
			m_pathBlocked = false;
			SetBlockFlag(BlockFlag.None);
			if (currentSpeed >= GetWalkSpeed())
			{
				m_currentSpeed = currentSpeed;
			}
			if (m_path.corners.Length < 0)
			{
				return false;
			}
			UpdateCornerTransition(base.transform.position);
			UpdateHeading(0f);
			UpdateBlockedByDoor();
			if (this.OnMovementStarted != null)
			{
				this.OnMovementStarted(base.gameObject, EventArgs.Empty);
			}
			return true;
		}
		SetBlockFlag(BlockFlag.Error);
		if (this.OnMovementBlocked != null)
		{
			this.OnMovementBlocked(this, EventArgs.Empty);
		}
		return true;
	}

	public bool PathDirectly(Vector3 destination, float arrivalDist)
	{
		m_goal = destination;
		m_arrivalDist = arrivalDist;
		m_lastReportedPosition = m_lastPosition;
		m_lastPosition = base.transform.position;
		m_moveDirectly = true;
		m_isBeingNudged = false;
		m_isCombatSteering = false;
		return true;
	}

	public void CombatSteer(Vector2 targetLocation, float arrivalDist)
	{
		m_combatSteeringTarget = targetLocation;
		m_isCombatSteering = true;
		m_combatSteeringDir.x = targetLocation.x - base.transform.position.x;
		m_combatSteeringDir.y = targetLocation.y - base.transform.position.z;
	}

	public void StopCombatSteering()
	{
		if (m_isCombatSteering)
		{
			m_isCombatSteering = false;
			RecalculatePath();
		}
	}

	public void SetBlockedByCombat()
	{
		SetBlockFlag(BlockFlag.Edge);
		m_pathBlocked = true;
		if (this.OnMovementBlocked != null)
		{
			this.OnMovementBlocked(this, EventArgs.Empty);
		}
		Stop();
	}

	public void ClearForcedTurnDirection()
	{
		m_forcedTurnDirection = TurnDirection.Undecided;
		m_forcedTurnObstacle = null;
		m_pivotObstacle = null;
		m_hasSteeredAroundPivotObstacle = false;
	}

	public void ClearAllTurnDirections()
	{
		m_turnDirection = TurnDirection.Undecided;
		m_obstacleCluster = null;
		m_steerEnd = Vector3.zero;
		m_steerIntersection = Vector3.zero;
		m_turnDirection = TurnDirection.Undecided;
		m_prevTurnDirection = TurnDirection.Undecided;
		ClearForcedTurnDirection();
	}

	public void Stop()
	{
		if (HasGoal && this.OnMovementStopped != null)
		{
			this.OnMovementStopped(base.gameObject, EventArgs.Empty);
		}
		if (m_path != null)
		{
			m_path.ClearCorners();
		}
		m_moveDirectly = false;
		m_isBeingNudged = false;
		m_isCombatSteering = false;
		m_forcedTurnDirection = TurnDirection.Undecided;
		m_forcedTurnObstacle = null;
		m_pivotObstacle = null;
		m_hasSteeredAroundPivotObstacle = false;
		m_prevSoftSteerObstacle = null;
		m_prevStationarySoftSteerOffset = Vector2.zero;
		m_currentSpeed = 0f;
		m_goal = base.transform.position;
		if (m_blockFlags == 0)
		{
			SetBlockFlag(BlockFlag.None);
		}
	}

	public void SaveBlockedRoute()
	{
		m_blockedRoute = m_path.corners;
	}

	public void UseRunSpeed()
	{
		m_desiredSpeed = GetRunSpeed();
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.Walk = false;
		}
	}

	public float GetRunSpeed()
	{
		return Mathf.Max(0f, RunSpeed);
	}

	public void SetRunSpeed(float newValue)
	{
		if (m_desiredSpeed == GetRunSpeed())
		{
			m_desiredSpeed = newValue;
		}
		RunSpeed = newValue;
	}

	public void UseCustomSpeed(float newValue)
	{
		m_desiredSpeed = Mathf.Max(0f, newValue);
		if (m_desiredSpeed < 0f)
		{
			m_desiredSpeed = 0f;
		}
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.Walk = m_desiredSpeed <= GetWalkSpeed() * 1.01f;
		}
	}

	public float GetWalkSpeed()
	{
		return Mathf.Max(0f, WalkSpeed);
	}

	public void SetWalkSpeed(float newValue)
	{
		WalkSpeed = newValue;
	}

	public void UseWalkSpeed()
	{
		m_desiredSpeed = GetWalkSpeed();
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.Walk = true;
		}
	}

	public bool OverlapsPosition(Vector3 position)
	{
		return GameUtilities.V3SqrDistance2D(position, base.transform.position) <= Radius * Radius;
	}

	public bool OverlapsPosition(Vector3 position, float radius)
	{
		float num = GameUtilities.V3SqrDistance2D(position, base.transform.position);
		float num2 = radius + Radius;
		return num <= num2 * num2;
	}

	public static void GetMoversInRange(List<Mover> results, Vector3 position, float radius, float maxDistance, Mover ignoredMover)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && !(mover == ignoredMover) && mover.gameObject.activeInHierarchy && !(mover.m_aiController == null) && !mover.m_aiController.IsPet)
			{
				float num = GameUtilities.V3SqrDistance2D(mover.transform.position, position);
				float num2 = mover.Radius + radius + maxDistance;
				if (num2 * num2 >= num)
				{
					results.Add(mover);
				}
			}
		}
	}

	public static bool PositionOrGoalOccupied(Vector3 point)
	{
		return PositionOrGoalOccupied(point, null);
	}

	public static bool PositionOrGoalOccupied(Vector3 point, Mover ignoredMover)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = mover.Radius * mover.Radius;
				if (GameUtilities.V3SqrDistance2D(mover.HasGoal ? mover.Goal : mover.transform.position, point) <= num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool PositionOccupied(Vector3 point)
	{
		return PositionOccupied(point, null);
	}

	public static bool PositionOccupied(Vector3 point, Mover ignoredMover)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = mover.Radius * mover.Radius;
				if (GameUtilities.V3SqrDistance2D(mover.transform.position, point) <= num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool PositionOverlaps(Vector3 point, Mover ignoredMover, bool onlyCheckPathingObstacles = false)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && (!onlyCheckPathingObstacles || mover.IsPathingObstacle()) && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = mover.Radius + Radius;
				num *= num;
				if (GameUtilities.V3SqrDistance2D(mover.transform.position, point) <= num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Mover PositionOverlapsWith(Vector3 point, Mover ignoredMover)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = mover.Radius + Radius;
				num *= num;
				if (GameUtilities.V3SqrDistance2D(mover.transform.position, point) <= num)
				{
					return mover;
				}
			}
		}
		return null;
	}

	public bool IsOccupyingArea(Vector3 point, float radius)
	{
		float num = Radius + radius;
		num *= num;
		return GameUtilities.V3SqrDistance2D(base.transform.position, point) <= num;
	}

	public static Mover PositionOccupiedBy(Vector3 point, Mover ignoredMover, out float overlapDistance)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = GameUtilities.V3Distance2D(mover.transform.position, point);
				if (num <= mover.Radius)
				{
					overlapDistance = mover.Radius - num;
					return mover;
				}
			}
		}
		overlapDistance = 0f;
		return null;
	}

	public static Mover AreaOccupiedBy(Vector3 point, Mover ignoredMover, float radius, out float overlapDistance)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && !(mover == ignoredMover) && mover.gameObject.activeInHierarchy)
			{
				float num = GameUtilities.V3SqrDistance2D(mover.transform.position, point);
				float num2 = mover.Radius + radius;
				num2 *= num2;
				if (num2 > num)
				{
					overlapDistance = Mathf.Sqrt(num2) - Mathf.Sqrt(num);
					return mover;
				}
			}
		}
		overlapDistance = 0f;
		return null;
	}

	private Mover PotentialPositionOccupiedBy(Vector3 point, Mover ignoredMover, float radius)
	{
		for (int i = 0; i < s_moverList.Count; i++)
		{
			Mover mover = s_moverList[i];
			if (!(mover == null) && mover.IsPathingObstacle() && mover.gameObject.activeInHierarchy && !(mover == ignoredMover) && !(mover.Radius < 0.1f))
			{
				float num = GameUtilities.V3SqrDistance2D(mover.transform.position, point);
				float num2 = radius + mover.Radius + 0.05f;
				num2 *= num2;
				if (num <= num2)
				{
					return mover;
				}
			}
		}
		return null;
	}

	private void OnDrawGizmosSelected()
	{
		if (HasGoal)
		{
			for (int i = 1; i < m_path.corners.Length; i++)
			{
				Gizmos.DrawLine(m_path.corners[i - 1], m_path.corners[i]);
				Gizmos.DrawWireSphere(m_goal, Mathf.Max(m_arrivalDist, Radius));
			}
		}
	}

	public bool IsPathingObstacle()
	{
		if (ClipsThroughObstacles)
		{
			return false;
		}
		if (m_aiController != null)
		{
			return m_aiController.IsPathingObstacle();
		}
		return false;
	}

	public bool CanBeNudgedBy(Mover pather)
	{
		if (m_aiController != null)
		{
			return m_aiController.CanBeNudgedBy(pather);
		}
		return false;
	}

	public bool IsMoving()
	{
		if (m_aiController != null)
		{
			return m_aiController.IsMoving();
		}
		return HasGoal;
	}

	public bool IsPathBlocked()
	{
		if (m_aiController != null)
		{
			return m_aiController.IsPathBlocked();
		}
		return HasGoal;
	}

	public bool PerformsSoftSteering()
	{
		if (m_aiController != null)
		{
			return m_aiController.PerformsSoftSteering();
		}
		return false;
	}

	public bool IsBlockedBy(BlockFlag flag)
	{
		return ((uint)m_blockFlags & (uint)flag) != 0;
	}

	public void SetBlockFlag(BlockFlag flag)
	{
		if (flag == BlockFlag.None)
		{
			m_blockFlags = 0;
		}
		else
		{
			m_blockFlags |= (int)flag;
		}
	}

	public void RemoveBlockFlag(BlockFlag flag)
	{
		if (IsBlockedBy(flag))
		{
			m_blockFlags ^= (int)flag;
		}
	}
}

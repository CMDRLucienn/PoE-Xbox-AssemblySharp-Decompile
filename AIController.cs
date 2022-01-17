using System;
using System.Collections.Generic;
using System.Text;
using AI;
using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;

[RequireComponent(typeof(AudioBank))]
[RequireComponent(typeof(CharacterStats))]
public abstract class AIController : MonoBehaviour, IGameEventListener
{
	protected class DisengagementTracker
	{
		public GameObject Enemy;

		public float WaitDuration;

		public bool IgnoreEngagementRange;

		public DisengagementTracker(GameObject enemy, bool ignoreEngagementRange)
		{
			Enemy = enemy;
			WaitDuration = 0f;
			IgnoreEngagementRange = ignoreEngagementRange;
		}
	}

	public enum AISummonType
	{
		NotSummoned,
		AnimalCompanion,
		Summoned,
		Pet
	}

	public enum AggressionType
	{
		DefendMyself,
		Passive,
		Defensive,
		Aggressive
	}

	internal class ObjectsWithLifetime
	{
		public GameObject Obj;

		public float Lifetime;

		public bool Expired => Lifetime <= 0f;

		public ObjectsWithLifetime(GameObject obj, float lifetime)
		{
			Obj = obj;
			Lifetime = lifetime;
		}

		public void Update()
		{
			Lifetime -= Time.deltaTime;
		}

		public override bool Equals(object obj)
		{
			return Obj.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Obj.GetHashCode();
		}
	}

	public float ShoutRange = 5f;

	protected AIStateManager m_ai;

	protected CharacterStats m_stats;

	protected Health m_health;

	protected AnimationController m_animController;

	protected Mover m_mover;

	protected List<GameObject> m_summonedCreatures = new List<GameObject>();

	private List<ObjectsWithLifetime> m_trackedObjectList = new List<ObjectsWithLifetime>();

	private List<DisengagementTracker> m_disengagementTrackers = new List<DisengagementTracker>();

	protected Vector3 m_retreatPosition = Vector3.zero;

	protected float m_kiteTimer;

	protected float m_kiteDistanceSq = float.MaxValue;

	protected GameObject m_highestDamageInflictor;

	protected float m_highestDamageInflicted;

	protected bool m_isConfused;

	protected Team m_teamBeforeConfusion;

	protected bool m_ignoreAsCutsceneObstacle;

	protected bool m_inCutscene;

	protected Encounter m_parentEncounter;

	protected bool m_mustDieForCombatToEnd;

	[Persistent(Persistent.ConversionType.GUIDLink)]
	protected Waypoint m_prevWaypoint;

	[Persistent(Persistent.ConversionType.GUIDLink)]
	protected Waypoint m_currentWaypoint;

	public const float DefendMyselfMeleeSearchDistance = 3f;

	public const float DefendMyselfRangedSearchDistance = 6f;

	public const float DefensiveMeleeSearchDistance = 6f;

	public const float DefensiveRangedSearchDistance = 9f;

	public const float AggressiveSearchDistance = 20f;

	public bool m_detectsStealthedCharacters = true;

	[Tooltip("If set, the character can target enemies at any range.")]
	public bool UnlimitedPerception;

	private bool m_needComp = true;

	private bool m_hasShoutedForHelp;

	private bool m_hasBeenInitialized;

	private bool m_isBusy;

	private const float MaxSteering = 16.7551613f;

	private float CosMaxSteering = Mathf.Cos(16.7551613f);

	public const float KiteDistanceSq = 144f;

	public const float FarFromStartKiteDistanceSq = 9f;

	protected static List<SpellCastData> s_possibleInstructions = new List<SpellCastData>();

	protected static TargetPreference s_defaultTargetPreference = null;

	protected static float s_totalRandomScore = 0f;

	protected static SpellCastData s_defaultInstruction = null;

	protected float m_instructionTimer;

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public GameObject Summoner { get; set; }

	public bool IsConfused => m_isConfused;

	public Vector3 RetreatPosition => m_retreatPosition;

	public bool IgnoreKiteTargets => m_kiteTimer > 0f;

	public float KiteDistanceFromStartPositionSq => m_kiteDistanceSq;

	public bool InCutscene
	{
		get
		{
			return m_inCutscene;
		}
		set
		{
			m_inCutscene = value;
		}
	}

	[Persistent]
	public AISummonType SummonType { get; set; }

	[Persistent]
	[HideInInspector]
	public AggressionType Aggression { get; set; }

	public static List<SpellCastData> PossibleInstructions => s_possibleInstructions;

	public static TargetPreference FilteredDefaultTargetPreference => s_defaultTargetPreference;

	public static float TotalRandomScore
	{
		get
		{
			return s_totalRandomScore;
		}
		set
		{
			s_totalRandomScore = value;
		}
	}

	public static SpellCastData DefaultInstruction => s_defaultInstruction;

	public Mover Mover
	{
		get
		{
			if (m_mover == null)
			{
				m_mover = GetComponent<Mover>();
			}
			return m_mover;
		}
	}

	public GameObject HighestDamageInflictor => m_highestDamageInflictor;

	public bool MustDieForCombatToEnd
	{
		get
		{
			return m_mustDieForCombatToEnd;
		}
		set
		{
			m_mustDieForCombatToEnd = value;
		}
	}

	public Waypoint PrevWaypoint
	{
		get
		{
			return m_prevWaypoint;
		}
		set
		{
			m_prevWaypoint = value;
		}
	}

	public Waypoint CurrentWaypoint
	{
		get
		{
			return m_currentWaypoint;
		}
		set
		{
			m_currentWaypoint = value;
		}
	}

	public bool IsInvisible
	{
		get
		{
			if (m_stats != null)
			{
				return m_stats.IsInvisible;
			}
			return false;
		}
	}

	public bool IsDead
	{
		get
		{
			if (m_health != null)
			{
				return m_health.Dead;
			}
			return false;
		}
	}

	public bool IsUnconscious
	{
		get
		{
			if (m_health != null)
			{
				return m_health.Unconscious;
			}
			return false;
		}
	}

	public bool IsUnconsciousButNotDead
	{
		get
		{
			if (m_health != null)
			{
				if (m_health.Unconscious)
				{
					return !m_health.Dead;
				}
				return false;
			}
			return false;
		}
	}

	public bool IgnoreAsCutsceneObstacle
	{
		get
		{
			return m_ignoreAsCutsceneObstacle;
		}
		set
		{
			m_ignoreAsCutsceneObstacle = value;
		}
	}

	[HideInInspector]
	public AIStateManager StateManager => m_ai;

	public List<GameObject> EngagedEnemies { get; set; }

	public List<GameObject> EngagedBy { get; set; }

	public virtual float StealthedCharacterSuspicionDistance
	{
		get
		{
			if (m_health != null && (m_health.Unconscious || m_health.Dead || !m_health.gameObject.activeInHierarchy))
			{
				return 0f;
			}
			if (m_stats == null)
			{
				return 5f;
			}
			return m_stats.StealthedCharacterSuspicionDistance;
		}
	}

	public virtual float PerceptionDistance
	{
		get
		{
			if (m_health != null && (m_health.Unconscious || m_health.Dead || !m_health.gameObject.activeInHierarchy))
			{
				return 0f;
			}
			if (UnlimitedPerception)
			{
				return float.MaxValue;
			}
			if (m_stats == null)
			{
				return 5f;
			}
			return m_stats.NonStealthPerceptionDistance;
		}
	}

	public List<GameObject> SummonedCreatureList => m_summonedCreatures;

	public bool IsBusy
	{
		get
		{
			return m_isBusy;
		}
		set
		{
			m_isBusy = value;
		}
	}

	public virtual List<SpellCastData> Instructions => null;

	public virtual SpellList.InstructionSelectionType InstructionSelectionType => SpellList.InstructionSelectionType.Random;

	public virtual TargetPreference DefaultTargetPreference => null;

	public bool ReadyForNextInstruction => m_instructionTimer <= 0f;

	public virtual bool IsPet => false;

	public bool InCombat
	{
		get
		{
			if (IsConfused)
			{
				return true;
			}
			if (m_mustDieForCombatToEnd && m_health != null && !m_health.Dead)
			{
				return true;
			}
			if (m_ai != null)
			{
				return m_ai.InCombat;
			}
			return false;
		}
	}

	public GameObject CurrentTarget
	{
		get
		{
			if (m_ai != null)
			{
				return m_ai.CurrentTarget;
			}
			return null;
		}
	}

	public AttackBase CurrentAttack
	{
		get
		{
			if (m_ai != null)
			{
				return m_ai.CurrentAttack;
			}
			return null;
		}
	}

	public virtual void Start()
	{
		Init();
		GameState.OnCombatEnd += HandleCombatEnd;
	}

	protected virtual void OnDestroy()
	{
		GameState.OnCombatEnd -= HandleCombatEnd;
		if (m_ai != null)
		{
			m_ai.DestroyStateStack();
			AIStateManager.StateManagerPool.Free(m_ai);
			m_ai = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public virtual void Awake()
	{
		if (!m_hasBeenInitialized)
		{
			Init();
		}
	}

	protected void Init()
	{
		if (EngagedBy == null)
		{
			EngagedBy = new List<GameObject>();
		}
		else
		{
			EngagedBy.Clear();
		}
		if (EngagedEnemies == null)
		{
			EngagedEnemies = new List<GameObject>();
		}
		else
		{
			EngagedEnemies.Clear();
		}
		m_mover = GetComponent<Mover>();
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null && m_mover != null)
		{
			Vector3 size = new Vector3(m_mover.Radius * 2f, 1.8f, m_mover.Radius * 2f);
			size.x /= base.transform.localScale.x;
			size.y /= base.transform.localScale.y;
			size.z /= base.transform.localScale.z;
			component.size = size;
			Vector3 vector2 = (component.center = new Vector3(component.center.x, size.y / 2f, component.center.z));
		}
		if (!PE_Paperdoll.IsObjectPaperdoll(base.gameObject))
		{
			base.gameObject.layer = LayerMask.NameToLayer("Character");
		}
		if (GetComponent<AudioBank>() == null)
		{
			Debug.LogError("AI GameObject " + base.name + " missing SoundBank!", base.gameObject);
		}
		if (m_ai == null)
		{
			m_ai = AIStateManager.StateManagerPool.Allocate();
			m_ai.Owner = base.gameObject;
			m_ai.AIController = this;
			InitAI();
		}
		m_hasBeenInitialized = true;
	}

	private void GameState_OnLevelLoaded(object sender, EventArgs e)
	{
		if (SummonedCreatureList == null)
		{
			return;
		}
		foreach (GameObject summonedCreature in SummonedCreatureList)
		{
			if (summonedCreature != null)
			{
				float radius = 0.5f;
				Mover component = summonedCreature.GetComponent<Mover>();
				if (component != null)
				{
					radius = component.Radius;
				}
				summonedCreature.transform.position = GameUtilities.NearestUnoccupiedLocation(summonedCreature.transform.position, radius, 10f, component);
			}
		}
	}

	public virtual void OnEnable()
	{
		GameState.OnLevelLoaded += GameState_OnLevelLoaded;
	}

	public virtual void OnDisable()
	{
		if (!m_inCutscene)
		{
			foreach (GameObject summonedCreature in m_summonedCreatures)
			{
				if (!(summonedCreature == null))
				{
					AIController component = summonedCreature.GetComponent<AIController>();
					if ((bool)component)
					{
						component.Summoner = null;
					}
				}
			}
			m_summonedCreatures.Clear();
		}
		GameState.OnLevelLoaded -= GameState_OnLevelLoaded;
	}

	public AggressionType GetAutoAttackAggression()
	{
		PartyMemberAI partyMemberAI = this as PartyMemberAI;
		if (partyMemberAI != null && !partyMemberAI.UseInstructionSet)
		{
			return AggressionType.Passive;
		}
		return Aggression;
	}

	public void TransferSummonedCreatures(AIController aiController)
	{
		if (aiController == null || aiController.m_summonedCreatures == null || m_summonedCreatures == null)
		{
			return;
		}
		aiController.m_summonedCreatures.Clear();
		foreach (GameObject summonedCreature in m_summonedCreatures)
		{
			if (!(summonedCreature == null))
			{
				aiController.m_summonedCreatures.Add(summonedCreature);
				AIController component = summonedCreature.GetComponent<AIController>();
				if ((bool)component)
				{
					component.Summoner = aiController.gameObject;
				}
			}
		}
		m_summonedCreatures.Clear();
	}

	private void HandleSuspicionDetection()
	{
		if (!m_detectsStealthedCharacters || !FogOfWar.PointVisibleInFog(base.gameObject.transform.position))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		Faction component = GetComponent<Faction>();
		TargetScanner targetScanner = GetTargetScanner();
		if (targetScanner == null)
		{
			return;
		}
		targetScanner.GetPotentialTargets(base.gameObject, this, -1f, list, mustBeStealthed: true);
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject == null)
			{
				continue;
			}
			CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
			if (component2 == null)
			{
				continue;
			}
			Stealth component3 = gameObject.GetComponent<Stealth>();
			if (component3 == null)
			{
				continue;
			}
			Mover component4 = gameObject.GetComponent<Mover>();
			if (component4 == null)
			{
				continue;
			}
			Health component5 = gameObject.GetComponent<Health>();
			if (component5 == null || !component5.Targetable)
			{
				continue;
			}
			float num = GameUtilities.V3SqrDistance2D(base.transform.position, gameObject.transform.position);
			float stealthedCharacterSuspicionDistance = StealthedCharacterSuspicionDistance;
			if (component2.NoiseLevelRadius * component2.NoiseLevelRadius * 2f * 2f > num)
			{
				component3.AddSuspicion(base.gameObject, Mathf.Max(100f - component3.GetSuspicion(base.gameObject), 0f), component.GetRelationship(gameObject));
			}
			if (num < stealthedCharacterSuspicionDistance * stealthedCharacterSuspicionDistance && component3 != null)
			{
				float num2 = component4.Radius;
				if (Mover != null)
				{
					num2 += Mover.Radius;
				}
				float distancePercent = (Mathf.Sqrt(num) - num2) / Mathf.Max(stealthedCharacterSuspicionDistance - num2, 1f);
				if (num - num2 * num2 < 0.45f)
				{
					distancePercent = 0f;
				}
				component3.AddSuspicion(base.gameObject, InvestigationSuspicionRate(distancePercent, component2.CalculateSkill(CharacterStats.SkillType.Stealth), m_stats.ScaledLevel), component.GetRelationship(gameObject));
			}
		}
	}

	protected float InvestigationSuspicionRate(float distancePercent, float stealthSkill, float creatureLevel)
	{
		if (distancePercent <= float.Epsilon)
		{
			return 200f;
		}
		float num = AttackData.Instance.Slope * Mathf.Clamp01(distancePercent) + AttackData.Instance.MinDistanceMultiplier;
		float num2 = (float)AttackData.Instance.BaseSuspicionRate * Time.deltaTime * num;
		float num3 = (stealthSkill - creatureLevel * 0.75f) / creatureLevel;
		if (num3 > 0f)
		{
			num3 = Mathf.Min(num3, 1f - AttackData.Instance.MaxSuspicionStealthDecelerationMultiplier);
			return num2 - num2 * num3;
		}
		num3 = Mathf.Abs(num3);
		if (num3 > AttackData.Instance.MaxSuspicionStealthAccelerationMultiplier)
		{
			num3 = AttackData.Instance.MaxSuspicionStealthAccelerationMultiplier;
		}
		return num2 + num2 * num3;
	}

	public virtual void Update()
	{
		if (GameState.IsLoading)
		{
			return;
		}
		if (m_kiteTimer > 0f && !GameState.Paused)
		{
			m_kiteTimer -= Time.deltaTime;
		}
		CheckForNullEngagements();
		m_hasShoutedForHelp = false;
		for (int num = m_trackedObjectList.Count - 1; num >= 0; num--)
		{
			if (m_trackedObjectList[num].Obj == null)
			{
				m_trackedObjectList.RemoveAt(num);
			}
			else
			{
				m_trackedObjectList[num].Update();
				if (m_trackedObjectList[num].Expired)
				{
					m_trackedObjectList.RemoveAt(num);
				}
			}
		}
		if (m_needComp)
		{
			m_stats = GetComponent<CharacterStats>();
			m_health = GetComponent<Health>();
			m_animController = GetComponent<AnimationController>();
			if (m_stats == null || m_health == null || m_animController == null)
			{
				return;
			}
			m_needComp = false;
		}
		m_ai.Update();
		for (int i = 0; i < EngagedEnemies.Count; i++)
		{
			HudEngagementManager.Instance.Verify(base.gameObject, EngagedEnemies[i]);
		}
		for (int j = 0; j < EngagedBy.Count; j++)
		{
			HudEngagementManager.Instance.Verify(EngagedBy[j], base.gameObject);
		}
		HandleSuspicionDetection();
	}

	protected virtual string BuildDebugText(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text);
		stringBuilder.AppendLine(" ---- STATE STACK ----");
		if (m_ai != null)
		{
			m_ai.BuildDebugText(stringBuilder);
		}
		return stringBuilder.ToString();
	}

	public abstract void InitAI();

	protected void InitMover()
	{
		if (m_mover != null)
		{
			m_mover.AIController = this;
		}
	}

	public bool IsPathingObstacle()
	{
		if ((bool)m_health && (m_health.Dead || m_health.Unconscious))
		{
			return false;
		}
		if (m_ai != null)
		{
			return m_ai.IsPathingObstacle();
		}
		return false;
	}

	public bool CanBeNudgedBy(Mover pather)
	{
		if (m_ai != null)
		{
			return m_ai.CanBeNudgedBy(pather);
		}
		return false;
	}

	public bool IsMoving()
	{
		if (m_ai != null)
		{
			return m_ai.IsMoving();
		}
		return false;
	}

	public bool IsPathBlocked()
	{
		if (m_ai != null)
		{
			return m_ai.IsPathBlocked();
		}
		return false;
	}

	public bool PerformsSoftSteering()
	{
		if (m_ai != null)
		{
			return m_ai.PerformsSoftSteering();
		}
		return false;
	}

	public bool IsIdling()
	{
		if (m_ai != null)
		{
			return m_ai.IsIdling();
		}
		return false;
	}

	public bool IsPerformingSecondPartOfFullAttack()
	{
		if (m_ai != null)
		{
			return m_ai.IsPerformingSecondPartOfFullAttack();
		}
		return false;
	}

	public void TrackObjectLifetime(GameObject obj, float lifetime)
	{
		ObjectsWithLifetime item = new ObjectsWithLifetime(obj, lifetime);
		m_trackedObjectList.Add(item);
	}

	public bool IsObjectTracked(GameObject obj)
	{
		for (int i = 0; i < m_trackedObjectList.Count; i++)
		{
			if (m_trackedObjectList[i] != null && m_trackedObjectList[i].Obj == obj)
			{
				return true;
			}
		}
		return false;
	}

	public static void BreakAllEngagements(GameObject target)
	{
		if (target == null)
		{
			return;
		}
		AIController aIController = GameUtilities.FindActiveAIController(target);
		if (aIController == null)
		{
			return;
		}
		for (int num = aIController.EngagedBy.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = aIController.EngagedBy[num];
			if (gameObject != null)
			{
				AIController aIController2 = GameUtilities.FindActiveAIController(gameObject);
				if ((bool)aIController2)
				{
					aIController2.CancelEngagement(target);
				}
			}
		}
		for (int num2 = aIController.EngagedEnemies.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject2 = aIController.EngagedEnemies[num2];
			if (gameObject2 != null)
			{
				aIController.CancelEngagement(gameObject2);
			}
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(base.gameObject.transform.position, base.gameObject.transform.position + base.gameObject.transform.forward);
	}

	public bool UpdateFollowSummoner(bool isCutscene, AIStateManager stateManager)
	{
		if (SummonType != AISummonType.AnimalCompanion || Summoner == null)
		{
			return false;
		}
		if (!isCutscene && this is PartyMemberAI)
		{
			return false;
		}
		if (stateManager.CurrentState is Follow)
		{
			return true;
		}
		if (Summoner.GetComponent<AIController>() == null)
		{
			return false;
		}
		if (GameUtilities.V3SqrDistance2D(base.transform.position, Summoner.transform.position) < 4f)
		{
			return false;
		}
		Follow follow = AIStateManager.StatePool.Allocate<Follow>();
		if (!isCutscene)
		{
			follow.TargetScanner = GetTargetScanner();
		}
		stateManager.PushState(follow);
		return true;
	}

	public virtual void OnEvent(GameEventArgs args)
	{
		if (m_ai != null)
		{
			((IGameEventListener)m_ai.CurrentState)?.OnEvent(args);
			switch (args.Type)
			{
			case GameEventType.Unconscious:
				HandleUnconsciousEvent(args);
				break;
			case GameEventType.Dead:
				HandleDeadEvent(args);
				break;
			case GameEventType.Revived:
				HandleRevivedEvent(args);
				break;
			case GameEventType.Stunned:
				HandleStunnedEvent(args);
				break;
			case GameEventType.Paralyzed:
				HandleParalyzedEvent(args);
				break;
			case GameEventType.Confused:
				HandleConfusedEvent(args);
				break;
			case GameEventType.Damaged:
				HandleDamagedEvent(args);
				break;
			case GameEventType.RequestHelp:
				HandleRequestHelpEvent(args);
				break;
			case GameEventType.MeleeEngaged:
				HandleMeleeEngagedEvent(args);
				break;
			case GameEventType.HitReact:
				HandleHitReactEvent(args);
				break;
			case GameEventType.KnockedDown:
				HandleKnockedDownEvent(args);
				break;
			case GameEventType.Healed:
			case GameEventType.UsableClicked:
			case GameEventType.PlayerCommand:
			case GameEventType.Ability:
			case GameEventType.Killed:
			case GameEventType.Destroyed:
			case GameEventType.MeleeEngageBroken:
			case GameEventType.MeleeEngagementForceBreak:
			case GameEventType.Gibbed:
				break;
			}
		}
	}

	private void HandleUnconsciousEvent(GameEventArgs args)
	{
		if (!(m_ai.CurrentState is Unconscious))
		{
			if (m_isConfused)
			{
				RemoveConfusion();
			}
			CancelCurrentAttack();
			Unconscious state = AIStateManager.StatePool.Allocate<Unconscious>();
			m_ai.PushState(state, clearStack: true);
		}
	}

	private void HandleDeadEvent(GameEventArgs args)
	{
		if (m_isConfused)
		{
			RemoveConfusion();
		}
		CancelCurrentAttack();
		Dead state = AIStateManager.StatePool.Allocate<Dead>();
		m_ai.PushState(state, clearStack: true);
		GameObject gameObject = args.GameObjectData[0];
		if (gameObject != null)
		{
			GameObject owner = StateManager.CurrentState.Owner;
			Faction component = owner.GetComponent<Faction>();
			Faction component2 = gameObject.GetComponent<Faction>();
			if (component2 != null && component != null && component.CurrentTeamInstance != null && component2.CurrentTeamInstance != component.CurrentTeamInstance)
			{
				GameEventArgs obj = new GameEventArgs
				{
					Type = GameEventType.RequestHelp,
					GameObjectData = new GameObject[3]
				};
				obj.GameObjectData[0] = owner;
				obj.GameObjectData[1] = gameObject;
				obj.GameObjectData[2] = owner;
				BroadcastToAllies(obj, owner, ShoutRange);
			}
		}
	}

	private void HandleRevivedEvent(GameEventArgs args)
	{
		AIState currentState = m_ai.CurrentState;
		if (currentState is Dead || currentState is Unconscious)
		{
			m_ai.PopCurrentState();
		}
		if (Mover != null)
		{
			Mover.transform.position = GameUtilities.NearestUnoccupiedLocation(Mover.transform.position, Mover.Radius, 10f, Mover);
		}
		PerformReaction performReaction = AIStateManager.StatePool.Allocate<PerformReaction>();
		performReaction.Setup(AnimationController.ReactionType.Standup);
		m_ai.PushState(performReaction);
	}

	private void HandleStunnedEvent(GameEventArgs args)
	{
		if (args.IntData == null || args.IntData.Length == 0)
		{
			Debug.LogError("Malformed GameEventArgs in Stunned event: bad IntData.");
		}
		else if (args.IntData[0] == 1)
		{
			CancelCurrentAttack();
			Stunned stunned = AIStateManager.StatePool.Allocate<Stunned>();
			stunned.Setup(AnimationController.ReactionType.Stun, args.FloatData[0]);
			AIState currentState = m_ai.CurrentState;
			if (currentState.Priority > stunned.Priority && currentState.AllowsQueueing)
			{
				m_ai.QueueStateAtTop(stunned);
			}
			else
			{
				if (currentState is AI.Achievement.Attack attack)
				{
					m_ai.PopCurrentState();
					attack.OnCancel();
				}
				m_ai.PushState(stunned);
			}
			if (this is PartyMemberAI && args.GameObjectData != null && args.GameObjectData.Length != 0)
			{
				PartyMemberAI component = ComponentUtils.GetComponent<PartyMemberAI>(args.GameObjectData[0]);
				if ((bool)component && (bool)component.gameObject && component.gameObject.activeInHierarchy)
				{
					stunned.InCombatOverride = false;
				}
			}
		}
		else if (StateManager != null)
		{
			StateManager.PopState(typeof(Stunned));
		}
	}

	private void HandleParalyzedEvent(GameEventArgs args)
	{
		if (args.IntData[0] == 1)
		{
			CancelCurrentAttack();
			Paralyzed paralyzed = AIStateManager.StatePool.Allocate<Paralyzed>();
			paralyzed.Duration = args.FloatData[0];
			m_ai.PushState(paralyzed);
			if (this is PartyMemberAI && args.GameObjectData[0] != null)
			{
				PartyMemberAI component = args.GameObjectData[0].GetComponent<PartyMemberAI>();
				if (component != null && component.gameObject.activeInHierarchy)
				{
					paralyzed.InCombatOverride = false;
				}
			}
		}
		else
		{
			StateManager.PopState(typeof(Paralyzed));
		}
	}

	private void HandleConfusedEvent(GameEventArgs args)
	{
		if (args.IntData[0] == 1)
		{
			CancelCurrentAttack();
			AddConfusion(args.FloatData[0]);
			return;
		}
		RemoveConfusion();
		PartyMemberAI partyMemberAI = this as PartyMemberAI;
		if (partyMemberAI != null && PartyMemberAI.GetSelectedPartyMembers().Count <= 0)
		{
			partyMemberAI.Selected = true;
		}
	}

	private void HandleDamagedEvent(GameEventArgs args)
	{
		UpdateMustDieForCombatToEnd();
		GameObject owner = StateManager.CurrentState.Owner;
		Faction component = owner.GetComponent<Faction>();
		Faction component2 = args.GameObjectData[0].GetComponent<Faction>();
		bool flag = true;
		m_hasShoutedForHelp = true;
		if (m_highestDamageInflictor == null || args.FloatData[0] > m_highestDamageInflicted)
		{
			m_highestDamageInflictor = args.GameObjectData[0];
			m_highestDamageInflicted = args.FloatData[0];
		}
		if (component2 != null && component.CurrentTeamInstance != null && !IsConfused)
		{
			if (component2.CurrentTeamInstance != component.CurrentTeamInstance)
			{
				GameEventArgs obj = new GameEventArgs
				{
					Type = GameEventType.RequestHelp,
					GameObjectData = new GameObject[3]
				};
				obj.GameObjectData[0] = owner;
				obj.GameObjectData[1] = args.GameObjectData[0];
				obj.GameObjectData[2] = owner;
				BroadcastToAllies(obj, owner, ShoutRange);
			}
			else
			{
				flag = false;
			}
		}
		PartyMemberAI partyMemberAI = this as PartyMemberAI;
		if (component != null)
		{
			if (partyMemberAI != null)
			{
				if (component.IsFriendly(args.GameObjectData[0]))
				{
					return;
				}
			}
			else if (!component.IsHostile(args.GameObjectData[0]))
			{
				if (component2 != null && component.IsInPlayerFaction && component2.IsInPlayerFaction)
				{
					return;
				}
				if (component2 != null && component.CurrentTeamInstance != null && component2.CurrentTeamInstance != component.CurrentTeamInstance)
				{
					if (component2.IsInPlayerFaction)
					{
						component.UnitHostileToPlayer = true;
					}
					else if (component.IsInPlayerFaction)
					{
						component.CurrentTeamInstance.SetRelationship(component2.CurrentTeamInstance, Faction.Relationship.Hostile, mutual: true);
					}
				}
			}
		}
		if (((!(CurrentTarget == null) || StateManager.FindState(typeof(AI.Achievement.Attack)) != null) && !IsPathBlocked()) || !flag || IsConfused)
		{
			return;
		}
		if (partyMemberAI != null)
		{
			if (partyMemberAI.GetAutoAttackAggression() != AggressionType.Passive && IsIdling() && args.GameObjectData[0] != null && args.GameObjectData[0].GetComponent<AIController>() != null)
			{
				AttackBase primaryAttack = GetPrimaryAttack(owner);
				AttackBase attackBase = null;
				if (args.GenericData[0] is DamageInfo damageInfo)
				{
					attackBase = damageInfo.Attack;
				}
				if (primaryAttack is AttackRanged || attackBase is AttackMelee || GameUtilities.V3SqrDistance2D(base.transform.position, args.GameObjectData[0].transform.position) < 9f)
				{
					AI.Player.Attack attack = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
					attack.IsAutoAttack = true;
					StateManager.PushState(attack);
					attack.Target = args.GameObjectData[0];
				}
			}
		}
		else
		{
			if (!(StateManager.AIController is PartyMemberAI) && StateManager.CurrentState is AI.Plan.WaitForClearPath)
			{
				StateManager.PopAllStates();
			}
			ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
			StateManager.PushState(approachTarget);
			approachTarget.Target = args.GameObjectData[0];
			approachTarget.TargetScanner = GetTargetScanner();
			if (approachTarget.TargetScanner == null)
			{
				approachTarget.Attack = GetPrimaryAttack(owner);
			}
		}
	}

	private void HandleRequestHelpEvent(GameEventArgs args)
	{
		if (!(CurrentTarget == null) || IsConfused || StateManager == null)
		{
			return;
		}
		AIState currentState = StateManager.CurrentState;
		if (currentState == null)
		{
			return;
		}
		GameObject owner = currentState.Owner;
		if (args.GameObjectData[0] == owner || IsPet)
		{
			return;
		}
		Faction component = args.GameObjectData[0].GetComponent<Faction>();
		Faction component2 = args.GameObjectData[1].GetComponent<Faction>();
		Faction component3 = args.GameObjectData[2].GetComponent<Faction>();
		if (component2 != null && component2.IsInPlayerFaction)
		{
			component3.NotifyAttackWitnessed();
		}
		if (!(this is PartyMemberAI) && (!(m_health != null) || (!m_health.Dead && m_health.gameObject.activeInHierarchy)) && GameUtilities.LineofSight(owner.transform.position, args.GameObjectData[2].transform.position, 1f, includeDynamics: false))
		{
			if (!(StateManager.AIController is PartyMemberAI) && StateManager.CurrentState is AI.Plan.WaitForClearPath)
			{
				StateManager.PopAllStates();
			}
			ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
			StateManager.PushState(approachTarget);
			approachTarget.TargetScanner = GetTargetScanner();
			approachTarget.Target = args.GameObjectData[1];
			if (approachTarget.TargetScanner == null)
			{
				approachTarget.Attack = GetPrimaryAttack(owner);
			}
			Faction component4 = owner.GetComponent<Faction>();
			if (component2.IsInPlayerFaction && !component.CurrentTeamInstance.Equals(component4.CurrentTeamInstance))
			{
				component4.RelationshipToPlayer = Faction.Relationship.Hostile;
			}
			m_hasShoutedForHelp = true;
			GameEventArgs obj = new GameEventArgs
			{
				Type = GameEventType.RequestHelp,
				GameObjectData = new GameObject[3]
			};
			obj.GameObjectData[0] = args.GameObjectData[0];
			obj.GameObjectData[1] = args.GameObjectData[1];
			obj.GameObjectData[2] = StateManager.CurrentState.Owner;
			BroadcastToAllies(obj, owner, ShoutRange);
			GetTargetScanner()?.ExecuteAllOnPercept();
		}
	}

	private void HandleMeleeEngagedEvent(GameEventArgs args)
	{
		if (!TriggersDisengagementIfMoving(args.GameObjectData[0]) || !IsMoving())
		{
			return;
		}
		PartyMemberAI partyMemberAI = this as PartyMemberAI;
		if (partyMemberAI != null)
		{
			if (GameState.Option.GetOption(GameOption.BoolOption.DISABLE_PARTY_MOVEMENT_STOP_ON_ENGAGEMENT))
			{
				return;
			}
			if (partyMemberAI.GetAutoAttackAggression() == AggressionType.Passive)
			{
				if (!(StateManager.CurrentState is WaitForEngagementToEnd))
				{
					WaitForEngagementToEnd state = AIStateManager.StatePool.Allocate<WaitForEngagementToEnd>();
					StateManager.PushState(state);
				}
			}
			else
			{
				AI.Player.Attack attack = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
				attack.IsAutoAttack = true;
				StateManager.PushState(attack);
				attack.Target = args.GameObjectData[0];
			}
		}
		else
		{
			SafePopAllStates();
		}
	}

	private void HandleHitReactEvent(GameEventArgs args)
	{
		StateManager.CurrentState.OnCancel();
		HitReact hitReact = AIStateManager.StatePool.Allocate<HitReact>();
		StateManager.PushState(hitReact);
		if (this is PartyMemberAI && args.GameObjectData[0] != null)
		{
			PartyMemberAI component = args.GameObjectData[0].GetComponent<PartyMemberAI>();
			if (component != null && component.gameObject.activeInHierarchy)
			{
				hitReact.InCombatOverride = false;
			}
		}
	}

	private void HandleKnockedDownEvent(GameEventArgs args)
	{
		if (args.IntData[0] == 1)
		{
			CancelCurrentAttack();
			KnockedDown knockedDown = StateManager.FindState(typeof(KnockedDown)) as KnockedDown;
			if (knockedDown != null)
			{
				knockedDown.ResetKnockedDown(args.FloatData[0]);
			}
			else
			{
				knockedDown = AIStateManager.StatePool.Allocate<KnockedDown>();
				if (StateManager.CurrentState is PushedBack)
				{
					(StateManager.CurrentState as PushedBack).SetKnockedDownState(knockedDown);
				}
				else
				{
					StateManager.CurrentState.OnCancel();
					StateManager.PushState(knockedDown);
				}
				knockedDown.SetKnockdownTime(args.FloatData[0]);
			}
			if (this is PartyMemberAI && args.GameObjectData[0] != null)
			{
				PartyMemberAI component = args.GameObjectData[0].GetComponent<PartyMemberAI>();
				if (component != null && component.gameObject.activeInHierarchy)
				{
					knockedDown.InCombatOverride = false;
				}
			}
		}
		else if (StateManager.FindState(typeof(KnockedDown)) is KnockedDown knockedDown2)
		{
			knockedDown2.Standup();
		}
	}

	public void UpdateAggressionOfSummonedCreatures(bool includeCompanion)
	{
		PartyMemberAI partyMemberAI = this as PartyMemberAI;
		for (int i = 0; i < SummonedCreatureList.Count; i++)
		{
			if (!(SummonedCreatureList[i] != null))
			{
				continue;
			}
			AIController aIController = GameUtilities.FindActiveAIController(SummonedCreatureList[i]);
			if (aIController != null && aIController.SummonType != AISummonType.Pet && (aIController.SummonType != AISummonType.AnimalCompanion || includeCompanion))
			{
				if (aIController.SummonType != AISummonType.AnimalCompanion)
				{
					aIController.Aggression = Aggression;
				}
				PartyMemberAI partyMemberAI2 = aIController as PartyMemberAI;
				if (partyMemberAI != null && (bool)partyMemberAI2)
				{
					partyMemberAI2.UseInstructionSet = partyMemberAI.UseInstructionSet;
					partyMemberAI2.UsePerRestAbilitiesInInstructionSet = partyMemberAI.UseInstructionSet;
				}
			}
		}
	}

	public virtual bool EngagementsEnabled()
	{
		return true;
	}

	private void DisengageUntargetable()
	{
		for (int num = EngagedEnemies.Count - 1; num >= 0; num--)
		{
			if (!IsTargetable(EngagedEnemies[num]))
			{
				StopEngagement(EngagedEnemies[num]);
			}
		}
		for (int num2 = EngagedBy.Count - 1; num2 >= 0; num2--)
		{
			if (!(EngagedBy[num2] == null))
			{
				Health component = EngagedBy[num2].GetComponent<Health>();
				if (component != null && (component.Dead || component.Unconscious))
				{
					EngagedBy.RemoveAt(num2);
				}
			}
		}
	}

	protected void CheckForNullEngagements()
	{
		for (int num = EngagedEnemies.Count - 1; num >= 0; num--)
		{
			if (EngagedEnemies[num] == null)
			{
				EngagedEnemies.RemoveAt(num);
			}
		}
		for (int num2 = EngagedBy.Count - 1; num2 >= 0; num2--)
		{
			if (EngagedBy[num2] == null)
			{
				EngagedBy.RemoveAt(num2);
			}
		}
		DisengageUntargetable();
		for (int i = 0; i < m_disengagementTrackers.Count; i++)
		{
			m_disengagementTrackers[i].WaitDuration += Time.deltaTime;
		}
	}

	public void UpdateEngagement(GameObject owner, AttackBase attack)
	{
		if (GameState.Paused || !(attack is AttackMelee) || (m_stats != null && m_stats.EngageableEnemyCount == 0) || !m_ai.AllowEngagementUpdate() || (m_stats != null && (m_stats.HasStatusEffectFromAffliction(AfflictionData.Charmed) || m_stats.HasStatusEffectFromAffliction(AfflictionData.Dominated) || m_stats.HasStatusEffectFromAffliction(AfflictionData.Paralyzed) || m_stats.HasStatusEffectFromAffliction(AfflictionData.Petrified))))
		{
			return;
		}
		for (int num = m_disengagementTrackers.Count - 1; num >= 0; num--)
		{
			if (!IsTargetable(m_disengagementTrackers[num].Enemy) || (m_disengagementTrackers[num].WaitDuration >= AttackData.Instance.ReengagementWaitPeriod && (m_disengagementTrackers[num].IgnoreEngagementRange || !IsWithinEngagementRange(m_disengagementTrackers[num].Enemy, attack))))
			{
				m_disengagementTrackers.RemoveAt(num);
			}
		}
		for (int num2 = EngagedEnemies.Count - 1; num2 >= 0; num2--)
		{
			if (!IsTargetable(EngagedEnemies[num2]))
			{
				StopEngagement(EngagedEnemies[num2]);
			}
			else
			{
				Faction component = EngagedEnemies[num2].GetComponent<Faction>();
				if ((bool)component && !component.IsHostile(owner) && !owner.GetComponent<Faction>().IsHostile(EngagedEnemies[num2]))
				{
					StopEngagement(EngagedEnemies[num2]);
				}
				else if (IsTargetBeingPushedBack(EngagedEnemies[num2]))
				{
					StopEngagement(EngagedEnemies[num2]);
				}
				else
				{
					AIController component2 = EngagedEnemies[num2].GetComponent<AIController>();
					if (component2.IsMoving() && component2.TriggersDisengagementIfMoving(owner))
					{
						if (component2.StateManager.IsMoving())
						{
							DisengageEnemy(EngagedEnemies[num2], attack);
						}
						else if (StateManager.IsMoving())
						{
							CancelEngagement(EngagedEnemies[num2]);
						}
					}
				}
			}
		}
		GameObject currentTarget = CurrentTarget;
		if (currentTarget != null && !HasEngaged(currentTarget) && IsWithinEngagementRange(currentTarget, attack))
		{
			if (m_stats != null && EngagedEnemies.Count >= m_stats.EngageableEnemyCount)
			{
				CancelEngagement(EngagedEnemies[0]);
			}
			if (CanEngageEnemy(currentTarget, attack))
			{
				EngageEnemy(currentTarget);
			}
		}
		if (!FogOfWar.PointVisibleInFog(base.gameObject.transform.position) || !CanEngageAnyEnemy(attack))
		{
			return;
		}
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (!(faction == null) && CanEngageEnemy(faction.gameObject, attack))
			{
				EngageEnemy(faction.gameObject);
			}
		}
	}

	private bool TriggersDisengagementIfMoving(GameObject engager)
	{
		if (engager == null)
		{
			return false;
		}
		GameObject currentTarget = CurrentTarget;
		if (currentTarget == null)
		{
			return true;
		}
		if (engager == currentTarget)
		{
			return false;
		}
		if (IsEngagedBy(currentTarget))
		{
			return false;
		}
		Mover component = engager.GetComponent<Mover>();
		Mover component2 = currentTarget.GetComponent<Mover>();
		if (component != null && component2 != null)
		{
			float num = GameUtilities.V3SqrDistance2D(engager.transform.position, currentTarget.transform.position);
			float num2 = component.Radius + component2.Radius;
			if (num < num2 * num2)
			{
				return false;
			}
		}
		return true;
	}

	protected void AddEngagedBy(GameObject enemy)
	{
		if (EngagedBy.Contains(enemy))
		{
			return;
		}
		EngagedBy.Add(enemy);
		CharacterStats component = StateManager.CurrentState.Owner.GetComponent<CharacterStats>();
		if (component != null)
		{
			component.NotifyEngagedByOther(enemy);
		}
		for (int i = 0; i < m_disengagementTrackers.Count; i++)
		{
			if (m_disengagementTrackers[i].Enemy == enemy)
			{
				m_disengagementTrackers.RemoveAt(i);
				break;
			}
		}
	}

	public bool IsEngagedBy(GameObject enemy)
	{
		return EngagedBy.Contains(enemy);
	}

	public bool IsEngaged()
	{
		return EngagedBy.Count > 0;
	}

	public bool IsEngaging(GameObject enemy)
	{
		return EngagedEnemies.Contains(enemy);
	}

	public void StopEngagement(GameObject enemy)
	{
		if (EngagedEnemies.Contains(enemy))
		{
			EngagedEnemies.Remove(enemy);
			TrackObjectLifetime(enemy, 2f);
			AIController component = enemy.GetComponent<AIController>();
			if (component != null && component.EngagedBy.Contains(base.gameObject))
			{
				component.EnemyBreaksEngagement(base.gameObject);
			}
		}
	}

	public void EnemyBreaksEngagement(GameObject enemy)
	{
		if (EngagedBy.Contains(enemy))
		{
			EngagedBy.Remove(enemy);
			CharacterStats component = StateManager.CurrentState.Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.NotifyEngagementByOtherBroken(enemy);
			}
		}
	}

	public void EngageEnemy(GameObject enemy)
	{
		AIController component = enemy.GetComponent<AIController>();
		if ((bool)component && !(component == this))
		{
			if (!EngagedEnemies.Contains(enemy))
			{
				EngagedEnemies.Add(enemy);
			}
			GameObject owner = StateManager.CurrentState.Owner;
			CharacterStats component2 = owner.GetComponent<CharacterStats>();
			if (component2 != null)
			{
				component2.NotifyEngagement(enemy);
			}
			GameEventArgs gameEventArgs = new GameEventArgs();
			gameEventArgs.Type = GameEventType.MeleeEngaged;
			gameEventArgs.GameObjectData = new GameObject[1];
			gameEventArgs.GameObjectData[0] = owner;
			component.OnEvent(gameEventArgs);
			if (FogOfWar.Instance.PointVisible(owner.transform.position))
			{
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(100), CharacterStats.NameColored(owner), CharacterStats.NameColored(enemy)));
			}
			component.AddEngagedBy(owner);
		}
	}

	public void DisengageEnemy(GameObject enemy, AttackBase attack)
	{
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if (component != null && !component.IsImmuneToEngagement)
		{
			attack.IsDisengagementAttack = true;
			attack.Launch(enemy, -1);
			UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2150), enemy, 2.5f);
		}
		GameObject owner = StateManager.CurrentState.Owner;
		m_disengagementTrackers.Add(new DisengagementTracker(enemy, ignoreEngagementRange: false));
		StopEngagement(enemy);
		if (m_stats != null)
		{
			m_stats.NotifyEngagementBreak(enemy);
		}
		EnemyBreaksEngagement(enemy);
		AIController component2 = enemy.GetComponent<AIController>();
		if ((bool)component2)
		{
			GameEventArgs gameEventArgs = new GameEventArgs();
			gameEventArgs.Type = GameEventType.MeleeEngageBroken;
			gameEventArgs.GameObjectData = new GameObject[1];
			gameEventArgs.GameObjectData[0] = owner;
			component2.OnEvent(gameEventArgs);
			component2.EnemyBreaksEngagement(owner);
		}
	}

	public void CancelEngagement(GameObject enemy)
	{
		if (EngagedEnemies.Contains(enemy))
		{
			StopEngagement(enemy);
		}
		GameObject owner = StateManager.CurrentState.Owner;
		CharacterStats component = owner.GetComponent<CharacterStats>();
		if (component != null)
		{
			component.NotifyEngagementBreak(enemy);
		}
		AIController component2 = enemy.GetComponent<AIController>();
		if ((bool)component2)
		{
			GameEventArgs gameEventArgs = new GameEventArgs();
			gameEventArgs.Type = GameEventType.MeleeEngageBroken;
			gameEventArgs.GameObjectData = new GameObject[1];
			gameEventArgs.GameObjectData[0] = owner;
			component2.OnEvent(gameEventArgs);
			component2.EnemyBreaksEngagement(owner);
		}
	}

	public void CancelAllEngagements()
	{
		if (EngagedEnemies == null)
		{
			return;
		}
		for (int num = EngagedEnemies.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = EngagedEnemies[num];
			if (gameObject != null)
			{
				CancelEngagement(gameObject);
			}
		}
	}

	public void CancelAllEngagementsAndDelayReengagement()
	{
		for (int num = EngagedEnemies.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = EngagedEnemies[num];
			if (gameObject != null)
			{
				CancelEngagement(gameObject);
				m_disengagementTrackers.Add(new DisengagementTracker(gameObject, ignoreEngagementRange: true));
			}
		}
	}

	public bool HasEngaged(GameObject enemy)
	{
		if ((bool)enemy)
		{
			return EngagedEnemies.Contains(enemy);
		}
		return false;
	}

	public bool CanEngageAnyEnemy(AttackBase attack)
	{
		if (IsInvisible || !(attack is AttackMelee))
		{
			return false;
		}
		if ((bool)m_stats && EngagedEnemies.Count + m_trackedObjectList.Count >= m_stats.EngageableEnemyCount)
		{
			return false;
		}
		return true;
	}

	public bool CanEngageEnemy(GameObject enemy, AttackBase attack)
	{
		if (!IsWithinEngagementRange(enemy, attack))
		{
			return false;
		}
		if (!CanEngageAnyEnemy(attack))
		{
			return false;
		}
		GameObject owner = StateManager.CurrentState.Owner;
		if (enemy == owner)
		{
			return false;
		}
		if (Stealth.IsInStealthMode(base.gameObject) || Stealth.IsInStealthMode(enemy))
		{
			return false;
		}
		if (HasEngaged(enemy) || IsObjectTracked(enemy) || !IsTargetable(enemy))
		{
			return false;
		}
		GameObject currentTarget = m_ai.CurrentTarget;
		if (IsMoving() && currentTarget != enemy)
		{
			return false;
		}
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if ((bool)component && component.IsImmuneToEngagement)
		{
			return false;
		}
		if ((bool)m_stats)
		{
			int num = component.MinimumLevelThatCanEngageMe();
			if (m_stats.ScaledLevel < num)
			{
				return false;
			}
		}
		Faction component2 = enemy.GetComponent<Faction>();
		if ((bool)component2 && !component2.IsHostile(owner) && !owner.GetComponent<Faction>().IsHostile(enemy))
		{
			return false;
		}
		for (int i = 0; i < m_disengagementTrackers.Count; i++)
		{
			if (m_disengagementTrackers[i].Enemy == enemy)
			{
				return false;
			}
		}
		AIController component3 = enemy.GetComponent<AIController>();
		if ((bool)component3 && component3.IsInvisible)
		{
			return false;
		}
		return true;
	}

	protected bool IsWithinEngagementRange(GameObject enemy, AttackBase attack)
	{
		if (enemy == null || m_stats == null)
		{
			return false;
		}
		AttackMelee attackMelee = attack as AttackMelee;
		if (!attackMelee)
		{
			return false;
		}
		GameObject owner = StateManager.CurrentState.Owner;
		float num = GameUtilities.V3SqrDistance2D(enemy.transform.position, owner.transform.position);
		float radius = m_mover.Radius;
		Mover component = enemy.GetComponent<Mover>();
		float num2 = 0.5f;
		if (component != null)
		{
			num2 = component.Radius;
		}
		float num3 = attackMelee.EngagementRadius + m_stats.EngagementDistanceBonus + radius + num2;
		return num <= num3 * num3;
	}

	public bool IsTargetable(GameObject character)
	{
		return IsTargetable(character, null);
	}

	public bool IsTargetable(GameObject character, AttackBase attack)
	{
		if (character == null || StateManager == null || StateManager.CurrentState == null)
		{
			return false;
		}
		Health component = character.GetComponent<Health>();
		if (component == null)
		{
			return false;
		}
		AIController component2 = character.GetComponent<AIController>();
		if (component2 != null && component2.IsInvisible)
		{
			return false;
		}
		PartyMemberAI component3 = StateManager.CurrentState.Owner.GetComponent<PartyMemberAI>();
		if (component3 == null || !component3.enabled || !component3.gameObject.activeInHierarchy)
		{
			return component.Targetable;
		}
		return component.IsTargetableByAttack(attack);
	}

	public bool IsTargetBeingPushedBack(GameObject character)
	{
		if (character == null || StateManager == null || StateManager.CurrentState == null)
		{
			return false;
		}
		return StateManager.CurrentState is PushedBack;
	}

	public void FaceTarget(AttackBase attack)
	{
		if (attack != null && !attack.FaceTarget)
		{
			return;
		}
		GameObject currentTarget = CurrentTarget;
		if (currentTarget == base.gameObject || (m_stats != null && (m_stats.HasStatusEffectFromAffliction(AfflictionData.Paralyzed) || m_stats.HasStatusEffectFromAffliction(AfflictionData.Petrified))))
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		if (currentTarget == null)
		{
			AIState currentState = StateManager.CurrentState;
			if (currentState == null || !currentState.IsTargetingPosition())
			{
				return;
			}
			zero = currentState.GetTargetedPosition();
		}
		else
		{
			zero = currentTarget.transform.position;
		}
		Vector3 newFacing = zero - base.transform.position;
		newFacing.Normalize();
		FaceDirection(newFacing);
	}

	public void FaceDirection(Vector3 newFacing)
	{
		Vector2 rhs = GameUtilities.V3ToV2(base.transform.forward);
		Vector2 vector = GameUtilities.V3ToV2(newFacing);
		vector.Normalize();
		float num = Time.deltaTime;
		if (GameState.Paused)
		{
			num = 0.025f;
		}
		else if (num < float.Epsilon)
		{
			return;
		}
		float num2 = Vector2.Dot(vector, rhs);
		float num3 = CosMaxSteering * num;
		Vector2 zero = Vector2.zero;
		zero = ((!(num2 >= 1f - num3)) ? GameUtilities.V3ToV2(Vector3.RotateTowards(base.transform.forward, GameUtilities.V2ToV3(vector), 16.7551613f * num, 0f)) : vector);
		if (zero.sqrMagnitude > float.Epsilon)
		{
			base.transform.rotation = Quaternion.LookRotation(GameUtilities.V2ToV3(zero));
		}
	}

	public static void BroadcastToAllies(GameEventArgs args, GameObject owner, float radius)
	{
		Faction component = owner.GetComponent<Faction>();
		float num = radius * radius;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (!(faction == null) && !(faction.gameObject == owner) && faction.GetRelationship(component) == Faction.Relationship.Friendly && !(GameUtilities.V3SqrDistance2D(owner.transform.position, faction.transform.position) > num))
			{
				AIController component2 = faction.GetComponent<AIController>();
				if (!(component2 == null) && !component2.m_hasShoutedForHelp)
				{
					component2.OnEvent(args);
				}
			}
		}
	}

	public static AttackBase GetPrimaryAttack(GameObject owner)
	{
		if (owner == null)
		{
			return null;
		}
		Equipment component = owner.GetComponent<Equipment>();
		if (component != null)
		{
			if (component.PrimaryAttack != null)
			{
				return component.PrimaryAttack;
			}
			if (component.SecondaryAttack != null)
			{
				return component.SecondaryAttack;
			}
			EquipmentSet currentItems = component.CurrentItems;
			if (currentItems != null)
			{
				for (int i = 0; i < currentItems.AlternateWeaponSets.Length; i++)
				{
					WeaponSet weaponSet = currentItems.AlternateWeaponSets[i];
					if (weaponSet.PrimaryWeapon != null)
					{
						AttackBase component2 = weaponSet.PrimaryWeapon.GetComponent<AttackBase>();
						if (component2 != null)
						{
							return component2;
						}
					}
					if (weaponSet.SecondaryWeapon != null)
					{
						AttackBase component3 = weaponSet.SecondaryWeapon.GetComponent<AttackBase>();
						if (component3 != null)
						{
							return component3;
						}
					}
				}
			}
		}
		AttackBase component4 = owner.GetComponent<AttackBase>();
		if (component4 != null)
		{
			return component4;
		}
		return null;
	}

	protected virtual void HandleCombatEnd(object sender, EventArgs e)
	{
		if (m_ai != null)
		{
			ReloadFirearmsInAlternateWeaponSets(m_ai.Owner);
		}
	}

	public static void ReloadFirearmsInAlternateWeaponSets(GameObject owner)
	{
		if (owner == null)
		{
			return;
		}
		Equipment component = owner.GetComponent<Equipment>();
		if (!(component != null))
		{
			return;
		}
		EquipmentSet currentItems = component.CurrentItems;
		if (currentItems == null)
		{
			return;
		}
		for (int i = 0; i < currentItems.AlternateWeaponSets.Length; i++)
		{
			if (i == currentItems.SelectedWeaponSet)
			{
				continue;
			}
			WeaponSet weaponSet = currentItems.AlternateWeaponSets[i];
			if (weaponSet.PrimaryWeapon != null)
			{
				AttackFirearm component2 = weaponSet.PrimaryWeapon.GetComponent<AttackFirearm>();
				if (component2 != null)
				{
					component2.Reload();
				}
			}
			if (weaponSet.SecondaryWeapon != null)
			{
				AttackFirearm component3 = weaponSet.SecondaryWeapon.GetComponent<AttackFirearm>();
				if (component3 != null)
				{
					component3.Reload();
				}
			}
		}
	}

	public void CancelCurrentAttack()
	{
		if (StateManager.CurrentState is AI.Achievement.Attack attack && attack.CurrentAttack != null && attack.CurrentAttack.CanCancel)
		{
			attack.OnCancel();
		}
	}

	public bool IsFactionSwapped()
	{
		if (IsConfused || (m_stats != null && m_stats.HasFactionSwapEffect()))
		{
			return true;
		}
		return false;
	}

	public virtual bool BeingKited()
	{
		if (!IsTethered())
		{
			return false;
		}
		GameObject currentTarget = CurrentTarget;
		if (currentTarget == null)
		{
			return false;
		}
		AIController component = currentTarget.GetComponent<AIController>();
		if (component == null)
		{
			return false;
		}
		if (!component.IsMoving())
		{
			return false;
		}
		if (m_mover != null && component.m_mover != null && m_mover.DesiredSpeed > component.m_mover.DesiredSpeed)
		{
			return false;
		}
		float num = GameUtilities.V3SqrDistance2D(base.transform.position, component.transform.position);
		float num2 = GameUtilities.V3SqrDistance2D(base.transform.position, m_retreatPosition);
		bool flag = m_retreatPosition.sqrMagnitude > float.Epsilon;
		if (flag && num2 > GetTetherDistanceSq())
		{
			if (num < 9f)
			{
				return false;
			}
		}
		else
		{
			if (flag && num2 < GetTetherDistanceSq() * 0.33f)
			{
				return false;
			}
			if (num < 144f)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsTethered()
	{
		return false;
	}

	public virtual float GetTetherDistanceSq()
	{
		return 400f;
	}

	public void RecordRetreatPosition(Vector3 position)
	{
		m_retreatPosition = position;
	}

	public void StopKiting()
	{
		m_kiteTimer = 4f;
		m_kiteDistanceSq = GameUtilities.V3SqrDistance2D(m_retreatPosition, base.transform.position);
	}

	public bool MoveToRetreatPosition()
	{
		if (!IsTethered())
		{
			return false;
		}
		if (m_retreatPosition.sqrMagnitude < float.Epsilon)
		{
			return false;
		}
		if (GameUtilities.V3SqrDistance2D(base.transform.position, m_retreatPosition) > 4f)
		{
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			pathToPosition.Parameters.Destination = m_retreatPosition;
			pathToPosition.Parameters.StopOnLOS = false;
			pathToPosition.Parameters.Range = 0.75f;
			pathToPosition.Parameters.MovementType = AnimationController.MovementType.Walk;
			pathToPosition.Parameters.GetAsCloseAsPossible = true;
			pathToPosition.Parameters.DesiresMaxRange = false;
			pathToPosition.Parameters.TargetScanner = GetTargetScanner();
			StateManager.PushState(pathToPosition);
			return true;
		}
		return false;
	}

	public void SetParentEncounter(Encounter encounter)
	{
		m_parentEncounter = encounter;
	}

	public virtual bool ShouldCombatEndOnDeath()
	{
		return false;
	}

	public void UpdateMustDieForCombatToEnd()
	{
		if (m_mustDieForCombatToEnd)
		{
			return;
		}
		if (m_parentEncounter != null)
		{
			List<GameObject> spawnedList = m_parentEncounter.GetSpawnedList();
			if (spawnedList.Count <= 0)
			{
				return;
			}
			{
				foreach (GameObject item in spawnedList)
				{
					AIController component = item.GetComponent<AIController>();
					if (component != null)
					{
						component.m_mustDieForCombatToEnd = true;
						component.MarkSummonsMustDieForCombatToEnd();
					}
				}
				return;
			}
		}
		if (ShouldCombatEndOnDeath())
		{
			m_mustDieForCombatToEnd = true;
			MarkSummonsMustDieForCombatToEnd();
		}
	}

	private void MarkSummonsMustDieForCombatToEnd()
	{
		foreach (GameObject summonedCreature in SummonedCreatureList)
		{
			if (summonedCreature != null)
			{
				AIController component = summonedCreature.GetComponent<AIController>();
				if (component != null)
				{
					component.m_mustDieForCombatToEnd = true;
				}
			}
		}
	}

	public TargetScanner GetTargetScanner()
	{
		if (StateManager.DefaultState is ScanForTarget scanForTarget)
		{
			return scanForTarget.TargetScanner;
		}
		if (StateManager.DefaultState is CasterScanForTarget casterScanForTarget)
		{
			return casterScanForTarget.TargetScanner;
		}
		return null;
	}

	private void AddConfusion(float duration)
	{
		Confusion[] components = base.gameObject.GetComponents<Confusion>();
		for (int i = 0; i < components.Length; i++)
		{
			GameUtilities.Destroy(components[i]);
		}
		SafePopAllStates();
		m_teamBeforeConfusion = null;
		Confusion confusion = base.gameObject.AddComponent<Confusion>();
		confusion.Duration = duration;
		confusion.AIController = this;
		m_isConfused = true;
	}

	public void RemoveConfusion()
	{
		if (m_isConfused)
		{
			SafePopAllStates();
			RestoreFactionBeforeConfusion();
			Confusion[] components = base.gameObject.GetComponents<Confusion>();
			for (int i = 0; i < components.Length; i++)
			{
				GameUtilities.Destroy(components[i]);
			}
			m_isConfused = false;
		}
	}

	public void ChangeFactionBecauseOfConfusion(GameObject enemy)
	{
		Faction component = StateManager.CurrentState.Owner.GetComponent<Faction>();
		Faction component2 = enemy.GetComponent<Faction>();
		if (component != null && component2 != null)
		{
			m_teamBeforeConfusion = component.CurrentTeamInstance;
			component.CurrentTeamInstance = component2.CurrentTeamInstance;
			CancelAllEngagements();
		}
	}

	public void RestoreFactionBeforeConfusion()
	{
		if (!(m_teamBeforeConfusion == null))
		{
			Faction component = StateManager.CurrentState.Owner.GetComponent<Faction>();
			if (component != null)
			{
				component.CurrentTeamInstance = m_teamBeforeConfusion;
				CancelAllEngagements();
			}
		}
	}

	public Team GetOriginalTeam()
	{
		if (m_teamBeforeConfusion != null)
		{
			return m_teamBeforeConfusion;
		}
		if (m_stats != null)
		{
			StatusEffect statusEffect = m_stats.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.SwapFaction);
			if (statusEffect != null)
			{
				Team cachedTeam = statusEffect.GetCachedTeam();
				if (cachedTeam != null)
				{
					return cachedTeam;
				}
			}
		}
		Faction component = StateManager.CurrentState.Owner.GetComponent<Faction>();
		if (component != null)
		{
			return component.CurrentTeam;
		}
		return null;
	}

	public static void InitInstructions(AIController aiController)
	{
		List<SpellCastData> instructions = aiController.Instructions;
		s_possibleInstructions.Clear();
		s_defaultTargetPreference = aiController.DefaultTargetPreference;
		s_totalRandomScore = 0f;
		s_defaultInstruction = null;
		PartyMemberAI partyMemberAI = aiController as PartyMemberAI;
		foreach (SpellCastData item in instructions)
		{
			if (!(partyMemberAI != null) || partyMemberAI.UsePerRestAbilitiesInInstructionSet || !item.PerRestAbility)
			{
				if (!item.InCooldown && (IsPartyMemberInstructionValid(item, aiController) || item.Ready))
				{
					s_possibleInstructions.Add(item);
					s_totalRandomScore += item.CastingPriority;
				}
				else if (item.CastInstruction == SpellCastData.Instruction.UseWeapon && s_defaultInstruction == null)
				{
					s_defaultInstruction = item;
				}
			}
		}
	}

	private static bool IsPartyMemberInstructionValid(SpellCastData instruction, AIController aiController)
	{
		PartyMemberAI partyMemberAI = aiController as PartyMemberAI;
		if (partyMemberAI != null)
		{
			CharacterStats component = partyMemberAI.gameObject.GetComponent<CharacterStats>();
			if (component != null)
			{
				GenericAbility genericAbility = component.FindAbilityInstance(instruction.Spell);
				if (genericAbility != null)
				{
					if (!genericAbility.Ready)
					{
						return genericAbility.WhyNotReady == GenericAbility.NotReadyValue.InRecovery;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static SpellCastData GetRandomInstruction()
	{
		float num = 0f;
		float num2 = OEIRandom.Range(0f, s_totalRandomScore);
		for (int i = 0; i < s_possibleInstructions.Count; i++)
		{
			num += (float)s_possibleInstructions[i].CastingPriority;
			if (num > num2)
			{
				return s_possibleInstructions[i];
			}
		}
		Debug.LogError("AIController weighted random selected no valid instructions.");
		return null;
	}

	[Obsolete("BMac: I have no idea what this is supposed to do.")]
	private SpellCastData RandomInstruction()
	{
		float num = 0f;
		float num2 = UnityEngine.Random.Range(0f, 1f);
		List<SpellCastData> instructions = Instructions;
		for (int i = 0; i < instructions.Count; i++)
		{
			num += instructions[i].Odds;
			if (num >= num2)
			{
				return instructions[i];
			}
		}
		return null;
	}

	public virtual SpellCastData GetNextInstruction()
	{
		if (s_possibleInstructions.Count <= 0)
		{
			return null;
		}
		SpellCastData result = null;
		if (InstructionSelectionType == SpellList.InstructionSelectionType.Random)
		{
			result = GetRandomInstruction();
		}
		else if (InstructionSelectionType == SpellList.InstructionSelectionType.TopToBottom)
		{
			result = s_possibleInstructions[0];
		}
		return result;
	}

	public void InstructionProcessed(SpellCastData instruction)
	{
		m_instructionTimer = GetCooldownBetweenSpells();
		instruction.StartTimer();
		instruction.IncrementCastCount();
	}

	public virtual float GetCooldownBetweenSpells()
	{
		return 0f;
	}

	public void InterruptAnimationForCutscene()
	{
		if (StateManager != null)
		{
			StateManager.CurrentState?.InterruptAnimationForCutscene();
		}
	}

	public void SafePopAllStates()
	{
		StateManager.ClearQueuedStates();
		AIState currentState = StateManager.CurrentState;
		if (currentState != null && currentState.Priority < 3)
		{
			StateManager.PopAllStates();
		}
	}
}

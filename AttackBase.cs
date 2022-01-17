using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AttackBase : MonoBehaviour
{
	public enum TargetType
	{
		All,
		Hostile,
		Friendly,
		FriendlyUnconscious,
		AllDeadOrUnconscious,
		HostileWithGrimoire,
		HostileVessel,
		HostileBeast,
		Dead,
		Ally,
		AllyNotSelf,
		AllyNotSelfOrHostile,
		NotSelf,
		DragonOrDrake,
		None,
		FriendlyIncludingCharmed,
		Self,
		FriendlyNotVessel,
		SpiritOrSummonedCreature,
		OwnAnimalCompanion,
		HostileWithNpcAppearance,
		OwnerOfPairedAbility,
		AnyWithResonance
	}

	public enum EffectAttachType
	{
		RightHand,
		LeftHand,
		BothHands,
		Mouth,
		Tail,
		Head,
		Chest,
		LeftFoot,
		RightFoot,
		Hips,
		ElbowRight,
		ElbowLeft,
		LeftEye,
		RightEye,
		Root,
		Fx_Bone_01,
		Fx_Bone_02,
		Fx_Bone_03,
		Fx_Bone_04,
		Fx_Bone_05,
		Fx_Bone_06,
		Fx_Bone_07,
		Fx_Bone_08,
		Fx_Bone_09,
		Fx_Bone_10
	}

	public enum AttackSpeedType
	{
		Instant,
		Short,
		Long
	}

	public enum UIAttackSpeedType
	{
		Undefined,
		Fast,
		Average,
		Slow,
		VerySlow,
		ExtremelySlow
	}

	public class FormattableTarget
	{
		private readonly DatabaseString m_Standalone;

		private readonly DatabaseString m_Qualified;

		private readonly string m_StandaloneOverride;

		private readonly string m_QualifiedOverride;

		private readonly TargetType m_LayerType;

		public FormattableTarget ExtraFormatParameter;

		public string QualifiedString
		{
			get
			{
				if (HasQualifiedString)
				{
					if (string.IsNullOrEmpty(m_QualifiedOverride))
					{
						return m_Qualified.GetText();
					}
					return m_QualifiedOverride;
				}
				return StandaloneString;
			}
		}

		public string StandaloneString
		{
			get
			{
				if (string.IsNullOrEmpty(m_StandaloneOverride))
				{
					return m_Standalone.GetText();
				}
				return m_StandaloneOverride;
			}
		}

		public bool HasQualifiedString
		{
			get
			{
				if (string.IsNullOrEmpty(m_QualifiedOverride))
				{
					return m_Qualified.IsValidString;
				}
				return true;
			}
		}

		public FormattableTarget(int standalone)
			: this(standalone, -1)
		{
		}

		public FormattableTarget(int standalone, int qualified)
			: this(standalone, qualified, TargetType.All)
		{
		}

		public FormattableTarget(int standalone, int qualified, TargetType layerType)
		{
			m_Standalone = new GUIDatabaseString(standalone);
			m_Qualified = new GUIDatabaseString(qualified);
			m_LayerType = layerType;
		}

		public FormattableTarget(DatabaseString standalone, TargetType layerType)
		{
			m_Standalone = standalone;
			m_LayerType = layerType;
		}

		public FormattableTarget(string standalone, string qualified)
		{
			m_StandaloneOverride = standalone;
			m_QualifiedOverride = qualified;
		}

		public string GetText()
		{
			return StandaloneString;
		}

		public string GetText(TargetType targetType)
		{
			targetType = TargetTypeUtils.LayerTargetTypes(targetType, m_LayerType);
			if (TargetTypeUtils.ValidTargetAny(targetType))
			{
				return StringUtility.Format(StandaloneString, TargetTypeUtils.GetValidTargetString(targetType), (ExtraFormatParameter != null) ? ExtraFormatParameter.GetText(targetType) : "");
			}
			if (TargetTypeUtils.ValidTargetSelf(targetType))
			{
				return GUIUtils.GetText(1609);
			}
			if (HasQualifiedString && !TargetTypeUtils.ValidTargetNone(targetType))
			{
				return StringUtility.Format(QualifiedString, TargetTypeUtils.GetValidTargetString(targetType), (ExtraFormatParameter != null) ? ExtraFormatParameter.GetText(targetType) : "");
			}
			return GetText();
		}

		public static implicit operator string(FormattableTarget target)
		{
			return target.GetText();
		}
	}

	public struct AttackEffect
	{
		public AttackBase Attack;

		public string Effect;

		public bool Hostile;

		public bool Secondary;

		public CharacterStats.DefenseType OverrideDefenseType;

		public string EffectPostFormat;

		public bool ConsiderSecondary
		{
			get
			{
				if (Secondary && (bool)Attack)
				{
					return Attack.SecondaryDefense != CharacterStats.DefenseType.None;
				}
				return false;
			}
		}

		public AttackEffect(string effect, AttackBase attack)
			: this(effect, attack, hostile: true)
		{
		}

		public AttackEffect(string effect, CharacterStats.DefenseType overrideDefense)
			: this(effect, null, hostile: true, secondary: true)
		{
			OverrideDefenseType = overrideDefense;
		}

		public AttackEffect(string effect, AttackBase attack, bool hostile)
			: this(effect, attack, hostile, secondary: false)
		{
		}

		public AttackEffect(string effect, AttackBase attack, bool hostile, bool secondary)
		{
			Effect = effect;
			Attack = attack;
			Hostile = hostile;
			Secondary = secondary;
			OverrideDefenseType = CharacterStats.DefenseType.None;
			EffectPostFormat = "{0}";
		}

		public override bool Equals(object obj)
		{
			if (obj is AttackEffect attackEffect)
			{
				if (EffectPostFormat != attackEffect.EffectPostFormat)
				{
					return false;
				}
				bool flag = false;
				flag = (OverrideDefenseType != CharacterStats.DefenseType.None && OverrideDefenseType == attackEffect.OverrideDefenseType) || attackEffect.Secondary == Secondary || !attackEffect.Attack || attackEffect.Attack.SecondaryDefense == CharacterStats.DefenseType.None;
				return attackEffect.Attack == Attack && attackEffect.Hostile == Hostile && flag;
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (!Attack)
			{
				return 0;
			}
			int num = Attack.GetHashCode();
			if (ConsiderSecondary)
			{
				num += 13;
			}
			return num;
		}

		public override string ToString()
		{
			return Effect;
		}
	}

	public string Keywords;

	public float AttackDistance = 6f;

	public float MinAttackDistance;

	public int AttackVariation = 1;

	[FormerlySerializedAs("UseAttackVariationOnFullAttack")]
	[Tooltip("If set, the attack variation is applied to the user's primary or full attack.")]
	public bool UseAttackVariationOnFullOrPrimary;

	public bool ZeroAttackRecovery;

	public AttackSpeedType AttackSpeed = AttackSpeedType.Short;

	public UIAttackSpeedType UiAttackSpeed;

	public float SpeedFactor = 1f;

	public float AttackHitTime = 0.4f;

	[Tooltip("Defines what kinds of characters are valid as initial targets for this attack.")]
	public TargetType ValidPrimaryTargets;

	[Tooltip("Defines what kinds of characters are valid as AOE or beam targets for this attack.")]
	public TargetType ValidTargets;

	public int Bounces;

	public float BounceMultiplier = 0.5f;

	public float BounceRange = 5f;

	public bool BounceInRangeOrder;

	public bool BounceNoRepeatTargets;

	public bool AlwaysBounceAtEnemies;

	public float BounceDelay;

	public float PushDistance;

	[Tooltip("If true, the attacker will orient himself to face his current target.")]
	public bool FaceTarget = true;

	public int AccuracyBonus;

	[Tooltip("If set, the attack will not scale with the attacker's attributes or effects.")]
	public bool IgnoreCharacterStats;

	public AttackBaseScaling LevelScaling = new AttackBaseScaling();

	public AnimationController.ReactionType Reaction;

	public List<StatusEffectParams> StatusEffects = new List<StatusEffectParams>();

	public List<AfflictionParams> Afflictions = new List<AfflictionParams>();

	public bool ApplyOneRandomAffliction;

	[HideInInspector]
	public List<StatusEffectParams> CleanedUpStatusEffects = new List<StatusEffectParams>();

	public DamagePacket DamageData;

	public float DamageMultiplier = 1f;

	public bool RequiresHitObject = true;

	[Tooltip("Determines what the attack's damage is defended with. Also reveals the enemy's defense.")]
	public CharacterStats.DefenseType DefendedBy;

	[Tooltip("Determines what the attack's afflictions are defended with. Also reveals the enemy's defense.")]
	public CharacterStats.DefenseType SecondaryDefense = CharacterStats.DefenseType.None;

	public float DTBypass;

	[Tooltip("Overrides elemental types. Slash, Crush, and Pierce will still show the victim's blood fx from their Health component.")]
	public GameObject OnHitVisualEffect;

	public GameObject OnStartVisualEffect;

	public GameObject OnStartGroundVisualEffect;

	public GameObject OnLaunchVisualEffect;

	public GameObject OnDisengagementVisualEffect;

	public GameObject OnMissVisualEffect;

	[Tooltip("Effect to play on the attacker on hit")]
	public GameObject OnHitAttackerVisualEffect;

	public GameObject OnHitGroundVisualEffect;

	[Tooltip("Effect to play on the attacker root, oriented toward the defender.")]
	public GameObject OnHitAttackerToTargetVisualEffect;

	[Tooltip("Visual effect that attaches to giblets.")]
	public GameObject OnGibletVisualEffect;

	public AttackAOE ExtraAOE;

	public EffectAttachType OnStartAttach;

	[Tooltip("Target's bone to attach when attack hits target")]
	public EffectAttachType OnHitAttach = EffectAttachType.Chest;

	[Tooltip("Owner's bone to attach when attack hits target")]
	public EffectAttachType OnHitAttackerAttach = EffectAttachType.Root;

	public CameraControl.ScreenShakeValues OnHitShakeDuration;

	public CameraControl.ScreenShakeValues OnHitShakeStrength;

	public CharacterStats.NoiseLevelType NoiseLevel;

	public AttackData.InterruptScale BaseInterrupt = AttackData.InterruptScale.Average;

	[Range(0f, 360f)]
	public float TargetAngle;

	[Tooltip("If true, the attack always hits the caster with no targeting cursor.")]
	public bool ApplyToSelfOnly;

	[Tooltip("If true, a target is invalid if it is already under the effects of this attack.")]
	public bool ApplyOnceOnly;

	[Tooltip("If true, the character will move until in range and then attack. If false, the character will attack in the general direction.")]
	public bool PathsToPos = true;

	[HideInInspector]
	public bool IsStealthAttack;

	[Tooltip("This is used for spells and abilities that don't do anything, but still need an animation/visuals")]
	public bool IsFakeAttack;

	[Tooltip("Allows this attack to do things like generate focus and proc Carnage.")]
	public bool TreatAsWeaponAttack;

	protected GameObject m_parent;

	protected GameObject m_enemy;

	protected Vector3 m_destination = Vector3.zero;

	private bool m_skipAnimation;

	protected bool m_disengagementAttack;

	protected float m_animLength = -1f;

	protected float m_speedMultiplier = 1f;

	protected bool m_impactFrameHit;

	protected bool m_hitFrameNotified;

	protected float m_prevRecoveryTime = 1f;

	protected int m_numImpacts;

	private static Vector3 m_bouncePosition;

	protected int m_bounceCount;

	protected List<GameObject> m_bounceObjects;

	protected float m_bounceTimer;

	protected GameObject m_bounceSelf;

	protected GameObject m_bounceEnemy;

	protected Vector3 m_bounceNormal;

	protected const float m_minIdealAttackDistance = 0.15f;

	private const float m_damaged_weapon_damage_multiplier = 0.75f;

	private KeywordCollection m_keywords;

	protected CharacterStats m_ownerStats;

	private Transform m_ownerLaunchTransform;

	private Transform m_ownerTransformRightHand;

	private Transform m_ownerTransformLeftHand;

	private Transform m_ownerTransformMouth;

	private Transform m_ownerTransformTail;

	private AnimationBoneMapper m_mapper;

	protected bool m_first_update;

	protected float m_durationOverride;

	protected float m_friendlyRadiusOverride;

	protected bool m_cancelled;

	protected bool m_can_cancel;

	protected GenericAbility m_ability;

	protected Equippable m_equippable;

	private bool m_attachedToHit;

	private bool m_attachedToHitLoc;

	private bool m_initialized;

	private bool m_launchingDirectlyToImpact;

	private bool m_skipAbilityModAttackStatusEffects;

	private int m_temporaryAccuracyBonus;

	private float m_statAttackSpeedAfterLaunch = 1f;

	private List<object> m_eventsHideSlot = new List<object>();

	private List<object> m_eventsMoveToHand = new List<object>();

	public static bool s_PostUseDelta = false;

	private GenericAbility m_triggeringAbility;

	[HideInInspector]
	public bool PrefabForTrap;

	public static readonly FormattableTarget GENERAL_TARGET = new FormattableTarget(1595, 1600);

	protected static readonly FormattableTarget JUMP_TARGETS = new FormattableTarget(1613, 1608);

	protected static readonly FormattableTarget TARGET_ANIMALCOMPANION = new FormattableTarget(2104);

	public static Color StringKeyColor = new Color(0.68f, 0.68f, 0.68f);

	public bool DestroyAfterImpact { get; set; }

	public AttackBase ParentAttack { get; set; }

	public bool SkipAbilityActivation { get; set; }

	public bool ImpactFrameHit => m_impactFrameHit;

	public float PrevRecoveryTime => m_prevRecoveryTime;

	public bool HasImpactCountRemaining => m_numImpacts > 0;

	public bool DoNotCleanOneHitEffects { get; set; }

	public virtual bool AlreadyActivated => false;

	protected virtual AnimationController.ActionType ActionType => AnimationController.ActionType.Attack;

	public float TotalAttackDistance => GetTotalAttackDistance(Owner);

	public virtual float TotalIdealAttackDistance
	{
		get
		{
			if (IdealAttackDistance < 0.15f)
			{
				return 0.15f;
			}
			return IdealAttackDistance;
		}
	}

	public virtual bool IsRangeUsed => !ApplyToSelfOnly;

	public bool HasForcedTarget
	{
		get
		{
			if (!ApplyToSelfOnly && ValidPrimaryTargets != TargetType.OwnAnimalCompanion)
			{
				return ValidPrimaryTargets == TargetType.OwnerOfPairedAbility;
			}
			return true;
		}
	}

	private GameObject UnvalidatedForcedTarget
	{
		get
		{
			GameObject result = null;
			if (ApplyToSelfOnly && (bool)Owner)
			{
				return Owner;
			}
			if (ValidPrimaryTargets == TargetType.OwnAnimalCompanion && (bool)Owner)
			{
				result = GameUtilities.FindAnimalCompanion(Owner);
			}
			else if (ValidPrimaryTargets == TargetType.OwnerOfPairedAbility)
			{
				PairedAbility component = GetComponent<PairedAbility>();
				if ((bool)component && (bool)component.OtherAbility)
				{
					result = component.OtherAbility.Owner;
				}
			}
			return result;
		}
	}

	public GameObject ForcedTarget
	{
		get
		{
			GameObject unvalidatedForcedTarget = UnvalidatedForcedTarget;
			if ((bool)unvalidatedForcedTarget && IsValidPrimaryTarget(unvalidatedForcedTarget))
			{
				return unvalidatedForcedTarget;
			}
			return null;
		}
	}

	public float RecoveryTimer
	{
		get
		{
			if (m_ownerStats != null)
			{
				return m_ownerStats.RecoveryTimer;
			}
			return 0f;
		}
	}

	public bool CanCancel => m_can_cancel;

	public virtual bool Channeled => false;

	public bool IsDisengagementAttack
	{
		get
		{
			return m_disengagementAttack;
		}
		set
		{
			m_disengagementAttack = value;
		}
	}

	public bool SkipAnimation
	{
		get
		{
			return m_skipAnimation;
		}
		set
		{
			m_skipAnimation = value;
		}
	}

	public GenericAbility TriggeringAbility
	{
		get
		{
			return m_triggeringAbility;
		}
		set
		{
			LastTriggeringAbility = m_triggeringAbility;
			m_triggeringAbility = value;
		}
	}

	public GenericAbility LastTriggeringAbility { get; set; }

	public bool SkipAbilityModAttackStatusEffects
	{
		get
		{
			return m_skipAbilityModAttackStatusEffects;
		}
		set
		{
			m_skipAbilityModAttackStatusEffects = value;
		}
	}

	public int MaxBounces
	{
		get
		{
			if (Bounces > 0)
			{
				return Bounces;
			}
			CharacterStats characterStats = null;
			GameObject owner = Owner;
			if (owner != null)
			{
				characterStats = owner.GetComponent<CharacterStats>();
			}
			if (characterStats == null)
			{
				return Bounces;
			}
			return Bounces + characterStats.ExtraStraightBounces;
		}
	}

	public virtual bool IsBouncing
	{
		get
		{
			if (!(m_bounceTimer > 0f))
			{
				return m_bounceCount > 0;
			}
			return true;
		}
	}

	public float IdealAttackDistance
	{
		get
		{
			float num = AttackDistance - MinAttackDistance;
			return MinAttackDistance + 0.9f * num;
		}
	}

	public int AccuracyBonusTotal
	{
		get
		{
			int num = GetAccuracyBonus() + TemporaryAccuracyBonus;
			if (m_ability != null)
			{
				num += (int)m_ability.GatherAbilityModSum(AbilityMod.AbilityModType.AttackAccuracyBonus);
			}
			return num;
		}
	}

	public int TemporaryAccuracyBonus
	{
		get
		{
			return m_temporaryAccuracyBonus;
		}
		set
		{
			m_temporaryAccuracyBonus = value;
		}
	}

	public float DTBypassTotal
	{
		get
		{
			float num = DTBypass;
			if (m_ability != null)
			{
				num += m_ability.GatherAbilityModSum(AbilityMod.AbilityModType.AttackDTBypass);
			}
			return num;
		}
	}

	public float PushSpeed => 15f;

	public float AnimLength
	{
		get
		{
			return m_animLength;
		}
		set
		{
			m_animLength = value;
		}
	}

	public float AttackSpeedTime
	{
		get
		{
			float result = 0f;
			switch (AttackSpeed)
			{
			case AttackSpeedType.Instant:
				result = AttackData.Instance.InstantAttackSpeed;
				break;
			case AttackSpeedType.Short:
				result = AttackData.Instance.ShortAttackSpeed;
				break;
			case AttackSpeedType.Long:
				result = AttackData.Instance.LongAttackSpeed;
				break;
			}
			return result;
		}
	}

	public GameObject Owner
	{
		get
		{
			if (m_ability != null)
			{
				return m_ability.Owner;
			}
			if (m_parent == null)
			{
				m_parent = GameUtilities.FindParentWithComponent<CharacterStats>(base.gameObject);
			}
			return m_parent;
		}
		set
		{
			m_parent = value;
			if ((bool)value)
			{
				GameObject owner = Owner;
				m_ownerStats = owner.GetComponent<CharacterStats>();
				m_mapper = owner.GetComponent<AnimationBoneMapper>();
			}
			else
			{
				m_ownerStats = null;
				m_mapper = null;
			}
		}
	}

	public bool Jumps
	{
		get
		{
			if (Bounces > 0)
			{
				return RequiresHitObject;
			}
			return false;
		}
	}

	public GenericAbility AbilityOrigin
	{
		get
		{
			return m_ability;
		}
		set
		{
			m_ability = value;
		}
	}

	public Equippable EquippableOrigin
	{
		get
		{
			return m_equippable;
		}
		set
		{
			m_equippable = value;
		}
	}

	public bool LaunchingDirectlyToImpact
	{
		get
		{
			return m_launchingDirectlyToImpact;
		}
		set
		{
			m_launchingDirectlyToImpact = value;
		}
	}

	protected Transform LaunchTransform => GetLaunchTransform();

	public bool IsInvocation => PhraseCost > 0;

	public int PhraseCost
	{
		get
		{
			int result = 0;
			if (m_ability != null && m_ability.ActivationPrerequisites != null)
			{
				PrerequisiteData[] activationPrerequisites = m_ability.ActivationPrerequisites;
				foreach (PrerequisiteData prerequisiteData in activationPrerequisites)
				{
					if (prerequisiteData.Type == PrerequisiteType.CasterPhraseCount && prerequisiteData.IsConsumed)
					{
						result = (int)prerequisiteData.Value;
						break;
					}
				}
			}
			return result;
		}
	}

	public event CombatEventHandler OnLaunched;

	public event CombatEventHandler OnAttackRollCalculated;

	public event CombatEventHandler OnHit;

	public event CombatEventHandler OnCriticalHit;

	public event CombatEventHandler OnAttackComplete;

	public event CombatEventHandler OnKill;

	protected virtual void Init()
	{
		if (m_initialized)
		{
			return;
		}
		AttackDistance = Mathf.Max(AttackDistance, 0.1f);
		NotifyStatusEffectsChanged();
		if (AttackVariation <= 0)
		{
			SkipAnimation = true;
		}
		m_first_update = true;
		m_ability = GetComponent<GenericAbility>();
		m_equippable = GetComponent<Equippable>();
		if (HasForcedTarget)
		{
			if (this is AttackAOE)
			{
				RequiresHitObject = false;
			}
			else
			{
				RequiresHitObject = true;
			}
		}
		GameState.OnLevelUnload += GameState_OnLevelUnload;
		OnCriticalHit += CriticalDeathScreenShake;
		OnCriticalHit += CriticalDeathGib;
		OnAttackComplete += AttackCompleteScreenShake;
		m_initialized = true;
	}

	public void NotifyStatusEffectsChanged()
	{
		UICleanStatusEffects();
	}

	public void UICleanStatusEffects()
	{
		CleanedUpStatusEffects.Clear();
		CleanedUpStatusEffects.AddRange(StatusEffects);
		if ((bool)AbilityOrigin)
		{
			foreach (AbilityMod abilityMod in AbilityOrigin.AbilityMods)
			{
				if (abilityMod.Type == AbilityMod.AbilityModType.AddAttackStatusEffects || abilityMod.Type == AbilityMod.AbilityModType.AddAttackStatusEffectOnCasterOnly)
				{
					CleanedUpStatusEffects.AddRange(abilityMod.StatusEffects);
				}
			}
		}
		StatusEffectParams.CleanUp(CleanedUpStatusEffects);
	}

	protected virtual void Start()
	{
		Init();
	}

	public void ForceInit()
	{
		Init();
	}

	protected virtual void OnDestroy()
	{
		GameState.OnLevelUnload -= GameState_OnLevelUnload;
		OnCriticalHit -= CriticalDeathScreenShake;
		OnCriticalHit -= CriticalDeathGib;
		OnAttackComplete -= AttackCompleteScreenShake;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void GameState_OnLevelUnload(object sender, EventArgs e)
	{
		Cancel();
	}

	private void CriticalDeathScreenShake(object sender, CombatEventArgs e)
	{
		if (!(e.Victim == null) && e.Damage.IsKillingBlow)
		{
			float shakeDuration = CameraControl.Instance.GetShakeDuration(CameraControl.ScreenShakeValues.Catastrophic);
			float shakeStrength = CameraControl.Instance.GetShakeStrength(CameraControl.ScreenShakeValues.Medium);
			CameraControl.Instance.ScreenShake(shakeDuration, shakeStrength);
		}
	}

	private void CriticalDeathGib(object sender, CombatEventArgs e)
	{
		if (!(e.Victim == null))
		{
			Health component = e.Victim.GetComponent<Health>();
			if (component != null && e.Damage.IsKillingBlow && component.CanGib && component.GibList != null)
			{
				GameObject gameObject = null;
				gameObject = ((!(OnGibletVisualEffect == null)) ? OnGibletVisualEffect : GetGibletEffect(e.Damage.Damage.Type, e.Victim));
				component.SpawnGibs(gameObject, GetHitTransform(e.Victim).position);
			}
		}
	}

	private void AttackCompleteScreenShake(object sender, CombatEventArgs e)
	{
		if (e.Damage == null || !e.Damage.IsMiss)
		{
			float shakeDuration = CameraControl.Instance.GetShakeDuration(OnHitShakeDuration);
			float shakeStrength = CameraControl.Instance.GetShakeStrength(OnHitShakeStrength);
			CameraControl.Instance.ScreenShake(shakeDuration, shakeStrength);
		}
	}

	public virtual void SetAbilityParamOverrides(float duration, float friendlyRadius)
	{
		m_durationOverride = duration;
		m_friendlyRadiusOverride = friendlyRadius;
	}

	public virtual void BeginTargeting()
	{
	}

	public virtual void TargetingStopped()
	{
	}

	public virtual bool ForceTarget(out GameObject hitObject)
	{
		hitObject = null;
		return false;
	}

	public virtual void Update()
	{
		m_launchingDirectlyToImpact = false;
		if (m_first_update)
		{
			GameObject owner = Owner;
			if (owner != null && (m_ownerStats == null || m_mapper == null))
			{
				m_ownerStats = owner.GetComponent<CharacterStats>();
				m_mapper = owner.GetComponent<AnimationBoneMapper>();
			}
			m_first_update = false;
		}
		if (m_ownerLaunchTransform == null && m_mapper == null)
		{
			GameObject owner2 = Owner;
			if (owner2 != null)
			{
				m_ownerTransformRightHand = GetRightHandTransform(owner2);
				m_ownerTransformLeftHand = GetLeftHandTransform(owner2);
				m_ownerTransformMouth = GetMouthTransform(owner2, OnStartAttach == EffectAttachType.Mouth);
				m_ownerTransformTail = GetTailTransform(owner2, OnStartAttach == EffectAttachType.Tail);
				m_ownerLaunchTransform = GetLaunchTransform();
			}
		}
		if (m_bounceTimer > 0f)
		{
			m_bounceTimer -= Time.deltaTime;
			CheckBounceTimer();
		}
	}

	public GameObject Launch(GameObject enemy)
	{
		return Launch(enemy, -1);
	}

	public GameObject Launch(GameObject enemy, GenericAbility triggering)
	{
		return Launch(enemy, -1, triggering);
	}

	public GameObject Launch(GameObject enemy, int variationOverride, GenericAbility triggering)
	{
		TriggeringAbility = triggering;
		return Launch(enemy, variationOverride);
	}

	public virtual GameObject Launch(GameObject enemy, int variationOverride)
	{
		m_numImpacts = 1;
		if (m_launchingDirectlyToImpact)
		{
			TriggeringAbility = null;
			return enemy;
		}
		m_cancelled = false;
		m_can_cancel = true;
		m_impactFrameHit = false;
		m_hitFrameNotified = false;
		enemy = CheckRedirect(enemy);
		GameObject owner = Owner;
		GameObject gameObject = owner;
		CharacterStats component = gameObject.GetComponent<CharacterStats>();
		AttackRanged attackRanged = this as AttackRanged;
		if (!attackRanged || !attackRanged.MultiHitRay)
		{
			m_enemy = enemy;
		}
		int num = AttackVariation;
		if (variationOverride >= 0)
		{
			num = variationOverride;
		}
		if (num <= 0)
		{
			SkipAnimation = true;
		}
		AnimationController component2 = gameObject.GetComponent<AnimationController>();
		if (!SkipAnimation && !m_disengagementAttack && component2 != null)
		{
			component2.DesiredAction.m_actionType = AnimationController.ActionType.Attack;
			component2.DesiredAction.m_variation = num;
			component2.DesiredAction.m_speed = CalculateAttackSpeed();
			component2.DesiredAction.m_offhand = CharacterStats.IsOffhandAttack(gameObject, this);
			component2.Instant = AttackSpeed == AttackSpeedType.Instant;
			AnimationController component3 = GetComponent<AnimationController>();
			if ((bool)component3)
			{
				component3.DesiredAction.m_actionType = AnimationController.ActionType.Attack;
				component3.DesiredAction.m_variation = num;
				component3.DesiredAction.m_speed = CalculateAttackSpeed();
				component3.DesiredAction.m_offhand = CharacterStats.IsOffhandAttack(gameObject, this);
			}
			else
			{
				Equipment component4 = gameObject.GetComponent<Equipment>();
				if ((bool)component4)
				{
					Equippable itemInSlot = component4.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Grimoire);
					if ((bool)itemInSlot && component4.ShouldGrimoireBeDisplayed())
					{
						AnimationController component5 = itemInSlot.GetComponent<AnimationController>();
						if (component5 != null && component5 != component2)
						{
							component5.DesiredAction.m_actionType = ActionType;
							component5.DesiredAction.m_variation = num;
							component5.DesiredAction.m_speed = CalculateAttackSpeed();
							itemInSlot.IsAnimating = true;
						}
					}
				}
			}
			AudioBank component6 = GetComponent<AudioBank>();
			if ((bool)component6)
			{
				component6.PlayFrom("Ability");
				component6.PlayFrom("Swing");
			}
			else
			{
				AudioBank component7 = owner.GetComponent<AudioBank>();
				if (component7 != null)
				{
					component7.PlayFrom("Swing");
				}
			}
			if ((bool)owner)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(owner, SoundSet.SoundAction.IAttack, SoundSet.s_ShortVODelay, forceInterrupt: true);
			}
		}
		Equippable component8 = base.gameObject.GetComponent<Equippable>();
		if (component8 != null)
		{
			component8.ApplyLaunchEffects(gameObject);
		}
		if ((bool)component)
		{
			component.TriggerWhenLaunchesAttack(enemy, this, enemy.transform.position);
			CharacterStats characterStats = (enemy ? enemy.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats)
			{
				characterStats.TriggerWhenAttacked(enemy, this);
			}
			m_statAttackSpeedAfterLaunch = component.AttackSpeedMultiplier;
		}
		else
		{
			m_statAttackSpeedAfterLaunch = 1f;
		}
		if (!(this is AttackRanged))
		{
			if (m_disengagementAttack || SkipAnimation)
			{
				m_launchingDirectlyToImpact = true;
				OnImpact(base.gameObject, enemy);
			}
			else if (component2 != null)
			{
				if (!m_attachedToHit)
				{
					component2.OnEventHit += anim_OnEventHit;
					m_attachedToHit = true;
				}
			}
			else
			{
				StartCoroutine(OnImpactDelay(SpeedFactor * AttackHitTime, base.gameObject, enemy));
			}
		}
		if ((bool)component)
		{
			component.NoiseLevel = NoiseLevel;
			component.AdjustLoreReveal(enemy);
		}
		LaunchOnStartVisualEffect();
		GameUtilities.LaunchEffect(OnStartGroundVisualEffect, 1f, Owner.transform.position, m_ability);
		if (this.OnLaunched != null)
		{
			this.OnLaunched(gameObject, new CombatEventArgs(gameObject, enemy));
		}
		if (component2 != null)
		{
			component2.OnEventShowSlot += HandleAnimShowSlot;
			component2.OnEventHideSlot += HandleAnimHideSlot;
			component2.OnEventMoveToHand += HandleAnimMoveToHand;
			component2.OnEventMoveFromHand += HandleAnimMoveFromHand;
		}
		return enemy;
	}

	private void anim_OnEventHit(object sender, EventArgs e)
	{
		if ((bool)this)
		{
			OnImpact(base.gameObject, m_enemy);
			AnimationController component = Owner.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.OnEventHit -= anim_OnEventHit;
			}
			m_attachedToHit = false;
		}
	}

	private void anim_OnEventHitLocation(object sender, EventArgs e)
	{
		if ((bool)this)
		{
			GameObject owner = Owner;
			OnImpact(owner, m_destination);
			AnimationController component = owner.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.OnEventHit -= anim_OnEventHitLocation;
			}
			m_attachedToHitLoc = false;
		}
	}

	private GameObject CheckRedirect(GameObject enemy)
	{
		if (this is AttackMelee && enemy.GetComponent<CharacterStats>().RedirectMeleeAttacks)
		{
			GameObject[] array = GameUtilities.CreaturesInRange(Owner.transform.position, TotalAttackDistance, enemy, includeUnconscious: false);
			if (array != null)
			{
				enemy = array[OEIRandom.Index(array.Length)];
			}
		}
		return enemy;
	}

	public IEnumerator OnImpactDelay(float time, GameObject gameObject, GameObject enemy)
	{
		yield return new WaitForSeconds(time);
		OnImpact(gameObject, enemy);
	}

	public IEnumerator OnImpactDelay(float time, GameObject gameObject, Vector3 position)
	{
		yield return new WaitForSeconds(time);
		OnImpact(gameObject, position);
	}

	public IEnumerator OnImpactDelay(float time, GameObject gameObject, GameObject enemy, int bounceCount)
	{
		yield return new WaitForSeconds(time);
		m_bounceCount = bounceCount;
		OnImpact(gameObject, enemy);
	}

	public virtual void OnDeactivateAbility()
	{
	}

	public void Launch(Vector3 location, GameObject enemy)
	{
		Launch(location, enemy, -1);
	}

	public virtual void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		m_numImpacts = 1;
		m_destination = location;
		m_cancelled = false;
		m_can_cancel = true;
		m_impactFrameHit = false;
		m_hitFrameNotified = false;
		GameObject owner = Owner;
		GameObject gameObject = owner;
		CharacterStats component = gameObject.GetComponent<CharacterStats>();
		if (!(this is AttackRanged) || !(this as AttackRanged).MultiHitRay)
		{
			m_enemy = enemy;
		}
		int num = AttackVariation;
		if (variationOverride >= 0)
		{
			num = variationOverride;
		}
		if (num <= 0)
		{
			SkipAnimation = true;
		}
		AnimationController component2 = gameObject.GetComponent<AnimationController>();
		if (!SkipAnimation && !IsDisengagementAttack && component2 != null)
		{
			component2.DesiredAction.m_actionType = ActionType;
			component2.DesiredAction.m_variation = num;
			component2.DesiredAction.m_speed = CalculateAttackSpeed();
			component2.Instant = AttackSpeed == AttackSpeedType.Instant;
			AnimationController component3 = GetComponent<AnimationController>();
			if (component3 != null && component3 != component2)
			{
				component3.DesiredAction.m_actionType = ActionType;
				component3.DesiredAction.m_variation = num;
				component3.DesiredAction.m_speed = CalculateAttackSpeed();
			}
			else
			{
				Equipment component4 = gameObject.GetComponent<Equipment>();
				if ((bool)component4)
				{
					Equippable itemInSlot = component4.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Grimoire);
					if ((bool)itemInSlot && component4.ShouldGrimoireBeDisplayed())
					{
						AnimationController component5 = itemInSlot.GetComponent<AnimationController>();
						if (component5 != null && component5 != component2)
						{
							component5.DesiredAction.m_actionType = ActionType;
							component5.DesiredAction.m_variation = num;
							component5.DesiredAction.m_speed = CalculateAttackSpeed();
							itemInSlot.IsAnimating = true;
						}
					}
				}
			}
			AudioBank component6 = GetComponent<AudioBank>();
			if ((bool)component6)
			{
				component6.PlayFrom("Ability");
				component6.PlayFrom("Swing");
			}
			else
			{
				AudioBank component7 = owner.GetComponent<AudioBank>();
				if (component7 != null)
				{
					component7.PlayFrom("Swing");
				}
			}
		}
		Equippable component8 = base.gameObject.GetComponent<Equippable>();
		if (component8 != null)
		{
			component8.ApplyLaunchEffects(gameObject);
		}
		if ((bool)component)
		{
			component.TriggerWhenLaunchesAttack(enemy, this, location);
			CharacterStats characterStats = (enemy ? enemy.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats)
			{
				characterStats.TriggerWhenAttacked(enemy, this);
			}
			m_statAttackSpeedAfterLaunch = component.AttackSpeedMultiplier;
		}
		else
		{
			m_statAttackSpeedAfterLaunch = 1f;
		}
		if (!(this is AttackRanged))
		{
			if (component2 != null && !SkipAnimation)
			{
				if (!m_attachedToHitLoc)
				{
					component2.OnEventHit += anim_OnEventHitLocation;
				}
				m_attachedToHitLoc = true;
			}
			else if ((bool)enemy)
			{
				StartCoroutine(OnImpactDelay(SpeedFactor * AttackHitTime, base.gameObject, enemy));
			}
			else
			{
				StartCoroutine(OnImpactDelay(SpeedFactor * AttackHitTime, base.gameObject, location));
			}
		}
		if ((bool)component)
		{
			component.AdjustLoreReveal(enemy);
		}
		if (IsDisengagementAttack && OnDisengagementVisualEffect != null)
		{
			GameUtilities.LaunchEffect(OnDisengagementVisualEffect, 1f, LaunchTransform.position, m_ability);
		}
		else
		{
			LaunchOnStartVisualEffect();
			GameUtilities.LaunchEffect(OnStartGroundVisualEffect, 1f, owner.transform.position, m_ability);
		}
		if (this.OnLaunched != null)
		{
			this.OnLaunched(gameObject, new CombatEventArgs(gameObject, location));
		}
		if (component2 != null)
		{
			component2.OnEventShowSlot += HandleAnimShowSlot;
			component2.OnEventHideSlot += HandleAnimHideSlot;
			component2.OnEventMoveToHand += HandleAnimMoveToHand;
			component2.OnEventMoveFromHand += HandleAnimMoveFromHand;
		}
	}

	protected void TriggerNoise()
	{
		GameObject owner = Owner;
		if (owner != null)
		{
			CharacterStats component = owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.NoiseLevel = NoiseLevel;
			}
		}
	}

	public void LaunchOnStartVisualEffect()
	{
		if (m_mapper != null)
		{
			GameObject owner = base.gameObject;
			if (base.gameObject.GetComponent<Equippable>() == null)
			{
				owner = Owner;
			}
			GameUtilities.LaunchEffect(OnStartVisualEffect, 1f, m_mapper[owner, OnStartAttach], m_ability);
		}
	}

	public float CalculateAttackSpeed()
	{
		m_speedMultiplier = 1f;
		if (m_ownerStats != null)
		{
			m_speedMultiplier *= m_ownerStats.StatAttackSpeedMultiplier;
		}
		if (m_ability != null)
		{
			m_speedMultiplier *= m_ability.GatherAbilityModProduct(AbilityMod.AbilityModType.AttackSpeedMultiplier);
		}
		return m_speedMultiplier;
	}

	public float FindEquipmentLaunchAccuracyBonus()
	{
		float num = 0f;
		Equippable component = base.gameObject.GetComponent<Equippable>();
		if (component != null)
		{
			num += component.FindLaunchAccuracyBonus(this);
		}
		return num;
	}

	public virtual float GetTotalAttackDistance(GameObject character)
	{
		if (m_ability != null && character != null && (m_ability.UsePrimaryAttack || m_ability.UseFullAttack))
		{
			Equipment component = character.GetComponent<Equipment>();
			if (component != null)
			{
				AttackBase primaryAttack = component.PrimaryAttack;
				if (primaryAttack != null)
				{
					return primaryAttack.TotalAttackDistance;
				}
			}
		}
		return AttackDistance;
	}

	public bool IsInRange(GameObject caster, GameObject target, Vector3 targetLocation)
	{
		float num = TotalAttackDistance;
		if (this is AttackMelee)
		{
			Mover component = caster.GetComponent<Mover>();
			if ((bool)component)
			{
				num += component.Radius;
			}
		}
		Mover mover = (target ? target.GetComponent<Mover>() : null);
		if ((bool)mover)
		{
			num += mover.Radius;
		}
		return GameUtilities.V3Distance2D(caster.transform.position, targetLocation) <= num;
	}

	public virtual void Interrupt()
	{
		if (this == null)
		{
			return;
		}
		GameObject owner = Owner;
		AnimationController component = owner.GetComponent<AnimationController>();
		Health component2 = owner.GetComponent<Health>();
		if (component != null)
		{
			component.DesiredAction.Reset();
			component.ClearActions();
			if (component2 == null || !component2.Dead)
			{
				component.Interrupt();
			}
			if (m_attachedToHit)
			{
				component.OnEventHit -= anim_OnEventHit;
				m_attachedToHit = false;
			}
			if (m_attachedToHitLoc)
			{
				component.OnEventHit -= anim_OnEventHitLocation;
				m_attachedToHitLoc = false;
			}
		}
		if (m_ability != null && m_ability is GenericSpell)
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(owner, SoundSet.SoundAction.SpellCastFailure, SoundSet.s_LongVODelay, forceInterrupt: false);
		}
		AnimationController component3 = GetComponent<AnimationController>();
		if (component3 != null)
		{
			component3.DesiredAction.Reset();
			component3.ClearActions();
			component3.Interrupt();
		}
		Equipment component4 = owner.GetComponent<Equipment>();
		if ((bool)component4)
		{
			Equippable itemInSlot = component4.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Grimoire);
			if ((bool)itemInSlot && component4.ShouldGrimoireBeDisplayed())
			{
				AnimationController component5 = itemInSlot.GetComponent<AnimationController>();
				if (component5 != null && component5 != component)
				{
					component5.DesiredAction.Reset();
					component5.ClearActions();
					component5.Interrupt();
					itemInSlot.IsAnimating = false;
				}
			}
		}
		ClearAnimEventLists(warn_if_not_empty: false);
		if (m_ability != null)
		{
			m_ability.AttackComplete = true;
		}
		if (owner != null)
		{
			if (m_ownerStats != null)
			{
				m_ownerStats.TriggerWhenInterrupted();
			}
			CleanUpAttack(m_enemy);
		}
		m_temporaryAccuracyBonus = 0;
		m_can_cancel = false;
	}

	public virtual void OnImpact(GameObject self, Vector3 hitPosition)
	{
		if (!m_cancelled)
		{
			m_can_cancel = false;
			TriggerNoise();
			if ((bool)m_ownerStats && !m_hitFrameNotified)
			{
				m_hitFrameNotified = true;
				m_ownerStats.NotifyHitFrame(null, new DamageInfo(null, 0f, this));
			}
			m_impactFrameHit = true;
			ResetAttackVars();
			if (m_ability != null && !(this is AttackRanged))
			{
				m_ability.Activate(hitPosition);
			}
			if (this.OnAttackComplete != null)
			{
				this.OnAttackComplete(Owner, new CombatEventArgs(Owner, hitPosition));
			}
			if (m_ability != null)
			{
				m_ability.AttackComplete = true;
			}
			if (DestroyAfterImpact)
			{
				GameUtilities.Destroy(base.gameObject, 1f);
				DestroyAfterImpact = false;
			}
		}
	}

	protected GameObject FindBounceTarget(GameObject bounceSource, GameObject bouncer)
	{
		if (BounceInRangeOrder || BounceNoRepeatTargets)
		{
			int bounceCount = GetBounceCount(bouncer);
			List<GameObject> list;
			if (bounceCount == 0)
			{
				list = MakeBounceList(bounceSource);
				if (BounceInRangeOrder)
				{
					m_bouncePosition = Owner.transform.position;
					list.Sort(CompareByDistance);
				}
				else
				{
					list.Shuffle();
				}
				SetBounceList(bouncer, list);
			}
			else
			{
				list = GetBounceList(bouncer);
			}
			if (list == null || bounceCount >= list.Count)
			{
				return null;
			}
			return list[bounceCount];
		}
		List<GameObject> list2 = MakeBounceList(bounceSource);
		if (list2.Count == 0)
		{
			return null;
		}
		return list2[OEIRandom.Index(list2.Count)];
	}

	protected List<GameObject> GetBounceList(GameObject bouncer)
	{
		if (bouncer != null)
		{
			Projectile component = bouncer.GetComponent<Projectile>();
			if (component != null)
			{
				return component.BounceObjects;
			}
		}
		return m_bounceObjects;
	}

	protected void SetBounceList(GameObject bouncer, List<GameObject> list)
	{
		if (bouncer != null)
		{
			Projectile component = bouncer.GetComponent<Projectile>();
			if (component != null)
			{
				component.BounceObjects = list;
			}
		}
		m_bounceObjects = list;
	}

	protected List<GameObject> MakeBounceList(GameObject bounceSource)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject owner = Owner;
		if (owner == null)
		{
			return list;
		}
		float num = BounceRange;
		Faction component = bounceSource.GetComponent<Faction>();
		if (component != null)
		{
			num += component.CachedRadius;
		}
		GameObject[] array = GameUtilities.CreaturesInRange(bounceSource.transform.position, num, playerEnemiesOnly: false, includeUnconscious: false);
		if (array != null)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject == bounceSource || (Bounces == 0 && Vector3.Dot(bounceSource.transform.position - owner.transform.position, gameObject.transform.position - bounceSource.transform.position) < 0.7071f))
				{
					continue;
				}
				if (AlwaysBounceAtEnemies || Bounces == 0)
				{
					if (IsValidTarget(gameObject, TargetType.Hostile))
					{
						list.Add(gameObject);
					}
				}
				else if (IsValidTarget(gameObject))
				{
					list.Add(gameObject);
				}
			}
		}
		return list;
	}

	private static int CompareByDistance(GameObject i1, GameObject i2)
	{
		float sqrMagnitude = (i1.transform.position - m_bouncePosition).sqrMagnitude;
		float sqrMagnitude2 = (i2.transform.position - m_bouncePosition).sqrMagnitude;
		if (sqrMagnitude < sqrMagnitude2)
		{
			return -1;
		}
		if (sqrMagnitude > sqrMagnitude2)
		{
			return 1;
		}
		return 0;
	}

	protected virtual void CheckBouncing(GameObject self, GameObject enemy)
	{
		if (Bounces == 0 && (self == null || self.GetComponent<Projectile>() == null))
		{
			ResetBounceCount(self);
			return;
		}
		if (MaxBounces == GetBounceCount(self))
		{
			ResetBounceCount(self);
			return;
		}
		if (enemy != null)
		{
			Health component = enemy.GetComponent<Health>();
			if ((bool)component && component.Dead)
			{
				ResetBounceCount(self);
				return;
			}
		}
		float bounceDelay = BounceDelay;
		if (self != null)
		{
			Projectile component2 = self.GetComponent<Projectile>();
			if (component2 != null)
			{
				component2.PrepareBounceEnemy(bounceDelay, enemy);
				return;
			}
		}
		m_bounceTimer = bounceDelay;
		m_bounceSelf = self;
		m_bounceEnemy = enemy;
		ProjectileIsDestructible(self, destructible: false);
		CheckBounceTimer();
	}

	public void CheckBounceTimer()
	{
		if (m_bounceTimer <= 0f)
		{
			m_bounceTimer = 0f;
			if (m_bounceEnemy != null)
			{
				HandleBouncing(m_bounceSelf, m_bounceEnemy);
			}
			else if (this is AttackRanged)
			{
				(this as AttackRanged).HandleBouncing(m_bounceSelf, m_bounceNormal);
			}
		}
	}

	public virtual void HandleBouncing(GameObject self, GameObject enemy)
	{
		ProjectileIsDestructible(self, destructible: true);
		GameObject gameObject = FindBounceTarget(enemy, self);
		if (gameObject != null)
		{
			IncrementBounceCount(self);
			OnImpact(self, gameObject);
		}
		else
		{
			ResetBounceCount(self);
		}
	}

	protected virtual void ProjectileIsDestructible(GameObject projectile, bool destructible)
	{
		if (projectile != null)
		{
			Projectile component = projectile.GetComponent<Projectile>();
			if (component != null)
			{
				component.IsDestructible = destructible;
			}
		}
	}

	public void HandleBeam(GameObject self, GameObject enemy, Vector3 targetPosition)
	{
		Stealth.SetInStealthMode(self, inStealth: false);
		Stealth.SetInStealthMode(enemy, inStealth: false);
		if (enemy != null)
		{
			targetPosition = enemy.transform.position;
		}
		GameObject[] array = GameUtilities.CreaturesAlongBeam(self.transform.position, targetPosition, playerEnemiesOnly: false);
		TargetType validType = ValidTargets;
		if (this is AttackBeam)
		{
			validType = (this as AttackBeam).BeamTargets;
		}
		if (array == null)
		{
			return;
		}
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject != null && gameObject != self && gameObject != enemy && IsValidTarget(gameObject, validType))
			{
				OnImpact(self, gameObject, isMainTarget: false);
			}
		}
	}

	private GameObject GetGibletEffect(DamagePacket.DamageType type, GameObject enemy)
	{
		GameObject gameObject = null;
		if (type == DamagePacket.DamageType.Slash || type == DamagePacket.DamageType.Pierce || type == DamagePacket.DamageType.Crush)
		{
			gameObject = enemy.GetComponent<Health>().GibletEffect;
		}
		else if (OnGibletVisualEffect != null)
		{
			return gameObject;
		}
		if (gameObject == null)
		{
			gameObject = AttackData.GetDefaultGibEffect(type);
		}
		return gameObject;
	}

	private GameObject GetHitEffect(DamagePacket.DamageType type, GameObject enemy)
	{
		GameObject gameObject = null;
		if (type == DamagePacket.DamageType.Slash || type == DamagePacket.DamageType.Pierce || type == DamagePacket.DamageType.Crush)
		{
			gameObject = enemy.GetComponent<Health>().BloodEffect;
		}
		else if (OnHitVisualEffect != null)
		{
			return gameObject;
		}
		if (gameObject == null)
		{
			gameObject = AttackData.GetDefaultEffect(type);
		}
		return gameObject;
	}

	public virtual void OnImpact(GameObject self, GameObject enemy)
	{
		OnImpact(self, enemy, isMainTarget: true);
	}

	public virtual void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		if (m_cancelled || enemy == null)
		{
			m_can_cancel = false;
			TriggerNoise();
			if (!isMainTarget)
			{
				return;
			}
			m_numImpacts--;
			if (m_numImpacts <= 0)
			{
				CleanUpAttack(enemy);
				if (this.OnAttackComplete != null)
				{
					this.OnAttackComplete(Owner, new CombatEventArgs(Owner, enemy));
				}
				if (m_ability != null)
				{
					m_ability.AttackComplete = true;
				}
			}
			return;
		}
		m_can_cancel = false;
		TriggerNoise();
		if ((bool)m_ownerStats && !m_hitFrameNotified)
		{
			m_hitFrameNotified = true;
			m_ownerStats.NotifyHitFrame(enemy, new DamageInfo(enemy, 0f, this));
		}
		m_impactFrameHit = true;
		GameObject owner = Owner;
		CharacterStats component = owner.GetComponent<CharacterStats>();
		CharacterStats component2 = enemy.GetComponent<CharacterStats>();
		if (!SkipAbilityActivation && GetBounceCount(self) == 0 && m_ability != null && !(this is AttackRanged) && !(this is AttackBeam))
		{
			m_ability.Activate(enemy);
		}
		Console.Instance.ResetMessageDelta();
		List<StatusEffect> appliedEffects = new List<StatusEffect>();
		List<StatusEffect> list = new List<StatusEffect>();
		DamageInfo damageInfo = null;
		CalcDamage(enemy, self, out var damage);
		if ((bool)component)
		{
			damage.AttackerDTBypass = component.DTBypass;
			if (this is AttackRanged)
			{
				damage.AttackerDTBypass += component.RangedDTBypass;
			}
			if (this is AttackMelee)
			{
				damage.AttackerDTBypass += component.MeleeDTBypass;
			}
		}
		if (component != null)
		{
			if (damage.IsMiss)
			{
				component.TriggerWhenMisses(enemy, damage);
			}
			else
			{
				component.TriggerWhenHits(enemy, damage);
			}
		}
		if (!damage.IsMiss && (bool)component2)
		{
			component2.TriggerWhenHit(owner, damage);
		}
		if (damage.IsMiss)
		{
			GameUtilities.LaunchEffect(OnMissVisualEffect, 1f, enemy.transform, m_ability);
			int num = damage.AccuracyRating - damage.DefenseRating;
			if (damage.RawRoll + num < 0)
			{
				PlayCriticalMiss();
			}
			AudioBank component3 = owner.GetComponent<AudioBank>();
			if (component3 != null)
			{
				bool flag = false;
				Weapon component4 = base.gameObject.GetComponent<Weapon>();
				if (component4 != null)
				{
					flag = component4.PlayMissSound();
				}
				if (!flag)
				{
					component3.PlayFrom("Miss");
				}
			}
		}
		else
		{
			PushEnemy(enemy, PushDistance, Owner ? Owner.transform.position : base.transform.position, damage, appliedEffects);
			if ((bool)component2)
			{
				if ((HasKeyword("poison") || HasAfflictionWithKeyword("poison")) && !component2.HasStatusEffectWithSearchFunction((StatusEffect effect) => effect?.IsPoisonEffect ?? false))
				{
					SoundSet.TryPlayVoiceEffectWithLocalCooldown(component2.gameObject, SoundSet.SoundAction.Poisoned, SoundSet.s_LongVODelay, forceInterrupt: false);
				}
				ApplyStatusEffects(enemy, damage, deleteOnClear: true, appliedEffects);
				damageInfo = ApplyAfflictions(enemy, damage, deleteOnClear: true, appliedEffects, list);
				Equippable component5 = base.gameObject.GetComponent<Equippable>();
				if (component5 != null)
				{
					component5.ApplyItemModAttackEffects(base.gameObject, component2, damage, appliedEffects);
				}
			}
			Transform parent = GetTransform(owner, OnHitAttackerAttach);
			GameUtilities.LaunchEffect(OnHitAttackerVisualEffect, 1f, parent, m_ability);
			GameUtilities.LaunchEffect(OnHitGroundVisualEffect, 1f, enemy.transform.position, m_ability);
			Transform hitTransform = GetHitTransform(enemy);
			GameUtilities.LaunchEffect(OnHitVisualEffect, 1f, hitTransform, m_ability);
			if ((bool)OnHitAttackerToTargetVisualEffect && (bool)enemy)
			{
				Quaternion orientation = Quaternion.LookRotation(enemy.transform.position - Owner.transform.position);
				GameUtilities.LaunchEffect(OnHitAttackerToTargetVisualEffect, 1f, Owner.transform.position, orientation, AbilityOrigin);
			}
			Weapon component6 = base.gameObject.GetComponent<Weapon>();
			if (component6 != null)
			{
				component6.Deteriorate();
			}
			Equipment component7 = enemy.GetComponent<Equipment>();
			if ((bool)component7 && component7.CurrentItems != null)
			{
				Equippable chest = component7.CurrentItems.Chest;
				if ((bool)chest)
				{
					chest.Deteriorate();
				}
				Shield equippedShield = component7.EquippedShield;
				if (equippedShield != null)
				{
					chest = equippedShield.gameObject.GetComponent<Equippable>();
					if ((bool)chest)
					{
						chest.Deteriorate();
					}
				}
			}
			if (IsDisengagementAttack && AttackData.Instance.DefaultDisengagementFx != null)
			{
				Quaternion orientation2 = Quaternion.LookRotation((owner.transform.position - enemy.transform.position).normalized);
				Vector3 position = owner.transform.position;
				position.y += 0f;
				GameUtilities.LaunchEffect(AttackData.Instance.DefaultDisengagementFx, 1f, position, orientation2, m_ability);
			}
			AudioBank component8 = owner.GetComponent<AudioBank>();
			if (component8 != null)
			{
				component8.PlayFrom("HitEnemy");
			}
		}
		Health component9 = enemy.GetComponent<Health>();
		if ((bool)component9)
		{
			bool showDead = component9.ShowDead;
			bool flag2 = true;
			if (!IsFakeAttack)
			{
				flag2 = component9.DoDamage(damage, owner) > 0f;
				if ((bool)component2 && flag2)
				{
					component.TriggerWhenInflictsDamage(enemy, damage);
				}
			}
			if (!showDead && component9.ShowDead && this.OnKill != null)
			{
				this.OnKill(Owner, new CombatEventArgs(damage, self, enemy));
			}
			if (!damage.IsMiss && !component9.Unconscious && !component9.Dead)
			{
				Weapon component10 = base.gameObject.GetComponent<Weapon>();
				if (component10 != null)
				{
					if (!damage.IsIneffective || !component10.PlayMinDamageSound())
					{
						component10.PlayHitSound();
					}
				}
				else if (this is AttackMelee && (this as AttackMelee).Unarmed)
				{
					AudioBank component11 = owner.GetComponent<AudioBank>();
					ClipBankSet weaponHitSoundSet = GlobalAudioPlayer.Instance.GetWeaponHitSoundSet(WeaponSpecializationData.WeaponType.Unarmed);
					if ((bool)component11 && weaponHitSoundSet != null)
					{
						component11.PlayFrom(weaponHitSoundSet);
					}
				}
			}
			GameObject hitEffect = GetHitEffect(damage.Damage.Type, enemy);
			if (hitEffect != null && flag2)
			{
				Transform hitTransform2 = GetHitTransform(enemy);
				GameUtilities.LaunchEffect(hitEffect, 1f, hitTransform2, m_ability);
			}
		}
		if (damage.IsKillingBlow)
		{
			bool flag3 = false;
			if ((bool)component9)
			{
				flag3 = component9.AlwaysGib;
			}
			if ((damage.IsCriticalHit && this.OnCriticalHit != null) || Health.BloodyMess || flag3)
			{
				this.OnCriticalHit(owner, new CombatEventArgs(damage, owner, enemy));
			}
		}
		if (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(owner.transform.position) || FogOfWar.Instance.PointVisible(enemy.transform.position))
		{
			s_PostUseDelta = true;
			PostAttackMessages(enemy, damage, appliedEffects, primaryAttack: true);
			if (damageInfo != null)
			{
				PostAttackMessagesSecondaryEffect(enemy, damageInfo, list);
			}
			s_PostUseDelta = false;
		}
		ResetAttackVars();
		CheckBouncing(self, enemy);
		if (!damage.IsMiss && ExtraAOE != null && (!(this is AttackRanged) || !(this as AttackRanged).ExtraAOEOnBounce))
		{
			AttackAOE attackAOE = UnityEngine.Object.Instantiate(ExtraAOE);
			attackAOE.DestroyAfterImpact = true;
			attackAOE.Owner = owner;
			attackAOE.transform.parent = owner.transform;
			attackAOE.SkipAnimation = true;
			attackAOE.ParentAttack = this;
			attackAOE.m_ability = m_ability;
			attackAOE.ShowImpactEffect(enemy.transform.position);
			GameObject excludedObject = null;
			if (attackAOE.ExcludeTarget)
			{
				excludedObject = enemy;
			}
			attackAOE.OnImpactShared(null, enemy.transform.position, excludedObject);
		}
		if (isMainTarget)
		{
			m_numImpacts--;
			if (m_numImpacts <= 0)
			{
				CleanUpAttack(enemy);
				if (this.OnAttackComplete != null)
				{
					this.OnAttackComplete(owner, new CombatEventArgs(damage, owner, enemy));
				}
				if (m_ability != null)
				{
					m_ability.AttackComplete = true;
				}
				if (IsHostile(enemy, DamageData))
				{
					GameState.AutoPause(AutoPauseOptions.PauseEvent.CharacterAttacked, enemy, self);
				}
			}
		}
		if (DestroyAfterImpact)
		{
			GameUtilities.Destroy(base.gameObject, 1f);
			DestroyAfterImpact = false;
		}
		if (!damage.TargetPreviouslyDead && (bool)component9 && (component9.Dead || component9.Unconscious))
		{
			if ((bool)GameState.GetActiveCombatant(enemyOnly: true))
			{
				GameState.AutoPause(AutoPauseOptions.PauseEvent.TargetDestroyed, owner, enemy);
			}
		}
		else
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.PartyMemberAttacked, enemy, owner);
		}
	}

	protected void PlayCriticalMiss()
	{
		SoundSet.TryPlayVoiceEffectWithLocalCooldown(Owner, SoundSet.SoundAction.CriticalMiss, SoundSet.s_MediumVODelay, forceInterrupt: false);
	}

	protected void CleanUpAttack(GameObject enemy)
	{
		CleanUpOneHitUseEffects(enemy);
		TriggeringAbility = null;
	}

	protected void CleanUpOneHitUseEffects(GameObject enemy)
	{
		GameObject owner = Owner;
		if (DoNotCleanOneHitEffects)
		{
			return;
		}
		if (m_ownerStats != null)
		{
			m_ownerStats.CleanOneHitUseEffects(owner);
		}
		if (!(enemy != null))
		{
			return;
		}
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			component.CleanOneHitUseEffects(owner);
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(enemy);
		if (gameObject != null)
		{
			CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
			if (component2 != null)
			{
				component2.CleanOneHitUseEffects(owner);
			}
		}
	}

	protected void ResetAttackVars()
	{
		IsDisengagementAttack = false;
		IsStealthAttack = false;
		m_temporaryAccuracyBonus = 0;
	}

	public void PushEnemy(GameObject enemy, float distance, Vector3 source, DamageInfo damage, List<StatusEffect> appliedEffects)
	{
		if (damage != null)
		{
			if (damage.IsGraze)
			{
				distance *= CharacterStats.GrazeMultiplier;
			}
			else if (damage.IsCriticalHit)
			{
				distance *= CharacterStats.CritMultiplier;
			}
		}
		if (distance != 0f)
		{
			StatusEffectParams param = new StatusEffectParams
			{
				AffectsStat = StatusEffect.ModifiedStat.Push,
				Duration = 0.5f,
				Value = distance,
				ExtraValue = PushSpeed
			};
			StatusEffect item = CreateChildEffect(param, 0.5f, damage, deleteOnClear: true);
			appliedEffects?.Add(item);
			PushEnemyHelper(enemy, source, distance, PushSpeed);
		}
	}

	public static void PushEnemyHelper(GameObject enemy, Vector3 source, float distance, float speed)
	{
		if (enemy == null)
		{
			return;
		}
		Mover component = enemy.GetComponent<Mover>();
		if (!(component == null))
		{
			if (distance > float.Epsilon)
			{
				component.Push(enemy, (enemy.transform.position - source).normalized, distance, speed, lockOrientation: false, orientBackwards: true);
			}
			else if (distance < 0f)
			{
				component.Push(enemy, (source - enemy.transform.position).normalized, 0f - distance, speed, lockOrientation: false, orientBackwards: false);
			}
		}
	}

	public virtual void ApplyStatusEffects(GameObject enemy, DamageInfo hitInfo, bool deleteOnClear, List<StatusEffect> appliedEffects)
	{
		if (StatusEffects.Count > 0)
		{
			CharacterStats component = enemy.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				foreach (StatusEffectParams statusEffect2 in StatusEffects)
				{
					float duration = statusEffect2.Duration;
					if (m_durationOverride > 0f)
					{
						duration = m_durationOverride;
					}
					duration = statusEffect2.AdjustDuration(m_ownerStats, duration);
					StatusEffect statusEffect = CreateChildEffect(statusEffect2, duration, hitInfo, deleteOnClear);
					if (component.ApplyStatusEffectImmediate(statusEffect))
					{
						appliedEffects?.Add(statusEffect);
					}
				}
			}
		}
		if (!SkipAbilityModAttackStatusEffects && m_ability != null)
		{
			m_ability.ApplyAbilityModAttackStatusEffects(enemy, hitInfo, deleteOnClear, appliedEffects);
		}
	}

	protected StatusEffect CreateChildEffect(StatusEffectParams param, float duration, DamageInfo hitInfo, bool deleteOnClear)
	{
		GenericAbility.AbilityType abType = GenericAbility.AbilityType.Ability;
		if (m_ability != null && m_ability is GenericSpell)
		{
			abType = GenericAbility.AbilityType.Spell;
		}
		StatusEffect statusEffect = StatusEffect.Create(Owner, param, abType, hitInfo, deleteOnClear, duration);
		statusEffect.AbilityOrigin = statusEffect.AbilityOrigin;
		if (statusEffect.AbilityOrigin == null)
		{
			statusEffect.AbilityOrigin = m_ability;
		}
		if (statusEffect.AbilityOrigin == null)
		{
			statusEffect.AbilityOrigin = TriggeringAbility;
		}
		if (statusEffect.AbilityOrigin == null && (bool)ParentAttack)
		{
			statusEffect.AbilityOrigin = ParentAttack.AbilityOrigin;
		}
		if (statusEffect.EquipmentOrigin == null)
		{
			statusEffect.EquipmentOrigin = m_equippable;
		}
		if (statusEffect.EquipmentOrigin == null && (bool)ParentAttack)
		{
			statusEffect.EquipmentOrigin = ParentAttack.m_equippable;
		}
		statusEffect.FriendlyRadius = m_friendlyRadiusOverride;
		return statusEffect;
	}

	public virtual DamageInfo ApplyAfflictions(GameObject enemy, DamageInfo hitInfo, bool deleteOnClear, List<StatusEffect> appliedEffects, List<StatusEffect> appliedSecondaryEffects)
	{
		if (Afflictions.Count == 0)
		{
			return null;
		}
		bool flag = false;
		DamageInfo damageInfo;
		if (hitInfo == null)
		{
			flag = true;
			damageInfo = null;
		}
		else if (m_ownerStats == null || SecondaryDefense == CharacterStats.DefenseType.None)
		{
			flag = true;
			damageInfo = new DamageInfo();
			damageInfo.Attack = this;
			damageInfo.HitType = hitInfo.HitType;
		}
		else
		{
			damageInfo = m_ownerStats.ComputeSecondaryAttack(this, enemy, SecondaryDefense);
		}
		if (damageInfo == null || !damageInfo.IsMiss)
		{
			int num = 0;
			int num2 = Afflictions.Count - 1;
			if (ApplyOneRandomAffliction)
			{
				num = OEIRandom.Index(Afflictions.Count);
				num2 = num;
			}
			for (int i = num; i <= num2; i++)
			{
				if (flag)
				{
					ApplyAffliction(Afflictions[i], enemy, damageInfo, deleteOnClear, appliedEffects);
				}
				else
				{
					ApplyAffliction(Afflictions[i], enemy, damageInfo, deleteOnClear, appliedSecondaryEffects);
				}
			}
		}
		if (flag)
		{
			return null;
		}
		return damageInfo;
	}

	protected virtual void ApplyAffliction(AfflictionParams ap, GameObject enemy, DamageInfo hitInfo, bool deleteOnClear, List<StatusEffect> appliedEffects)
	{
		if (!ap.AfflictionPrefab || ap.AfflictionPrefab.StatusEffects == null)
		{
			return;
		}
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if (!component)
		{
			return;
		}
		if (!component.CanApplyAffliction(ap.AfflictionPrefab))
		{
			UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2188), enemy);
			if (PartyHelper.IsPartyMember(Owner))
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(Owner, SoundSet.SoundAction.TargetImmune, SoundSet.s_LongVODelay, forceInterrupt: false);
			}
			return;
		}
		if (ap.AfflictionPrefab.Exclusive)
		{
			component.ClearEffectFromAffliction(ap.AfflictionPrefab);
		}
		if (ap.AfflictionPrefab.Overrides != null)
		{
			Affliction[] overrides = ap.AfflictionPrefab.Overrides;
			foreach (Affliction aff in overrides)
			{
				component.SuppressEffectFromAffliction(aff);
			}
		}
		StatusEffectParams[] statusEffects = ap.AfflictionPrefab.StatusEffects;
		foreach (StatusEffectParams param in statusEffects)
		{
			float duration = ap.Duration;
			if (m_durationOverride > 0f)
			{
				duration = m_durationOverride;
			}
			StatusEffect statusEffect = CreateChildEffect(param, duration, hitInfo, deleteOnClear);
			statusEffect.AfflictionOrigin = ap.AfflictionPrefab;
			statusEffect.AfflictionKeyword = ap.Keyword;
			if (component.ApplyStatusEffectImmediate(statusEffect))
			{
				appliedEffects?.Add(statusEffect);
			}
		}
		if ((bool)m_ownerStats)
		{
			m_ownerStats.NotifyCausedAffliction(enemy, ap.AfflictionPrefab);
		}
		if (!MaterialReplacement.IsNullOrEmpty(ap.AfflictionPrefab.Material))
		{
			ap.AfflictionPrefab.Material.Replace(enemy);
		}
		if (ap.AfflictionPrefab.DisengageAll)
		{
			AIController component2 = enemy.GetComponent<AIController>();
			if (component2 != null)
			{
				component2.CancelAllEngagements();
			}
		}
	}

	public static void PostAttackMessages(GameObject enemy, GameObject attacker, DamageInfo info, IEnumerable<StatusEffect> appliedEffects, bool primaryAttack)
	{
		string missFormatString = Console.Format(GUIUtils.GetTextWithLinks(822), "{0}", "{2}");
		if ((bool)info.Attack && info.Attack.IsDisengagementAttack)
		{
			PostAttackMessages(enemy, attacker, info, appliedEffects, primaryAttack, GUIUtils.GetTextWithLinks(53), missFormatString);
		}
		else
		{
			PostAttackMessages(enemy, attacker, info, appliedEffects, primaryAttack, GUIUtils.GetTextWithLinks(52), missFormatString);
		}
	}

	public static void PostAttackMessages(GameObject enemy, DamageInfo info, IEnumerable<StatusEffect> appliedEffects, bool primaryAttack)
	{
		PostAttackMessages(enemy, info.Owner, info, appliedEffects, primaryAttack);
	}

	public static void PostAttackMessagesSecondaryEffect(GameObject enemy, DamageInfo info, IEnumerable<StatusEffect> appliedEffects)
	{
		PostAttackMessages(enemy, info.Owner, info, appliedEffects, primaryAttack: false, GUIUtils.GetTextWithLinks(1323), GUIUtils.GetTextWithLinks(1322));
	}

	public static void PostAttackMessages(GameObject enemy, GameObject attacker, DamageInfo info, IEnumerable<StatusEffect> appliedEffects, bool primaryAttack, string simpleFormatString, string missFormatString)
	{
		if (info.TargetPreviouslyDead)
		{
			return;
		}
		try
		{
			Faction component = enemy.GetComponent<Faction>();
			Faction component2 = attacker.GetComponent<Faction>();
			if (info.PostponedDisplayEffects != null)
			{
				appliedEffects = ((appliedEffects == null) ? info.PostponedDisplayEffects : appliedEffects.Concat(info.PostponedDisplayEffects));
			}
			string text = CharacterStats.NameColored(attacker);
			string text2 = text;
			string text3 = CharacterStats.NameColored(enemy);
			GenericAbility genericAbility = (info.Attack ? (info.Attack.TriggeringAbility ?? info.Attack.AbilityOrigin ?? info.Attack.GetComponent<GenericAbility>()) : info.Ability);
			string text4 = "";
			if ((bool)genericAbility)
			{
				text4 = GenericAbility.Name(genericAbility);
				text += GUIUtils.Format(1731, text4);
			}
			Color messageColor = GetMessageColor(component2, component);
			if (info.IsMiss)
			{
				StringBuilder stringBuilder = new StringBuilder();
				info.SimpleMissReport(missFormatString, text, text3, stringBuilder);
				StringBuilder stringBuilder2 = new StringBuilder();
				info.GetToHitReport(attacker, enemy, stringBuilder2);
				if (s_PostUseDelta)
				{
					Console.InsertBatchedMessage(stringBuilder.ToString(), stringBuilder2.ToString(), Color.white, Console.Instance.MessageDelta, info);
				}
				else
				{
					Console.AddBatchedMessage(stringBuilder.ToString(), stringBuilder2.ToString(), Color.white, info);
				}
				return;
			}
			bool flag = info.DefendedBy != CharacterStats.DefenseType.None || info.MinDamage > 0f || (appliedEffects?.Any((StatusEffect e) => e.Params.IsHostile) ?? false);
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			if (info.DefendedBy != CharacterStats.DefenseType.None)
			{
				info.GetToHitReport(attacker, enemy, stringBuilder3);
			}
			StringBuilder stringBuilder5 = new StringBuilder(stringBuilder3.ToString());
			if (info.DamageAmount != 0f)
			{
				stringBuilder5.Append(" ");
				info.GetDamageReport(enemy, stringBuilder5);
			}
			string text5 = "*EffectError*";
			if ((bool)genericAbility || (bool)info.Ability)
			{
				text5 = GenericAbility.Name(genericAbility ? genericAbility : info.Ability);
			}
			else if ((bool)info.Attack)
			{
				if ((bool)info.Attack.m_equippable)
				{
					text5 = info.Attack.m_equippable.Name;
				}
				else if ((bool)info.Attack.ParentAttack)
				{
					if ((bool)info.Attack.ParentAttack.m_ability)
					{
						text5 = info.Attack.ParentAttack.m_ability.Name();
					}
					else if ((bool)info.Attack.ParentAttack.m_equippable)
					{
						text5 = info.Attack.ParentAttack.m_equippable.Name;
					}
				}
			}
			float a = 0f;
			if (appliedEffects != null && appliedEffects.Any())
			{
				List<StatusEffect> list = new List<StatusEffect>();
				list.AddRange(appliedEffects);
				StatusEffectParams.CleanUp(list);
				bool flag2 = true;
				if (list.Count > 0)
				{
					stringBuilder5.Append(" ");
					stringBuilder5.Append(StatusEffectParams.ListToString(list.Where((StatusEffect e) => e.Applied), attacker ? attacker.GetComponent<CharacterStats>() : null, genericAbility, null, null, StatusEffectFormatMode.CombatLog, TargetType.All));
				}
				while (list.Count > 0)
				{
					float longestDuration = 0f;
					List<StatusEffect> list2 = StatusEffect.BundleEffects(list, out longestDuration);
					a = Mathf.Max(a, longestDuration);
					if (list2 == null || list2.Count == 0)
					{
						break;
					}
					string text6 = ((!string.IsNullOrEmpty(list2[0].BundleName)) ? list2[0].BundleName : (list2[0].AfflictionOrigin ? list2[0].AfflictionOrigin.ToString() : list2[0].GetDisplayName()));
					if (!flag2)
					{
						stringBuilder4.Append(GUIUtils.Comma());
					}
					if (text6 == text4)
					{
						text6 = GUIUtils.GetText(1213);
					}
					if (longestDuration >= 1f)
					{
						stringBuilder4.AppendGuiFormat(1665, text6, GUIUtils.Format(211, longestDuration.ToString("#0.0")));
					}
					else
					{
						stringBuilder4.Append(text6);
					}
					flag2 = false;
				}
			}
			StringBuilder stringBuilder6 = new StringBuilder();
			if (!flag)
			{
				if (string.IsNullOrEmpty(stringBuilder5.ToString().Trim()))
				{
					return;
				}
				stringBuilder6.AppendGuiFormat(824, text2, text3, text5);
			}
			else if ((info.DamageAmount != 0f || !primaryAttack) && info.MinDamage > 0f)
			{
				info.SimpleAttackReport(simpleFormatString, text, text3, info.GetExtraAttackEffects(), stringBuilder6);
			}
			else
			{
				if (info.MinDamage == 0f && string.IsNullOrEmpty(stringBuilder5.ToString().Trim()))
				{
					return;
				}
				if (info.DefendedBy != CharacterStats.DefenseType.None)
				{
					info.SimpleAttackReportNoDamage(GUIUtils.GetTextWithLinks(1324), text, text3, stringBuilder6);
				}
				else
				{
					stringBuilder6.AppendGuiFormat(824, text2, text3, text5);
				}
			}
			string text7 = "";
			if (stringBuilder4.Length > 0)
			{
				text7 = GUIUtils.Format(1731, stringBuilder4.ToString());
			}
			if (s_PostUseDelta)
			{
				Console.InsertBatchedMessage(stringBuilder6.ToString() + text7, stringBuilder5.ToString(), messageColor, Console.Instance.MessageDelta, info);
			}
			else
			{
				Console.AddBatchedMessage(stringBuilder6.ToString() + text7, stringBuilder5.ToString(), messageColor, info);
			}
		}
		catch (Exception ex)
		{
			Console.AddMessage("PostAttackMessages Error: " + ex.Message, Color.red);
			Debug.LogException(ex);
		}
	}

	public static Color GetMessageColor(GameObject attacker, GameObject defender)
	{
		Faction attacker2 = (attacker ? attacker.GetComponent<Faction>() : null);
		Faction defender2 = (defender ? defender.GetComponent<Faction>() : null);
		return GetMessageColor(attacker2, defender2);
	}

	public static Color GetMessageColor(Faction attacker, Faction defender)
	{
		Color result = new Color(1f, 4f / 15f, 4f / 15f);
		Color result2 = InGameHUD.GetFriendlyColor().Max(40f / 51f);
		if (defender != null && attacker != null && defender.IsInPlayerFaction)
		{
			if (!attacker.IsInPlayerFaction)
			{
				return result;
			}
			return result2;
		}
		return result2;
	}

	protected void CalcDamage(GameObject enemy, GameObject self, out DamageInfo damage)
	{
		GameObject owner = Owner;
		CharacterStats component = owner.GetComponent<CharacterStats>();
		PartyMemberAI component2 = owner.GetComponent<PartyMemberAI>();
		if ((bool)component2)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_ATTACK_ROLL);
		}
		damage = new DamageInfo(enemy, 0f, this);
		damage.Self = self;
		damage.AttackIsHostile = IsHostile(enemy, damage);
		if (!damage.AttackIsHostile)
		{
			damage.DefendedBy = CharacterStats.DefenseType.None;
		}
		Health health = (enemy ? enemy.GetComponent<Health>() : null);
		damage.TargetPreviouslyDead = (bool)health && (health.Dead || health.Unconscious);
		Weapon component3 = base.gameObject.GetComponent<Weapon>();
		if (component3 != null)
		{
			damage.Damage.Minimum *= component.WeaponDamageMinMult;
			if (component3.DurabilityState == Equippable.DurabilityStateType.Damaged)
			{
				damage.Damage.Maximum *= 0.75f;
			}
		}
		float minPercentAdjust = 0f;
		if (this is AttackMelee)
		{
			minPercentAdjust = component.MeleeDamageRangePctIncreaseToMin;
		}
		StatusEffect statusEffect = component.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.DamageAlwaysMinimumAgainstCCD);
		if (statusEffect != null)
		{
			CharacterStats component4 = enemy.GetComponent<CharacterStats>();
			if (component4.HasStatusEffectOfType(StatusEffect.ModifiedStat.SwapFaction) || component4.HasStatusEffectOfType(StatusEffect.ModifiedStat.Confused))
			{
				Faction component5 = enemy.GetComponent<Faction>();
				Faction component6 = Owner.GetComponent<Faction>();
				if ((bool)component5 && (bool)component6 && component6.CurrentTeam.GetRelationship(component5.OriginalTeamInstance) == Faction.Relationship.Friendly)
				{
					damage.Damage.Maximum = damage.Damage.Minimum;
					damage.DamageMult(statusEffect.ParamsValue());
				}
			}
		}
		damage.DamageBase = damage.Damage.RollDamage(component, minPercentAdjust);
		damage.DamageMult(DamageMultiplier);
		damage.DamageMult(BounceFactor(self));
		if (damage.Ability != null && damage.Ability.EffectType == GenericAbility.AbilityType.Trap)
		{
			PartyMemberAI partyMemberAI = (enemy ? enemy.GetComponent<PartyMemberAI>() : null);
			if ((bool)partyMemberAI && partyMemberAI.enabled)
			{
				if (GameState.Instance != null)
				{
					switch (GameState.Instance.Difficulty)
					{
					case GameDifficulty.Easy:
					case GameDifficulty.StoryTime:
						damage.DamageBase *= 0.5f;
						break;
					case GameDifficulty.Normal:
						damage.DamageBase *= 0.75f;
						break;
					}
				}
			}
			else if (damage.Ability.Owner != null)
			{
				Trap component7 = damage.Ability.Owner.GetComponent<Trap>();
				if (component7 == null || !component7.IsPlayerOwnedTrap)
				{
					damage.DamageBase *= 0.25f;
				}
			}
		}
		component.OnAttackRollCalculated += NotifyAttackRollCalculated;
		component.AdjustDamageDealt(enemy, damage, testing: false);
		component.OnAttackRollCalculated -= NotifyAttackRollCalculated;
		if (this.OnHit != null)
		{
			this.OnHit(owner, new CombatEventArgs(damage, owner, enemy));
		}
		if (damage.IsCriticalHit && (bool)component2)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_GETS_CRIT);
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(component2.gameObject, SoundSet.SoundAction.CriticalHit, SoundSet.s_MediumVODelay, forceInterrupt: false);
		}
	}

	private void NotifyAttackRollCalculated(GameObject sender, CombatEventArgs e)
	{
		if (this.OnAttackRollCalculated != null)
		{
			this.OnAttackRollCalculated(sender, e);
		}
	}

	private bool IsHostile(GameObject enemy, DamageInfo damage)
	{
		return IsHostile(enemy, damage?.Damage);
	}

	public virtual bool IsHostile(GameObject enemy, DamagePacket damage)
	{
		if (damage != null && damage.Type != DamagePacket.DamageType.None && damage.DoesDamage)
		{
			return true;
		}
		if (PushDistance != 0f)
		{
			return true;
		}
		if (Afflictions.Count > 0)
		{
			return true;
		}
		if (StatusEffects.Count > 0)
		{
			foreach (StatusEffectParams statusEffect in StatusEffects)
			{
				if (statusEffect.IsHostile && statusEffect.CanApply(Owner, enemy, enemy))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual Transform GetLaunchTransform()
	{
		GameObject owner = Owner;
		if (owner == null)
		{
			return base.transform;
		}
		if (OnStartAttach == EffectAttachType.Root)
		{
			return owner.transform;
		}
		Transform transform = null;
		if ((bool)m_mapper)
		{
			if (AbilityOrigin == null)
			{
				transform = m_mapper[base.gameObject, OnStartAttach];
				if (transform == owner.transform)
				{
					transform = m_mapper[owner, OnStartAttach];
				}
			}
			else
			{
				transform = m_mapper[owner, OnStartAttach];
			}
		}
		if (transform == null)
		{
			switch (OnStartAttach)
			{
			case EffectAttachType.RightHand:
			case EffectAttachType.BothHands:
				transform = m_ownerTransformRightHand;
				break;
			case EffectAttachType.LeftHand:
				transform = m_ownerTransformLeftHand;
				break;
			case EffectAttachType.Mouth:
				transform = m_ownerTransformMouth;
				break;
			case EffectAttachType.Tail:
				transform = m_ownerTransformTail;
				break;
			}
			if (transform == null)
			{
				transform = owner.transform;
			}
		}
		if (transform != null && (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z)))
		{
			Debug.LogError(base.gameObject.name + " doesn't have a valid LaunchTransform. Reverting to the owner's transform.");
			transform = owner.transform;
		}
		return transform;
	}

	public virtual Transform GetHitTransform(GameObject target)
	{
		return GetTransform(target, OnHitAttach);
	}

	public static Transform GetTransform(GameObject target, EffectAttachType attachType)
	{
		Transform transform = null;
		if (attachType == EffectAttachType.Root)
		{
			return target.transform;
		}
		AnimationBoneMapper component = target.GetComponent<AnimationBoneMapper>();
		if (component != null)
		{
			transform = component[target, attachType];
		}
		if (transform == null)
		{
			switch (attachType)
			{
			case EffectAttachType.RightHand:
			case EffectAttachType.BothHands:
				transform = GetRightHandTransform(target);
				break;
			case EffectAttachType.LeftHand:
				transform = GetLeftHandTransform(target);
				break;
			case EffectAttachType.Mouth:
				transform = GetMouthTransform(target, required: true);
				break;
			case EffectAttachType.Tail:
				transform = GetTailTransform(target, required: true);
				break;
			}
			if (transform == null)
			{
				transform = target.transform;
			}
		}
		return transform;
	}

	public static Transform GetRightHandTransform(GameObject obj)
	{
		AnimationController component = obj.GetComponent<AnimationController>();
		if (component == null)
		{
			return obj.transform;
		}
		return component.GetBoneTransform("primaryWeapon", obj.transform);
	}

	public static Transform GetLeftHandTransform(GameObject obj)
	{
		AnimationController component = obj.GetComponent<AnimationController>();
		if (component == null)
		{
			return obj.transform;
		}
		return component.GetBoneTransform("secondaryWeapon", obj.transform);
	}

	public static Transform GetMouthTransform(GameObject obj, bool required)
	{
		AnimationController component = obj.GetComponent<AnimationController>();
		if (component == null)
		{
			return obj.transform;
		}
		Transform boneTransform = component.GetBoneTransform("w_mouth", obj.transform);
		if (boneTransform != null)
		{
			return boneTransform;
		}
		if (required)
		{
			Debug.LogError(obj.name + " doesn't have a mouth bone (w_mouth). Check " + obj);
		}
		return GetRightHandTransform(obj);
	}

	public static Transform GetTailTransform(GameObject obj, bool required)
	{
		AnimationController component = obj.GetComponent<AnimationController>();
		if (component == null)
		{
			return obj.transform;
		}
		Transform boneTransform = component.GetBoneTransform("w_tail", obj.transform);
		if (boneTransform != null)
		{
			return boneTransform;
		}
		if (required)
		{
			Debug.LogError(obj.name + " doesn't have a tail bone (w_tail). Check " + obj);
		}
		return GetRightHandTransform(obj);
	}

	public virtual bool IsAutoAttack()
	{
		if (!GetComponent<Equippable>())
		{
			return TreatAsWeaponAttack;
		}
		return true;
	}

	public virtual bool IsReady()
	{
		if (IsDisengagementAttack)
		{
			return true;
		}
		if (IsBouncing)
		{
			return false;
		}
		if (m_ownerStats != null && m_ownerStats.RecoveryTimer > 0f)
		{
			return false;
		}
		return true;
	}

	public virtual bool IgnoreValidTargetCheck()
	{
		return false;
	}

	public bool IsValidTarget(GameObject target)
	{
		return IsValidTarget(target, ValidTargets);
	}

	public bool IsValidPrimaryTarget(GameObject target)
	{
		return IsValidTarget(target, (ValidPrimaryTargets != TargetType.None) ? ValidPrimaryTargets : ValidTargets);
	}

	public bool IsValidTarget(GameObject target, GameObject caster)
	{
		return IsValidTarget(target, caster, ValidTargets);
	}

	public bool IsValidTarget(GameObject target, TargetType validType)
	{
		return IsValidTarget(target, null, validType);
	}

	public bool IsValidTarget(GameObject target, GameObject caster, TargetType validType)
	{
		if (IgnoreValidTargetCheck())
		{
			return true;
		}
		if (target == null)
		{
			return false;
		}
		if (caster == null)
		{
			caster = Owner;
		}
		Health component = target.GetComponent<Health>();
		if (component == null || !component.CanBeTargeted || !target.activeInHierarchy)
		{
			return false;
		}
		if (component.Dead)
		{
			if (!TargetTypeUtils.ValidTargetDead(validType) && UnvalidatedForcedTarget != target)
			{
				return false;
			}
		}
		else if (component.Unconscious && !TargetTypeUtils.ValidTargetUnconscious(validType) && UnvalidatedForcedTarget != target)
		{
			return false;
		}
		CharacterStats component2 = target.GetComponent<CharacterStats>();
		if (ApplyOnceOnly && m_ability != null && component2 != null && component2.CountStatusEffects(m_ability) > 0)
		{
			return false;
		}
		Faction component3 = ComponentUtils.GetComponent<Faction>(caster);
		Faction component4 = ComponentUtils.GetComponent<Faction>(target);
		AIController aIController = GameUtilities.FindActiveAIController(target);
		if (aIController != null && aIController.IsBusy && !component4.IsHostile(component3))
		{
			return false;
		}
		switch (validType)
		{
		case TargetType.All:
			return true;
		case TargetType.Hostile:
			if ((component3 != null && component3.IsHostile(target)) || (component4 != null && component4.IsHostile(caster)))
			{
				return true;
			}
			break;
		case TargetType.Friendly:
			if (component3 != null && !component3.IsHostile(target) && (component4 == null || !component4.IsHostile(caster)))
			{
				return true;
			}
			break;
		case TargetType.FriendlyUnconscious:
			if (component3 != null && component.Unconscious && !component.Dead && !component3.IsHostile(target) && (component4 == null || !component4.IsHostile(caster)))
			{
				return true;
			}
			break;
		case TargetType.AllDeadOrUnconscious:
			if (component.Unconscious || component.Dead)
			{
				return true;
			}
			break;
		case TargetType.HostileWithGrimoire:
			if ((component3 != null && component3.IsHostile(target)) || (component4 != null && component4.IsHostile(caster)))
			{
				Equipment component9 = target.GetComponent<Equipment>();
				if (component9 != null && component9.CurrentItems != null && component9.CurrentItems.Grimoire != null)
				{
					return true;
				}
			}
			break;
		case TargetType.HostileVessel:
			if (((component3 != null && component3.IsHostile(target)) || (component4 != null && component4.IsHostile(caster))) && component2 != null && component2.CharacterRace == CharacterStats.Race.Vessel)
			{
				return true;
			}
			break;
		case TargetType.HostileBeast:
			if (((component3 != null && component3.IsHostile(target)) || (component4 != null && component4.IsHostile(caster))) && component2 != null && component2.CharacterRace == CharacterStats.Race.Beast)
			{
				return true;
			}
			break;
		case TargetType.Dead:
			if (component.Dead)
			{
				return true;
			}
			break;
		case TargetType.Ally:
		{
			PartyMemberAI component7 = caster.GetComponent<PartyMemberAI>();
			PartyMemberAI component8 = target.GetComponent<PartyMemberAI>();
			if (component7 != null && component7.isActiveAndEnabled)
			{
				if (component8 != null && component8.isActiveAndEnabled)
				{
					return true;
				}
			}
			else if (component8 == null || !component8.isActiveAndEnabled)
			{
				return true;
			}
			break;
		}
		case TargetType.AllyNotSelf:
			if (target != caster && IsValidTarget(target, caster, TargetType.Ally))
			{
				return true;
			}
			break;
		case TargetType.AllyNotSelfOrHostile:
			if (IsValidTarget(target, caster, TargetType.Hostile) || IsValidTarget(target, caster, TargetType.AllyNotSelf))
			{
				return true;
			}
			break;
		case TargetType.NotSelf:
			if (target != caster)
			{
				return true;
			}
			break;
		case TargetType.DragonOrDrake:
			if (component2 != null && CharacterStats.ClassIsDragonOrDrake(component2.CharacterClass))
			{
				return true;
			}
			break;
		case TargetType.FriendlyIncludingCharmed:
			if (component3 != null && !component3.IsHostile(target))
			{
				return true;
			}
			if ((bool)component3 && (bool)aIController)
			{
				Team originalTeam = aIController.GetOriginalTeam();
				if (originalTeam != null && originalTeam.GetRelationship(component3.CurrentTeam) != Faction.Relationship.Hostile)
				{
					return true;
				}
			}
			break;
		case TargetType.Self:
			if (target == caster)
			{
				return true;
			}
			break;
		case TargetType.FriendlyNotVessel:
			if (component2 != null && component2.CharacterRace != CharacterStats.Race.Vessel && component3 != null && !component3.IsHostile(target) && (component4 == null || !component4.IsHostile(caster)))
			{
				return true;
			}
			break;
		case TargetType.SpiritOrSummonedCreature:
			if (((bool)component2 && component2.CharacterRace == CharacterStats.Race.Spirit) || ((bool)aIController && aIController.SummonType == AIController.AISummonType.Summoned))
			{
				return true;
			}
			break;
		case TargetType.OwnAnimalCompanion:
			if (aIController.SummonType == AIController.AISummonType.AnimalCompanion && aIController.Summoner == caster)
			{
				return true;
			}
			break;
		case TargetType.HostileWithNpcAppearance:
		{
			NPCAppearance component6 = target.GetComponent<NPCAppearance>();
			if ((bool)component6 && component6.isActiveAndEnabled && IsValidTarget(target, caster, TargetType.Hostile))
			{
				return true;
			}
			break;
		}
		case TargetType.OwnerOfPairedAbility:
		{
			PairedAbility component5 = GetComponent<PairedAbility>();
			if (!component5)
			{
				UIDebug.Instance.LogOnScreenWarning("Ability '" + base.name + "' must have a PairedAbility component to use OwnerOfPairedAbility target type.", UIDebug.Department.Design, 10f);
				return false;
			}
			if ((bool)component5.OtherAbility)
			{
				return target == component5.OtherAbility.Owner;
			}
			return false;
		}
		case TargetType.AnyWithResonance:
			return component2.CountStatusEffects("resonance", caster) > 0;
		}
		return false;
	}

	public virtual bool IsCharacterImmuneToAnyAffliction(CharacterStats character)
	{
		if (!character)
		{
			return false;
		}
		if (Afflictions != null)
		{
			for (int i = 0; i < Afflictions.Count; i++)
			{
				if (character.IsImmuneToAffliction(Afflictions[i].AfflictionPrefab))
				{
					return true;
				}
			}
		}
		if ((bool)ExtraAOE && ExtraAOE.IsCharacterImmuneToAnyAffliction(character))
		{
			return true;
		}
		return false;
	}

	public bool HasStatusEffect(StatusEffect.ModifiedStat effectStat)
	{
		for (int i = 0; i < StatusEffects.Count; i++)
		{
			if (StatusEffects[i].AffectsStat == effectStat)
			{
				return true;
			}
		}
		for (int j = 0; j < Afflictions.Count; j++)
		{
			AfflictionParams afflictionParams = Afflictions[j];
			if (afflictionParams == null || !afflictionParams.AfflictionPrefab || afflictionParams.AfflictionPrefab.StatusEffects == null)
			{
				continue;
			}
			StatusEffectParams[] statusEffects = afflictionParams.AfflictionPrefab.StatusEffects;
			for (int k = 0; k < statusEffects.Length; k++)
			{
				if (statusEffects[k].AffectsStat == effectStat)
				{
					return true;
				}
			}
		}
		if (m_ability != null && m_ability.HasAbilityModAttackStatusEffect(effectStat))
		{
			return true;
		}
		return false;
	}

	public bool HasAffliction(Affliction aff)
	{
		if (Afflictions.Count > 0)
		{
			foreach (AfflictionParams affliction in Afflictions)
			{
				if (affliction.AfflictionPrefab == aff)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasKeyword(string keyword)
	{
		if (m_keywords == null)
		{
			m_keywords = new KeywordCollection(Keywords);
		}
		return m_keywords.Contains(keyword);
	}

	public string GetKeywordsString()
	{
		if (m_keywords == null)
		{
			m_keywords = new KeywordCollection(Keywords);
		}
		return m_keywords.GetListString();
	}

	public bool HasAfflictionWithKeyword(string keyword)
	{
		if (Afflictions.Count > 0)
		{
			for (int i = 0; i < Afflictions.Count; i++)
			{
				if (Afflictions[i] != null && Afflictions[i].Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Cancel()
	{
		if (m_can_cancel)
		{
			m_cancelled = true;
			Interrupt();
		}
		StopAllCoroutines();
	}

	public void NotifyAttackComplete(float totalTime)
	{
		AnimLength = totalTime;
		ClearAnimEventLists(warn_if_not_empty: true);
		if (ZeroAttackRecovery)
		{
			m_ownerStats.RecoveryTimer = 0f;
			m_prevRecoveryTime = 0f;
			return;
		}
		GameObject gameObject = Owner;
		if (m_ownerStats == null && gameObject != null)
		{
			GameObject obj = gameObject;
			gameObject = null;
			gameObject = obj;
		}
		if (m_ownerStats == null)
		{
			return;
		}
		float num = 0f;
		num += m_ownerStats.GetArmorSpeedFactor() - 1f;
		num += m_statAttackSpeedAfterLaunch - 1f;
		if (this is AttackRanged)
		{
			num += m_ownerStats.RateOfFireMultiplier - 1f;
		}
		if (m_ability == null && gameObject != null)
		{
			Equipment component = gameObject.GetComponent<Equipment>();
			if (component != null)
			{
				if (component.DualWielding && component.EquippedShield == null)
				{
					num += m_ownerStats.DualWieldAttackSpeedMultiplier - 1f;
				}
				else
				{
					float num2 = AttackData.Instance.Single1HWeapRecovFactor;
					if ((bool)m_ownerStats)
					{
						num2 += m_ownerStats.SingleWeaponSpeedFactorAdj;
					}
					if (num2 > 1f)
					{
						num2 = 1f;
					}
					num += num2 - 1f;
				}
				if (this is AttackMelee)
				{
					num += m_ownerStats.MeleeAttackSpeedMultiplier - 1f;
				}
				if (this is AttackRanged)
				{
					num += m_ownerStats.RangedAttackSpeedMultiplier - 1f;
				}
			}
		}
		if (totalTime > float.Epsilon)
		{
			float num3 = totalTime;
			num *= 2f;
			num3 -= num * num3;
			if (AttackData.Instance.RecoveryFactor > 0f)
			{
				num3 /= AttackData.Instance.RecoveryFactor;
			}
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			if (m_ownerStats.NegateNextRecovery)
			{
				m_ownerStats.NegateNextRecovery = false;
				num3 = 0f;
			}
			num3 *= m_ownerStats.DifficultyRecoveryTimeMult;
			m_ownerStats.RecoveryTimer = num3;
			m_prevRecoveryTime = num3;
		}
	}

	public float BounceFactor(GameObject self)
	{
		return Mathf.Pow(BounceMultiplier, GetBounceCount(self));
	}

	public int GetBounceCount(GameObject self)
	{
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				return component.GetBounceCount();
			}
		}
		return m_bounceCount;
	}

	public int IncrementBounceCount(GameObject self)
	{
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				return component.IncrementBounceCount();
			}
		}
		m_bounceCount++;
		return m_bounceCount;
	}

	public void ResetBounceCount(GameObject self)
	{
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				component.ResetBounceCount();
				return;
			}
		}
		m_bounceCount = 0;
	}

	public float GetProjectileDistanceTraveled(GameObject self)
	{
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				return component.TotalDistanceTraveled;
			}
		}
		return 0f;
	}

	private void HandleAnimShowSlot(object obj, EventArgs args)
	{
		if (m_eventsHideSlot != null && obj != null && m_eventsHideSlot.Contains(obj))
		{
			m_eventsHideSlot.Remove(obj);
		}
	}

	private void HandleAnimHideSlot(object obj, EventArgs args)
	{
		if (m_eventsHideSlot != null && obj != null && m_eventsHideSlot.Contains(obj))
		{
			Debug.LogWarning("Anim Event Warning on attack variation " + AttackVariation + "! More than one anim event Hide Slot for slot: " + obj.ToString());
		}
	}

	private void HandleAnimMoveToHand(object obj, EventArgs args)
	{
		if (m_eventsMoveToHand != null && m_eventsMoveToHand.Contains(obj))
		{
			Debug.LogWarning("Anim Event Warning on attack variation " + AttackVariation + "! More than one anim event Move to Hand for slot: " + obj.ToString());
		}
	}

	private void HandleAnimMoveFromHand(object obj, EventArgs args)
	{
		if (m_eventsMoveToHand != null && m_eventsMoveToHand.Contains(obj))
		{
			m_eventsMoveToHand.Remove(obj);
		}
	}

	private void ClearAnimEventLists(bool warn_if_not_empty)
	{
		GameObject owner = Owner;
		Equipment component = owner.GetComponent<Equipment>();
		if (component != null)
		{
			if (m_eventsHideSlot.Count > 0)
			{
				if (warn_if_not_empty)
				{
					Debug.Log("Anim Event Warning on attack variation " + AttackVariation + "! Missing Show Slot events!");
				}
				foreach (object item in m_eventsHideSlot)
				{
					component.HandleAnimShowSlot(item, EventArgs.Empty);
				}
			}
			if (m_eventsMoveToHand.Count > 0)
			{
				if (warn_if_not_empty)
				{
					Debug.Log("Anim Event Warning on attack variation " + AttackVariation + "! Missing Move From Hand events!");
				}
				foreach (object item2 in m_eventsMoveToHand)
				{
					component.HandleAnimMoveFromHand(item2, EventArgs.Empty);
				}
			}
			m_eventsHideSlot.Clear();
			m_eventsMoveToHand.Clear();
			component.HandleAnimShowSlot("PrimaryWeapon", EventArgs.Empty);
			component.HandleAnimShowSlot("SecondaryWeapon", EventArgs.Empty);
			Equippable itemInSlot = component.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Grimoire);
			if ((bool)itemInSlot)
			{
				AnimationController component2 = itemInSlot.GetComponent<AnimationController>();
				if (component2 != null)
				{
					component2.DesiredAction.Reset();
					component2.ClearActions();
					component2.Loop = false;
					itemInSlot.IsAnimating = false;
				}
			}
		}
		AnimationController component3 = owner.GetComponent<AnimationController>();
		if (component3 != null)
		{
			component3.OnEventShowSlot -= HandleAnimShowSlot;
			component3.OnEventHideSlot -= HandleAnimHideSlot;
			component3.OnEventMoveToHand -= HandleAnimMoveToHand;
			component3.OnEventMoveFromHand -= HandleAnimMoveFromHand;
		}
	}

	public void ValidateOwnerStats(GameObject owner)
	{
		if (m_parent == null)
		{
			Owner = owner;
		}
	}

	public int GetAccuracyBonus()
	{
		if ((bool)m_ownerStats)
		{
			return LevelScaling.AdjustAccuracy(AccuracyBonus, m_ownerStats.ScaledLevel);
		}
		return AccuracyBonus;
	}

	public void ResetLaunchTransforms()
	{
		m_ownerLaunchTransform = null;
	}

	public static void AddStringEffect(string target, AttackEffect effect, StringEffects stringEffects)
	{
		if (!stringEffects.Effects.ContainsKey(target))
		{
			stringEffects[target] = new List<AttackEffect>();
		}
		stringEffects[target].Add(effect);
	}

	protected void AddStringEffect(string target, string effect, bool hostile, StringEffects stringEffects)
	{
		AddStringEffect(target, new AttackEffect(effect, this, hostile), stringEffects);
	}

	public string GetString(GenericAbility ability, GameObject character, StringEffects stringEffects, IEnumerable<StatusEffectParams> statusEffects = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AttackRanged attackRanged = this as AttackRanged;
		if (!PrefabForTrap)
		{
			UIAttackSpeedType uIAttackSpeedType = UiAttackSpeed;
			if (uIAttackSpeedType == UIAttackSpeedType.Undefined)
			{
				uIAttackSpeedType = ((AttackSpeed == AttackSpeedType.Instant) ? UIAttackSpeedType.Fast : ((AttackSpeed != AttackSpeedType.Short) ? UIAttackSpeedType.Slow : UIAttackSpeedType.Average));
			}
			stringBuilder.AppendLine(FormatWC(GUIUtils.GetText(443), GUIUtils.GetAttackSpeedTypeString(uIAttackSpeedType)));
		}
		string text = GetRangeString(ability, character);
		if (Bounces > 0)
		{
			string text2 = "";
			if ((bool)attackRanged && attackRanged.MultiHitRay)
			{
				if (attackRanged.BounceMultiplier != 1f)
				{
					stringBuilder.AppendLine(GUIUtils.Format(2407, TextUtils.MultiplierAsPercentBonus(attackRanged.BounceMultiplier)));
				}
			}
			else
			{
				text2 = GUIUtils.Format(1626, GUIUtils.Format(1533, BounceRange.ToString("#0.##")));
			}
			if (!string.IsNullOrEmpty(text2))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " + ";
				}
				text += text2;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.AppendLine(FormatWC(GUIUtils.GetText(444), text));
		}
		string text3 = GetAoeString(ability, character);
		if ((bool)ExtraAOE)
		{
			ExtraAOE.Owner = Owner;
			string aoeString = ExtraAOE.GetAoeString(ability, character);
			if (!string.IsNullOrEmpty(aoeString))
			{
				if (!string.IsNullOrEmpty(text3))
				{
					text3 += " + ";
				}
				text3 += aoeString;
			}
		}
		if ((bool)attackRanged && attackRanged.MultiHitRay && Bounces > 0)
		{
			if (!string.IsNullOrEmpty(text3))
			{
				text3 += GUIUtils.Comma();
			}
			text3 += GUIUtils.Format(2406, Bounces);
		}
		if (!string.IsNullOrEmpty(text3))
		{
			if (RequiresHitObject)
			{
				string text4 = ((!TargetTypeUtils.ValidTargetAny(ValidTargets)) ? GUIUtils.Format(1600, TargetTypeUtils.GetValidTargetString(ValidTargets)) : GUIUtils.GetText(1595));
				text3 = text4 + " + " + text3;
			}
			stringBuilder.AppendLine(FormatWC(GUIUtils.GetText(1590), text3));
		}
		string durationString = GetDurationString(ability);
		if (!string.IsNullOrEmpty(durationString))
		{
			stringBuilder.AppendLine(durationString);
		}
		if (BaseInterrupt != 0 && IsHostile(null, DamageData))
		{
			float num = AttackData.InterruptDuration(BaseInterrupt);
			string text5 = GUIUtils.Format(211, num.ToString("#0.0#"));
			text5 += GUIUtils.Format(1731, GUIUtils.GetInterruptScaleString(BaseInterrupt));
			stringBuilder.AppendLine(FormatWC(GUIUtils.GetText(1567), text5));
		}
		if (AffectsTarget())
		{
			AddEffects(GetMainTargetString(ability), ability, character, 1f, stringEffects, TargetType.None, statusEffects);
		}
		if (Jumps)
		{
			FormattableTarget formatTarget = new FormattableTarget(GUIUtils.GetText(1613) + GUIUtils.Format(1731, GUIUtils.Format(1278, Bounces)), GUIUtils.GetText(1608) + GUIUtils.Format(1731, GUIUtils.Format(1278, Bounces)));
			AddEffects(formatTarget, ability, character, BounceMultiplier, stringEffects, AlwaysBounceAtEnemies ? TargetType.Hostile : TargetType.None, statusEffects);
		}
		if ((bool)ExtraAOE)
		{
			ExtraAOE.GetString(ability, character, stringEffects, statusEffects);
		}
		GetAdditionalEffects(stringEffects, ability, character);
		return stringBuilder.ToString().TrimEnd();
	}

	protected virtual bool AffectsTarget()
	{
		return true;
	}

	public void AddEffects(FormattableTarget formatTarget, GenericAbility ability, GameObject character, float damageMult, StringEffects stringEffects, TargetType externalTargets = TargetType.None, IEnumerable<StatusEffectParams> statusEffects = null)
	{
		CharacterStats characterStats = null;
		if ((bool)character)
		{
			characterStats = character.GetComponent<CharacterStats>();
		}
		TargetType targetType = ((externalTargets != TargetType.None) ? externalTargets : ValidTargets);
		string text = formatTarget.GetText(targetType);
		string targetQualifiers = GetTargetQualifiers();
		if (!string.IsNullOrEmpty(targetQualifiers))
		{
			text += GUIUtils.Format(1731, targetQualifiers);
		}
		bool flag = DamageData.Type != DamagePacket.DamageType.None && DamageData.DoesDamage;
		if ((bool)ability && (ability.UsePrimaryAttack || ability.UseFullAttack))
		{
			flag = false;
		}
		if (flag)
		{
			AddStringEffect(text, GUIUtils.Format(1329, DamageData.GetString(characterStats, this, damageMult, statusEffects, showBase: true)), hostile: true, stringEffects);
			if (DamageData.DamageProc.Count > 0)
			{
				for (int i = 0; i < DamageData.DamageProc.Count; i++)
				{
					AddStringEffect(text, GUIUtils.Format(1208, GUIUtils.Format(1277, DamageData.DamageProc[i].PercentOfBaseDamage), GUIUtils.GetDamageTypeString(DamageData.DamageProc[i].Type)), hostile: true, stringEffects);
				}
			}
			float dTBypassTotal = DTBypassTotal;
			if (dTBypassTotal != 0f)
			{
				AddStringEffect(text, new AttackEffect(GUIUtils.Format(1183, dTBypassTotal), this), stringEffects);
			}
		}
		int id = 1614;
		float num = PushDistance;
		if (num < 0f)
		{
			num *= -1f;
			id = 1901;
		}
		string effect = GUIUtils.Format(id, GUIUtils.Format(1533, num.ToString("#0.0")));
		if (CleanedUpStatusEffects == null || CleanedUpStatusEffects.Count == 0)
		{
			NotifyStatusEffectsChanged();
		}
		StatusEffectParams.ListToStringEffects(CleanedUpStatusEffects, characterStats, m_ability ? m_ability : ability, this, null, null, StatusEffectFormatMode.InspectWindow, formatTarget, targetQualifiers, targetType, stringEffects);
		foreach (StatusEffectParams cleanedUpStatusEffect in CleanedUpStatusEffects)
		{
			if (StatusEffect.EffectLaunchesAttack(cleanedUpStatusEffect.AffectsStat) && (bool)cleanedUpStatusEffect.AttackPrefab && TargetTypeUtils.LayerTargetTypes(targetType, cleanedUpStatusEffect.AttackPrefab.ValidTargets) != TargetType.None)
			{
				cleanedUpStatusEffect.AttackPrefab.UICleanStatusEffects();
				cleanedUpStatusEffect.AttackPrefab.AddEffects(cleanedUpStatusEffect.AttackPrefab.GetMainTargetString(ability), null, character, damageMult, stringEffects);
			}
		}
		if (PushDistance != 0f)
		{
			AddStringEffect(text, effect, hostile: true, stringEffects);
		}
		foreach (AfflictionParams affliction in Afflictions)
		{
			affliction.AddStringEffects(text, characterStats, ability, this, StatusEffectFormatMode.InspectWindow, stringEffects);
		}
	}

	public virtual void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
	}

	public static string StringEffects(StringEffects stringEffects, bool targets)
	{
		return StringEffects(stringEffects, targets, "", "\n");
	}

	public static string StringEffects(StringEffects stringEffects, bool targets, string indent, string delimit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (stringEffects.Count > 0)
		{
			foreach (KeyValuePair<string, List<AttackEffect>> effect in stringEffects.Effects)
			{
				IEnumerable<IGrouping<AttackEffect, AttackEffect>> enumerable = from ae in effect.Value
					group ae by ae;
				string seperator = (string.IsNullOrEmpty(effect.Key) ? (delimit + indent) : GUIUtils.Comma());
				if (enumerable.Any())
				{
					stringBuilder.Append(indent);
					if (targets && !string.IsNullOrEmpty(effect.Key))
					{
						string text = effect.Key;
						if (stringEffects.Count == 1 && text == GUIUtils.GetText(1595) && effect.Value.Any() && (bool)effect.Value.First().Attack && (bool)effect.Value.First().Attack.GetComponent<Weapon>())
						{
							text = GUIUtils.GetText(428);
						}
						stringBuilder.Append("[" + NGUITools.EncodeColor(StringKeyColor) + "]" + text + ": [-]");
					}
				}
				bool flag = false;
				foreach (IGrouping<AttackEffect, AttackEffect> item in enumerable)
				{
					AttackEffect key = item.Key;
					StringBuilder stringBuilder2 = new StringBuilder();
					AttackBase attack = item.First().Attack;
					AttackRanged attackRanged = attack as AttackRanged;
					bool flag2 = !item.First().ConsiderSecondary;
					bool flag3 = item.Any((AttackEffect effect) => effect.Hostile);
					string text2 = TextUtils.FuncJoin((AttackEffect ae) => ae.ToString(), item, seperator);
					CharacterStats.DefenseType defenseType = CharacterStats.DefenseType.None;
					if (key.OverrideDefenseType != CharacterStats.DefenseType.None)
					{
						defenseType = key.OverrideDefenseType;
					}
					else if (flag3 && (bool)attack && (flag2 ? attack.DefendedBy : attack.SecondaryDefense) != CharacterStats.DefenseType.None)
					{
						defenseType = (flag2 ? attack.DefendedBy : attack.SecondaryDefense);
					}
					if (defenseType != CharacterStats.DefenseType.None)
					{
						string text3 = GUIUtils.GetText(369);
						if (flag3 && (bool)attack && attack.AccuracyBonusTotal != 0)
						{
							text3 += GUIUtils.Format(1731, TextUtils.NumberBonus(attack.AccuracyBonusTotal));
						}
						if ((bool)attackRanged && attackRanged.VeilPiercing)
						{
							text3 += GUIUtils.Format(1731, GUIUtils.GetText(2327));
						}
						text2 = text2 + " | " + GUIUtils.Format(1605, text3, GUIUtils.GetDefenseTypeString(defenseType));
					}
					if (item.First().EffectPostFormat != "{0}")
					{
						text2 = StringUtility.Format(item.First().EffectPostFormat, text2);
					}
					stringBuilder2.Append(text2);
					string text4 = stringBuilder2.ToString();
					if (key.Attack is AttackBeam)
					{
						AttackBeam attackBeam = key.Attack as AttackBeam;
						text4 = GUIUtils.Format(1419, text4, GUIUtils.Format(211, attackBeam.BeamInterval.ToString("#0.0")));
					}
					bool flag4 = false;
					if (!flag2 && defenseType != CharacterStats.DefenseType.None && attack.DefendedBy != CharacterStats.DefenseType.None)
					{
						flag4 = true;
						text4 = GUIUtils.Format(2315, text4);
					}
					if (flag)
					{
						if (flag4)
						{
							stringBuilder.AppendLine();
						}
						else
						{
							stringBuilder.Append(GUIUtils.Comma());
						}
					}
					stringBuilder.Append(text4);
					flag = item.Any();
				}
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	protected virtual string GetRangeString(GenericAbility ability, GameObject character)
	{
		if (ApplyToSelfOnly || GetTotalAttackDistance(character) <= 1f)
		{
			return "";
		}
		if ((bool)ability && (ability.Modal || ability.Passive || ability.UsePrimaryAttack || ability.UseFullAttack))
		{
			return "";
		}
		return GUIUtils.Format(1533, GetTotalAttackDistance(character).ToString("#0.##"));
	}

	protected virtual string GetAoeString(GenericAbility ability, GameObject character)
	{
		string result = "";
		if (Jumps)
		{
			result = ((!AlwaysBounceAtEnemies) ? GUIUtils.Format((Bounces > 1) ? 1608 : 1607, Bounces) : GUIUtils.Format((Bounces > 1) ? 1602 : 1601, Bounces, GUIUtils.GetText(1598)));
		}
		return result;
	}

	public virtual string GetDurationString(GenericAbility ability)
	{
		return "";
	}

	public virtual FormattableTarget GetMainTargetString(GenericAbility ability)
	{
		if (ApplyToSelfOnly)
		{
			if (!ability)
			{
				return GenericAbility.TARGET_SELF;
			}
			return ability.GetSelfTarget();
		}
		if (ValidPrimaryTargets == TargetType.OwnAnimalCompanion)
		{
			return TARGET_ANIMALCOMPANION;
		}
		if (ValidTargets == TargetType.OwnerOfPairedAbility)
		{
			return new FormattableTarget(2325);
		}
		return GENERAL_TARGET;
	}

	protected string GetTargetQualifiers()
	{
		if (ValidTargets == TargetType.HostileBeast)
		{
			return GUIUtils.GetText(1612);
		}
		if (ValidTargets == TargetType.HostileVessel)
		{
			return GUIUtils.GetText(1368);
		}
		if (ValidTargets == TargetType.FriendlyNotVessel)
		{
			return GUIUtils.Format(1751, GUIUtils.GetText(432));
		}
		if (ValidTargets == TargetType.SpiritOrSummonedCreature)
		{
			return GUIUtils.GetText(431) + "/" + GUIUtils.GetText(2254);
		}
		return "";
	}

	public virtual void GetAllDefenses(CharacterStats attacker, GenericAbility ability, bool[] defenses, IList<int> accuracies)
	{
		if (DefendedBy != CharacterStats.DefenseType.None)
		{
			defenses[(int)DefendedBy] = true;
			int item = attacker.CalculateAccuracyForUi(this, ability, null);
			if (!accuracies.Contains(item))
			{
				accuracies.Add(item);
			}
		}
		if (SecondaryDefense != CharacterStats.DefenseType.None)
		{
			defenses[(int)SecondaryDefense] = true;
			int item2 = attacker.CalculateAccuracyForUi(this, ability, null);
			if (!accuracies.Contains(item2))
			{
				accuracies.Add(item2);
			}
		}
		if ((bool)ExtraAOE)
		{
			ExtraAOE.GetAllDefenses(attacker, ability, defenses, accuracies);
		}
		AttackAOE attackAOE = this as AttackAOE;
		if ((bool)attackAOE && (bool)attackAOE.SecondAOE)
		{
			attackAOE.SecondAOE.GetAllDefenses(attacker, ability, defenses, accuracies);
		}
		foreach (StatusEffectParams cleanedUpStatusEffect in CleanedUpStatusEffects)
		{
			if (StatusEffect.EffectLaunchesAttack(cleanedUpStatusEffect.AffectsStat) && (bool)cleanedUpStatusEffect.AttackPrefab)
			{
				cleanedUpStatusEffect.AttackPrefab.GetAllDefenses(attacker, ability, defenses, accuracies);
			}
		}
	}

	public static string FormatWC(string fstring, params object[] pms)
	{
		return StringUtility.FormatWithColor(fstring, StringKeyColor, pms);
	}

	public static string FormatBase(string adjusted, string baseVal, bool isBuff)
	{
		return (isBuff ? "[url=buffvalue://" : "[url=debuffvalue://") + baseVal + "]" + adjusted + "[/url]";
	}

	public bool ValidTargetSelf()
	{
		return TargetTypeUtils.ValidTargetSelf(ValidTargets);
	}

	public bool ValidTargetAny()
	{
		return TargetTypeUtils.ValidTargetAny(ValidTargets);
	}

	public bool ValidTargetAlly()
	{
		return TargetTypeUtils.ValidTargetAlly(ValidTargets);
	}

	public bool ValidTargetHostile()
	{
		return TargetTypeUtils.ValidTargetHostile(ValidTargets);
	}

	public bool ValidTargetFriendly()
	{
		return TargetTypeUtils.ValidTargetFriendly(ValidTargets);
	}

	public bool ValidTargetDead()
	{
		return TargetTypeUtils.ValidTargetDead(ValidTargets);
	}

	public bool ValidTargetUnconscious()
	{
		return TargetTypeUtils.ValidTargetUnconscious(ValidTargets);
	}
}

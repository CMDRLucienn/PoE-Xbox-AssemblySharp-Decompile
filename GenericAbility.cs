using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AI.Player;
using UnityEngine;
using UnityEngine.Serialization;

public class GenericAbility : MonoBehaviour, ITooltipContent
{
	public class NameComparer : IEqualityComparer<GenericAbility>
	{
		private static NameComparer s_instance;

		public static NameComparer Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new NameComparer();
				}
				return s_instance;
			}
		}

		public bool Equals(GenericAbility a, GenericAbility b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			if (a.DisplayName.StringID != b.DisplayName.StringID)
			{
				return false;
			}
			return a.name.Replace("(Clone)", "") == b.name.Replace("(Clone)", "");
		}

		public int GetHashCode(GenericAbility ability)
		{
			return ability.DisplayName.StringID;
		}
	}

	public enum ActivationGroup
	{
		None,
		A,
		B,
		C,
		D,
		E,
		F,
		Count
	}

	public enum UniqueSetType
	{
		None,
		Walls,
		Storms
	}

	public enum AbilityType
	{
		Undefined,
		Equipment,
		Ring,
		Spell,
		Attribute,
		Ability,
		Talent,
		Consumable,
		Racial,
		WeaponOrShield,
		Trap,
		Count
	}

	public enum CooldownMode
	{
		None,
		PerEncounter,
		PerRest,
		Charged,
		PerStrongholdTurn
	}

	[Flags]
	public enum NotReadyValue
	{
		AlreadyActivated = 1,
		InRecovery = 2,
		AtMaxPer = 4,
		OnlyInCombat = 8,
		FailedPrerequisite = 0x10,
		NotWhileMoving = 0x20,
		NotInGrimoire = 0x40,
		GrimoireCooldown = 0x80,
		SpellCastingDisabled = 0x100,
		NotEnoughFocus = 0x200,
		NoGrimoire = 0x400,
		NotInitialized = 0x800,
		Dead = 0x1000,
		OnlyOutsideCombat = 0x2000,
		InModalRecovery = 0x4000,
		InStealth = 0x8000,
		Invisible = 0x10000,
		AbilitiesDisabled = 0x20000
	}

	public delegate void AbilityEventHandler(GameObject source);

	public class HiddenGenericAbility : ITooltipContent
	{
		public GenericAbility Ability;

		public HiddenGenericAbility(GenericAbility ability)
		{
			Ability = ability;
		}

		public string GetTooltipContent(GameObject owner)
		{
			return "";
		}

		public string GetTooltipName(GameObject owner)
		{
			return Ability.GetTooltipName(owner);
		}

		public Texture GetTooltipIcon()
		{
			return Ability.GetTooltipIcon();
		}
	}

	public const int NotReadyCount = 11;

	[Persistent]
	[HideInInspector]
	public string OverrideName = "";

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public DatabaseString Description = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public CharacterStats.Class VocalizationClass;

	public Texture2D Icon;

	public float Cooldown = 10f;

	public CooldownMode CooldownType;

	public bool Passive;

	[Tooltip("This ability activates itself when the owner is hit.")]
	public bool TriggerOnHit;

	[Tooltip("If the caster activates a Modal ability, all other active Modal abilities from the same group are deactivated and go on cooldown. You can always deactivate a Modal ability.")]
	public bool Modal;

	public bool CombatOnly;

	private bool? originalCombatOnly;

	public bool NonCombatOnly;

	[Tooltip("Hides this ability and its status effects from the user completely. Use for backend abilities (e.g. ChanterTrait)")]
	[FormerlySerializedAs("HideFromLog")]
	public bool HideFromUi;

	[Tooltip("Hides activation messages for this ability in the log.")]
	[FormerlySerializedAs("HideFromLog")]
	public bool HideFromCombatLog;

	[Tooltip("Is this a Watcher ability?")]
	public bool IsWatcherAbility;

	[Tooltip("Abilities in the same group share cooldowns.")]
	public ActivationGroup Grouping;

	[Tooltip("Abilities in the same set stop other abilities in the same set that are active.")]
	public UniqueSetType UniqueSet;

	[Persistent]
	public AbilityType EffectType;

	public CharacterStats.NoiseLevelType NoiseLevel;

	[Tooltip("Float. Distance (in meters) from target location that Status Effects associated with this object will affect friendly targets.")]
	public float FriendlyRadius;

	[Tooltip("Override for how long (in seconds) all Status Effects associated with the Ability component will last.")]
	public float DurationOverride;

	[Persistent]
	[Tooltip("What level of mastery this ability has obtained.")]
	[HideInInspector]
	public int MasteryLevel;

	[Tooltip("Ability effect ends if character moves.")]
	public bool ClearsOnMovement;

	[Tooltip("Cannot activate while in Stealth mode. Useful for abilities like AttackPulseAOEs that assume you are visible or would break you out of stealth.")]
	public bool CannotActivateWhileInStealth;

	[Tooltip("Cannot activate while invisible. Useful for abilities like AttackPulseAOEs that assume you are visible.")]
	public bool CannotActivateWhileInvisible;

	public PrerequisiteData[] ActivationPrerequisites;

	public PrerequisiteData[] ApplicationPrerequisites;

	public StatusEffectParams[] StatusEffects;

	[Persistent]
	[HideInInspector]
	public bool AppliedViaMod;

	protected BindingList<StatusEffect> m_effects = new BindingList<StatusEffect>();

	protected List<StatusEffect> CleanedUpStatusEffects = new List<StatusEffect>();

	[Tooltip("A list of afflictions applied to the ability's caster, unless using a special ability script.")]
	public AfflictionParams[] Afflictions;

	public GameObject OnActivateGroundVisualEffect;

	public GameObject OnActivateRootVisualEffect;

	public MaterialReplacement SelfMaterialReplacement;

	public bool UsePrimaryAttack;

	public bool UseFullAttack;

	[Tooltip("Plays a cast vocalization based on character class. 0 prevents vocalization.")]
	[Range(0f, 10f)]
	public int VocalizationNumber;

	[Tooltip("Normally, abilities are ordered by unlock level. If this is not 0, it will override that sort key.")]
	public int OverrideSortKey;

	[HideInInspector]
	public int AcquisitionLevel;

	protected List<AbilityMod> m_abilityMods = new List<AbilityMod>();

	[Persistent]
	protected bool m_activated;

	[Persistent]
	protected bool m_activatedLaunching;

	[Persistent]
	protected bool m_applied;

	[Persistent]
	protected int m_cooldownCounter;

	[Persistent]
	protected bool m_UITriggered;

	protected GameObject m_owner;

	protected GameObject m_target;

	protected Vector3 m_targetPoint = Vector3.zero;

	protected CharacterStats m_ownerStats;

	protected Health m_ownerHealth;

	protected PartyMemberAI m_ownerPartyAI;

	protected bool m_permanent;

	protected NotReadyValue m_reason_not_ready;

	protected PrerequisiteType m_reason_not_ready_prereq;

	protected Mover m_mover;

	protected AttackBase m_attackBase;

	protected bool m_initialized;

	private bool m_isVisibleOnUI = true;

	[Persistent]
	protected bool m_statusEffectsActivated;

	[Persistent]
	protected bool m_statusEffectsNeeded;

	[Persistent]
	protected float m_perEncounterResetTimer;

	private float m_rez_modal_cooldown;

	public static readonly AttackBase.FormattableTarget TARGET_SELF = new AttackBase.FormattableTarget(1609);

	protected static readonly AttackBase.FormattableTarget TARGET_CASTER = new AttackBase.FormattableTarget(1611);

	protected static readonly AttackBase.FormattableTarget TARGET_USER = new AttackBase.FormattableTarget(1610);

	protected static readonly AttackBase.FormattableTarget TARGET_ANIMALCOMPANION = new AttackBase.FormattableTarget(2104);

	public static readonly AttackBase.FormattableTarget TARGET_FRIENDLY_AURA = new AttackBase.FormattableTarget(1618, 1617, AttackBase.TargetType.Friendly);

	private bool m_DisplayAsAnimalCompanion;

	public virtual bool ListenForDamageEvents => false;

	public virtual bool TriggeredAutomatically => false;

	public bool EffectTriggeredThisFrame { get; set; }

	public bool EffectUntriggeredThisFrame { get; set; }

	public int Sortkey
	{
		get
		{
			if (OverrideSortKey == 0)
			{
				return AcquisitionLevel;
			}
			return OverrideSortKey;
		}
	}

	public bool HasLevelScalingStatusEffect
	{
		get
		{
			for (int num = m_effects.Count - 1; num >= 0; num--)
			{
				if (!m_effects[num].Params.LevelScaling.Empty)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected bool ShouldShowActivation
	{
		get
		{
			if (!HideFromCombatLog && Faction.IsFowVisible(Owner))
			{
				if (!UseFullAttack)
				{
					return !UsePrimaryAttack;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsInCooldownRecovery
	{
		get
		{
			if (Passive)
			{
				return false;
			}
			if (m_ownerStats == null)
			{
				return false;
			}
			if (m_ownerStats.RecoveryTimer > 0f)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsInModalRecovery
	{
		get
		{
			if (!Modal)
			{
				return false;
			}
			if (m_ownerStats == null)
			{
				return false;
			}
			if (m_ownerStats.InModalRecovery(Grouping))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsInCooldownAtMax
	{
		get
		{
			if (CooldownType != 0 && (float)m_cooldownCounter >= MaxCooldown)
			{
				return true;
			}
			return false;
		}
	}

	public float MaxCooldown
	{
		get
		{
			float num = GatherAbilityModSum(AbilityMod.AbilityModType.AdditionalUse);
			if (m_ownerStats != null && CooldownType == CooldownMode.PerRest && Cooldown > 3f)
			{
				num += (float)m_ownerStats.BonusUsesPerRestPastThree;
			}
			num += (float)MasteryLevel;
			return Cooldown + num;
		}
	}

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public virtual GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			if (m_owner == value)
			{
				return;
			}
			m_owner = value;
			if (m_owner == null)
			{
				if (m_ownerStats != null && CooldownType == CooldownMode.PerRest)
				{
					m_ownerStats.OnResting -= HandleGameOnResting;
				}
				if ((TriggerOnHit || ListenForDamageEvents) && (bool)m_ownerHealth)
				{
					m_ownerHealth.OnDamaged -= HandleOnDamaged;
					m_ownerHealth.OnHealed -= HandleOnHealed;
				}
				m_ownerStats = null;
				m_ownerHealth = null;
				m_ownerPartyAI = null;
				m_mover = null;
				return;
			}
			for (int i = 0; i < ActiveStatusEffects.Count; i++)
			{
				ActiveStatusEffects[i].Owner = m_owner;
			}
			m_ownerStats = m_owner.GetComponent<CharacterStats>();
			if (m_ownerStats != null && CooldownType == CooldownMode.PerRest)
			{
				m_ownerStats.OnResting += HandleGameOnResting;
			}
			m_ownerHealth = m_owner.GetComponent<Health>();
			m_ownerPartyAI = m_owner.GetComponent<PartyMemberAI>();
			base.transform.position = m_owner.transform.position;
			base.transform.parent = m_owner.transform;
			m_mover = m_owner.GetComponent<Mover>();
			if ((TriggerOnHit || ListenForDamageEvents) && (bool)m_ownerHealth)
			{
				m_ownerHealth.OnDamaged += HandleOnDamaged;
				m_ownerHealth.OnHealed += HandleOnHealed;
			}
		}
	}

	public PartyMemberAI OwnerAI => m_ownerPartyAI;

	protected virtual float AttackRange
	{
		get
		{
			float result = 0f;
			if ((bool)m_attackBase)
			{
				result = m_attackBase.TotalAttackDistance;
			}
			return result;
		}
	}

	public IList<AbilityMod> AbilityMods => m_abilityMods;

	public virtual bool Activated => m_activated;

	public bool UiActivated => m_UITriggered;

	public virtual bool Applied => m_applied;

	public virtual bool ReadyIgnoreRecovery
	{
		get
		{
			if (!Ready)
			{
				return WhyNotReady == NotReadyValue.InRecovery;
			}
			return true;
		}
	}

	public virtual bool Ready
	{
		get
		{
			CalculateWhyNotReady();
			if (WhyNotReady == (NotReadyValue)0)
			{
				return true;
			}
			return false;
		}
	}

	public bool ReadyForUI
	{
		get
		{
			if (TriggeredAutomatically)
			{
				return false;
			}
			if (Modal && WhyNotReady == NotReadyValue.OnlyInCombat && CooldownType == CooldownMode.None)
			{
				return true;
			}
			if (!Ready)
			{
				return (WhyNotReady & ~NotReadyValue.InRecovery) == 0;
			}
			return true;
		}
	}

	public NotReadyValue WhyNotReady
	{
		get
		{
			return m_reason_not_ready;
		}
		set
		{
			if (value == (NotReadyValue)0)
			{
				m_reason_not_ready = (NotReadyValue)0;
			}
			else
			{
				m_reason_not_ready |= value;
			}
		}
	}

	public PrerequisiteType WhyNotReadyPrereq
	{
		get
		{
			return m_reason_not_ready_prereq;
		}
		set
		{
			m_reason_not_ready_prereq = value;
		}
	}

	private bool IsMoving
	{
		get
		{
			if (m_mover != null && m_mover.HasGoal)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsTriggeredPassive
	{
		get
		{
			if (!Passive)
			{
				return false;
			}
			if (m_effects.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < m_effects.Count; i++)
			{
				if (!m_effects[i].HasTriggerActivation)
				{
					return false;
				}
			}
			return true;
		}
	}

	public virtual float RadiusMultiplier
	{
		get
		{
			if (m_ownerStats != null)
			{
				return m_ownerStats.AoERadiusMult;
			}
			return 1f;
		}
	}

	public float AdjustedFriendlyRadius
	{
		get
		{
			float friendlyRadius = FriendlyRadius;
			friendlyRadius *= RadiusMultiplier;
			if (m_ownerStats != null)
			{
				friendlyRadius *= m_ownerStats.StatEffectRadiusMultiplier;
			}
			return friendlyRadius;
		}
	}

	public bool IsAura
	{
		get
		{
			if (FriendlyRadius > 0f)
			{
				return StatusEffects.Length != 0;
			}
			return false;
		}
	}

	public AttackBase Attack => m_attackBase;

	public bool AttackComplete { get; set; }

	public IList<StatusEffect> ActiveStatusEffects => m_effects;

	public bool IsLoaded { get; set; }

	[Persistent]
	public bool IsVisibleOnUI
	{
		get
		{
			return m_isVisibleOnUI;
		}
		set
		{
			m_isVisibleOnUI = value;
			if (!m_isVisibleOnUI && Activated)
			{
				Deactivate(null);
			}
			if (UIAbilityBar.Instance != null)
			{
				UIAbilityBar.Instance.RefreshAbilities();
			}
		}
	}

	public bool IgnoreCharacterStats
	{
		get
		{
			if ((bool)Attack)
			{
				return Attack.IgnoreCharacterStats;
			}
			return false;
		}
	}

	public virtual bool OverrideStatusEffectDisplay => false;

	public virtual bool OverrideActivationPrerequisiteDisplay => false;

	public event AbilityEventHandler OnCooldown;

	public static bool AbilityTypeIsAnyEquipment(AbilityType type)
	{
		if (type != AbilityType.Equipment && type != AbilityType.Ring)
		{
			return type == AbilityType.WeaponOrShield;
		}
		return true;
	}

	public static string Name(GenericAbility abil)
	{
		return abil.Name();
	}

	public static string Name(GameObject go)
	{
		if (!go)
		{
			Debug.LogError("Tried to get the name of a null game object.");
			return "*NameError*";
		}
		GenericAbility component = go.GetComponent<GenericAbility>();
		if ((bool)component)
		{
			return component.Name();
		}
		Debug.LogError("Tried to get the name of something that wasn't a generic ability (" + go.name + ")");
		return "*NameError*";
	}

	public static string Name(MonoBehaviour mb)
	{
		return Name(mb.gameObject);
	}

	public string Name()
	{
		if (!string.IsNullOrEmpty(OverrideName))
		{
			return OverrideName;
		}
		return DisplayName.GetText();
	}

	protected virtual void Init()
	{
		if (m_initialized)
		{
			return;
		}
		if (CooldownType == CooldownMode.PerStrongholdTurn && GameState.Stronghold != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			stronghold.OnAdvanceTurn = (Stronghold.AdvanceTurnDelegate)Delegate.Combine(stronghold.OnAdvanceTurn, new Stronghold.AdvanceTurnDelegate(HandleGameOnStrongholdTurn));
		}
		AbilityType source = AbilityType.Ability;
		if (this is GenericSpell)
		{
			source = AbilityType.Spell;
		}
		if (!GameState.LoadedGame)
		{
			if (StatusEffects != null)
			{
				StatusEffectParams[] statusEffects = StatusEffects;
				foreach (StatusEffectParams param in statusEffects)
				{
					AddStatusEffect(param, source, DurationOverride);
				}
			}
			if (Afflictions != null)
			{
				AfflictionParams[] afflictions = Afflictions;
				foreach (AfflictionParams ap in afflictions)
				{
					AddAffliction(ap, source);
				}
			}
		}
		m_attackBase = GetComponent<AttackBase>();
		if (m_attackBase != null)
		{
			m_attackBase.SetAbilityParamOverrides(DurationOverride, FriendlyRadius);
		}
		m_initialized = true;
	}

	protected virtual void Start()
	{
		Init();
		m_effects.ListChanged += NotifyStatusEffectsChanged;
		NotifyStatusEffectsChanged(m_effects, null);
		if (!Icon)
		{
			Item component = GetComponent<Item>();
			if ((bool)component)
			{
				Icon = component.IconTexture;
			}
		}
		if (!GameState.LoadedGame)
		{
			IsLoaded = true;
		}
	}

	private void NotifyStatusEffectsChanged(object sender, ListChangedEventArgs e)
	{
		UICleanStatusEffects();
	}

	public void UICleanStatusEffects()
	{
		CleanedUpStatusEffects.Clear();
		CleanedUpStatusEffects.AddRange(m_effects);
		StatusEffectParams.CleanUp(CleanedUpStatusEffects);
	}

	public void ForceInit()
	{
		Init();
	}

	public void HandleStuckAbilityDeactivation()
	{
		if (Activated && !Modal && !Passive)
		{
			UIDebug.Instance.LogOnScreenWarning("Ability found activated that should not be activated at this point. Name = " + Name(), UIDebug.Department.Programming, 10f);
			Deactivate(null);
		}
	}

	public virtual void Restored()
	{
		if (IsLoaded || (!GameState.LoadedGame && !GameState.IsRestoredLevel))
		{
			return;
		}
		if (MasteryLevel > 0)
		{
			int masteryLevel = MasteryLevel;
			MasteryLevel = 0;
			for (int i = 0; i < masteryLevel; i++)
			{
				IncrementMasteryLevel();
			}
		}
		IsLoaded = true;
		Init();
		if (Activated && Owner != null)
		{
			HookupEvents();
		}
		AbilityType source = AbilityType.Ability;
		if (this is GenericSpell)
		{
			source = AbilityType.Spell;
		}
		if (m_ownerStats == null)
		{
			if (StatusEffects != null)
			{
				StatusEffectParams[] statusEffects = StatusEffects;
				foreach (StatusEffectParams param in statusEffects)
				{
					AddStatusEffect(param, source, DurationOverride);
				}
			}
			if (Afflictions != null)
			{
				AfflictionParams[] afflictions = Afflictions;
				foreach (AfflictionParams ap in afflictions)
				{
					AddAffliction(ap, source);
				}
			}
			return;
		}
		int num = 0;
		bool flag = false;
		if (Activated)
		{
			foreach (StatusEffect activeStatusEffect in m_ownerStats.ActiveStatusEffects)
			{
				activeStatusEffect.Restored();
				if (!(activeStatusEffect.AbilityOrigin == null) && activeStatusEffect.AbilityOrigin.DisplayName.StringID == DisplayName.StringID)
				{
					activeStatusEffect.AbilityOrigin = this;
					ActiveStatusEffects.Add(activeStatusEffect);
					num++;
				}
			}
		}
		for (int num2 = ActiveStatusEffects.Count - 1; num2 >= 0; num2--)
		{
			StatusEffect statusEffect = ActiveStatusEffects[num2];
			if (statusEffect.AbilityType == AbilityType.Ability && StatusEffects != null)
			{
				int stackingKey = statusEffect.GetStackingKey();
				bool flag2 = false;
				for (int k = 0; k < StatusEffects.Length; k++)
				{
					if (flag2)
					{
						break;
					}
					if (StatusEffects[k].CalculateStackingKey() == stackingKey)
					{
						flag2 = true;
						if (StatusEffects[k].Value != statusEffect.Params.Value && DisplayName.StringID == 68)
						{
							Debug.Log("Repairing value of status effect '" + statusEffect.Params.GetDebuggerString() + "' from ability '" + base.name + "'.");
							RemoveStatusEffect(statusEffect.Params, AbilityType.Ability);
							AddStatusEffect(StatusEffects[k], AbilityType.Ability, DurationOverride);
							flag = true;
						}
					}
				}
				if (!flag2 && statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.BonusGrazeToHitRatioMeleeOneHand)
				{
					Debug.Log("Removed old status effect '" + statusEffect.Params.GetDebuggerString() + "' from ability '" + base.name + "'.");
					RemoveStatusEffect(statusEffect.Params, AbilityType.Ability);
					flag = true;
				}
			}
		}
		if (StatusEffects != null)
		{
			for (int l = 0; l < StatusEffects.Length; l++)
			{
				int num3 = StatusEffects[l].CalculateStackingKey();
				bool flag3 = false;
				for (int m = 0; m < ActiveStatusEffects.Count; m++)
				{
					if (flag3)
					{
						break;
					}
					if (ActiveStatusEffects[m].GetStackingKey() == num3)
					{
						flag3 = true;
					}
				}
				if (!flag3 && StatusEffects[l].AffectsStat == StatusEffect.ModifiedStat.BonusHitToCritRatioMeleeOneHand)
				{
					Debug.Log("Adding new status effect '" + StatusEffects[l].GetDebuggerString() + "' to ability '" + base.name + "'.");
					AddStatusEffect(StatusEffects[l], AbilityType.Ability, DurationOverride);
					flag = true;
				}
			}
		}
		HandleStuckAbilityDeactivation();
		if (StatusEffects != null)
		{
			StatusEffectParams[] statusEffects = StatusEffects;
			foreach (StatusEffectParams param2 in statusEffects)
			{
				AddStatusEffect(param2, source, DurationOverride);
			}
		}
		if (Afflictions != null)
		{
			AfflictionParams[] afflictions = Afflictions;
			foreach (AfflictionParams ap2 in afflictions)
			{
				AddAffliction(ap2, source);
			}
		}
		if (m_statusEffectsActivated && (ActiveStatusEffects.Count != num || flag) && (Modal || Passive))
		{
			ActivateStatusEffects();
		}
		UIPartyPortraitBar.Instance.RebundleEffectsAll();
	}

	protected virtual void OnDestroy()
	{
		if (m_ownerStats != null && CooldownType == CooldownMode.PerRest)
		{
			m_ownerStats.OnResting -= HandleGameOnResting;
		}
		else if (CooldownType == CooldownMode.PerStrongholdTurn && GameState.Stronghold != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			stronghold.OnAdvanceTurn = (Stronghold.AdvanceTurnDelegate)Delegate.Remove(stronghold.OnAdvanceTurn, new Stronghold.AdvanceTurnDelegate(HandleGameOnStrongholdTurn));
		}
		if (m_ownerHealth != null)
		{
			m_ownerHealth.OnDamaged -= HandleOnDamaged;
			m_ownerHealth.OnHealed -= HandleOnHealed;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public StatusEffect AddStatusEffect(StatusEffectParams param, AbilityType source, float duration)
	{
		StatusEffect statusEffect = StatusEffect.Create(base.gameObject, param, source, null, deleteOnClear: false, duration);
		statusEffect.AbilityOrigin = this;
		statusEffect.FriendlyRadius = FriendlyRadius;
		for (int i = 0; i < ActiveStatusEffects.Count; i++)
		{
			if (ActiveStatusEffects[i].AbilityType == statusEffect.AbilityType && ActiveStatusEffects[i].GetStackingKey() == statusEffect.GetStackingKey())
			{
				return null;
			}
		}
		m_effects.Add(statusEffect);
		return statusEffect;
	}

	public void RemoveStatusEffect(StatusEffectParams param, AbilityType source)
	{
		foreach (StatusEffect effect in m_effects)
		{
			if (effect.Params == param && effect.AbilityType == source)
			{
				if (m_statusEffectsActivated && (bool)m_ownerStats)
				{
					m_ownerStats.ClearEffect(effect);
				}
				m_effects.Remove(effect);
				break;
			}
		}
	}

	public void RemoveStatusEffectByOrigin(StatusEffectParams param, MonoBehaviour origin)
	{
		foreach (StatusEffect effect in m_effects)
		{
			if (effect.Params == param && effect.AnyOriginIs(origin))
			{
				if (m_statusEffectsActivated && (bool)m_ownerStats)
				{
					m_ownerStats.ClearEffect(effect);
				}
				m_effects.Remove(effect);
				break;
			}
		}
	}

	public void AddAffliction(AfflictionParams ap, AbilityType source)
	{
		if (!(ap.AfflictionPrefab != null) || ap.AfflictionPrefab.StatusEffects == null)
		{
			return;
		}
		StatusEffectParams[] statusEffects = ap.AfflictionPrefab.StatusEffects;
		foreach (StatusEffectParams param in statusEffects)
		{
			float duration = ap.Duration;
			if (DurationOverride > 0f)
			{
				duration = DurationOverride;
			}
			StatusEffect statusEffect = AddStatusEffect(param, source, duration);
			if (statusEffect != null)
			{
				statusEffect.AfflictionOrigin = ap.AfflictionPrefab;
				statusEffect.AfflictionKeyword = ap.Keyword;
			}
		}
	}

	public virtual void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (Passive && Ready && TriggerOnHit && args.GameObjectData != null && !(args.GameObjectData[0] == null))
		{
			Activate(args.GameObjectData[0]);
			if ((bool)m_attackBase)
			{
				m_attackBase.SkipAnimation = true;
				m_attackBase.Launch(args.GameObjectData[0]);
			}
			else
			{
				ActivateStatusEffects();
			}
		}
	}

	public virtual void HandleOnHealed(GameObject myObject, GameEventArgs args)
	{
	}

	protected virtual void ReportActivation(bool overridePassive)
	{
		if (ShouldShowActivation && (!Passive || overridePassive))
		{
			Consumable component = GetComponent<Consumable>();
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(((bool)component && component.IsFoodDrugOrPotion) ? 1982 : 124), CharacterStats.NameColored(m_owner), Name(this)), Color.white);
		}
	}

	protected virtual void ReportDeactivation(bool overridePassive)
	{
		if (ShouldShowActivation && (!Passive || overridePassive))
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(130), CharacterStats.NameColored(m_owner), Name(this)), Color.white);
		}
	}

	public void Activate()
	{
		Activate(Owner);
	}

	protected virtual void HookupEvents()
	{
		m_ownerStats.OnPreApply += HandleStatsOnPreApply;
		m_ownerStats.OnAttackLaunch += HandleStatsOnAttackLaunch;
		m_ownerStats.OnAttackHitFrame += HandleStatsOnAttackHitFrame;
		m_ownerStats.OnAttackHits += HandleStatsOnAttackHits;
		m_ownerStats.OnBeamHits += HandleStatsOnBeamHits;
		m_ownerStats.OnEffectApply += HandleStatsOnEffectApply;
		m_ownerStats.OnAddDamage += HandleStatsOnAddDamage;
		m_ownerStats.OnAdjustCritGrazeMiss += HandleStatsOnAdjustCritGrazeMiss;
		m_ownerStats.OnApplyDamageThreshhold += HandleStatsOnApplyDamageThreshhold;
		m_ownerStats.OnApplyProcs += HandleStatsOnApplyProcs;
		m_ownerStats.OnAttackRollCalculated += HandleStatsOnAttackRollCalculated;
		m_ownerStats.OnPostDamageApplied += HandleStatsOnPostDamageApplied;
		m_ownerStats.OnPostDamageDealt += HandleStatsOnPostDamageDealt;
		m_ownerStats.OnPreDamageApplied += HandleStatsOnPreDamageApplied;
		m_ownerStats.OnPreDamageDealt += HandleStatsOnPreDamageDealt;
		m_ownerStats.OnDeactivate += HandleStatsOnDeactivate;
		m_ownerStats.OnDamageFinal += HandleStatsOnDamageFinal;
	}

	public virtual void Activate(Vector3 target)
	{
		if (!m_activated && Ready && CanApply())
		{
			CheckUniqueSet();
			m_activated = true;
			m_targetPoint = target;
			HookupEvents();
			Apply(target);
			ActivationAutopause();
		}
	}

	public virtual void Activate(GameObject target)
	{
		if (Ready)
		{
			ActivateImpl(target);
		}
	}

	public virtual void ActivateIgnoreRecovery(GameObject target)
	{
		if (ReadyIgnoreRecovery)
		{
			ActivateImpl(target);
		}
	}

	private void ActivateImpl(GameObject target)
	{
		if (m_activated || !CanApply())
		{
			return;
		}
		if (m_ownerStats == null)
		{
			Debug.LogError(base.name + " failed because it has no owner! It may have not been initialized.", base.gameObject);
			return;
		}
		CheckUniqueSet();
		m_activated = true;
		m_target = target;
		HookupEvents();
		Apply(target);
		ApplyMaterialReplacement();
		if ((CooldownType == CooldownMode.PerEncounter || CooldownType == CooldownMode.PerRest) && (bool)m_ownerPartyAI)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.USED_PER_REST_ENCOUNTER);
		}
		ActivationAutopause();
	}

	private void ActivationAutopause()
	{
		if (!Passive && !Modal)
		{
			AIController aIController = GameUtilities.FindActiveAIController(Owner);
			if ((bool)aIController && !(aIController is PartyMemberAI) && !Cutscene.CutsceneActive)
			{
				GameState.AutoPause(AutoPauseOptions.PauseEvent.SpellCast, Owner, aIController.CurrentTarget, this);
			}
		}
	}

	protected virtual bool CanApply()
	{
		return PrerequisiteData.CheckPrerequisites(base.gameObject, Owner, ApplicationPrerequisites, Owner);
	}

	protected virtual void Apply(GameObject target)
	{
		if (m_ownerStats != null)
		{
			m_ownerStats.ApplyAbility(base.gameObject, target);
			m_ownerStats.NoiseLevel = NoiseLevel;
		}
		if (Modal)
		{
			DeactivateOtherModal();
			if (m_ownerStats != null)
			{
				m_ownerStats.SetModalRecovery(Grouping);
			}
		}
		else
		{
			ActivateGroupCooldown();
		}
		RemoveConsumables();
		RemoveInvisibility();
		if (target != null)
		{
			GameUtilities.LaunchEffect(OnActivateGroundVisualEffect, 1f, target.transform.position, this);
			GameUtilities.LaunchEffect(OnActivateRootVisualEffect, 1f, target.transform, this);
		}
		m_activated = true;
		m_statusEffectsNeeded = true;
		m_statusEffectsActivated = false;
		m_applied = true;
		if ((bool)m_attackBase)
		{
			AttackComplete = false;
			if (Passive && !m_attackBase.RequiresHitObject)
			{
				m_attackBase.SkipAnimation = true;
			}
			if (m_attackBase.SkipAnimation)
			{
				m_attackBase.Launch(target.transform.position, target);
			}
		}
		Consumable component = GetComponent<Consumable>();
		if (component != null)
		{
			component.EndUse(m_owner);
		}
		if (ShowNormalActivationMessages() && (((bool)target && FogOfWar.PointVisibleInFog(target.transform.position)) || ((bool)m_owner && FogOfWar.PointVisibleInFog(m_owner.transform.position))) && (bool)m_ownerStats && !m_ownerStats.GetComponent<Trap>())
		{
			ReportActivation(overridePassive: false);
		}
	}

	protected virtual void Apply(Vector3 target)
	{
		if (m_ownerStats != null)
		{
			m_ownerStats.ApplyAbility(base.gameObject, target);
			m_ownerStats.NoiseLevel = NoiseLevel;
		}
		if (Modal)
		{
			DeactivateOtherModal();
			if (m_ownerStats != null)
			{
				m_ownerStats.SetModalRecovery(Grouping);
			}
		}
		else
		{
			ActivateGroupCooldown();
		}
		RemoveConsumables();
		RemoveInvisibility();
		GameUtilities.LaunchEffect(OnActivateGroundVisualEffect, 1f, target, this);
		m_activated = true;
		m_statusEffectsNeeded = true;
		m_statusEffectsActivated = false;
		m_applied = true;
		if ((bool)m_attackBase)
		{
			AttackComplete = false;
		}
		Consumable component = GetComponent<Consumable>();
		if (component != null)
		{
			component.EndUse(m_owner);
		}
		if (ShowNormalActivationMessages() && (FogOfWar.PointVisibleInFog(target) || (m_owner != null && FogOfWar.PointVisibleInFog(m_owner.transform.position))))
		{
			ReportActivation(overridePassive: false);
		}
	}

	public void UpdateStatusEffectActivation()
	{
		if (m_statusEffectsNeeded && !m_statusEffectsActivated)
		{
			ActivateStatusEffects();
		}
		else if (!m_statusEffectsNeeded && m_statusEffectsActivated)
		{
			DeactivateStatusEffects();
		}
	}

	private void CheckUniqueSet()
	{
		if (UniqueSet == UniqueSetType.None)
		{
			return;
		}
		switch (UniqueSet)
		{
		case UniqueSetType.Walls:
		{
			Trap[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(Trap)) as Trap[];
			foreach (Trap trap in array2)
			{
				if (trap.Owner == m_owner && trap.IsWallTrap)
				{
					trap.Destruct();
				}
			}
			break;
		}
		case UniqueSetType.Storms:
		{
			Projectile[] array = UnityEngine.Object.FindObjectsOfType(typeof(Projectile)) as Projectile[];
			foreach (Projectile projectile in array)
			{
				if (projectile.Owner == m_owner && projectile.AbilityOrigin != null && projectile.AbilityOrigin.UniqueSet == UniqueSetType.Storms)
				{
					projectile.DestroyProjectile();
				}
			}
			break;
		}
		}
	}

	protected virtual void ActivateStatusEffects()
	{
		if (m_ownerStats == null)
		{
			Debug.LogError("'" + base.name + "': m_ownerStats is null.", this);
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			m_ownerStats.ApplyStatusEffectImmediate(effect);
		}
		if (Afflictions != null)
		{
			for (int i = 0; i < Afflictions.Length; i++)
			{
				if (Afflictions[i] != null && Afflictions[i].AfflictionPrefab != null && Afflictions[i].AfflictionPrefab.Material != null && !Afflictions[i].AfflictionPrefab.Material.Empty)
				{
					Afflictions[i].AfflictionPrefab.Material.Replace(Owner);
					break;
				}
			}
		}
		m_statusEffectsActivated = true;
	}

	public void ActivateOneHitStatusEffects()
	{
		foreach (StatusEffect effect in m_effects)
		{
			if (effect.Params.OneHitUse)
			{
				m_ownerStats.ApplyStatusEffectImmediate(effect);
			}
		}
	}

	protected void ApplyMaterialReplacement()
	{
		if ((bool)Owner && !SelfMaterialReplacement.Empty)
		{
			SelfMaterialReplacement.Replace(Owner);
		}
	}

	public void DeactivateMaterialReplacement()
	{
		if ((bool)Owner && !SelfMaterialReplacement.Empty)
		{
			SelfMaterialReplacement.Restore(Owner);
		}
	}

	public virtual void DeactivateStatusEffects()
	{
		foreach (StatusEffect effect in m_effects)
		{
			if (!effect.Params.IgnoreAbilityDeactivation)
			{
				m_ownerStats.ClearEffect(effect);
			}
		}
		m_statusEffectsActivated = false;
	}

	public virtual void Deactivate(GameObject target)
	{
		if (m_activated && !(m_ownerStats == null))
		{
			DeactivateMaterialReplacement();
			m_ownerStats.DeactivateAbility(base.gameObject, target);
			if ((bool)Attack)
			{
				Attack.OnDeactivateAbility();
			}
			m_activated = false;
			m_activatedLaunching = false;
			m_statusEffectsNeeded = false;
			m_applied = false;
			if (Modal)
			{
				ActivateGroupCooldown();
				m_ownerStats.SetModalRecovery(Grouping);
			}
			OnInactive();
			if (ShowNormalActivationMessages() && (bool)target && FogOfWar.PointVisibleInFog(target.transform.position))
			{
				ReportDeactivation(overridePassive: false);
			}
		}
	}

	public void ForceDeactivate(GameObject target)
	{
		Deactivate(target);
		DeactivateStatusEffects();
	}

	protected virtual void ActivateGroupCooldown()
	{
		ActivateCooldown();
	}

	public void DeactivateOtherModal(bool onlyUi = false)
	{
		if (Grouping == ActivationGroup.None || m_ownerStats == null)
		{
			return;
		}
		foreach (GenericAbility activeAbility in m_ownerStats.ActiveAbilities)
		{
			if (!(activeAbility != this) || activeAbility.Grouping != Grouping)
			{
				continue;
			}
			activeAbility.UntriggerFromUI();
			if (activeAbility.m_activated)
			{
				if (!onlyUi)
				{
					activeAbility.Deactivate(Owner);
				}
				break;
			}
		}
	}

	protected virtual void orig_Update()
	{
		if (CooldownType == CooldownMode.PerEncounter && !GameState.InCombat && m_perEncounterResetTimer > 0f)
		{
			m_perEncounterResetTimer -= Time.deltaTime;
			if (m_perEncounterResetTimer <= 0f)
			{
				m_cooldownCounter = 0;
				m_perEncounterResetTimer = 0f;
			}
		}
		if (m_activated)
		{
			if (ClearsOnMovement && IsMoving)
			{
				HideFromCombatLog = true;
				Deactivate(m_owner);
				return;
			}
			if (CombatOnly && !GameState.InCombat)
			{
				Deactivate(m_owner);
				return;
			}
			if (NonCombatOnly && GameState.InCombat)
			{
				Deactivate(m_owner);
				return;
			}
		}
		if (!GameState.Paused)
		{
			UpdateStatusEffectActivation();
		}
		if (m_activated && !m_applied)
		{
			if (CanApply())
			{
				if (m_target != null)
				{
					Apply(m_target);
				}
				else
				{
					Apply(m_targetPoint);
				}
			}
			return;
		}
		if (!Passive && Modal)
		{
			bool flag = (bool)m_ownerHealth && (m_ownerHealth.Dead || m_ownerHealth.Unconscious);
			if (m_activatedLaunching)
			{
				if (flag || !m_UITriggered)
				{
					m_activatedLaunching = false;
					m_rez_modal_cooldown = 5f;
				}
				else if (m_UITriggered && CombatOnly && GameState.InCombat && !m_activated && m_rez_modal_cooldown <= 0f && !(Attack is TeleportAbility))
				{
					PartyMemberAI component = m_owner.GetComponent<PartyMemberAI>();
					if ((bool)component)
					{
						if (!(component.StateManager.FindState(typeof(Ability)) is Ability ability) || ability.QueuedAbility != this)
						{
							m_activatedLaunching = false;
						}
					}
					else
					{
						m_activatedLaunching = false;
					}
				}
				else if (m_UITriggered && NonCombatOnly && !GameState.InCombat && !m_activated && m_rez_modal_cooldown <= 0f && !(Attack is TeleportAbility))
				{
					PartyMemberAI component2 = m_owner.GetComponent<PartyMemberAI>();
					if ((bool)component2)
					{
						if (!(component2.StateManager.FindState(typeof(Ability)) is Ability ability2) || ability2.QueuedAbility != this)
						{
							m_activatedLaunching = false;
						}
					}
					else
					{
						m_activatedLaunching = false;
					}
				}
				else if (m_rez_modal_cooldown > 0f)
				{
					m_rez_modal_cooldown -= Time.deltaTime;
				}
			}
			if (m_activated && CombatOnly && flag)
			{
				Deactivate(m_owner);
				m_activatedLaunching = false;
			}
			if (m_ownerPartyAI != null && m_ownerPartyAI.gameObject.activeInHierarchy && m_UITriggered != (m_activated || m_activatedLaunching))
			{
				if (m_activated)
				{
					if (!GameState.Paused)
					{
						Deactivate(m_owner);
					}
				}
				else if (m_ownerPartyAI != null && m_ownerPartyAI.QueuedAbility == this)
				{
					m_ownerPartyAI.QueuedAbility = null;
				}
				else if (!GameState.Paused || Attack != null)
				{
					if (m_UITriggered && !flag)
					{
						if (Ready)
						{
							LaunchAttack(base.gameObject, useFullAttack: false, null, null, null);
						}
					}
					else
					{
						m_rez_modal_cooldown = 5f;
						m_activatedLaunching = false;
					}
				}
			}
		}
		else if (!Passive && m_UITriggered)
		{
			m_UITriggered = false;
			m_ownerPartyAI = ComponentUtils.GetComponent<PartyMemberAI>(Owner);
			if (m_ownerPartyAI != null && m_ownerPartyAI.Selected)
			{
				if (TriggerOnHit)
				{
					m_activated = true;
					return;
				}
				if (UsePrimaryAttack || UseFullAttack)
				{
					Equipment component3 = m_owner.GetComponent<Equipment>();
					if (component3 != null)
					{
						AttackBase primaryAttack = component3.PrimaryAttack;
						if (primaryAttack != null && m_ownerStats != null)
						{
							GenericAbility component4 = base.gameObject.GetComponent<GenericAbility>();
							AttackBase component5 = base.gameObject.GetComponent<AttackBase>();
							if (m_attackBase != null)
							{
								StatusEffect[] array = new StatusEffect[1];
								StatusEffectParams statusEffectParams = new StatusEffectParams();
								statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.ApplyAttackEffects;
								statusEffectParams.AttackPrefab = m_attackBase;
								statusEffectParams.OneHitUse = true;
								statusEffectParams.IsHostile = false;
								AbilityType abType = AbilityType.Ability;
								if (this is GenericSpell)
								{
									abType = AbilityType.Spell;
								}
								array[0] = StatusEffect.Create(m_owner, this, statusEffectParams, abType, null, deleteOnClear: true);
								array[0].ApplyAttackEffectsOnlyForAttacks = new AttackBase[2] { primaryAttack, component3.SecondaryAttack };
								if (m_attackBase.UseAttackVariationOnFullOrPrimary && (UsePrimaryAttack || UseFullAttack))
								{
									LaunchAttack(primaryAttack.gameObject, UseFullAttack, component4, array, component5, m_attackBase.AttackVariation);
								}
								else
								{
									LaunchAttack(primaryAttack.gameObject, UseFullAttack, component4, array, component5);
								}
							}
							else
							{
								LaunchAttack(primaryAttack.gameObject, UseFullAttack, component4, null, component5);
							}
						}
					}
				}
				else
				{
					LaunchAttack(base.gameObject, useFullAttack: false, null, null, null);
				}
			}
		}
		else if (m_activated && m_applied && !Passive && !Modal && !m_permanent && !UseFullAttack && !UsePrimaryAttack && (m_attackBase == null || AttackComplete))
		{
			m_activated = false;
			m_activatedLaunching = false;
			m_applied = false;
			OnInactive();
		}
		if (!IsTriggeredPassive)
		{
			return;
		}
		if (EffectTriggeredThisFrame)
		{
			if ((bool)m_owner && FogOfWar.PointVisibleInFog(m_owner.transform.position))
			{
				ReportActivation(overridePassive: true);
			}
			EffectTriggeredThisFrame = false;
		}
		if (EffectUntriggeredThisFrame)
		{
			if ((bool)m_owner && FogOfWar.PointVisibleInFog(m_owner.transform.position))
			{
				ReportDeactivation(overridePassive: true);
			}
			EffectUntriggeredThisFrame = false;
		}
	}

	protected virtual void Update()
    {
		if (originalCombatOnly == null)
		{
			originalCombatOnly = CombatOnly;
		}
		CombatOnly = !IEModOptions.CombatOnlyMod && originalCombatOnly.Value;
		orig_Update();
	}

	protected void LaunchAttack(GameObject attackObj, bool useFullAttack, GenericAbility weaponAbility, StatusEffect[] effectsOnLaunch, AttackBase weaponAttack)
	{
		LaunchAttack(attackObj, useFullAttack, weaponAbility, effectsOnLaunch, weaponAttack, -1);
	}

	protected virtual void LaunchAttack(GameObject attackObj, bool useFullAttack, GenericAbility weaponAbility, StatusEffect[] effectsOnLaunch, AttackBase weaponAttack, int animVariation)
	{
		m_activatedLaunching = true;
		GameEventArgs gameEventArgs = new GameEventArgs();
		gameEventArgs.Type = GameEventType.Ability;
		gameEventArgs.GameObjectData = new GameObject[2];
		gameEventArgs.GameObjectData[0] = attackObj;
		gameEventArgs.GameObjectData[1] = Owner;
		gameEventArgs.GenericData = new object[5];
		gameEventArgs.GenericData[0] = "activate";
		if (useFullAttack)
		{
			gameEventArgs.GenericData[1] = "full";
		}
		else
		{
			gameEventArgs.GenericData[1] = "";
		}
		if (effectsOnLaunch != null)
		{
			gameEventArgs.GenericData[2] = effectsOnLaunch;
		}
		else
		{
			gameEventArgs.GenericData[2] = null;
		}
		gameEventArgs.GenericData[3] = animVariation;
		gameEventArgs.GenericData[4] = weaponAttack;
		if (m_ownerPartyAI != null)
		{
			m_ownerPartyAI.TriggerAbility(gameEventArgs, weaponAbility);
		}
	}

	protected virtual void OnInactive()
	{
		if (!(m_ownerStats == null))
		{
			m_ownerStats.OnPreApply -= HandleStatsOnPreApply;
			m_ownerStats.OnAttackLaunch -= HandleStatsOnAttackLaunch;
			m_ownerStats.OnAttackHitFrame -= HandleStatsOnAttackHitFrame;
			m_ownerStats.OnAttackHits -= HandleStatsOnAttackHits;
			m_ownerStats.OnBeamHits -= HandleStatsOnBeamHits;
			m_ownerStats.OnEffectApply -= HandleStatsOnEffectApply;
			m_ownerStats.OnAddDamage -= HandleStatsOnAddDamage;
			m_ownerStats.OnAdjustCritGrazeMiss -= HandleStatsOnAdjustCritGrazeMiss;
			m_ownerStats.OnApplyDamageThreshhold -= HandleStatsOnApplyDamageThreshhold;
			m_ownerStats.OnApplyProcs -= HandleStatsOnApplyProcs;
			m_ownerStats.OnAttackRollCalculated -= HandleStatsOnAttackRollCalculated;
			m_ownerStats.OnPostDamageApplied -= HandleStatsOnPostDamageApplied;
			m_ownerStats.OnPostDamageDealt -= HandleStatsOnPostDamageDealt;
			m_ownerStats.OnPreDamageApplied -= HandleStatsOnPreDamageApplied;
			m_ownerStats.OnPreDamageDealt -= HandleStatsOnPreDamageDealt;
			m_ownerStats.OnDeactivate -= HandleStatsOnDeactivate;
			m_ownerStats.OnDamageFinal -= HandleStatsOnDamageFinal;
		}
	}

	public virtual void ActivateCooldown()
	{
		if (CooldownType != 0)
		{
			m_cooldownCounter++;
			if (CooldownType == CooldownMode.PerEncounter)
			{
				m_perEncounterResetTimer = 1f;
			}
		}
		if (this.OnCooldown != null)
		{
			this.OnCooldown(base.gameObject);
		}
	}

	public virtual void RestoreCooldown()
	{
		if (Grouping == ActivationGroup.None && CooldownType != 0 && m_cooldownCounter > 0)
		{
			m_cooldownCounter--;
		}
	}

	public void GiveUses(int uses)
	{
		m_cooldownCounter = Mathf.Max(0, m_cooldownCounter - uses);
	}

	public virtual void IncrementMasteryLevel()
	{
		MasteryLevel++;
		if (MasteryLevel > 0)
		{
			CooldownType = CooldownMode.PerEncounter;
		}
	}

	public static void MasterAbility(CharacterStats targetStats, GenericAbility genericAbilityToMaster)
	{
		if ((bool)targetStats && (bool)genericAbilityToMaster)
		{
			GenericAbility genericAbility = targetStats.FindMasteredAbilityInstance(genericAbilityToMaster);
			if (genericAbility == null)
			{
				genericAbility = genericAbilityToMaster;
			}
			if (genericAbility.MasteryLevel == 0)
			{
				genericAbility = targetStats.InstantiateAbility(genericAbilityToMaster, (genericAbilityToMaster is GenericSpell) ? AbilityType.Spell : AbilityType.Ability);
			}
			genericAbility.IncrementMasteryLevel();
		}
	}

	public virtual int UsesLeft()
	{
		int result = int.MaxValue;
		if (CooldownType != 0)
		{
			int num = (int)MaxCooldown;
			result = ((m_cooldownCounter < num) ? (num - m_cooldownCounter) : 0);
		}
		return result;
	}

	public virtual void TriggerFromUI()
	{
		m_UITriggered = !m_UITriggered;
	}

	public void ForceTriggerFromUI()
	{
		m_UITriggered = true;
	}

	public void UntriggerFromUI()
	{
		m_UITriggered = false;
	}

	public bool IsValidTarget(GameObject target)
	{
		if ((UsePrimaryAttack || UseFullAttack) && target != null && target == Owner)
		{
			return false;
		}
		return true;
	}

	public void CancelCasting()
	{
		if ((bool)GetComponent<Consumable>())
		{
			GameUtilities.Destroy(base.gameObject, 0.5f);
		}
	}

	public virtual void RemoveConsumables()
	{
		if (ActivationPrerequisites == null || m_ownerStats == null)
		{
			return;
		}
		PrerequisiteData[] activationPrerequisites = ActivationPrerequisites;
		foreach (PrerequisiteData prerequisiteData in activationPrerequisites)
		{
			if (!prerequisiteData.IsConsumed)
			{
				continue;
			}
			switch (prerequisiteData.Type)
			{
			case PrerequisiteType.StatusEffectCount:
			{
				for (int k = 0; (float)k < prerequisiteData.Value; k++)
				{
					m_ownerStats.ClearEffect(prerequisiteData.Tag);
				}
				break;
			}
			case PrerequisiteType.StatusEffectCountFromOwner:
			{
				for (int j = 0; (float)j < prerequisiteData.Value; j++)
				{
					m_ownerStats.ClearEffect(prerequisiteData.Tag, Owner);
				}
				break;
			}
			}
		}
	}

	public virtual void RemoveInvisibility()
	{
		if (!Passive && m_ownerStats != null)
		{
			m_ownerStats.ClearStatusEffects(StatusEffect.ModifiedStat.Invisible);
		}
	}

	public void AddAbilityMod(AbilityMod mod, AbilityType source)
	{
		AddAbilityMod(mod, source, null);
	}

	public void AddAbilityMod(AbilityMod mod, AbilityType source, Equippable equipOrigin)
	{
		if (mod != null && !m_abilityMods.Contains(mod))
		{
			mod.SourceType = source;
			mod.EquipmentOrigin = equipOrigin;
			m_abilityMods.Add(mod);
			ApplyToAbility(mod);
		}
	}

	private void ApplyToAbility(AbilityMod mod)
	{
		if (mod == null)
		{
			throw new ArgumentNullException("mod");
		}
		if (mod.Type == AbilityMod.AbilityModType.AddAbilityStatusEffects && mod.StatusEffects != null)
		{
			bool activated = m_activated;
			bool hideFromCombatLog = HideFromCombatLog;
			if (activated)
			{
				HideFromCombatLog = true;
				ForceDeactivate(Owner);
			}
			StatusEffectParams[] statusEffects = mod.StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				StatusEffect statusEffect = AddStatusEffect(param, mod.SourceType, DurationOverride);
				if (statusEffect != null)
				{
					statusEffect.EquipmentOrigin = mod.EquipmentOrigin;
				}
			}
			if (activated)
			{
				Activate(Owner);
			}
			HideFromCombatLog = hideFromCombatLog;
		}
		if ((bool)Attack)
		{
			Attack.NotifyStatusEffectsChanged();
		}
	}

	public void RemoveAbilityModByOrigin(AbilityMod mod, MonoBehaviour origin)
	{
		if (mod == null)
		{
			return;
		}
		if (mod.Type == AbilityMod.AbilityModType.AddAbilityStatusEffects && mod.StatusEffects != null)
		{
			StatusEffectParams[] statusEffects = mod.StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				RemoveStatusEffectByOrigin(param, origin);
			}
		}
		m_abilityMods.Remove(mod);
	}

	public void RemoveAbilityMod(AbilityMod mod, AbilityType source)
	{
		if (mod == null)
		{
			return;
		}
		if (mod.Type == AbilityMod.AbilityModType.AddAbilityStatusEffects && mod.StatusEffects != null)
		{
			StatusEffectParams[] statusEffects = mod.StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				RemoveStatusEffect(param, source);
			}
		}
		m_abilityMods.Remove(mod);
	}

	public float GatherAbilityModSum(AbilityMod.AbilityModType type)
	{
		float num = 0f;
		for (int i = 0; i < m_abilityMods.Count; i++)
		{
			AbilityMod abilityMod = m_abilityMods[i];
			if (abilityMod.Type == type)
			{
				num += abilityMod.Value;
			}
		}
		return num;
	}

	public float GatherAbilityModProduct(AbilityMod.AbilityModType type)
	{
		float num = 1f;
		for (int i = 0; i < m_abilityMods.Count; i++)
		{
			AbilityMod abilityMod = m_abilityMods[i];
			if (abilityMod.Type == type)
			{
				num *= abilityMod.Value;
			}
		}
		return num;
	}

	public void ApplyAbilityModAttackStatusEffects(GameObject enemy, DamageInfo hitInfo, bool deleteOnClear, List<StatusEffect> appliedEffects)
	{
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (AbilityMod abilityMod in m_abilityMods)
		{
			if (abilityMod.Type != AbilityMod.AbilityModType.AddAttackStatusEffects || abilityMod.StatusEffects == null)
			{
				continue;
			}
			StatusEffectParams[] statusEffects = abilityMod.StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				StatusEffect statusEffect = StatusEffect.Create(base.gameObject, this, param, AbilityType.Talent, hitInfo, deleteOnClear);
				statusEffect.FriendlyRadius = FriendlyRadius;
				if (component.ApplyStatusEffectImmediate(statusEffect))
				{
					appliedEffects?.Add(statusEffect);
				}
			}
		}
	}

	public void ApplyAbilityModAttackStatusEffectsOnCasterOnly(DamageInfo hitInfo, bool deleteOnClear, List<StatusEffect> appliedEffects)
	{
		CharacterStats component = m_owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (AbilityMod abilityMod in m_abilityMods)
		{
			if (abilityMod.Type != AbilityMod.AbilityModType.AddAttackStatusEffectOnCasterOnly || abilityMod.StatusEffects == null)
			{
				continue;
			}
			StatusEffectParams[] statusEffects = abilityMod.StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				StatusEffect statusEffect = StatusEffect.Create(base.gameObject, this, param, AbilityType.Talent, hitInfo, deleteOnClear);
				statusEffect.FriendlyRadius = FriendlyRadius;
				if (component.ApplyStatusEffectImmediate(statusEffect))
				{
					appliedEffects?.Add(statusEffect);
				}
			}
		}
	}

	public bool HasAbilityModAttackStatusEffect(StatusEffect.ModifiedStat effectStat)
	{
		foreach (AbilityMod abilityMod in m_abilityMods)
		{
			if (abilityMod.Type != AbilityMod.AbilityModType.AddAttackStatusEffects || abilityMod.StatusEffects == null)
			{
				continue;
			}
			StatusEffectParams[] statusEffects = abilityMod.StatusEffects;
			for (int i = 0; i < statusEffects.Length; i++)
			{
				if (statusEffects[i].AffectsStat == effectStat)
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject CheckForReplacedParticleFX(GameObject existing)
	{
		if (!(existing != null))
		{
			return existing;
		}
		foreach (AbilityMod abilityMod in m_abilityMods)
		{
			if (abilityMod.Type != AbilityMod.AbilityModType.ReplaceParticleFX || abilityMod.ReplaceObjects == null)
			{
				continue;
			}
			AbilityMod.ReplaceObjectParams[] replaceObjects = abilityMod.ReplaceObjects;
			foreach (AbilityMod.ReplaceObjectParams replaceObjectParams in replaceObjects)
			{
				if (replaceObjectParams.Existing.name == existing.name)
				{
					return replaceObjectParams.ReplaceWith;
				}
			}
		}
		return existing;
	}

	public virtual void PlayVocalization()
	{
		if (VocalizationNumber == 0)
		{
			return;
		}
		string text = ((VocalizationClass == CharacterStats.Class.Undefined) ? m_ownerStats.CharacterClass.ToString() : VocalizationClass.ToString());
		string value = text + "Ability";
		SoundSet.SoundAction soundAction = SoundSet.SoundAction.Attack;
		try
		{
			soundAction = (SoundSet.SoundAction)Enum.Parse(typeof(SoundSet.SoundAction), value);
		}
		catch (Exception ex)
		{
			Debug.LogError("Vocalization class " + text + " isn't valid for audio! " + ex.Message, base.gameObject);
			return;
		}
		SoundSet soundSet = null;
		SoundSetComponent component = Owner.GetComponent<SoundSetComponent>();
		if (component != null)
		{
			soundSet = component.SoundSet;
		}
		if (soundSet == null)
		{
			Chatter component2 = Owner.GetComponent<Chatter>();
			if (component2 != null)
			{
				soundSet = component2.SoundSet;
			}
		}
		if (soundSet != null)
		{
			soundSet.PlaySound(Owner, soundAction, VocalizationNumber - 1);
		}
	}

	protected virtual void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnPreDamageApplied(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnPostDamageApplied(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnAttackRollCalculated(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnApplyProcs(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnApplyDamageThreshhold(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnAdjustCritGrazeMiss(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnAddDamage(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnPreApply(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnEffectApply(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
	}

	public virtual DamageInfo UIGetBonusAccuracyOnAttack(GameObject source, DamageInfo damage)
	{
		return damage;
	}

	public virtual DamageInfo UIAdjustDamageOnAttack(GameObject source, DamageInfo damage)
	{
		return damage;
	}

	protected virtual void HandleStatsOnAttackHitFrame(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnAttackHits(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnBeamHits(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnDeactivate(GameObject source, CombatEventArgs args)
	{
	}

	protected virtual void HandleStatsOnDamageFinal(GameObject source, CombatEventArgs args)
	{
	}

	public virtual void HandleStatsOnPostDamageDealtCallback(GameObject source, CombatEventArgs args)
	{
	}

	public virtual void HandleStatsOnPostDamageDealtCallbackComplete()
	{
	}

	public virtual void HandleStatsOnAdded()
	{
	}

	public virtual void HandleStatsOnRemoved()
	{
	}

	public virtual void HandleGameUtilitiesOnCombatEnd(object sender, EventArgs e)
	{
	}

	public virtual void HandleOnMyEffectRemoved()
	{
	}

	private void HandleGameOnResting(object sender, EventArgs e)
	{
		m_cooldownCounter = 0;
	}

	private void HandleGameOnStrongholdTurn()
	{
		m_cooldownCounter = 0;
	}

	protected virtual void CalculateWhyNotReady()
	{
		WhyNotReady = (NotReadyValue)0;
		if (!m_initialized || !IsVisibleOnUI)
		{
			WhyNotReady = NotReadyValue.NotInitialized;
		}
		if (IsInModalRecovery)
		{
			WhyNotReady = NotReadyValue.InModalRecovery;
		}
		if (!m_activated || !Modal)
		{
			if (m_activated || ((bool)Attack && Attack.AlreadyActivated))
			{
				WhyNotReady = NotReadyValue.AlreadyActivated;
			}
			if (IsInCooldownRecovery)
			{
				WhyNotReady = NotReadyValue.InRecovery;
			}
			if (IsInCooldownAtMax)
			{
				WhyNotReady = NotReadyValue.AtMaxPer;
			}
			if (!Passive && (bool)m_ownerStats && !m_ownerStats.CanUseAbilities)
			{
				WhyNotReady = NotReadyValue.AbilitiesDisabled;
			}
			if (CombatOnly && !GameState.InCombat)
			{
				WhyNotReady = NotReadyValue.OnlyInCombat;
			}
			if (NonCombatOnly && GameState.InCombat)
			{
				WhyNotReady = NotReadyValue.OnlyOutsideCombat;
			}
			if (!PrerequisiteData.CheckPrerequisites(base.gameObject, Owner, ActivationPrerequisites, Owner, out var failed_type))
			{
				WhyNotReady = NotReadyValue.FailedPrerequisite;
				WhyNotReadyPrereq = failed_type;
			}
			if (ClearsOnMovement && IsMoving)
			{
				WhyNotReady = NotReadyValue.NotWhileMoving;
			}
			if (CannotActivateWhileInStealth && Stealth.IsInStealthMode(Owner))
			{
				WhyNotReady = NotReadyValue.InStealth;
			}
			if (CannotActivateWhileInvisible && (bool)m_ownerStats && m_ownerStats.IsInvisible)
			{
				WhyNotReady = NotReadyValue.Invisible;
			}
			if (m_attackBase != null && (m_attackBase.CanCancel || m_attackBase.IsBouncing))
			{
				WhyNotReady = NotReadyValue.AlreadyActivated;
			}
			if (m_ownerHealth != null && m_ownerHealth.Dead)
			{
				WhyNotReady = NotReadyValue.Dead;
			}
			if (m_attackBase is AttackPulsedAOE && m_ownerHealth != null && m_ownerHealth.Unconscious)
			{
				WhyNotReady = NotReadyValue.InRecovery;
			}
			TeleportAbility teleportAbility = m_attackBase as TeleportAbility;
			if ((bool)teleportAbility && teleportAbility.TeleportBackTimer > 0f)
			{
				WhyNotReady = NotReadyValue.AlreadyActivated;
			}
		}
	}

	public bool HasActivationPrerequisite(PrerequisiteType ofType)
	{
		if (ActivationPrerequisites == null)
		{
			return false;
		}
		for (int i = 0; i < ActivationPrerequisites.Length; i++)
		{
			if (ActivationPrerequisites[i].Type == ofType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasApplicationPrerequisite(PrerequisiteType ofType)
	{
		if (ApplicationPrerequisites == null)
		{
			return false;
		}
		for (int i = 0; i < ApplicationPrerequisites.Length; i++)
		{
			if (ApplicationPrerequisites[i].Type == ofType)
			{
				return true;
			}
		}
		return false;
	}

	public float RequiresCombatTimeAtLeast()
	{
		if (ActivationPrerequisites == null)
		{
			return 0f;
		}
		float result = 0f;
		for (int i = 0; i < ActivationPrerequisites.Length; i++)
		{
			if (ActivationPrerequisites[i].Type == PrerequisiteType.CombatTimeAtLeast)
			{
				result = Mathf.Max(ActivationPrerequisites[i].Value);
			}
		}
		return result;
	}

	protected virtual bool ShowNormalActivationMessages()
	{
		return !IsTriggeredPassive;
	}

	public string GetStringFlat(bool summary, GameObject owner = null, StatusEffectFormatMode mode = StatusEffectFormatMode.Default, bool animalCompanion = false)
	{
		StringEffects stringEffects = new StringEffects();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(GetString(summary, stringEffects, owner, mode));
		if (stringEffects.Count > 0)
		{
			stringBuilder.AppendLine(GUIUtils.GetText(1604));
			stringBuilder.AppendLine(AttackBase.StringEffects(stringEffects, targets: true));
		}
		return stringBuilder.ToString();
	}

	public string GetString(bool summary, StringEffects stringEffects, GameObject owner = null, StatusEffectFormatMode mode = StatusEffectFormatMode.Default, bool animalCompanion = false)
	{
		m_DisplayAsAnimalCompanion = animalCompanion;
		StringBuilder stringBuilder = new StringBuilder();
		string introBlock = GetIntroBlock(summary);
		if (!string.IsNullOrEmpty(introBlock))
		{
			stringBuilder.AppendLine(introBlock.Trim());
		}
		string effectsString = GetEffectsString(owner ? owner : Owner, mode, stringEffects);
		if (!string.IsNullOrEmpty(effectsString))
		{
			stringBuilder.AppendLine(effectsString);
		}
		m_DisplayAsAnimalCompanion = false;
		return stringBuilder.ToString();
	}

	private string GetIntroBlock(bool summary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string frequencyString = GetFrequencyString();
		bool flag = this is GenericSpell && MasteryLevel == 0;
		if (!string.IsNullOrEmpty(frequencyString) && (summary || !flag))
		{
			stringBuilder.AppendLine(frequencyString);
		}
		string resourceString = GetResourceString();
		if (!string.IsNullOrEmpty(resourceString))
		{
			stringBuilder.AppendLine(resourceString);
		}
		if (!summary)
		{
			string notReadyString = GUIUtils.GetNotReadyString(WhyNotReady, Gender.Neuter);
			if (!Ready && !string.IsNullOrEmpty(notReadyString))
			{
				stringBuilder.AppendLine("[" + NGUITools.EncodeColor(Color.red) + "]" + notReadyString + "[-]");
			}
		}
		return stringBuilder.ToString();
	}

	public string GetTooltipContent(GameObject owner)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		AttackBase attackBase = Attack ?? GetComponent<AttackBase>();
		CharacterStats characterStats = (owner ? owner.GetComponent<CharacterStats>() : null);
		if ((bool)characterStats)
		{
			List<int> list = new List<int>();
			bool[] array = new bool[4];
			if ((bool)attackBase)
			{
				attackBase.GetAllDefenses(characterStats, this, array, list);
			}
			if (UsePrimaryAttack || UseFullAttack)
			{
				Equipment equipment = (owner ? owner.GetComponent<Equipment>() : null);
				if ((bool)equipment && (bool)equipment.PrimaryAttack)
				{
					equipment.PrimaryAttack.GetAllDefenses(characterStats, this, array, list);
				}
			}
			if (UseFullAttack)
			{
				Equipment equipment2 = (owner ? owner.GetComponent<Equipment>() : null);
				if ((bool)equipment2 && (bool)equipment2.SecondaryAttack)
				{
					equipment2.SecondaryAttack.GetAllDefenses(characterStats, this, array, list);
				}
			}
			foreach (StatusEffect cleanedUpStatusEffect in CleanedUpStatusEffects)
			{
				if (StatusEffect.EffectLaunchesAttack(cleanedUpStatusEffect.Params.AffectsStat) && (bool)cleanedUpStatusEffect.Params.AttackPrefab)
				{
					cleanedUpStatusEffect.Params.AttackPrefab.GetAllDefenses(characterStats, this, array, list);
				}
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j])
				{
					stringBuilder3.Append("<" + UIPartyMemberStatIconGetter.GetDefenseTypeSprite((CharacterStats.DefenseType)j) + ">");
				}
			}
			if (stringBuilder3.Length > 0)
			{
				stringBuilder.Append(stringBuilder3.ToString());
				flag = true;
			}
			if (stringBuilder2.Length > 0)
			{
				stringBuilder.AppendGuiFormat(446, stringBuilder2.ToString());
				stringBuilder.AppendLine();
				flag = true;
			}
			if (list.Count > 0)
			{
				stringBuilder.Append(GUIUtils.GetText(369));
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(TextUtils.FuncJoin((int i) => i.ToString(), list, GUIUtils.Comma()));
				flag = true;
			}
		}
		if ((bool)attackBase)
		{
			string keywordsString = attackBase.GetKeywordsString();
			if (!string.IsNullOrEmpty(keywordsString))
			{
				stringBuilder.AppendLine(keywordsString);
			}
		}
		string introBlock = GetIntroBlock(summary: false);
		if (!string.IsNullOrEmpty(introBlock))
		{
			stringBuilder.AppendLine(introBlock.Trim());
			flag = true;
		}
		if (flag)
		{
			stringBuilder.AppendLine();
		}
		if (Description.IsValidString)
		{
			stringBuilder.AppendLine(Description.GetText().Trim());
			stringBuilder.AppendLine();
		}
		stringBuilder.Append("[" + NGUITools.EncodeColor(AttackBase.StringKeyColor) + "]" + GUIUtils.GetText(1796) + "[-]");
		return stringBuilder.ToString().Trim();
	}

	public virtual string GetFrequencyString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Passive)
		{
			stringBuilder.AppendLine(GUIUtils.GetText(1587));
		}
		else if (Modal)
		{
			string text = "";
			if (m_activated)
			{
				text = GUIUtils.Format(1731, GUIUtils.GetText(447));
			}
			stringBuilder.AppendLine(GUIUtils.GetText(77) + text);
		}
		if (CooldownType != 0)
		{
			int num = Mathf.CeilToInt(MaxCooldown);
			string text2 = ((m_cooldownCounter != 0) ? StringUtility.Format(GUIUtils.GetText(451), Math.Max(0, num - m_cooldownCounter), num) : num.ToString());
			if (CooldownType == CooldownMode.PerEncounter)
			{
				stringBuilder.AppendCatchFormatLine(GUIUtils.GetText(449), text2);
			}
			else if (CooldownType == CooldownMode.PerRest)
			{
				stringBuilder.AppendCatchFormatLine(GUIUtils.GetText(450), text2);
			}
			else if (CooldownType == CooldownMode.PerStrongholdTurn)
			{
				stringBuilder.AppendCatchFormatLine(GUIUtils.GetText(1563), text2);
			}
		}
		return stringBuilder.ToString().Trim();
	}

	protected virtual string GetResourceString()
	{
		if (OverrideActivationPrerequisiteDisplay)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		PrerequisiteData[] activationPrerequisites = ActivationPrerequisites;
		for (int i = 0; i < activationPrerequisites.Length; i++)
		{
			string @string = activationPrerequisites[i].GetString();
			if (!string.IsNullOrEmpty(@string))
			{
				stringBuilder.Append(@string);
				stringBuilder.AppendLine();
			}
		}
		activationPrerequisites = ApplicationPrerequisites;
		for (int i = 0; i < activationPrerequisites.Length; i++)
		{
			string string2 = activationPrerequisites[i].GetString();
			if (!string.IsNullOrEmpty(string2))
			{
				stringBuilder.Append(string2);
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public string GetTooltipName(GameObject owner)
	{
		return Name(this);
	}

	public Texture GetTooltipIcon()
	{
		return Icon;
	}

	public string GetEffectsString(GameObject character, StatusEffectFormatMode mode, StringEffects stringEffects)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AttackBase attackBase = m_attackBase;
		if (!attackBase)
		{
			attackBase = GetComponent<AttackBase>();
		}
		if ((bool)attackBase)
		{
			string @string = attackBase.GetString(this, character, stringEffects);
			if (!string.IsNullOrEmpty(@string))
			{
				stringBuilder.AppendLine(@string);
			}
		}
		else if (!Passive)
		{
			stringBuilder.AppendCatchFormatLine(GUIUtils.GetText(443), GUIUtils.GetText(1507));
		}
		if (IsAura)
		{
			string text = TextUtils.FormatBase(FriendlyRadius, AdjustedFriendlyRadius, (float v) => GUIUtils.Format(1533, v.ToString("#0.#")));
			stringBuilder.AppendGuiFormat(1791, text);
			stringBuilder.AppendLine();
		}
		CharacterStats characterStats = (character ? character.GetComponent<CharacterStats>() : null);
		if (!characterStats)
		{
			characterStats = m_ownerStats;
		}
		AttackBase attackBase2 = Attack;
		if ((bool)character)
		{
			Equipment component = character.GetComponent<Equipment>();
			if ((bool)component)
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < ActivationPrerequisites.Length; i++)
				{
					if (ActivationPrerequisites[i].Type == PrerequisiteType.UsingMeleeWeapon)
					{
						flag = true;
					}
					else if (ActivationPrerequisites[i].Type == PrerequisiteType.UsingRangedWeapon)
					{
						flag2 = true;
					}
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				if (UseFullAttack)
				{
					stringBuilder2.Append(GUIUtils.GetText(1615));
					attackBase2 = component.PrimaryAttack;
					if ((bool)attackBase2)
					{
						if (flag && !(attackBase2 is AttackMelee))
						{
							stringBuilder2.AppendGuiFormat(1731, GUIUtils.GetText(1369));
						}
						else if (flag2 && !(attackBase2 is AttackRanged))
						{
							stringBuilder2.AppendGuiFormat(1731, GUIUtils.GetText(1367));
						}
						else
						{
							stringBuilder2.Append(" (");
							DamageInfo damageInfo = new DamageInfo(null, 0f, attackBase2);
							characterStats.AdjustDamageForUi(damageInfo);
							stringBuilder2.Append(damageInfo.GetAdjustedDamageRangeString() + " " + damageInfo.Damage.GetDamageTypeString());
							if ((bool)component.SecondaryAttack)
							{
								damageInfo = new DamageInfo(null, 0f, component.SecondaryAttack);
								characterStats.AdjustDamageForUi(damageInfo);
								stringBuilder2.Append(" + " + damageInfo.GetAdjustedDamageRangeString() + " " + damageInfo.Damage.GetDamageTypeString());
							}
							stringBuilder2.Append(")");
						}
					}
				}
				else if (UsePrimaryAttack)
				{
					stringBuilder2.Append(GUIUtils.GetText(1616));
					attackBase2 = component.PrimaryAttack;
					if ((bool)attackBase2)
					{
						if (flag && !(attackBase2 is AttackMelee))
						{
							stringBuilder2.AppendGuiFormat(1731, GUIUtils.GetText(1369));
						}
						else if (flag2 && !(attackBase2 is AttackRanged))
						{
							stringBuilder2.AppendGuiFormat(1731, GUIUtils.GetText(1367));
						}
						else
						{
							DamageInfo damageInfo2 = new DamageInfo(null, 0f, attackBase2);
							characterStats.AdjustDamageForUi(damageInfo2);
							stringBuilder2.AppendGuiFormat(1731, damageInfo2.GetAdjustedDamageRangeString() + " " + damageInfo2.Damage.GetDamageTypeString());
						}
					}
				}
				if (stringBuilder2.Length > 0)
				{
					AttackBase.AddStringEffect(GetAbilityTarget().GetText(), new AttackBase.AttackEffect(stringBuilder2.ToString(), attackBase2, hostile: true), stringEffects);
				}
			}
		}
		if (!OverrideStatusEffectDisplay)
		{
			AttackBase.TargetType externalTarget = ((FriendlyRadius > 0f) ? AttackBase.TargetType.Friendly : AttackBase.TargetType.All);
			if (CleanedUpStatusEffects != null && CleanedUpStatusEffects.Count > 0)
			{
				StatusEffectParams.ListToStringEffects(CleanedUpStatusEffects.Where((StatusEffect e) => e.AfflictionOrigin == null), characterStats, this, null, null, null, mode, GetAbilityTarget(), "", externalTarget, stringEffects);
			}
			else
			{
				StatusEffectParams.ListToStringEffects(StatusEffects, characterStats, this, null, null, null, mode, GetAbilityTarget(), "", externalTarget, stringEffects);
			}
			AfflictionParams[] afflictions = Afflictions;
			for (int j = 0; j < afflictions.Length; j++)
			{
				afflictions[j].AddStringEffects(GetAbilityTarget().GetText(), characterStats, this, attackBase2, mode, stringEffects);
			}
		}
		string additionalEffects = GetAdditionalEffects(stringEffects, mode, this, character);
		if (!string.IsNullOrEmpty(additionalEffects))
		{
			stringBuilder.AppendLine(additionalEffects);
		}
		if ((bool)characterStats && (bool)attackBase && attackBase.HasStatusEffect(StatusEffect.ModifiedStat.MarkedPrey))
		{
			foreach (GenericAbility activeAbility in characterStats.ActiveAbilities)
			{
				additionalEffects = activeAbility.GetMarkedPreyEffects(stringEffects, this, character);
				if (!string.IsNullOrEmpty(additionalEffects))
				{
					stringBuilder.AppendLine(additionalEffects);
				}
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	protected virtual string FormatAbilityStringEffects(StringEffects stringEffects)
	{
		return AttackBase.StringEffects(stringEffects, targets: true, "\r", "\n");
	}

	public virtual string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		return "";
	}

	public virtual string GetMarkedPreyEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		return "";
	}

	public virtual AttackBase.FormattableTarget GetAbilityTarget()
	{
		if (IsAura && !HasApplicationPrerequisite(PrerequisiteType.NoAlliesInFriendlyRadius))
		{
			return TARGET_FRIENDLY_AURA;
		}
		return GetSelfTarget();
	}

	public AttackBase.FormattableTarget GetSelfTarget()
	{
		if (m_DisplayAsAnimalCompanion)
		{
			return TARGET_ANIMALCOMPANION;
		}
		if (Passive)
		{
			return TARGET_SELF;
		}
		if (this is GenericSpell)
		{
			return TARGET_CASTER;
		}
		return TARGET_USER;
	}

	public void FixUpDefender()
	{
		for (int num = m_effects.Count - 1; num >= 0; num--)
		{
			if (m_effects[num].Params.AffectsStat == StatusEffect.ModifiedStat.AttackSpeed)
			{
				m_effects.RemoveAt(num);
				break;
			}
		}
	}
}

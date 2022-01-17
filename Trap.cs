using System;
using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;

[RequireComponent(typeof(Faction))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(Detectable))]
[AddComponentMenu("Toolbox/Trap")]
public class Trap : Usable, iCanBeDetected
{
	public GenericAbility AbilityOverride;

	public GenericAbility[] AbilityOverrides;

	public bool OneRandomAbility;

	public CharacterStats[] SourceOverride;

	public GameObject[] TargetOverride;

	public bool SuppressTriggerBark;

	public float TriggerDelay = 1f;

	public float SelfDestructTime;

	public int MaxHitCount = 1;

	public int TrapDifficulty = 1;

	public float DisarmRadius = 2f;

	public bool DestroyWhenDisarmed = true;

	[Tooltip("Give this item to the character disarming this trap.")]
	public Item DisarmItem;

	[Tooltip("Most of the time, a player can only place 1 trap at a time. Set this to true for traps used in wall spells, or other spells that drop multiple traps.")]
	public bool AllowMultiple;

	[Tooltip("This only needs to be set if we don't have another lookup method (i.e. DisarmItem).")]
	public InteractablesDatabaseString DisplayName;

	[Tooltip("Is this trap from an item? Certain talents only effect Traps that come from items.")]
	[Persistent]
	public bool FromItem;

	public bool TrapCanPulse;

	[Tooltip("Most traps only activate for someone hostile to their faction. Set to true to ignore faction and activate for anyone.")]
	public bool ActivatesForAnyone;

	[Tooltip("If ActivatesForAnyone is set, setting this stops the caster from activating the trap.")]
	public bool ButNotForCaster;

	protected AudioBank m_audioBank;

	protected ScriptEvent m_ScriptEvent;

	[Persistent]
	private int m_numHits;

	[Persistent]
	private int m_selfDestructStartTime;

	private bool m_isVisible;

	private bool m_isDisarmed;

	private Faction m_trap_faction;

	private OCL m_ocl;

	private PE_Collider2D m_collider2D;

	private Color m_originalLineColor;

	private GameObject m_owner;

	private CharacterStats m_ownerStats;

	private GameObject m_last_user;

	private bool m_trap_initialized;

	private List<AttackBase> m_trap_attack = new List<AttackBase>();

	[Persistent(Persistent.ConversionType.GUIDLink)]
	private GameObject m_triggerVictim;

	private List<GameObject> m_trap_pulse_victims = new List<GameObject>();

	private float m_pulseTimer;

	private float m_post_attack_destruct_time = 3f;

	private bool m_is_wall_trap;

	private ScaledContent m_CachedScaler;

	[Persistent]
	private float m_triggerTimer;

	[Persistent]
	private int m_trapID;

	public float TriggerRadius
	{
		get
		{
			if ((bool)GetComponent<Collider>())
			{
				return Mathf.Max(GetComponent<Collider>().bounds.extents.x, GetComponent<Collider>().bounds.extents.z);
			}
			return 1.5f;
		}
	}

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
			m_ownerStats = (m_owner ? m_owner.GetComponent<CharacterStats>() : null);
		}
	}

	[Persistent]
	public bool Visible
	{
		get
		{
			if (Disarmed)
			{
				return false;
			}
			return m_isVisible;
		}
		set
		{
			m_isVisible = value;
		}
	}

	[Persistent]
	public bool Disarmed
	{
		get
		{
			return m_isDisarmed;
		}
		set
		{
			m_isDisarmed = value;
		}
	}

	public bool CanDisarm
	{
		get
		{
			if ((bool)Owner && IsPlayerOwnedTrap && DisarmItem == null)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsPlayerOwnedTrap
	{
		get
		{
			if ((bool)Owner)
			{
				return Owner.GetComponent<PartyMemberAI>() != null;
			}
			return false;
		}
	}

	public bool IsWallTrap
	{
		get
		{
			return m_is_wall_trap;
		}
		set
		{
			m_is_wall_trap = value;
		}
	}

	public int TrapID
	{
		set
		{
			m_trapID = value;
		}
	}

	public override float UsableRadius => DisarmRadius;

	public override float ArrivalRadius => 0f;

	public override bool IsUsable
	{
		get
		{
			if (m_ocl != null && !Visible)
			{
				return m_ocl.IsUsable;
			}
			if (Visible && base.IsVisible)
			{
				return CanDisarm;
			}
			return false;
		}
	}

	public int GetTrapDifficulty()
	{
		int num = TrapDifficulty;
		if (!m_ownerStats && (bool)m_CachedScaler)
		{
			float scaleMultiplicative = DifficultyScaling.Instance.GetScaleMultiplicative(m_CachedScaler, (DifficultyScaling.ScaleData scaledata) => scaledata.DisarmDifficultyMult);
			num = Mathf.CeilToInt((float)num * scaleMultiplicative);
		}
		return num;
	}

	public string GetDisplayName()
	{
		if (DisplayName.IsValidString)
		{
			return DisplayName.ToString();
		}
		if (DisarmItem != null)
		{
			return DisarmItem.ToString();
		}
		return "*TrapNameError*";
	}

	private void Awake()
	{
		m_CachedScaler = GetComponent<ScaledContent>();
	}

	protected override void Start()
	{
		base.Start();
		m_trap_faction = GetComponent<Faction>();
		m_trap_faction.ShowTooltips = false;
		m_audioBank = GetComponent<AudioBank>();
		m_ScriptEvent = GetComponent<ScriptEvent>();
		m_ocl = GetComponent<OCL>();
		m_collider2D = GetComponent<PE_Collider2D>();
		if (m_collider2D != null)
		{
			if (m_ocl != null)
			{
				m_originalLineColor = m_collider2D.LineColor;
			}
			else
			{
				m_collider2D.RenderLines = Visible;
			}
		}
		if (AbilityOverrides != null)
		{
			AbilityOverrides = ArrayExtender.Compress(AbilityOverrides);
		}
		if (AbilityOverrides != null && AbilityOverrides.Length != 0)
		{
			for (int i = 0; i < AbilityOverrides.Length; i++)
			{
				if (AbilityOverrides[i].GetComponent<AttackBase>() == null)
				{
					Debug.LogError("Trap '" + base.name + "' uses ability '" + AbilityOverride.tag + "' which has no attack!");
				}
			}
		}
		else if (AbilityOverride == null)
		{
			AbilityOverride = GetComponent<GenericAbility>();
			if (AbilityOverride == null)
			{
				Debug.LogError("Trap '" + base.name + "' has no ability to use as its trap activation!");
			}
			else if (AbilityOverride.GetComponent<AttackBase>() == null)
			{
				Debug.LogError("Trap '" + base.name + "' uses ability '" + AbilityOverride.tag + "' which has no attack!");
			}
		}
		if (SourceOverride.Length != TargetOverride.Length)
		{
			Debug.LogError("Trap '" + base.tag + "': number of SourceOverrides must equal number of TargetOverrides!");
			return;
		}
		if (SelfDestructTime > 0f)
		{
			m_selfDestructStartTime = WorldTime.Instance.CurrentTime.TotalSeconds;
		}
		if ((bool)Owner && (bool)Owner.GetComponent<PartyMemberAI>())
		{
			Visible = true;
			if ((bool)m_collider2D && !GetComponentInChildren<ParticleSystem>())
			{
				m_collider2D.RenderLines = true;
				m_collider2D.UsePlayerColor = true;
			}
		}
		m_trap_initialized = true;
		GameState.OnLevelUnload += OnLevelUnloaded;
	}

	public void Restored()
	{
		if (m_collider2D != null && m_ocl == null)
		{
			m_collider2D.RenderLines = Visible;
		}
	}

	private void Update()
	{
		if (m_triggerVictim != null)
		{
			if (m_triggerTimer > 0f)
			{
				m_triggerTimer -= Time.deltaTime;
			}
			if (m_triggerTimer <= 0f)
			{
				ActivateTrapHelper(m_triggerVictim);
				m_triggerTimer = 0f;
				m_triggerVictim = null;
			}
		}
		if (m_trap_pulse_victims.Count > 0)
		{
			if (m_pulseTimer > 0f)
			{
				m_pulseTimer -= Time.deltaTime;
			}
			if (m_pulseTimer <= 0f)
			{
				HandleTrapPulse();
			}
		}
		CheckSelfDestruct();
		if (m_ocl == null && MaxHitCount > 0 && m_numHits >= MaxHitCount && m_triggerTimer <= 0f)
		{
			DestroyTrap();
		}
		if (IsWallTrap && !GameState.InCombat)
		{
			DestroyTrap(2f);
		}
		if (m_collider2D != null && m_ocl != null && !Visible)
		{
			m_collider2D.m_shouldRender = InGameHUD.Instance.HighlightActive || m_collider2D.MouseInPolygon();
		}
	}

	private void OnLevelUnloaded(object sender, EventArgs e)
	{
		if ((bool)Owner && (bool)Owner.GetComponent<PartyMemberAI>())
		{
			Persistence component = GetComponent<Persistence>();
			if ((bool)component)
			{
				PersistenceManager.RemoveObject(component);
			}
			GameUtilities.DestroyImmediate(base.gameObject);
		}
	}

	private void CheckSelfDestruct()
	{
		if (SelfDestructTime > 0f)
		{
			int num = WorldTime.Instance.CurrentTime.TotalSeconds - m_selfDestructStartTime;
			int num2 = (int)(SelfDestructTime * (float)WorldTime.Instance.GameSecondsPerRealSecond);
			if (num > num2)
			{
				DestroyTrap(2f);
			}
		}
	}

	public void DestroyTrap()
	{
		DestroyTrap(m_post_attack_destruct_time);
	}

	private void DestroyTrap(float delay)
	{
		Persistence component = ComponentUtils.GetComponent<Persistence>(base.gameObject);
		if ((bool)component)
		{
			if (Owner == null)
			{
				component.SetForDestroy();
			}
			else
			{
				PersistenceManager.RemoveObject(component);
			}
		}
		GameUtilities.ShutDownLoopingEffect(base.gameObject);
		GameUtilities.Destroy(base.gameObject, delay);
	}

	private void HandleTrapPulse()
	{
		for (int num = m_trap_pulse_victims.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = m_trap_pulse_victims[num];
			if (gameObject == null)
			{
				m_trap_pulse_victims.RemoveAt(num);
			}
			else
			{
				Health component = gameObject.GetComponent<Health>();
				if (component == null || component.Dead)
				{
					m_trap_pulse_victims.RemoveAt(num);
				}
			}
		}
		if (m_trap_pulse_victims.Count == 0)
		{
			m_pulseTimer = 0f;
			return;
		}
		for (int i = 0; i < m_trap_pulse_victims.Count; i++)
		{
			ActivateTrap(m_trap_pulse_victims[i], fromPulse: true);
		}
		m_pulseTimer = AttackData.Instance.TrapIntervalRate;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == null || (MaxHitCount > 0 && m_numHits >= MaxHitCount) || m_ocl != null || !other.GetComponent<Health>())
		{
			return;
		}
		if (m_collider2D != null)
		{
			if (!m_collider2D.PointInPolygon(other.transform.position))
			{
				return;
			}
		}
		else if (IsWallTrap)
		{
			CharacterStats component = other.gameObject.GetComponent<CharacterStats>();
			if (component != null)
			{
				if (component.HasTrapCooldownTimer(m_trapID))
				{
					return;
				}
				component.SetTrapCooldownTimer(m_trapID);
			}
		}
		if (TrapCanPulse)
		{
			m_trap_pulse_victims.Add(other.gameObject);
		}
		ActivateTrap(other.gameObject);
	}

	public void OnTriggerExit(Collider other)
	{
		if (TrapCanPulse)
		{
			m_trap_pulse_victims.Remove(other.gameObject);
		}
	}

	public bool IsPointInTrap(Vector3 position)
	{
		if (m_collider2D != null && m_collider2D.PointInPolygon(position))
		{
			return true;
		}
		return false;
	}

	public List<Vector3> GetWorldVertices()
	{
		List<Vector3> list = new List<Vector3>();
		if (m_collider2D != null)
		{
			foreach (Vector3 vert in m_collider2D.VertList)
			{
				list.Add(vert + m_collider2D.transform.position);
			}
			return list;
		}
		return list;
	}

	private bool CanActivate(GameObject victim)
	{
		if (!m_trap_initialized)
		{
			Debug.LogError("Cannot activate uninitialized trap!");
			return false;
		}
		if (Disarmed || m_triggerVictim != null)
		{
			return false;
		}
		if (MaxHitCount > 0 && m_numHits >= MaxHitCount)
		{
			return false;
		}
		if (GameUtilities.IsAnimalCompanion(victim))
		{
			return false;
		}
		if (GameUtilities.IsPet(victim))
		{
			return false;
		}
		if (m_ocl != null)
		{
			return true;
		}
		if (ActivatesForAnyone && (!ButNotForCaster || victim != Owner))
		{
			return true;
		}
		Faction component = victim.GetComponent<Faction>();
		if (component != null && (component.IsHostile(base.gameObject) || m_trap_faction.IsHostile(victim)))
		{
			return true;
		}
		return false;
	}

	public void ActivateTrap(GameObject victim)
	{
		ActivateTrap(victim, fromPulse: false);
	}

	public void ActivateTrap(GameObject victim, bool fromPulse)
	{
		if (!CanActivate(victim))
		{
			return;
		}
		if (!SuppressTriggerBark)
		{
			UIHealthstringManager.Instance.ShowWarning(GUIUtils.GetText(1578), victim, 2f);
		}
		AudioBank component = GetComponent<AudioBank>();
		if (component != null)
		{
			component.PlayFrom("Triggered");
		}
		if (MaxHitCount > 0)
		{
			m_numHits++;
		}
		PartyMemberAI component2 = victim.GetComponent<PartyMemberAI>();
		if (component2 != null && Stealth.IsInStealthMode(component2.gameObject))
		{
			Stealth.SetInStealthMode(component2.gameObject, inStealth: false);
		}
		if (m_ocl != null)
		{
			Disarmed = true;
			if (m_collider2D != null)
			{
				m_collider2D.LineColor = m_originalLineColor;
				m_collider2D.RenderLines = false;
			}
		}
		if (TriggerDelay > 0f)
		{
			m_triggerVictim = victim;
			m_triggerTimer = TriggerDelay;
		}
		else
		{
			ActivateTrapHelper(victim);
		}
	}

	private void ActivateTrapHelper(GameObject victim)
	{
		if (SourceOverride.Length == 0)
		{
			LaunchTrapAttack(null, null, victim);
		}
		else
		{
			for (int i = 0; i < SourceOverride.Length; i++)
			{
				LaunchTrapAttack(SourceOverride[i], TargetOverride[i], victim);
			}
		}
		if (m_ScriptEvent != null)
		{
			if (victim.GetComponent<PartyMemberAI>() != null)
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnter);
				if (Stealth.IsInStealthMode(victim))
				{
					m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnterWhileStealthed);
				}
				else
				{
					m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnterWhileNonStealthed);
				}
			}
			SpecialCharacterInstanceID.Add(victim, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnTriggered);
		}
		Detectable component = GetComponent<Detectable>();
		if ((bool)component)
		{
			component.IgnoreDetection();
		}
	}

	private void LaunchTrapAttack(CharacterStats source, GameObject target, GameObject victim)
	{
		if (OneRandomAbility)
		{
			LaunchTrapAttack(source, target, victim, AbilityOverrides[OEIRandom.Index(AbilityOverrides.Length)]);
		}
		else
		{
			for (int i = 0; i < AbilityOverrides.Length; i++)
			{
				LaunchTrapAttack(source, target, victim, AbilityOverrides[i]);
			}
		}
		LaunchTrapAttack(source, target, victim, AbilityOverride);
	}

	private void LaunchTrapAttack(CharacterStats source, GameObject target, GameObject victim, GenericAbility ability)
	{
		if (ability == null)
		{
			return;
		}
		GenericAbility genericAbility = UnityEngine.Object.Instantiate(ability);
		genericAbility.ForceInit();
		if (source == null)
		{
			genericAbility.transform.parent = base.transform;
			genericAbility.transform.position = base.transform.position;
			genericAbility.Owner = base.gameObject;
		}
		else
		{
			genericAbility.transform.parent = source.transform;
			genericAbility.transform.position = source.transform.position;
			genericAbility.Owner = source.gameObject;
		}
		if (genericAbility is GenericSpell)
		{
			(genericAbility as GenericSpell).NeedsGrimoire = false;
		}
		AttackBase component = genericAbility.GetComponent<AttackBase>();
		component.ForceInit();
		component.Owner = base.gameObject;
		genericAbility.EffectType = GenericAbility.AbilityType.Trap;
		if (m_ownerStats != null)
		{
			component.AccuracyBonus += m_ownerStats.CalculateAccuracy(component, victim);
			if (FromItem)
			{
				component.AccuracyBonus += m_ownerStats.TrapAccuracyBonus;
				component.AccuracyBonus += m_ownerStats.StatTrapAccuracyBonus;
				if (component.DamageData.DoesDamage)
				{
					component.DamageMultiplier += m_ownerStats.TrapDamageOrDurationMult - 1f;
				}
				else
				{
					foreach (StatusEffectParams statusEffect in component.StatusEffects)
					{
						statusEffect.Duration *= m_ownerStats.TrapDamageOrDurationMult;
					}
				}
			}
		}
		else if ((bool)m_CachedScaler)
		{
			component.AccuracyBonus += DifficultyScaling.Instance.GetScaleAdditive(m_CachedScaler, (DifficultyScaling.ScaleData scaleData) => scaleData.TrapAccuracyBonus);
			component.DamageMultiplier += DifficultyScaling.Instance.GetScaleMultiplicative(m_CachedScaler, (DifficultyScaling.ScaleData scaleData) => scaleData.TrapDamageMult) - 1f;
			float scaleMultiplicative = DifficultyScaling.Instance.GetScaleMultiplicative(m_CachedScaler, (DifficultyScaling.ScaleData scaleData) => scaleData.TrapEffectMult);
			foreach (StatusEffectParams statusEffect2 in component.StatusEffects)
			{
				statusEffect2.Duration *= scaleMultiplicative;
			}
		}
		component.AttackVariation = -1;
		component.SkipAnimation = true;
		if (target == null)
		{
			genericAbility.Activate(victim);
		}
		else
		{
			component.Launch(target.transform.position, null);
		}
		m_trap_attack.Add(component);
		if (component is AttackPulsedAOE)
		{
			m_post_attack_destruct_time = (component as AttackPulsedAOE).PulseDuration();
		}
		if ((bool)GameState.Instance)
		{
			GameState.Instance.ForceIsInTrapTriggeredCombat();
		}
	}

	public bool Disarm(GameObject user)
	{
		UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(1581), base.gameObject, 2f);
		Disarmed = true;
		if (m_collider2D != null)
		{
			m_collider2D.RenderLines = false;
			if (m_ocl != null)
			{
				m_collider2D.LineColor = m_originalLineColor;
			}
		}
		if (m_audioBank != null)
		{
			m_audioBank.PlayFrom("Disarm");
		}
		if (m_ScriptEvent != null)
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnDisarmed);
		}
		if (DestroyWhenDisarmed)
		{
			Destruct();
		}
		if (DisarmItem != null && (bool)user.GetComponent<PartyMemberAI>() && (bool)GameState.s_playerCharacter)
		{
			PlayerInventory component = GameState.s_playerCharacter.Inventory.GetComponent<PlayerInventory>();
			if (component != null)
			{
				component.AddItemAndLog(DisarmItem, 1, user);
			}
		}
		if (!Owner || !Owner.GetComponent<PartyMemberAI>())
		{
			int num = BonusXpManager.Instance.DisarmTrapXpModifier * TrapDifficulty;
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1649), GetTrapDifficulty(), num * PartyHelper.NumPartyMembers), Color.yellow);
			PartyHelper.AssignXPToParty(num, printMessage: false);
		}
		return true;
	}

	public void Destruct()
	{
		SelfDestructTime = 0.01f;
		m_selfDestructStartTime = WorldTime.Instance.CurrentTime.TotalSeconds;
	}

	public void OnDetection()
	{
		UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(1580), base.gameObject, 2f);
		Visible = true;
		if (m_collider2D != null)
		{
			m_collider2D.RenderLines = true;
			if (m_ocl != null)
			{
				m_collider2D.LineColor = Color.red;
			}
		}
	}

	protected override void OnDestroy()
	{
		GameState.OnLevelUnload -= OnLevelUnloaded;
		Owner = null;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override bool Use(GameObject user)
	{
		FireUseAudio();
		if (m_ocl == null)
		{
			TrapUse(user, fromOCL: false);
			return false;
		}
		return false;
	}

	public void TrapUseFromOCL(GameObject user)
	{
		if (m_ocl == null)
		{
			return;
		}
		if (Disarmed)
		{
			m_ocl.Open(user, ignoreLock: false);
			return;
		}
		if (Visible)
		{
			TrapUse(user, fromOCL: true);
			return;
		}
		ActivateTrap(user);
		if (!Disarmed)
		{
			m_ocl.Open(user, ignoreLock: false);
		}
	}

	private void OnConfirmDialog(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (!Disarmed && result == UIMessageBox.Result.AFFIRMATIVE)
		{
			ActivateTrap(m_last_user);
			if (!Disarmed)
			{
				m_ocl.Open(m_last_user, ignoreLock: false);
			}
		}
	}

	public void TrapUse(GameObject user, bool fromOCL)
	{
		AIController component = user.GetComponent<AIController>();
		if (component != null && CanDisarm)
		{
			DisarmTrap disarmTrap = AIStateManager.StatePool.Allocate<DisarmTrap>();
			disarmTrap.Trap = this;
			disarmTrap.FromOCL = fromOCL;
			component.StateManager.PushState(disarmTrap);
		}
	}

	public bool TriggerDisarm(GameObject user, bool fromOCL)
	{
		if (MaxHitCount > 0 && m_numHits >= MaxHitCount)
		{
			return false;
		}
		CharacterStats component = user.GetComponent<CharacterStats>();
		bool result = false;
		if ((bool)component)
		{
			if (GetTrapDifficulty() <= component.CalculateSkill(CharacterStats.SkillType.Mechanics) || IsPlayerOwnedTrap)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(353));
				Disarm(user);
				result = true;
			}
			else
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(354));
				if (fromOCL)
				{
					m_last_user = user;
					UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, GUIUtils.GetText(377), GUIUtils.GetText(378));
					uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnConfirmDialog));
				}
			}
		}
		return result;
	}
}

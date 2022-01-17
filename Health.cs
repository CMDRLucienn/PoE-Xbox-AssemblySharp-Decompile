using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Health : MonoBehaviour
{
	public enum DeathStatusType
	{
		Invalid = -1,
		KnockedOut,
		Death
	}

	public static bool NoDamage;

	[Persistent]
	public bool ShouldDecay = true;

	public float HitReactInterval = 1.3f;

	public ObjectList GibList;

	[Tooltip("The visual effect attached to giblets if dmg taken was Slash, Crush, or Pierce.")]
	public GameObject GibletEffect;

	[Tooltip("If not set will get AttackData's DefaultBloodFx.")]
	public GameObject BloodEffect;

	public bool CanBeTargeted = true;

	public bool NeverGib;

	public bool AlwaysGib;

	public static bool BloodyMess;

	public static bool HideGrazes;

	protected float m_staminaPerSecond = CharacterStats.NormalStaminaRechargeRate;

	protected float m_currentStamina = 100f;

	protected float m_currentHealth = 100f;

	protected bool m_restoredStamina;

	protected bool m_restoredHealth;

	private float m_lastBaseMaxHealth;

	private float m_lastBaseMaxStamina;

	protected bool m_attackedByPlayer;

	protected CharacterStats m_stats;

	private float m_reactionTimer;

	private float m_canRegenTimer;

	private float m_injuredTimer;

	[Persistent]
	private float m_checkPartyDeathTimer;

	private bool m_takesDamage = true;

	private bool m_targetable = true;

	[Persistent]
	private bool m_needs_current_values = true;

	private bool m_needComp = true;

	private bool m_shouldGib;

	private bool m_playsHitReactions = true;

	private bool m_destroyed;

	private const float UNCONSCIOUSNESS_DELAY = 3f;

	private const float mediumSoundDelay = 5f;

	private bool m_DecaySuspended;

	[Persistent]
	private float m_maxStaminaOverride = float.MinValue;

	[Persistent]
	private float m_maxHealthOverride = float.MinValue;

	public bool CanGib
	{
		get
		{
			if (NeverGib)
			{
				return false;
			}
			if (!GameState.Mode.Option.GetOption(GameOption.BoolOption.GIBS))
			{
				return false;
			}
			if (MaimAvailable())
			{
				return false;
			}
			return true;
		}
	}

	public bool ShouldGib
	{
		get
		{
			if (m_shouldGib)
			{
				return CanGib;
			}
			return false;
		}
		set
		{
			m_shouldGib = value;
			if (ShouldGib && this.OnGibbed != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Gibbed;
				this.OnGibbed(base.gameObject, gameEventArgs);
			}
		}
	}

	public bool m_isAnimalCompanion { get; set; }

	public bool PlaysHitReactions
	{
		get
		{
			return m_playsHitReactions;
		}
		set
		{
			m_playsHitReactions = value;
		}
	}

	public bool CanDie
	{
		get
		{
			if (m_isAnimalCompanion)
			{
				return false;
			}
			if (m_stats != null && m_stats.DeathPrevented > 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool ShowDead
	{
		get
		{
			if (!Dead)
			{
				if (Unconscious && ShouldDecay && !m_DecaySuspended)
				{
					return CanDie;
				}
				return false;
			}
			return true;
		}
	}

	public bool Dead
	{
		get
		{
			return m_currentHealth <= 0f;
		}
		set
		{
			if (value)
			{
				m_currentHealth = 0f;
			}
			else
			{
				m_currentHealth = ((m_stats != null) ? m_stats.Health : 100f);
			}
		}
	}

	public bool Unconscious
	{
		get
		{
			if (MaxStamina == 0f)
			{
				return false;
			}
			return m_currentStamina <= 0f;
		}
	}

	public bool Targetable
	{
		get
		{
			if (m_targetable && !Unconscious && !Dead && CanBeTargeted && base.gameObject.activeInHierarchy)
			{
				AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
				if (aIController != null)
				{
					PartyMemberAI partyMemberAI = aIController as PartyMemberAI;
					if (!aIController.IsBusy)
					{
						if (!(partyMemberAI == null))
						{
							return !partyMemberAI.IsBusy;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		set
		{
			m_targetable = value;
		}
	}

	public float MaxStamina
	{
		get
		{
			if (m_maxStaminaOverride > 0f)
			{
				return m_maxStaminaOverride;
			}
			if (m_stats == null)
			{
				m_stats = GetComponent<CharacterStats>();
			}
			if ((bool)m_stats)
			{
				return m_stats.Stamina;
			}
			return 100f;
		}
		set
		{
			m_maxStaminaOverride = value;
		}
	}

	public float MaxHealth
	{
		get
		{
			if (m_maxHealthOverride > 0f)
			{
				return m_maxHealthOverride;
			}
			if (m_stats == null)
			{
				m_stats = GetComponent<CharacterStats>();
			}
			if ((bool)m_stats)
			{
				return m_stats.Health;
			}
			return 100f;
		}
		set
		{
			m_maxHealthOverride = value;
		}
	}

	public float BaseMaxStamina
	{
		get
		{
			if (m_stats == null)
			{
				m_stats = GetComponent<CharacterStats>();
			}
			if ((bool)m_stats)
			{
				return m_stats.BaseMaxStamina;
			}
			return 100f;
		}
	}

	[Persistent]
	public float CurrentStamina
	{
		get
		{
			return m_currentStamina;
		}
		set
		{
			if (GameState.IsLoading)
			{
				if (value == 0f)
				{
					return;
				}
				m_restoredStamina = true;
			}
			if (!Dead || !CanDie)
			{
				bool unconscious = Unconscious;
				if (GameState.IsLoading)
				{
					m_currentStamina = value;
				}
				else if (!unconscious)
				{
					m_currentStamina = Mathf.Min(CurrentHealth, value);
				}
				if (!unconscious && m_currentStamina <= 0f)
				{
					m_currentStamina = 0f;
					HandleUnconscious(null);
				}
			}
		}
	}

	[Persistent]
	public float CurrentHealth
	{
		get
		{
			return m_currentHealth;
		}
		set
		{
			if (GameState.IsLoading)
			{
				if (value == 0f)
				{
					return;
				}
				m_restoredHealth = true;
			}
			bool dead = Dead;
			if (m_isAnimalCompanion)
			{
				if (value > m_currentHealth)
				{
					m_currentHealth = value;
				}
			}
			else if (!dead)
			{
				m_currentHealth = value;
			}
			if (m_currentHealth <= 0f)
			{
				if (!dead && CanDie)
				{
					m_currentHealth = 0f;
					HandleDeath(null);
				}
				else if (!CanDie)
				{
					m_currentHealth = 1f;
				}
			}
		}
	}

	public bool HealthVisible => !m_stats.HasStatusEffectOfType(StatusEffect.ModifiedStat.HidesHealthStamina);

	public bool Uninjured => CurrentStamina / MaxStamina >= InGameHUD.Instance.CharacterHealthStages[0].HealthRatioMin;

	public float HealthPercentage
	{
		get
		{
			return CurrentHealth / MaxHealth;
		}
		set
		{
			CurrentHealth = value * MaxHealth;
		}
	}

	public float StaminaPercentage
	{
		get
		{
			return CurrentStamina / MaxStamina;
		}
		set
		{
			CurrentStamina = value * MaxStamina;
		}
	}

	public bool TakesDamage
	{
		get
		{
			return m_takesDamage;
		}
		set
		{
			m_takesDamage = value;
		}
	}

	public event GameInputEventHandle OnDamaged;

	public event GameInputEventHandle OnDamageDealt;

	public event GameInputEventHandle OnHealed;

	public event GameInputEventHandle OnDeath;

	public event GameInputEventHandle OnKill;

	public event GameInputEventHandle OnUnconscious;

	public event GameInputEventHandle OnRevived;

	public event GameInputEventHandle OnGibbed;

	private void Start()
	{
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.OnTargetableToggled += anim_OnTargetableToggled;
		}
		CharacterStats component2 = GetComponent<CharacterStats>();
		if ((bool)component2)
		{
			component2.OnLevelUp += OnLevelUp;
		}
		if (BloodEffect == null && (bool)AttackData.Instance)
		{
			BloodEffect = AttackData.Instance.DefaultBlood;
		}
		if (GibList == null && !NeverGib)
		{
			GibList = AttackData.GetDefaultGibList(component2.CharacterRace);
		}
		m_isAnimalCompanion = GameUtilities.IsAnimalCompanion(base.gameObject);
	}

	private void OnDestroy()
	{
		CharacterStats component = GetComponent<CharacterStats>();
		if ((bool)component)
		{
			component.OnLevelUp -= OnLevelUp;
		}
		GameState.OnCombatEnd -= HandleMaimOnCombatEnd;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelUp(object sender, EventArgs e)
	{
		CurrentHealth = MaxHealth;
		CurrentStamina = MaxStamina;
	}

	private void anim_OnTargetableToggled(object sender, EventArgs e)
	{
		Targetable = !Targetable;
	}

	public void Restored()
	{
		m_isAnimalCompanion = GameUtilities.IsAnimalCompanion(base.gameObject);
		if (m_isAnimalCompanion)
		{
			if (m_stats == null)
			{
				m_stats = GetComponent<CharacterStats>();
			}
			m_currentHealth = MaxHealth;
			m_currentStamina = MaxStamina;
		}
		if (m_currentStamina <= 0f)
		{
			if (ShouldDecay || m_currentHealth <= 0f)
			{
				GameUtilities.Destroy(base.gameObject);
			}
			else
			{
				HandleUnconscious(null);
			}
		}
	}

	private void Update()
	{
		float num = 100f;
		float num2 = 100f;
		if (m_needComp)
		{
			m_stats = GetComponent<CharacterStats>();
			m_needComp = false;
		}
		if (m_stats != null)
		{
			num = MaxStamina;
			num2 = MaxHealth;
			if (CurrentHealth > num2)
			{
				CurrentHealth = num2;
			}
			if (CurrentStamina > num)
			{
				CurrentStamina = num;
			}
			m_staminaPerSecond = m_stats.TotalStaminaRate;
			if (m_needs_current_values)
			{
				m_currentStamina = num;
				m_currentHealth = num2;
				m_needs_current_values = false;
			}
			float baseMaxStaminaWithoutStat = m_stats.BaseMaxStaminaWithoutStat;
			float baseMaxHealthWithoutStat = m_stats.BaseMaxHealthWithoutStat;
			if (m_lastBaseMaxStamina > 0f && baseMaxStaminaWithoutStat > m_lastBaseMaxStamina)
			{
				CurrentStamina += (baseMaxStaminaWithoutStat - m_lastBaseMaxStamina) * m_stats.StatHealthStaminaMultiplier;
			}
			if (m_lastBaseMaxHealth > 0f && baseMaxHealthWithoutStat > m_lastBaseMaxHealth)
			{
				CurrentHealth += (baseMaxHealthWithoutStat - m_lastBaseMaxHealth) * m_stats.StatHealthStaminaMultiplier;
			}
			m_lastBaseMaxStamina = baseMaxStaminaWithoutStat;
			m_lastBaseMaxHealth = baseMaxHealthWithoutStat;
		}
		m_reactionTimer -= Time.deltaTime;
		if (m_canRegenTimer > 0f && !GameState.InCombat && !Dead)
		{
			m_canRegenTimer -= Time.deltaTime;
		}
		m_injuredTimer -= Time.deltaTime;
		if (m_checkPartyDeathTimer > 0f)
		{
			m_checkPartyDeathTimer -= Time.deltaTime;
			if (m_checkPartyDeathTimer <= 0f)
			{
				CheckPartyDeath();
				m_checkPartyDeathTimer = 0f;
			}
		}
		if (m_currentStamina >= num)
		{
			m_currentStamina = num;
			m_canRegenTimer = CharacterStats.StaminaRechargeDelay;
		}
		else if (ShouldDecay && !m_DecaySuspended && Unconscious && !Dead && CanDie)
		{
			m_currentHealth = 0f;
			HandleDeath(null);
		}
		else if (m_canRegenTimer <= 0f && !Dead && !Unconscious)
		{
			AddStamina(m_staminaPerSecond * Time.deltaTime, report: false);
		}
		if (!GameState.Instance || !GameState.Instance.CheatsEnabled)
		{
			return;
		}
		bool noDamage = NoDamage;
		NoDamage = false;
		if (GameInput.GetKeyDown(KeyCode.H) && GameInput.GetShiftkey() && GameCursor.CharacterUnderCursor == base.gameObject)
		{
			if (Unconscious || Dead)
			{
				OnRevive();
			}
			AddHealth(num2 - m_currentHealth);
			AddStamina(num - m_currentStamina);
		}
		if (GameInput.GetKeyDown(KeyCode.K) && GameCursor.CharacterUnderCursor == base.gameObject)
		{
			if (GameInput.GetShiftkey())
			{
				AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
				if (aIController != null)
				{
					GameEventArgs gameEventArgs = new GameEventArgs();
					gameEventArgs.Type = GameEventType.HitReact;
					gameEventArgs.GameObjectData = new GameObject[1] { GameState.s_playerCharacter.gameObject };
					aIController.OnEvent(gameEventArgs);
				}
			}
			else
			{
				ApplyDamageDirectlyAsPlayer(m_currentStamina * 10f);
			}
		}
		if (GameInput.GetKeyDown(KeyCode.U) && GameCursor.CharacterUnderCursor == base.gameObject)
		{
			ApplyStaminaChangeDirectly(0f - m_currentStamina, applyIfDead: false);
		}
		NoDamage = noDamage;
	}

	public void ReportDamage(string target, float damage, GameObject source, StatusEffect sourceEffect)
	{
		Report(122, target, damage, source, sourceEffect);
	}

	public void ReportStamina(string target, float amount, GameObject source, StatusEffect sourceEffect)
	{
		Report((amount >= 0f) ? 128 : 122, target, Mathf.Abs(amount), source, sourceEffect);
	}

	public void ReportHealth(string target, float amount, GameObject source, StatusEffect sourceEffect)
	{
		Report((amount >= 0f) ? 126 : 122, target, Mathf.Abs(amount), source, sourceEffect);
	}

	private void Report(int stringId, string target, float absamount, GameObject source, StatusEffect sourceEffect)
	{
		try
		{
			if (source != null)
			{
				string text = "*SourceError*";
				if (sourceEffect != null)
				{
					text = sourceEffect.BundleName;
					if (string.IsNullOrEmpty(text) && (bool)sourceEffect.AbilityOrigin)
					{
						text = GenericAbility.Name(sourceEffect.AbilityOrigin);
					}
				}
				if (string.IsNullOrEmpty(text) && (bool)source)
				{
					text = CharacterStats.NameColored(source);
				}
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(stringId + 1), target, absamount.ToString("####0"), text), Color.white);
			}
			else
			{
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(stringId), target, absamount.ToString("####0")), Color.white);
			}
		}
		catch (Exception ex)
		{
			Console.AddMessage("Health.Report Error: " + ex.Message, Color.red);
			Debug.LogException(ex, this);
		}
	}

	private float AdjustHealing(float amount, GameObject healer)
	{
		return amount * GetHealingMultiplier(healer);
	}

	public float GetHealingMultiplier(GameObject healer)
	{
		if ((bool)healer)
		{
			return GetHealingMultiplier(healer.GetComponent<CharacterStats>());
		}
		return 1f;
	}

	public float GetHealingMultiplier(CharacterStats healer)
	{
		float num = 1f;
		if ((bool)healer)
		{
			num *= healer.StatDamageHealMultiplier;
			num = ((!(num > 0f)) ? (num * healer.GetDifficultyDamageMultiplier(this)) : (num * healer.BonusHealingGivenMult));
		}
		if ((bool)m_stats)
		{
			num *= m_stats.BonusHealMult;
		}
		return num;
	}

	public void ApplyHealthChangeDirectly(float amount, bool applyIfDead)
	{
		ApplyHealthChangeDirectly(amount, null, null, applyIfDead);
	}

	public void ApplyHealthChangeDirectly(float amount, GameObject healer, StatusEffect healerEffect, bool applyIfDead)
	{
		if (Dead && !applyIfDead)
		{
			return;
		}
		amount = AdjustHealing(amount, healer);
		float currentHealth = m_currentHealth;
		if (m_isAnimalCompanion && amount < 0f)
		{
			amount = 0f;
		}
		m_currentHealth += amount;
		if (m_currentHealth > MaxHealth)
		{
			m_currentHealth = MaxHealth;
			amount = m_currentHealth - currentHealth;
			m_attackedByPlayer = false;
		}
		else if (m_currentHealth < 0f)
		{
			m_currentHealth = 0f;
			amount = m_currentHealth - currentHealth;
		}
		if (m_currentHealth <= 0f && amount < 0f)
		{
			if (CanDie)
			{
				m_currentStamina = 0f;
				HandleUnconscious(healer);
				HandleDeath(healer);
			}
			else
			{
				m_currentHealth = 1f;
			}
		}
		if (amount != 0f)
		{
			UIHealthstringManager.Instance.ShowNumber(0f - amount, base.gameObject);
		}
	}

	public void TransferStaminaFrom(GameObject caster, Health other, float amount, bool applyIfDead)
	{
		float num = MaxStamina - CurrentStamina;
		float currentStamina = other.CurrentStamina;
		float num2 = other.ApplyStaminaChangeDirectly(0f - Mathf.Min(num, currentStamina, amount), caster, applyIfDead, allowAdjustment: false);
		ApplyStaminaChangeDirectly(0f - num2, caster, applyIfDead, allowAdjustment: false);
	}

	public float ApplyStaminaChangeDirectly(float amount, bool applyIfDead)
	{
		return ApplyStaminaChangeDirectly(amount, null, applyIfDead);
	}

	public float ApplyStaminaChangeDirectly(float amount, GameObject healer, bool applyIfDead, bool allowAdjustment = true)
	{
		if ((Dead || Unconscious) && !applyIfDead)
		{
			return 0f;
		}
		if (allowAdjustment)
		{
			amount = AdjustHealing(amount, healer);
		}
		if (m_currentStamina > MaxStamina)
		{
			m_currentStamina = MaxStamina;
		}
		if (m_currentStamina > CurrentHealth)
		{
			m_currentStamina = CurrentHealth;
		}
		if (Unconscious && amount + m_currentStamina > 0f)
		{
			OnRevive();
		}
		float currentStamina = m_currentStamina;
		m_currentStamina += amount;
		if (m_currentStamina > MaxStamina)
		{
			m_currentStamina = MaxStamina;
			amount = m_currentStamina - currentStamina;
		}
		else if (m_currentStamina < 0f)
		{
			m_currentStamina = 0f;
			amount = m_currentStamina - currentStamina;
		}
		if (m_currentStamina > CurrentHealth)
		{
			m_currentStamina = CurrentHealth;
			amount = m_currentStamina - currentStamina;
		}
		if (m_currentStamina <= 0f && amount < 0f)
		{
			AddInjury(DamagePacket.DamageType.All);
			HandleUnconscious(healer);
		}
		if (amount != 0f)
		{
			if (amount > 0f && this.OnHealed != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Healed;
				gameEventArgs.FloatData = new float[2];
				gameEventArgs.FloatData[0] = amount;
				gameEventArgs.FloatData[1] = currentStamina;
				this.OnHealed(base.gameObject, gameEventArgs);
			}
			UIHealthstringManager.Instance.ShowNumber(0f - amount, base.gameObject);
		}
		if (amount > 0f)
		{
			GameObject gameObject = GameUtilities.FindAnimalCompanion(base.gameObject);
			if ((bool)gameObject)
			{
				SharedStats component = gameObject.GetComponent<SharedStats>();
				if ((bool)component)
				{
					component.NotifyHeal(amount);
				}
			}
		}
		return amount;
	}

	public void ApplyDamageDirectlyAsPlayer(float amount)
	{
		m_attackedByPlayer = true;
		ApplyDamageDirectly(amount, DamagePacket.DamageType.Raw, null, null);
	}

	public void ApplyDamageDirectly(float amount)
	{
		ApplyDamageDirectly(amount, DamagePacket.DamageType.Raw, null, null);
	}

	public void ApplyDamageDirectly(float amount, DamagePacket.DamageType damageType, GameObject source, StatusEffect sourceEffect)
	{
		if (Dead || Unconscious || !CanBeTargeted)
		{
			return;
		}
		Health health = null;
		if (amount < 0f)
		{
			amount = 0f - amount;
			Debug.LogWarning("Script Error: DoDamage(float) was passed a negative value. Damage should be positive.", base.gameObject);
		}
		if (source != null)
		{
			health = source.GetComponent<Health>();
			CharacterStats component = source.GetComponent<CharacterStats>();
			if (component != null)
			{
				amount *= component.StatDamageHealMultiplier;
				amount *= component.GetDifficultyDamageMultiplier(this);
			}
		}
		if (m_stats != null)
		{
			amount = m_stats.AdjustDamageByDTDR(amount, damageType, null, source, 0.25f);
		}
		float num = 100f;
		if ((bool)m_stats)
		{
			num = m_stats.Stamina;
		}
		if (m_currentStamina >= num)
		{
			m_canRegenTimer = CharacterStats.StaminaRechargeDelay;
		}
		float currentStamina = m_currentStamina;
		float currentHealth = m_currentHealth;
		if (TakesDamage && !NoDamage)
		{
			if (num > 0f)
			{
				m_currentStamina -= amount;
				if (!m_isAnimalCompanion)
				{
					m_currentHealth -= amount;
				}
			}
			else if (!m_isAnimalCompanion)
			{
				m_currentHealth -= amount;
			}
			TryPlayInjuredSound(currentStamina, currentHealth);
		}
		if (Unconscious && !Dead)
		{
			AddInjury(damageType);
			HandleUnconscious(source);
		}
		if (ShowDead)
		{
			if (CanDie)
			{
				m_currentHealth = 0f;
				HandleDeath(source);
				if (health != null && health.OnKill != null)
				{
					GameEventArgs gameEventArgs = new GameEventArgs();
					gameEventArgs.Type = GameEventType.Killed;
					gameEventArgs.GameObjectData = new GameObject[1];
					gameEventArgs.GameObjectData[0] = base.gameObject;
					health.OnKill(source, gameEventArgs);
				}
			}
			else
			{
				m_currentHealth = 1f;
			}
		}
		GameEventArgs gameEventArgs2 = new GameEventArgs();
		gameEventArgs2.Type = GameEventType.Damaged;
		gameEventArgs2.FloatData = new float[2];
		gameEventArgs2.FloatData[0] = amount;
		gameEventArgs2.FloatData[1] = currentStamina;
		gameEventArgs2.GameObjectData = new GameObject[1];
		gameEventArgs2.GameObjectData[0] = source;
		gameEventArgs2.GenericData = new object[2];
		gameEventArgs2.GenericData[0] = new DamageInfo(base.gameObject, amount, null)
		{
			DamageType = damageType
		};
		gameEventArgs2.GenericData[1] = sourceEffect;
		if (this.OnDamaged != null)
		{
			this.OnDamaged(base.gameObject, gameEventArgs2);
		}
		if (health != null && health.OnDamageDealt != null)
		{
			health.OnDamageDealt(base.gameObject, gameEventArgs2);
		}
		UIHealthstringManager.Instance.ShowNumber(amount, base.gameObject);
		if (sourceEffect != null && !sourceEffect.Params.IsHostile)
		{
			return;
		}
		ScriptEvent component2 = GetComponent<ScriptEvent>();
		if ((bool)component2)
		{
			component2.ExecuteScript(ScriptEvent.ScriptEvents.OnAttacked);
			if (m_attackedByPlayer)
			{
				component2.ExecuteScript(ScriptEvent.ScriptEvents.OnAttackedByParty);
			}
		}
	}

	public float DoDamage(DamageInfo damage, GameObject attacker)
	{
		if (Dead || Unconscious || !CanBeTargeted || damage == null)
		{
			return 0f;
		}
		float healthPercentage = HealthPercentage;
		float staminaPercentage = StaminaPercentage;
		bool flag = PartyMemberAI.IsInPartyList(GetComponent<PartyMemberAI>());
		float num = 100f;
		float currentStamina = m_currentStamina;
		float currentHealth = m_currentHealth;
		float num2 = Mathf.Max(0f, damage.DamageAmount);
		Health component = ComponentUtils.GetComponent<Health>(attacker);
		Faction component2 = ComponentUtils.GetComponent<Faction>(attacker);
		if ((bool)component2 && component2.IsInPlayerFaction && damage.AttackIsHostile)
		{
			m_attackedByPlayer = true;
		}
		if ((bool)m_stats)
		{
			num = m_stats.Stamina;
			num2 = m_stats.CalculateDamageTaken(damage, attacker);
		}
		if (flag && Cutscene.CutsceneActive)
		{
			num2 = 0f;
		}
		if (m_currentStamina >= num)
		{
			m_canRegenTimer = CharacterStats.StaminaRechargeDelay;
		}
		if (TakesDamage && !NoDamage)
		{
			if (flag)
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_TAKES_DAMAGE);
			}
			bool unconscious = Unconscious;
			bool dead = Dead;
			if (num > 0f)
			{
				m_currentStamina -= num2;
				if (!m_isAnimalCompanion)
				{
					m_currentHealth -= num2;
				}
			}
			else if (!m_isAnimalCompanion)
			{
				m_currentHealth -= num2;
			}
			if (damage.Damage.NonLethal)
			{
				if (!unconscious && Unconscious)
				{
					m_currentStamina = 1f;
				}
				if (!dead && Dead)
				{
					m_currentHealth = 1f;
				}
			}
		}
		if (Unconscious && !Dead)
		{
			AddInjury(damage.DamageType);
			HandleUnconscious(attacker);
			if (flag)
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_KNOCKED_OUT);
			}
		}
		if (ShowDead)
		{
			if (CanDie)
			{
				m_currentHealth = 0f;
				HandleDeath(attacker);
				if (component != null && component.OnKill != null)
				{
					GameEventArgs gameEventArgs = new GameEventArgs();
					gameEventArgs.Type = GameEventType.Killed;
					gameEventArgs.GameObjectData = new GameObject[1];
					gameEventArgs.GameObjectData[0] = base.gameObject;
					gameEventArgs.GenericData = new object[1];
					gameEventArgs.GenericData[0] = damage;
					component.OnKill(attacker, gameEventArgs);
				}
				damage.IsKillingBlow = true;
			}
			else
			{
				if (m_currentHealth < 1f)
				{
					m_currentHealth = 1f;
				}
				if (Unconscious)
				{
					AddInjury(damage.DamageType);
					HandleUnconscious(attacker);
					if (flag)
					{
						TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_KNOCKED_OUT);
					}
				}
			}
		}
		if (HealthPercentage <= 0.25f && healthPercentage > 0.25f)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.LowHealth, base.gameObject, null);
		}
		if (StaminaPercentage <= 0.25f && staminaPercentage > 0.25f)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.LowStamina, base.gameObject, null);
		}
		float num3 = currentStamina / MaxStamina;
		float staminaPercentage2 = StaminaPercentage;
		if (num3 > 0.75f && staminaPercentage2 <= 0.75f)
		{
			ScriptEvent component3 = GetComponent<ScriptEvent>();
			if ((bool)component3)
			{
				component3.ExecuteScript(ScriptEvent.ScriptEvents.OnHealthPercent75);
			}
		}
		else if (num3 > 0.5f && staminaPercentage2 <= 0.5f)
		{
			ScriptEvent component4 = GetComponent<ScriptEvent>();
			if ((bool)component4)
			{
				component4.ExecuteScript(ScriptEvent.ScriptEvents.OnHealthPercent50);
			}
		}
		else if (num3 > 0.25f && staminaPercentage2 <= 0.25f)
		{
			ScriptEvent component5 = GetComponent<ScriptEvent>();
			if ((bool)component5)
			{
				component5.ExecuteScript(ScriptEvent.ScriptEvents.OnHealthPercent25);
			}
		}
		if (damage.AttackIsHostile)
		{
			bool flag2 = true;
			if (damage.DamageAmount == 0f && !damage.Damage.DoesDamage)
			{
				flag2 = false;
			}
			if (flag2 && damage.IsMiss)
			{
				flag2 = damage.Ability;
			}
			if (flag2 && damage.IsGraze && HideGrazes)
			{
				flag2 = false;
			}
			if (flag2 && (bool)component2 && component2.IsInPlayerFaction)
			{
				UIHealthstringManager.Instance.ShowNumber(damage, base.gameObject);
			}
			ScriptEvent component6 = GetComponent<ScriptEvent>();
			if ((bool)component6)
			{
				component6.ExecuteScript(ScriptEvent.ScriptEvents.OnAttacked);
				if (m_attackedByPlayer)
				{
					component6.ExecuteScript(ScriptEvent.ScriptEvents.OnAttackedByParty);
				}
			}
			if (!damage.IsMiss && TakesDamage)
			{
				TryPlayInjuredSound(currentStamina, currentHealth);
				if ((bool)component2 && component2.IsInPlayerFaction)
				{
					bool flag3 = false;
					if (GameState.s_playerCharacter.FriendlyFireTimer <= 0f)
					{
						flag3 = SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.FriendlyFire, SoundSet.s_ShortVODelay, forceInterrupt: true);
						GameState.s_playerCharacter.FriendlyFireTimer = SoundSet.s_ShortVODelay;
					}
					if (!damage.IsMiss && TakesDamage && !flag3)
					{
						SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.ImHit, SoundSet.s_ShortVODelay, forceInterrupt: true);
					}
				}
			}
		}
		if (damage.IsCriticalHit)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.CharacterDamaged, base.gameObject, attacker);
		}
		else if (damage.IsGraze)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.CharacterDamaged, base.gameObject, attacker);
		}
		GameEventArgs gameEventArgs2 = new GameEventArgs();
		gameEventArgs2.Type = GameEventType.Damaged;
		gameEventArgs2.FloatData = new float[2];
		gameEventArgs2.FloatData[0] = num2;
		gameEventArgs2.FloatData[1] = currentStamina;
		gameEventArgs2.GameObjectData = new GameObject[1];
		gameEventArgs2.GameObjectData[0] = attacker;
		gameEventArgs2.GenericData = new object[1];
		gameEventArgs2.GenericData[0] = damage;
		if (this.OnDamaged != null)
		{
			this.OnDamaged(base.gameObject, gameEventArgs2);
		}
		if (component != null && component.OnDamageDealt != null)
		{
			component.OnDamageDealt(base.gameObject, gameEventArgs2);
		}
		AIController[] components = GetComponents<AIController>();
		foreach (AIController aIController in components)
		{
			if (!aIController.enabled)
			{
				continue;
			}
			IGameEventListener gameEventListener = aIController;
			if (gameEventListener == null)
			{
				continue;
			}
			if (damage.AttackIsHostile)
			{
				gameEventListener.OnEvent(gameEventArgs2);
			}
			bool flag4 = false;
			if (m_stats != null)
			{
				flag4 = m_stats.HasStatusEffectOfType(StatusEffect.ModifiedStat.KnockedDown) || m_stats.HasStatusEffectOfType(StatusEffect.ModifiedStat.Stunned);
			}
			flag4 = flag4 || Dead || Unconscious;
			if (m_reactionTimer <= 0f && damage.Interrupts && !damage.IsMiss && TakesDamage && !flag4 && m_playsHitReactions)
			{
				gameEventArgs2.Type = GameEventType.HitReact;
				gameEventListener.OnEvent(gameEventArgs2);
				m_reactionTimer = HitReactInterval;
			}
			else if (!damage.IsMiss && TakesDamage)
			{
				AnimationController component7 = GetComponent<AnimationController>();
				if ((bool)component7)
				{
					component7.Flinch();
				}
			}
		}
		if (damage.Interrupts && m_stats != null && damage.Attack != null)
		{
			m_stats.HandleInterruptRecovery(damage.Attack.BaseInterrupt);
		}
		return num2;
	}

	public void AddStamina(float amount, bool report = true)
	{
		if (Unconscious)
		{
			return;
		}
		float currentStamina = m_currentStamina;
		float num = amount;
		m_currentStamina += amount;
		if (m_currentStamina > MaxStamina)
		{
			m_currentStamina = MaxStamina;
			amount = m_currentStamina - currentStamina;
		}
		if (m_currentStamina > CurrentHealth)
		{
			m_currentStamina = CurrentHealth;
			amount = m_currentStamina - currentStamina;
		}
		if (amount != 0f || num != amount)
		{
			if (this.OnHealed != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Healed;
				gameEventArgs.FloatData = new float[2];
				gameEventArgs.FloatData[0] = amount;
				gameEventArgs.FloatData[1] = currentStamina;
				this.OnHealed(base.gameObject, gameEventArgs);
			}
			if (report)
			{
				UIHealthstringManager.Instance.ShowNumber(0f - amount, base.gameObject);
			}
		}
	}

	public void AddHealth(float amount)
	{
		if (!Dead)
		{
			float currentHealth = m_currentHealth;
			float num = amount;
			m_currentHealth += amount;
			if (m_currentHealth > MaxHealth)
			{
				m_currentHealth = MaxHealth;
				amount = m_currentHealth - currentHealth;
				m_attackedByPlayer = false;
			}
			if (amount != 0f || num != amount)
			{
				UIHealthstringManager.Instance.ShowNumber(0f - amount, base.gameObject);
			}
		}
	}

	public void DestroyKill(GameObject source)
	{
		if (!ShowDead && CanBeTargeted && CanDie)
		{
			m_destroyed = true;
			CurrentStamina = 0f;
			CurrentHealth = 0f;
			LogKill(source);
			Health health = (source ? source.GetComponent<Health>() : null);
			if ((bool)health && health.OnKill != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Killed;
				gameEventArgs.GameObjectData = new GameObject[1];
				gameEventArgs.GameObjectData[0] = base.gameObject;
				health.OnKill(source, gameEventArgs);
			}
			if (AlwaysGib && CanGib && (bool)GibList)
			{
				SpawnGibs(GibletEffect, base.transform.position);
			}
		}
	}

	public bool IsTargetableByAttack(AttackBase attack)
	{
		if (!attack && Dead && !GameInput.SelectDead)
		{
			return false;
		}
		if (!m_targetable)
		{
			return false;
		}
		if ((bool)attack && !attack.IsValidPrimaryTarget(base.gameObject) && base.gameObject != attack.ForcedTarget)
		{
			return false;
		}
		bool flag = false;
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if (component != null)
		{
			flag = component.IsBusy;
		}
		return !flag;
	}

	public void OnRevive()
	{
		if (m_currentStamina <= 0f)
		{
			m_currentStamina = 1f;
		}
		if (m_currentHealth <= 0f)
		{
			m_currentHealth = 1f;
		}
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = true;
		}
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().detectCollisions = true;
		}
		RemoveGrief(GameUtilities.FindMaster(base.gameObject));
		RemoveGrief(GameUtilities.FindAnimalCompanion(base.gameObject));
		AIController[] components = GetComponents<AIController>();
		foreach (IGameEventListener gameEventListener in components)
		{
			if (gameEventListener != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Revived;
				gameEventListener.OnEvent(gameEventArgs);
				if (this.OnRevived != null)
				{
					this.OnRevived(base.gameObject, gameEventArgs);
				}
			}
		}
	}

	public void AddInjury(DamagePacket.DamageType damageType)
	{
		if (GameState.Option.GetOption(GameOption.BoolOption.DISABLE_INJURIES))
		{
			return;
		}
		switch (damageType)
		{
		case DamagePacket.DamageType.Burn:
		case DamagePacket.DamageType.Corrode:
			if (!TryAddInjury(AfflictionData.Instance.BurnInjury))
			{
				m_stats.IncreaseFatigueLevel();
			}
			return;
		case DamagePacket.DamageType.Shock:
			if (!TryAddInjury(AfflictionData.Instance.ShockInjury))
			{
				m_stats.IncreaseFatigueLevel();
			}
			return;
		case DamagePacket.DamageType.Freeze:
			if (!TryAddInjury(AfflictionData.Instance.FreezeInjury))
			{
				m_stats.IncreaseFatigueLevel();
			}
			return;
		case DamagePacket.DamageType.Raw:
			if (!TryAddInjury(AfflictionData.Instance.RawInjury))
			{
				m_stats.IncreaseFatigueLevel();
			}
			return;
		}
		List<Affliction> list = new List<Affliction>();
		list.AddRange(AfflictionData.Instance.NormalInjuries);
		while (list.Count > 0)
		{
			int index = OEIRandom.Index(list.Count);
			if (TryAddInjury(list[index]))
			{
				break;
			}
			if (m_stats.GetFatigueLevel() < CharacterStats.FatigueLevel.Critical)
			{
				m_stats.IncreaseFatigueLevel();
				break;
			}
			list.RemoveAt(index);
		}
	}

	private bool TryAddInjury(Affliction affliction)
	{
		if (!m_stats.HasStatusEffectFromAffliction(affliction))
		{
			m_stats.ApplyAffliction(affliction);
			return true;
		}
		return false;
	}

	public void HandleUnconscious(GameObject killer)
	{
		if (m_stats != null && m_stats.UnconsciousnessDelayed > 0)
		{
			StartCoroutine(HandleUnconsciousDelay(killer, 3f));
		}
		else
		{
			HandleUnconsciousHelper(killer);
		}
	}

	private IEnumerator HandleUnconsciousDelay(GameObject killer, float time)
	{
		yield return new WaitForSeconds(time);
		if (m_currentStamina <= 0f)
		{
			HandleUnconsciousHelper(killer);
		}
	}

	private void HandleUnconsciousHelper(GameObject killer)
	{
		if (!GameState.IsLoading)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.CharacterDown, base.gameObject, null);
		}
		AIController[] components = GetComponents<AIController>();
		foreach (IGameEventListener gameEventListener in components)
		{
			if (gameEventListener != null)
			{
				GameEventArgs gameEventArgs = new GameEventArgs();
				gameEventArgs.Type = GameEventType.Unconscious;
				gameEventListener.OnEvent(gameEventArgs);
				if (this.OnUnconscious != null)
				{
					this.OnUnconscious(base.gameObject, gameEventArgs);
				}
			}
		}
		ApplyGrief(GameUtilities.FindMaster(base.gameObject));
		ApplyGrief(GameUtilities.FindAnimalCompanion(base.gameObject));
		LogKnockout(killer);
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnUnconscious);
		}
		if (PartyMemberAI.IsInPartyList(GetComponent<PartyMemberAI>()) && !GameState.IsLoading)
		{
			m_checkPartyDeathTimer = 0.5f;
			PlayPartyMemberIncapacitatedVoiceTrigger(base.gameObject.GetComponent<PartyMemberAI>(), DeathStatusType.KnockedOut);
		}
	}

	private static void ApplyGrief(GameObject victim)
	{
		if (!(victim == null))
		{
			CharacterStats component = victim.GetComponent<CharacterStats>();
			if (component != null && !component.HasStatusEffectFromAffliction(AttackData.Instance.BondedGriefAffliction))
			{
				component.ApplyAffliction(AttackData.Instance.BondedGriefAffliction);
				component.NotifyGriefStateChanged(victim, state: true);
			}
		}
	}

	private static void RemoveGrief(GameObject victim)
	{
		if (!(victim == null))
		{
			CharacterStats component = victim.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.ClearEffectFromAffliction(AttackData.Instance.BondedGriefAffliction);
				component.NotifyGriefStateChanged(victim, state: false);
			}
		}
	}

	private void LogKill(GameObject killer)
	{
		if (!(killer == null) && ShowDead && !MaimAvailable())
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(m_destroyed ? 2235 : 98), CharacterStats.NameColored(base.gameObject), CharacterStats.NameColored(killer)), Color.red);
		}
	}

	private void LogKnockout(GameObject killer)
	{
		if (!(killer == null) && Unconscious && !ShowDead)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(99), CharacterStats.NameColored(base.gameObject), CharacterStats.NameColored(killer)), Color.red);
		}
	}

	public bool MaimAvailable()
	{
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if (component != null && component.enabled && (!component || !component.Summoner || component.SummonType != AIController.AISummonType.Summoned) && GameState.Option.DeathIsNotPermanent && m_stats != null)
		{
			return !m_stats.HasStatusEffectFromAffliction(AfflictionData.Maimed);
		}
		return false;
	}

	private void PlayPartyMemberIncapacitatedVoiceTrigger(PartyMemberAI partyAI, DeathStatusType deathStatus)
	{
		if (partyAI == null)
		{
			return;
		}
		int num;
		switch (deathStatus)
		{
		case DeathStatusType.Invalid:
			return;
		default:
			num = 70;
			break;
		case DeathStatusType.Death:
			num = 11;
			break;
		}
		SoundSet.SoundAction actionID = (SoundSet.SoundAction)num;
		if (partyAI.gameObject == GameState.s_playerCharacter.gameObject)
		{
			actionID = ((deathStatus == DeathStatusType.Death) ? SoundSet.SoundAction.PlayerDeath : SoundSet.SoundAction.PlayerKO);
		}
		else
		{
			CompanionInstanceID component = partyAI.GetComponent<CompanionInstanceID>();
			if (component != null)
			{
				actionID = ((deathStatus == DeathStatusType.Death) ? SoundSet.GetSoundActionForCompanionDeath(component.Companion) : SoundSet.GetSoundActionForCompanionKO(component.Companion));
			}
		}
		HaveRandomAlivePartyMemberSay(partyAI, actionID);
	}

	public static void HaveRandomAlivePartyMemberSay(PartyMemberAI subject, SoundSet.SoundAction actionID)
	{
		List<int> list = new List<int> { 0, 1, 2, 3, 4, 5 };
		while (list.Count > 0)
		{
			int index = OEIRandom.Index(list.Count);
			PartyMemberAI partyMemberAI = PartyMemberAI.PartyMembers[list[index]];
			if ((bool)partyMemberAI && partyMemberAI != subject && !partyMemberAI.IsUnconscious && !partyMemberAI.IsDead && SoundSet.TryPlayVoiceEffectWithLocalCooldown(partyMemberAI.gameObject, actionID, 0f, forceInterrupt: true))
			{
				break;
			}
			list.RemoveAt(index);
		}
	}

	public void HandleDeath(GameObject killer)
	{
		m_currentStamina = 0f;
		m_currentHealth = 0f;
		PartyMemberAI component = base.gameObject.GetComponent<PartyMemberAI>();
		if ((bool)component)
		{
			if ((bool)m_stats)
			{
				m_stats.ClearEffectFromAffliction(AfflictionData.Instance.CharmedPrefab);
				m_stats.ClearEffectFromAffliction(AfflictionData.Instance.DominatedPrefab);
				m_stats.ClearEffectFromAffliction(AttackData.Instance.ConfusedAffliction);
				m_stats.ClearStatusEffects(StatusEffect.ModifiedStat.SwapFaction);
			}
			if (component.enabled)
			{
				PlayPartyMemberIncapacitatedVoiceTrigger(component, DeathStatusType.Death);
			}
		}
		SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.ImDead, SoundSet.s_MediumVODelay, forceInterrupt: true);
		if ((bool)m_stats && m_attackedByPlayer)
		{
			BestiaryManager.Instance.RecordKill(m_stats.BestiaryReference);
		}
		PartyMemberAI component2 = GetComponent<PartyMemberAI>();
		bool flag = true;
		if (component2 != null && component2.enabled)
		{
			if (component2.Summoner != null && component2.SummonType == AIController.AISummonType.Summoned)
			{
				HandlePartyMemberPermaDeath(component2, killer);
			}
			else if (GameState.Option.DeathIsNotPermanent)
			{
				if (m_stats != null && m_stats.HasStatusEffectFromAffliction(AfflictionData.Maimed))
				{
					HandlePartyMemberPermaDeath(component2, killer);
				}
				else
				{
					flag = false;
				}
				if (GameState.InCombat)
				{
					GameState.OnCombatEnd += HandleMaimOnCombatEnd;
				}
				else
				{
					StartCoroutine(waitToRevive(3f));
				}
			}
			else
			{
				HandlePartyMemberPermaDeath(component2, killer);
			}
			m_checkPartyDeathTimer = 0.5f;
		}
		else
		{
			GameUtilities.KillAnimalCompanion(base.gameObject);
		}
		LogKill(killer);
		GameEventArgs gameEventArgs = new GameEventArgs();
		gameEventArgs.Type = GameEventType.Dead;
		gameEventArgs.GameObjectData = new GameObject[1];
		gameEventArgs.GameObjectData[0] = killer;
		AIController[] components = GetComponents<AIController>();
		for (int i = 0; i < components.Length; i++)
		{
			((IGameEventListener)components[i])?.OnEvent(gameEventArgs);
		}
		if ((bool)m_stats && (m_stats.CharacterClass == CharacterStats.Class.SkyDragon || m_stats.CharacterClass == CharacterStats.Class.AdraDragon) && (bool)AchievementTracker.Instance)
		{
			Persistence component3 = m_stats.GetComponent<Persistence>();
			if ((bool)component3 && component3.ExportPackage == Persistence.AssetBundleExportPackage.X1)
			{
				AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumPX1DragonsKilled);
			}
			else
			{
				AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumBaseGameDragonsKilled);
			}
		}
		GameState.AutoPause(AutoPauseOptions.PauseEvent.CharacterDown, base.gameObject, null);
		if (this.OnDeath != null)
		{
			this.OnDeath(base.gameObject, gameEventArgs);
		}
		ScriptEvent component4 = GetComponent<ScriptEvent>();
		if ((bool)component4 && flag)
		{
			component4.ExecuteScript(ScriptEvent.ScriptEvents.OnDeath);
		}
	}

	private void HandlePartyMemberPermaDeath(PartyMemberAI partyMember, GameObject killer)
	{
		ShouldDecay = true;
		if ((bool)partyMember)
		{
			if (killer != null)
			{
				killer.GetComponent<CharacterStats>();
			}
			GameUtilities.KillAnimalCompanion(partyMember.gameObject);
			PartyMemberAI.NotifyPartyMemberPermaDeath(partyMember);
			PartyMemberAI.RemoveFromActiveParty(partyMember, purgePersistencePacket: true);
		}
	}

	private void CheckPartyDeath()
	{
		bool flag = false;
		bool flag2 = false;
		if (GetComponent<Player>() != null && ShouldDecay)
		{
			flag = true;
		}
		else
		{
			flag2 = true;
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (!(partyMemberAI == null))
				{
					Health component = partyMemberAI.GetComponent<Health>();
					if (component != null && !component.Unconscious && !component.Dead)
					{
						flag2 = false;
						break;
					}
				}
			}
		}
		if (flag || flag2)
		{
			GameState.PartyDead = true;
		}
	}

	public void HandleGameOnResting()
	{
		if (Unconscious)
		{
			OnRevive();
		}
		AddHealth(MaxHealth - CurrentHealth);
		AddStamina(MaxStamina - CurrentStamina, report: false);
	}

	private void WakeUp()
	{
		if (Unconscious)
		{
			OnRevive();
		}
	}

	private void HandleMaimOnCombatEnd(object sender, EventArgs e)
	{
		GameState.OnCombatEnd -= HandleMaimOnCombatEnd;
		CheckMaim();
	}

	private void CheckMaim()
	{
		if (m_stats == null || GameState.GameOver || m_isAnimalCompanion)
		{
			return;
		}
		if (m_stats.HasStatusEffectFromAffliction(AfflictionData.Maimed))
		{
			ShouldDecay = true;
			PartyMemberAI component = GetComponent<PartyMemberAI>();
			if ((bool)component)
			{
				HandlePartyMemberPermaDeath(component, null);
			}
			ScriptEvent component2 = GetComponent<ScriptEvent>();
			if ((bool)component2)
			{
				component2.ExecuteScript(ScriptEvent.ScriptEvents.OnDeath);
			}
		}
		else
		{
			OnRevive();
			m_stats.ApplyAffliction(AfflictionData.Maimed);
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1544), CharacterStats.NameColored(base.gameObject)), Color.red);
		}
	}

	public IEnumerator waitToRevive(float howLong)
	{
		yield return new WaitForSeconds(howLong);
		CheckMaim();
	}

	public void SuspendDecay(bool enabled)
	{
		m_DecaySuspended = enabled;
	}

	public string CurrentStaminaString()
	{
		if (HealthVisible)
		{
			return Mathf.Max(0, Mathf.CeilToInt(CurrentStamina)).ToString("#0");
		}
		return GUIUtils.GetText(1980);
	}

	public string CurrentHealthString()
	{
		if (HealthVisible)
		{
			return Mathf.Max(0, Mathf.CeilToInt(CurrentHealth)).ToString("#0");
		}
		return GUIUtils.GetText(1980);
	}

	private void TryPlayInjuredSound(float originalStamina, float originalHealth)
	{
		if (!(m_injuredTimer <= 0f))
		{
			return;
		}
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if ((bool)component && component.enabled && (bool)component.SoundSet)
		{
			if (HealthPercentage < 0.25f)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InjuredSevereHealth, SoundSet.s_ShortVODelay, forceInterrupt: false);
			}
			else if (StaminaPercentage < 0.25f)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InjuredSevereStamina, SoundSet.s_ShortVODelay, forceInterrupt: false);
			}
			else if (originalHealth != CurrentHealth)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InjuredHealth, SoundSet.s_ShortVODelay, forceInterrupt: false);
			}
			else
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InjuredStamina, SoundSet.s_ShortVODelay, forceInterrupt: false);
			}
			m_injuredTimer = SoundSet.s_ShortVODelay * 2f;
		}
	}

	public void SpawnGibs(GameObject gibletEffect, Vector3 position)
	{
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if (!component || !component.IsActiveInParty)
		{
			float num = 1f;
			Mover component2 = GetComponent<Mover>();
			if ((bool)component2)
			{
				num = component2.Radius * 2f;
			}
			if (num > 3f)
			{
				num = 3f;
			}
			else if (num < 0.25f)
			{
				num = 0.25f;
			}
			ShouldGib = true;
			GameObject obj = new GameObject("Gibs");
			obj.transform.position = position;
			GibSpawner gibSpawner = obj.AddComponent<GibSpawner>();
			gibSpawner.GibList = GibList;
			gibSpawner.GibTrailEffect = gibletEffect;
			gibSpawner.Spawn(destroy: true);
			if (BloodyMess)
			{
				gibSpawner.Spawn(destroy: true);
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTrait : GenericAbility
{
	public class EffectData
	{
		public GameObject m_target;

		public uint m_effectID;
	}

	private float m_focus;

	private List<EffectData> m_effectData = new List<EffectData>();

	private float m_beamDurationAfterBreak;

	private AttackBeam m_beamAttack;

	private GenericCipherAbility m_cipherAbility;

	[Persistent]
	protected float m_focusResetTimer;

	public float MaxFocusBonus { get; set; }

	public float Focus
	{
		get
		{
			return m_focus;
		}
		set
		{
			if (m_focusResetTimer <= 0f)
			{
				m_focusResetTimer = 1f;
			}
			float maxFocus = MaxFocus;
			if (value > maxFocus)
			{
				value = maxFocus;
			}
			m_focus = value;
			if (m_focus == maxFocus)
			{
				GenericAbility genericAbility = FindSoulWhip();
				if (genericAbility != null && genericAbility.Activated)
				{
					genericAbility.Deactivate(Owner);
				}
			}
		}
	}

	private float BaseFocus
	{
		get
		{
			float result = 0f;
			if (m_ownerStats != null)
			{
				result = (float)m_ownerStats.ScaledLevel * 5f;
				result += 10f;
				result /= 2f;
			}
			return result;
		}
	}

	public float StartingFocus => BaseFocus + MaxFocusBonus;

	public float MaxFocus => BaseFocus * 4f + MaxFocusBonus;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			GameState.OnCombatEnd += OnCombatEnd;
			if ((bool)m_ownerStats)
			{
				m_ownerStats.OnLevelUp += OnLevelUp;
			}
			if ((bool)m_ownerHealth)
			{
				m_ownerHealth.OnDamageDealt += OnDamageDealt;
			}
			m_permanent = true;
			ResetFocus();
		}
	}

	protected override void OnInactive()
	{
		base.OnInactive();
		GameState.OnCombatEnd -= OnCombatEnd;
		if ((bool)m_ownerStats)
		{
			m_ownerStats.OnLevelUp -= OnLevelUp;
		}
		if ((bool)m_ownerHealth)
		{
			m_ownerHealth.OnDamageDealt -= OnDamageDealt;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!GameState.InCombat && m_focusResetTimer > 0f)
		{
			m_focusResetTimer -= Time.deltaTime;
			if (m_focusResetTimer <= 0f)
			{
				ResetFocus();
				m_focusResetTimer = 0f;
			}
		}
	}

	protected override void HandleStatsOnPreApply(GameObject source, CombatEventArgs args)
	{
		CheckFocusBreak();
		m_cipherAbility = source.GetComponent<GenericCipherAbility>();
	}

	protected override void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
		if (args.Damage == null || args.Damage.Attack == null || !(args.Damage.Attack != m_beamAttack))
		{
			return;
		}
		if (m_cipherAbility == null)
		{
			CheckFocusBreak();
		}
		else if (args.Damage.Attack is AttackBeam)
		{
			m_beamAttack = args.Damage.Attack as AttackBeam;
			m_beamDurationAfterBreak = m_cipherAbility.DurationAfterBreak;
			if (m_beamDurationAfterBreak <= 0f)
			{
				m_beamDurationAfterBreak = 0.01f;
			}
		}
	}

	private void CheckFocusBreak()
	{
		if (m_effectData == null)
		{
			return;
		}
		foreach (EffectData effectDatum in m_effectData)
		{
			if (effectDatum.m_target != null)
			{
				CharacterStats component = effectDatum.m_target.GetComponent<CharacterStats>();
				if (component != null)
				{
					component.FindStatusEffect(effectDatum.m_effectID)?.FocusBreak();
				}
			}
		}
		_ = m_beamAttack != null;
		m_effectData.Clear();
		m_beamAttack = null;
		m_beamDurationAfterBreak = 0f;
		m_cipherAbility = null;
	}

	private IEnumerator OnBeamShutdown(float time, AttackBeam beamAttack)
	{
		yield return new WaitForSeconds(time);
		beamAttack.ShutdownBeam();
	}

	protected override void HandleStatsOnEffectApply(GameObject source, CombatEventArgs args)
	{
		StatusEffect statusEffect = args.CustomData as StatusEffect;
		if (statusEffect.DurationAfterBreak > 0f)
		{
			EffectData effectData = new EffectData();
			effectData.m_target = args.Victim;
			effectData.m_effectID = statusEffect.EffectID;
			m_effectData.Add(effectData);
		}
	}

	private void OnCombatEnd(object sender, EventArgs e)
	{
		ResetFocus();
	}

	private void OnLevelUp(object sender, EventArgs e)
	{
		ResetFocus();
	}

	private void OnDamageDealt(GameObject victim, GameEventArgs args)
	{
		float damageDealt = args.FloatData[0];
		DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
		if (damageInfo != null && damageInfo.Attack != null && victim != null && damageInfo.Attack.IsAutoAttack())
		{
			Faction component = victim.GetComponent<Faction>();
			if (component != null && component.IsHostile(Owner))
			{
				AddFocusByDamage(damageDealt);
			}
		}
	}

	public void AddFocusByDamage(float damageDealt)
	{
		float focusGained = damageDealt * AttackData.Instance.FocusPerWeaponDamageDealt;
		AddFocus(focusGained);
	}

	public void AddFocus(float focusGained)
	{
		if (m_ownerStats != null)
		{
			focusGained *= m_ownerStats.FocusGainMult;
		}
		Focus += focusGained;
	}

	private void ResetFocus()
	{
		Focus = StartingFocus;
	}

	private GenericAbility FindSoulWhip()
	{
		if (m_ownerStats != null && m_ownerStats.ActiveAbilities != null)
		{
			foreach (GenericAbility activeAbility in m_ownerStats.ActiveAbilities)
			{
				if (!(activeAbility != null) || activeAbility.ActivationPrerequisites == null)
				{
					continue;
				}
				PrerequisiteData[] activationPrerequisites = activeAbility.ActivationPrerequisites;
				for (int i = 0; i < activationPrerequisites.Length; i++)
				{
					if (activationPrerequisites[i].Type == PrerequisiteType.FocusBelowMax)
					{
						return activeAbility;
					}
				}
			}
		}
		return null;
	}
}

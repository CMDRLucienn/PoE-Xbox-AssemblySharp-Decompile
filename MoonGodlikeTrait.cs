using System;
using System.Collections.Generic;
using UnityEngine;

public class MoonGodlikeTrait : GenericAbility
{
	private static float[] s_staminaThreshold = new float[3] { 0.25f, 0.5f, 0.75f };

	private bool[] m_canTrigger;

	private float m_staminaHealBaseValue;

	private float m_staminaHealBaseExtraValue;

	private int m_originalOwnerLevel;

	public override bool ListenForDamageEvents => true;

	protected override void Init()
	{
		if (!m_initialized)
		{
			CooldownType = CooldownMode.PerEncounter;
			m_canTrigger = new bool[s_staminaThreshold.Length];
			ResetTriggers();
			base.Init();
			GetStaminaValue();
		}
	}

	public override void Restored()
	{
		base.Restored();
		GetStaminaValue();
	}

	private void GetStaminaValue()
	{
		foreach (StatusEffect effect in m_effects)
		{
			if (effect.Params.AffectsStat == StatusEffect.ModifiedStat.Stamina)
			{
				m_staminaHealBaseValue = effect.Params.GetValue(m_ownerStats);
				m_staminaHealBaseExtraValue = effect.Params.GetExtraValue(m_ownerStats);
				break;
			}
		}
		m_originalOwnerLevel = 0;
	}

	public override void HandleGameUtilitiesOnCombatEnd(object sender, EventArgs e)
	{
		ResetTriggers();
		base.HandleGameUtilitiesOnCombatEnd(sender, e);
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (!(m_ownerHealth != null))
		{
			return;
		}
		float num = args.FloatData[1] / m_ownerHealth.MaxStamina;
		float num2 = m_ownerHealth.CurrentStamina / m_ownerHealth.MaxStamina;
		for (int i = 0; i < m_canTrigger.Length; i++)
		{
			if (m_canTrigger[i] && num >= s_staminaThreshold[i] && num2 < s_staminaThreshold[i])
			{
				ActivateTrait();
				m_canTrigger[i] = false;
				break;
			}
		}
	}

	protected override void Update()
	{
		if ((bool)m_ownerStats && m_ownerStats.ScaledLevel != m_originalOwnerLevel)
		{
			m_originalOwnerLevel = m_ownerStats.ScaledLevel;
			foreach (StatusEffect effect in m_effects)
			{
				if (effect.Params.AffectsStat == StatusEffect.ModifiedStat.Stamina)
				{
					float num = m_staminaHealBaseValue * (float)(m_originalOwnerLevel - 1);
					num += m_staminaHealBaseExtraValue;
					effect.Params.Value = num;
					break;
				}
			}
		}
		base.Update();
	}

	protected void ActivateTrait()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			effect.Params.OverrideCleanedValue(effect.CurrentAppliedValue);
			component.ApplyStatusEffectImmediate(effect);
		}
		GameUtilities.LaunchEffect(OnActivateGroundVisualEffect, 1f, Owner.transform.position, this);
		if (m_effects.Count <= 0)
		{
			return;
		}
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.OtherOwner = Owner;
		damageInfo.Ability = this;
		foreach (KeyValuePair<GameObject, StatusEffect> item in m_effects[0].UiAuraEffectsApplied)
		{
			Console.AddBatchedMessage(GUIUtils.Format(824, CharacterStats.NameColored(Owner), CharacterStats.NameColored(item.Key), Name()), item.Value.Params.GetString(item.Value, m_ownerStats, this), InGameHUD.GetFriendlyColor().Max(40f / 51f), damageInfo);
		}
	}

	protected void DeactivateTrait()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			component.ClearEffect(effect);
		}
	}

	protected void ResetTriggers()
	{
		for (int i = 0; i < m_canTrigger.Length; i++)
		{
			m_canTrigger[i] = true;
		}
	}
}

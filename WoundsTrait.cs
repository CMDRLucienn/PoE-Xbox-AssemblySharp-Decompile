using System;
using System.Collections;
using UnityEngine;

public class WoundsTrait : GenericAbility
{
	[Range(0f, 100f)]
	[Tooltip("The % of incoming damage that is counted towards gaining a wound")]
	public int WoundDamageConversionPct = 20;

	[Range(1f, 10f)]
	public int WoundNumberMax = 10;

	[Persistent]
	protected float m_woundResetTimer;

	private float m_originalWoundValue;

	private float m_woundDamageBuffer;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_originalWoundValue = StatusEffects[0].Value;
			GameState.OnCombatEnd += OnCombatEnd;
			m_permanent = true;
		}
	}

	protected override void OnDestroy()
	{
		GameState.OnCombatEnd -= OnCombatEnd;
		base.OnDestroy();
	}

	protected override void Update()
	{
		base.Update();
		if (!GameState.InCombat && m_woundResetTimer > 0f)
		{
			m_woundResetTimer -= Time.deltaTime;
			if (m_woundResetTimer <= 0f)
			{
				ClearAllWounds();
				m_woundResetTimer = 0f;
			}
		}
	}

	private void OnCombatEnd(object sender, EventArgs e)
	{
		OnCombatEnd();
	}

	private void OnCombatEnd()
	{
		ClearAllWounds();
	}

	private void ClearAllWounds()
	{
		if (!(m_ownerStats == null))
		{
			m_woundDamageBuffer = 0f;
			m_ownerStats.ClearStatusEffects(StatusEffects[0].Tag);
		}
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		bool flag = false;
		flag = true;
		AdjustDamage(args.FloatData[0], flag);
	}

	public void AdjustDamage(float damage, bool forceWound)
	{
		if ((!GameState.InCombat && !forceWound) || m_ownerStats == null)
		{
			return;
		}
		int i = m_ownerStats.CountStatusEffects(StatusEffects[0].Tag);
		if (i >= WoundNumberMax)
		{
			return;
		}
		float num = damage * ((float)WoundDamageConversionPct / 100f);
		num += m_woundDamageBuffer;
		m_woundDamageBuffer = 0f;
		float num2 = 0f - m_originalWoundValue;
		num2 += GatherAbilityModSum(AbilityMod.AbilityModType.WoundThresholdAdjustment);
		for (; i < WoundNumberMax; i++)
		{
			if (num < num2)
			{
				m_woundDamageBuffer = num;
				num = 0f;
				break;
			}
			if (m_ownerStats.WoundDelay <= 0f)
			{
				AddWound(i, num2, forceWound);
			}
			else
			{
				StartCoroutine(AddWoundDelay(m_ownerStats.WoundDelay, i, num2, forceWound));
			}
			num -= num2;
		}
	}

	public IEnumerator AddWoundDelay(float time, int woundNumber, float newValue, bool forceWound)
	{
		yield return new WaitForSeconds(time);
		AddWound(woundNumber, newValue, forceWound);
	}

	private void AddWound(int woundNumber, float newValue, bool forceWound)
	{
		if ((GameState.InCombat || forceWound) && !(m_ownerStats == null))
		{
			StatusEffects[0].Value = newValue;
			StatusEffect statusEffect = StatusEffect.Create(base.gameObject, this, StatusEffects[0], AbilityType.Ability, null, deleteOnClear: true);
			statusEffect.BundleId = woundNumber;
			m_ownerStats.ApplyStatusEffect(statusEffect);
			if (m_woundResetTimer <= 0f)
			{
				m_woundResetTimer = 1f;
			}
			Console.AddMessage(GUIUtils.FormatWithLinks(1502, CharacterStats.NameColored(m_owner)), Color.white);
		}
	}

	protected override void ActivateStatusEffects()
	{
	}

	public float DamageNeededToWound()
	{
		float num = 0f - (m_originalWoundValue + m_woundDamageBuffer);
		if (WoundDamageConversionPct > 0)
		{
			num *= 100f / (float)WoundDamageConversionPct;
		}
		return num;
	}

	public void AddNewWound()
	{
		if (GameState.InCombat && !(m_ownerStats == null))
		{
			int num = m_ownerStats.CountStatusEffects(StatusEffects[0].Tag);
			if (num < WoundNumberMax)
			{
				m_woundDamageBuffer = 0f;
				float num2 = 0f - m_originalWoundValue;
				num2 += GatherAbilityModSum(AbilityMod.AbilityModType.WoundThresholdAdjustment);
				AddWound(num, num2, forceWound: false);
				num++;
			}
		}
	}
}

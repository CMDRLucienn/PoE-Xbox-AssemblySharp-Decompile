using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedStats : MonoBehaviour
{
	public enum StatType
	{
		MaxHealth,
		MaxStamina,
		Health,
		Stamina,
		MeleeAccuracyBonus,
		RangedAccuracyBonus,
		DeflectionBonus,
		FortitudeBonus,
		ReflexBonus,
		WillBonus,
		StaminaRechargeRate,
		AttackSpeed,
		StealthSkill,
		DamageMinimum,
		MovementRate,
		Targetable,
		Mobility,
		EngagedEnemyCount,
		EngagementRadius,
		EngagementAccuracy,
		EngagementDamage,
		Enageable,
		Damage,
		BonusUnarmedDamage,
		MeleeAttackDistanceMult,
		RangedAttackDistanceMult,
		BonusDTFromArmor,
		MeleeDamageRangePctIncreaseToMin,
		BonusMeleeDamage,
		ImmuneToEngageStop,
		HealthLossPercentMult_DEPRECATED,
		SlotNumber,
		Abilities,
		Level,
		StatusEffects,
		SimultaneousHitDefenseBonus,
		Experience,
		Intellect,
		Might,
		Constitution,
		Dexterity,
		Perception,
		Resolve,
		AthleticsSkill,
		LoreSkill,
		MechanicsSkill,
		SurvivalSkill,
		CraftingSkill,
		Count
	}

	public enum ShareType
	{
		Combined,
		Highest,
		Lowest,
		OtherCharacter,
		Mine
	}

	public SharedData[] SharingRules;

	protected CharacterStats m_myStats;

	protected Health m_myHealth;

	protected GameObject m_otherCharacter;

	protected CharacterStats m_otherStats;

	protected Health m_otherHealth;

	protected float[] m_myOldValues = new float[48];

	protected float[] m_otherOldValues = new float[48];

	protected bool m_saveValues;

	protected bool m_abilitiesShared;

	protected List<SharedData> m_sharingRules = new List<SharedData>();

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public GameObject SharedCharacter
	{
		get
		{
			return m_otherCharacter;
		}
		set
		{
			m_otherCharacter = value;
			if (m_otherCharacter == null)
			{
				m_otherHealth = null;
				m_otherStats = null;
			}
			else
			{
				m_otherHealth = m_otherCharacter.GetComponent<Health>();
				m_otherStats = m_otherCharacter.GetComponent<CharacterStats>();
				m_saveValues = true;
			}
		}
	}

	private void Start()
	{
		m_myStats = GetComponent<CharacterStats>();
		m_myHealth = GetComponent<Health>();
		if (SharingRules == null)
		{
			return;
		}
		SharedData[] sharingRules = SharingRules;
		foreach (SharedData sharedData in sharingRules)
		{
			if (sharedData != null)
			{
				m_sharingRules.Add(sharedData);
			}
		}
	}

	private void SaveCurrentValues()
	{
		Mover component = GetComponent<Mover>();
		Mover component2 = m_otherCharacter.GetComponent<Mover>();
		PartyMemberAI component3 = GetComponent<PartyMemberAI>();
		PartyMemberAI component4 = m_otherCharacter.GetComponent<PartyMemberAI>();
		m_myOldValues[0] = m_myStats.MaxHealth;
		m_otherOldValues[0] = m_otherStats.MaxHealth;
		m_myOldValues[1] = m_myStats.MaxStamina;
		m_otherOldValues[1] = m_otherStats.MaxStamina;
		m_myOldValues[2] = m_myHealth.CurrentHealth;
		m_otherOldValues[2] = m_otherHealth.CurrentHealth;
		m_myOldValues[3] = m_myHealth.CurrentStamina;
		m_otherOldValues[3] = m_otherHealth.CurrentStamina;
		m_myOldValues[10] = m_myStats.StaminaRechargeBonus;
		m_otherOldValues[10] = m_otherStats.StaminaRechargeBonus;
		m_myOldValues[11] = m_myStats.AttackSpeedMultiplier;
		m_otherOldValues[11] = m_otherStats.AttackSpeedMultiplier;
		m_myOldValues[12] = m_myStats.StealthSkill;
		m_otherOldValues[12] = m_otherStats.StealthSkill;
		m_myOldValues[13] = m_myStats.DamageMinBonus;
		m_otherOldValues[13] = m_otherStats.DamageMinBonus;
		m_myOldValues[14] = component.RunSpeed;
		m_otherOldValues[14] = component2.RunSpeed;
		m_myOldValues[15] = (m_myHealth.Targetable ? 1f : 0f);
		m_otherOldValues[15] = (m_otherHealth.Targetable ? 1f : 0f);
		m_myOldValues[16] = (component.Frozen ? 1f : 0f);
		m_otherOldValues[16] = (component2.Frozen ? 1f : 0f);
		m_myOldValues[17] = m_myStats.EngageableEnemyCount;
		m_otherOldValues[17] = m_otherStats.EngageableEnemyCount;
		m_myOldValues[18] = m_myStats.EngagementDistanceBonus;
		m_otherOldValues[18] = m_otherStats.EngagementDistanceBonus;
		m_myOldValues[19] = m_myStats.DisengagementAccuracyBonus;
		m_otherOldValues[19] = m_otherStats.DisengagementAccuracyBonus;
		m_myOldValues[21] = ((!m_myStats.ImmuneToEngagement) ? 1f : 0f);
		m_otherOldValues[21] = ((!m_otherStats.ImmuneToEngagement) ? 1f : 0f);
		m_myOldValues[24] = m_myStats.MeleeAttackDistanceMultiplier;
		m_otherOldValues[24] = m_otherStats.MeleeAttackDistanceMultiplier;
		m_myOldValues[25] = m_myStats.RangedAttackDistanceMultiplier;
		m_otherOldValues[25] = m_otherStats.RangedAttackDistanceMultiplier;
		m_myOldValues[26] = m_myStats.BonusDTFromArmor;
		m_otherOldValues[26] = m_otherStats.BonusDTFromArmor;
		m_myOldValues[27] = m_myStats.MeleeDamageRangePctIncreaseToMin;
		m_otherOldValues[27] = m_otherStats.MeleeDamageRangePctIncreaseToMin;
		m_myOldValues[29] = ((!m_myStats.ImmuneToEngageStop) ? 1f : 0f);
		m_otherOldValues[29] = ((!m_otherStats.ImmuneToEngageStop) ? 1f : 0f);
		if (component3 != null && component4 != null)
		{
			component3.Secondary = true;
			m_myOldValues[31] = component3.Slot;
			m_otherOldValues[31] = component4.Slot;
		}
		m_myOldValues[33] = m_myStats.Level;
		m_otherOldValues[33] = m_otherStats.Level;
		m_myOldValues[36] = m_myStats.Experience;
		m_otherOldValues[36] = m_otherStats.Experience;
		m_myOldValues[37] = m_myStats.Intellect;
		m_otherOldValues[37] = m_otherStats.Intellect;
		m_myOldValues[38] = m_myStats.Might;
		m_otherOldValues[38] = m_otherStats.Might;
		m_myOldValues[39] = m_myStats.Constitution;
		m_otherOldValues[39] = m_otherStats.Constitution;
		m_myOldValues[40] = m_myStats.Dexterity;
		m_otherOldValues[40] = m_otherStats.Dexterity;
		m_myOldValues[41] = m_myStats.Perception;
		m_otherOldValues[41] = m_otherStats.Perception;
		m_myOldValues[42] = m_myStats.Resolve;
		m_otherOldValues[42] = m_otherStats.Resolve;
		m_myOldValues[43] = m_myStats.AthleticsSkill;
		m_otherOldValues[43] = m_otherStats.AthleticsSkill;
		m_myOldValues[44] = m_myStats.LoreSkill;
		m_otherOldValues[44] = m_otherStats.LoreSkill;
		m_myOldValues[45] = m_myStats.MechanicsSkill;
		m_otherOldValues[45] = m_otherStats.MechanicsSkill;
		m_myOldValues[46] = m_myStats.SurvivalSkill;
		m_otherOldValues[46] = m_otherStats.SurvivalSkill;
		m_myOldValues[47] = m_myStats.CraftingSkill;
		m_otherOldValues[47] = m_otherStats.CraftingSkill;
	}

	private void Update()
	{
		if (m_otherCharacter == null || m_otherHealth == null || m_otherCharacter == null)
		{
			return;
		}
		if (m_saveValues)
		{
			SaveCurrentValues();
			m_saveValues = false;
		}
		foreach (SharedData sharingRule in m_sharingRules)
		{
			switch (sharingRule.SharedStat)
			{
			case StatType.MaxHealth:
			{
				float myValue6 = m_myStats.Health;
				float otherValue6 = m_otherStats.Health;
				float healthPercentage = m_myHealth.HealthPercentage;
				float healthPercentage2 = m_otherHealth.HealthPercentage;
				if (sharingRule.ShareRule == ShareType.Combined)
				{
					otherValue6 = (myValue6 += otherValue6);
				}
				else
				{
					HandleDataSharing((int)sharingRule.SharedStat, ref myValue6, ref otherValue6, sharingRule.ShareRule);
				}
				m_myHealth.MaxHealth = myValue6;
				m_otherHealth.MaxHealth = otherValue6;
				m_myHealth.CurrentHealth = myValue6 * healthPercentage;
				m_otherHealth.CurrentHealth = otherValue6 * healthPercentage2;
				break;
			}
			case StatType.MaxStamina:
			{
				float myValue5 = m_myStats.Stamina;
				float otherValue5 = m_otherStats.Stamina;
				float staminaPercentage = m_myHealth.StaminaPercentage;
				float staminaPercentage2 = m_otherHealth.StaminaPercentage;
				if (sharingRule.ShareRule == ShareType.Combined)
				{
					otherValue5 = (myValue5 += otherValue5);
				}
				else
				{
					HandleDataSharing((int)sharingRule.SharedStat, ref myValue5, ref otherValue5, sharingRule.ShareRule);
				}
				m_myHealth.MaxStamina = myValue5;
				m_otherHealth.MaxStamina = otherValue5;
				m_myHealth.CurrentStamina = myValue5 * staminaPercentage;
				m_otherHealth.CurrentStamina = otherValue5 * staminaPercentage2;
				break;
			}
			case StatType.Health:
			{
				float myValue3 = m_myHealth.CurrentHealth;
				float otherValue3 = m_otherHealth.CurrentHealth;
				HandleDataSharing((int)sharingRule.SharedStat, ref myValue3, ref otherValue3, sharingRule.ShareRule);
				m_myHealth.CurrentHealth = myValue3;
				m_otherHealth.CurrentHealth = otherValue3;
				break;
			}
			case StatType.Stamina:
			{
				float myValue2 = m_myHealth.CurrentStamina;
				float otherValue2 = m_otherHealth.CurrentStamina;
				HandleDataSharing((int)sharingRule.SharedStat, ref myValue2, ref otherValue2, sharingRule.ShareRule);
				m_myHealth.CurrentStamina = myValue2;
				m_otherHealth.CurrentStamina = otherValue2;
				break;
			}
			case StatType.StaminaRechargeRate:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.StaminaRechargeBonus, ref m_otherStats.StaminaRechargeBonus, sharingRule.ShareRule);
				break;
			case StatType.AttackSpeed:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.AttackSpeedMultiplier, ref m_otherStats.AttackSpeedMultiplier, sharingRule.ShareRule);
				break;
			case StatType.StealthSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.StealthSkill, ref m_otherStats.StealthSkill, sharingRule.ShareRule);
				break;
			case StatType.DamageMinimum:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.DamageMinBonus, ref m_otherStats.DamageMinBonus, sharingRule.ShareRule);
				break;
			case StatType.MovementRate:
			{
				Mover component5 = GetComponent<Mover>();
				Mover component6 = m_otherCharacter.GetComponent<Mover>();
				HandleDataSharing((int)sharingRule.SharedStat, ref component5.RunSpeed, ref component6.RunSpeed, sharingRule.ShareRule);
				break;
			}
			case StatType.Targetable:
			{
				bool myValue8 = m_myHealth.Targetable;
				bool otherValue8 = m_otherHealth.Targetable;
				HandleDataSharing(ref myValue8, ref otherValue8, sharingRule.ShareRule);
				m_myHealth.Targetable = myValue8;
				m_otherHealth.Targetable = otherValue8;
				break;
			}
			case StatType.Mobility:
			{
				Mover component3 = GetComponent<Mover>();
				Mover component4 = m_otherCharacter.GetComponent<Mover>();
				bool myValue7 = component3.Frozen;
				bool otherValue7 = component4.Frozen;
				HandleDataSharing(ref myValue7, ref otherValue7, sharingRule.ShareRule);
				component3.Frozen = myValue7;
				component4.Frozen = otherValue7;
				break;
			}
			case StatType.EngagedEnemyCount:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.EngageableEnemyCount, ref m_otherStats.EngageableEnemyCount, sharingRule.ShareRule);
				break;
			case StatType.EngagementRadius:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.EngagementDistanceBonus, ref m_otherStats.EngagementDistanceBonus, sharingRule.ShareRule);
				break;
			case StatType.EngagementAccuracy:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.DisengagementAccuracyBonus, ref m_otherStats.DisengagementAccuracyBonus, sharingRule.ShareRule);
				break;
			case StatType.Enageable:
				HandleDataSharing(ref m_myStats.ImmuneToEngagement, ref m_otherStats.ImmuneToEngagement, sharingRule.ShareRule);
				break;
			case StatType.Damage:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.DamageMinBonus, ref m_otherStats.MaxHealth, sharingRule.ShareRule);
				break;
			case StatType.MeleeAttackDistanceMult:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.MeleeAttackDistanceMultiplier, ref m_otherStats.MeleeAttackDistanceMultiplier, sharingRule.ShareRule);
				break;
			case StatType.RangedAttackDistanceMult:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.RangedAttackDistanceMultiplier, ref m_otherStats.RangedAttackDistanceMultiplier, sharingRule.ShareRule);
				break;
			case StatType.BonusDTFromArmor:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BonusDTFromArmor, ref m_otherStats.BonusDTFromArmor, sharingRule.ShareRule);
				break;
			case StatType.MeleeDamageRangePctIncreaseToMin:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.MeleeDamageRangePctIncreaseToMin, ref m_otherStats.MeleeDamageRangePctIncreaseToMin, sharingRule.ShareRule);
				break;
			case StatType.ImmuneToEngageStop:
				HandleDataSharing(ref m_myStats.ImmuneToEngageStop, ref m_otherStats.ImmuneToEngageStop, sharingRule.ShareRule);
				break;
			case StatType.SlotNumber:
			{
				PartyMemberAI component = GetComponent<PartyMemberAI>();
				PartyMemberAI component2 = m_otherCharacter.GetComponent<PartyMemberAI>();
				if (component != null && component2 != null)
				{
					int myValue4 = component.Slot;
					int otherValue4 = component2.Slot;
					HandleDataSharing((int)sharingRule.SharedStat, ref myValue4, ref otherValue4, sharingRule.ShareRule);
					component.Slot = myValue4;
					component2.Slot = otherValue4;
				}
				break;
			}
			case StatType.Abilities:
				if (!m_abilitiesShared)
				{
					HandleSharingAbilities(sharingRule.ShareRule);
					m_abilitiesShared = true;
				}
				break;
			case StatType.Level:
			{
				int level = m_myStats.Level;
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.Level, ref m_otherStats.Level, sharingRule.ShareRule);
				if (level != m_myStats.Level)
				{
					int level2 = m_myStats.Level;
					m_myStats.Level = level;
					m_myStats.LevelUpToLevel(level2);
				}
				break;
			}
			case StatType.Experience:
			{
				int myValue = m_myStats.Experience;
				int otherValue = m_otherStats.Experience;
				HandleDataSharing((int)sharingRule.SharedStat, ref myValue, ref otherValue, sharingRule.ShareRule);
				m_myStats.Experience = myValue;
				m_otherStats.Experience = otherValue;
				break;
			}
			case StatType.Intellect:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BaseIntellect, ref m_otherStats.BaseIntellect, sharingRule.ShareRule);
				break;
			case StatType.Might:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BaseMight, ref m_otherStats.BaseMight, sharingRule.ShareRule);
				break;
			case StatType.Constitution:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BaseConstitution, ref m_otherStats.BaseConstitution, sharingRule.ShareRule);
				break;
			case StatType.Dexterity:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BaseDexterity, ref m_otherStats.BaseDexterity, sharingRule.ShareRule);
				break;
			case StatType.Perception:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BasePerception, ref m_otherStats.BasePerception, sharingRule.ShareRule);
				break;
			case StatType.Resolve:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.BaseResolve, ref m_otherStats.BaseResolve, sharingRule.ShareRule);
				break;
			case StatType.AthleticsSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.AthleticsSkill, ref m_otherStats.AthleticsSkill, sharingRule.ShareRule);
				break;
			case StatType.LoreSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.LoreSkill, ref m_otherStats.LoreSkill, sharingRule.ShareRule);
				break;
			case StatType.MechanicsSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.MechanicsSkill, ref m_otherStats.MechanicsSkill, sharingRule.ShareRule);
				break;
			case StatType.SurvivalSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.SurvivalSkill, ref m_otherStats.SurvivalSkill, sharingRule.ShareRule);
				break;
			case StatType.CraftingSkill:
				HandleDataSharing((int)sharingRule.SharedStat, ref m_myStats.CraftingSkill, ref m_otherStats.CraftingSkill, sharingRule.ShareRule);
				break;
			case StatType.StatusEffects:
				HandleSharingStatusEffects(sharingRule.ShareRule);
				break;
			}
		}
	}

	protected void HandleSharingStatusEffects(ShareType shareRule)
	{
		switch (shareRule)
		{
		case ShareType.Combined:
			foreach (StatusEffect activeStatusEffect in m_myStats.ActiveStatusEffects)
			{
				if (activeStatusEffect.Applied)
				{
					activeStatusEffect.ForcedAuraTargets.Add(m_otherCharacter);
				}
			}
			{
				foreach (StatusEffect activeStatusEffect2 in m_otherStats.ActiveStatusEffects)
				{
					if (activeStatusEffect2.Applied)
					{
						activeStatusEffect2.ForcedAuraTargets.Add(base.gameObject);
					}
				}
				break;
			}
		case ShareType.Highest:
		{
			foreach (StatusEffect activeStatusEffect3 in m_myStats.ActiveStatusEffects)
			{
				if (!activeStatusEffect3.Applied)
				{
					continue;
				}
				List<StatusEffect> list2 = m_otherStats.FindStatusEffectsOfType(activeStatusEffect3.Params.AffectsStat).ToList();
				bool flag2 = false;
				foreach (StatusEffect item in list2)
				{
					if (Mathf.Abs(activeStatusEffect3.CurrentAppliedValue) >= Mathf.Abs(item.CurrentAppliedValue))
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					foreach (StatusEffect item2 in list2)
					{
						m_otherStats.ClearEffect(item2);
					}
					activeStatusEffect3.ForcedAuraTargets.Add(m_otherCharacter);
					continue;
				}
				m_myStats.ClearEffect(activeStatusEffect3);
				foreach (StatusEffect activeStatusEffect4 in m_otherStats.ActiveStatusEffects)
				{
					activeStatusEffect4.ForcedAuraTargets.Add(base.gameObject);
				}
			}
			break;
		}
		case ShareType.Lowest:
		{
			foreach (StatusEffect activeStatusEffect5 in m_myStats.ActiveStatusEffects)
			{
				if (!activeStatusEffect5.Applied)
				{
					continue;
				}
				List<StatusEffect> list = m_otherStats.FindStatusEffectsOfType(activeStatusEffect5.Params.AffectsStat).ToList();
				bool flag = false;
				foreach (StatusEffect item3 in list)
				{
					if (Mathf.Abs(activeStatusEffect5.Params.Value) <= Mathf.Abs(item3.Params.Value))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					foreach (StatusEffect item4 in list)
					{
						m_otherStats.ClearEffect(item4);
					}
					activeStatusEffect5.ForcedAuraTargets.Add(m_otherCharacter);
					continue;
				}
				m_myStats.ClearEffect(activeStatusEffect5);
				foreach (StatusEffect activeStatusEffect6 in m_otherStats.ActiveStatusEffects)
				{
					activeStatusEffect6.ForcedAuraTargets.Add(base.gameObject);
				}
			}
			break;
		}
		case ShareType.Mine:
		{
			foreach (StatusEffect activeStatusEffect7 in m_myStats.ActiveStatusEffects)
			{
				if (activeStatusEffect7.Applied)
				{
					activeStatusEffect7.ForcedAuraTargets.Add(m_otherCharacter);
				}
			}
			break;
		}
		case ShareType.OtherCharacter:
		{
			foreach (StatusEffect activeStatusEffect8 in m_otherStats.ActiveStatusEffects)
			{
				if (activeStatusEffect8.Applied)
				{
					activeStatusEffect8.ForcedAuraTargets.Add(base.gameObject);
				}
			}
			break;
		}
		}
	}

	protected void HandleSharingAbilities(ShareType shareRule)
	{
		switch (shareRule)
		{
		case ShareType.Combined:
		case ShareType.Highest:
		case ShareType.Lowest:
		{
			List<GenericAbility> list = new List<GenericAbility>();
			list.AddRange(m_myStats.ActiveAbilities);
			m_myStats.ActiveAbilities.AddRange(m_otherStats.ActiveAbilities);
			m_otherStats.ActiveAbilities.AddRange(list);
			break;
		}
		case ShareType.Mine:
			m_otherStats.ActiveAbilities.AddRange(m_myStats.ActiveAbilities);
			break;
		case ShareType.OtherCharacter:
			m_myStats.ActiveAbilities.AddRange(m_otherStats.ActiveAbilities);
			break;
		}
	}

	protected void HandleDataSharing(int valueIndex, ref int myValue, ref int otherValue, ShareType shareRule)
	{
		switch (shareRule)
		{
		case ShareType.Combined:
		{
			int num = (int)(m_myOldValues[valueIndex] + m_otherOldValues[valueIndex]);
			if ((float)myValue == m_myOldValues[valueIndex] && (float)otherValue == m_otherOldValues[valueIndex])
			{
				myValue = num;
				otherValue = num;
			}
			else if (myValue < num)
			{
				otherValue = myValue;
			}
			else if (otherValue < num)
			{
				myValue = otherValue;
			}
			else
			{
				myValue = num;
				otherValue = num;
			}
			break;
		}
		case ShareType.Highest:
			if (myValue > otherValue)
			{
				otherValue = myValue;
			}
			else
			{
				myValue = otherValue;
			}
			break;
		case ShareType.Lowest:
			if (myValue < otherValue)
			{
				otherValue = myValue;
			}
			else
			{
				myValue = otherValue;
			}
			break;
		case ShareType.OtherCharacter:
			myValue = otherValue;
			break;
		case ShareType.Mine:
			otherValue = myValue;
			break;
		}
	}

	protected void HandleDataSharing(int valueIndex, ref float myValue, ref float otherValue, ShareType shareRule)
	{
		switch (shareRule)
		{
		case ShareType.Combined:
		{
			float num = m_myOldValues[valueIndex] + m_otherOldValues[valueIndex];
			if (myValue == m_myOldValues[valueIndex] && otherValue == m_otherOldValues[valueIndex])
			{
				myValue = num;
				otherValue = num;
			}
			else if (myValue < num)
			{
				otherValue = myValue;
			}
			else if (otherValue < num)
			{
				myValue = otherValue;
			}
			break;
		}
		case ShareType.Highest:
			if (myValue > otherValue)
			{
				otherValue = myValue;
			}
			else
			{
				myValue = otherValue;
			}
			break;
		case ShareType.Lowest:
			if (myValue < otherValue)
			{
				otherValue = myValue;
			}
			else
			{
				myValue = otherValue;
			}
			break;
		case ShareType.OtherCharacter:
			myValue = otherValue;
			break;
		case ShareType.Mine:
			otherValue = myValue;
			break;
		}
	}

	protected void HandleDataSharing(ref bool myValue, ref bool otherValue, ShareType shareRule)
	{
		switch (shareRule)
		{
		case ShareType.Combined:
			otherValue = (myValue &= otherValue);
			break;
		case ShareType.Highest:
			myValue = (otherValue = myValue | otherValue);
			break;
		case ShareType.Lowest:
			otherValue = (myValue &= otherValue);
			break;
		case ShareType.OtherCharacter:
			myValue = otherValue;
			break;
		case ShareType.Mine:
			otherValue = myValue;
			break;
		}
	}

	public void AddSharedData(SharedData data)
	{
		m_sharingRules.Add(data);
	}

	public void NotifySimultaneousHitStart(AttackBase attack)
	{
		SharedData sharedData = null;
		foreach (SharedData sharingRule in m_sharingRules)
		{
			if (sharingRule.SharedStat == StatType.SimultaneousHitDefenseBonus)
			{
				sharedData = sharingRule;
				break;
			}
		}
		if (sharedData != null)
		{
			StatusEffectParams statusEffectParams = new StatusEffectParams();
			statusEffectParams.OneHitUse = true;
			statusEffectParams.IsHostile = false;
			statusEffectParams.Value = sharedData.Value;
			statusEffectParams.Value += m_myStats.ExtraSimultaneousHitDefenseBonus;
			statusEffectParams.Value += m_otherStats.ExtraSimultaneousHitDefenseBonus;
			switch (attack.DefendedBy)
			{
			case CharacterStats.DefenseType.Deflect:
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Deflection;
				break;
			case CharacterStats.DefenseType.Fortitude:
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Fortitude;
				break;
			case CharacterStats.DefenseType.Reflex:
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Reflex;
				break;
			case CharacterStats.DefenseType.Will:
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Will;
				break;
			}
			m_myStats.ApplyStatusEffectImmediate(StatusEffect.Create(attack.Owner, statusEffectParams, GenericAbility.AbilityType.Ability, null, deleteOnClear: true));
			m_otherStats.ApplyStatusEffectImmediate(StatusEffect.Create(attack.Owner, statusEffectParams, GenericAbility.AbilityType.Ability, null, deleteOnClear: true));
			int num = OEIRandom.Range(0, 100);
			int num2 = OEIRandom.Range(0, 100);
			if (num2 < num)
			{
				num = num2;
			}
			m_myStats.SetAttackerToHitRollOverride(num);
			m_otherStats.SetAttackerToHitRollOverride(num);
		}
	}

	public void NotifySimultaneousHitEnd()
	{
		m_myStats.SetAttackerToHitRollOverride(-1);
		m_otherStats.SetAttackerToHitRollOverride(-1);
	}

	public void NotifyHeal(float amount)
	{
		foreach (SharedData sharingRule in m_sharingRules)
		{
			if (sharingRule.SharedStat == StatType.Stamina && sharingRule.ShareRule == ShareType.Lowest)
			{
				m_myHealth.ApplyStaminaChangeDirectly(amount, applyIfDead: true);
			}
		}
	}
}

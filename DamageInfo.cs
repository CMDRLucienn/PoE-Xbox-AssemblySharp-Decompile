using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DamageInfo
{
	private bool m_Immune;

	private float m_damageBase;

	private float m_damageAdd;

	private float m_damageMult;

	private bool m_damageZero;

	public bool TargetPreviouslyDead;

	public float MinDamageBonus;

	public List<StatusEffect> PostponedDisplayEffects;

	public bool IsKillingBlow;

	public HitType HitType = HitType.HIT;

	public HitType OriginalHitType = HitType.HIT;

	public HitType AttackerChangedToHitType = HitType.NONE;

	private AttackBase m_Attack;

	public GenericAbility Ability;

	public GameObject OtherOwner;

	public bool AttackIsHostile;

	public CharacterStats.DefenseType DefendedBy;

	public DamagePacket Damage;

	public DamagePacket.DamageType DamageType = DamagePacket.DamageType.None;

	public bool Interrupts;

	public bool IsMin;

	public bool AttackAbsorbed;

	public GameObject Self;

	public GameObject Target;

	public GenericAbility HitTypeChangeAbility;

	public int DefenseRating;

	public int AccuracyRating;

	public int RawRoll;

	public float DTAdjustedDamage;

	public float PostDtDamageMult = 1f;

	public float FinalAdjustedDamage;

	public float PreBypassDT;

	public float AttackerDTBypass;

	public float DTRating;

	public float DRRating;

	public bool WeaponFocusApplied;

	public float[] ProcDamage = new float[11];

	private string m_additionalAttackEffects = string.Empty;

	public bool IsMiss
	{
		get
		{
			if (HitType != 0)
			{
				return Immune;
			}
			return true;
		}
		set
		{
			if (value)
			{
				HitType = HitType.MISS;
			}
			else if (IsMiss)
			{
				HitType = HitType.HIT;
			}
		}
	}

	public bool IsGraze
	{
		get
		{
			if (HitType == HitType.GRAZE)
			{
				return !Immune;
			}
			return false;
		}
		set
		{
			if (value)
			{
				HitType = HitType.GRAZE;
			}
			else if (IsGraze)
			{
				HitType = HitType.HIT;
			}
		}
	}

	public bool IsCriticalHit
	{
		get
		{
			if (HitType == HitType.CRIT)
			{
				return !Immune;
			}
			return false;
		}
		set
		{
			if (value)
			{
				HitType = HitType.CRIT;
			}
			else if (IsCriticalHit)
			{
				HitType = HitType.HIT;
			}
		}
	}

	public bool IsPlainHit
	{
		get
		{
			if (HitType == HitType.HIT)
			{
				return !Immune;
			}
			return false;
		}
		set
		{
			if (value)
			{
				HitType = HitType.HIT;
			}
			else if (IsPlainHit)
			{
				HitType = HitType.MISS;
			}
		}
	}

	public bool Immune
	{
		get
		{
			return m_Immune;
		}
		set
		{
			m_Immune = value;
			if (m_Immune)
			{
				HitType = HitType.MISS;
			}
		}
	}

	public float DamageAmount => AdjustDamage(m_damageBase);

	public float DamageBase
	{
		get
		{
			return m_damageBase;
		}
		set
		{
			m_damageBase = value;
		}
	}

	public AttackBase Attack
	{
		get
		{
			return m_Attack;
		}
		set
		{
			m_Attack = value;
			if ((bool)m_Attack)
			{
				Ability = m_Attack.TriggeringAbility ?? m_Attack.AbilityOrigin ?? m_Attack.GetComponent<GenericAbility>();
			}
		}
	}

	public GameObject Owner => (Attack ? Attack.Owner : null) ?? (Ability ? Ability.Owner : null) ?? OtherOwner;

	public bool IsIneffective => (MaxDamage - DTRating) / MaxDamage <= 0.2f;

	public float MinDamage
	{
		get
		{
			if (Damage != null)
			{
				return Damage.GetMinimum(Owner);
			}
			return 0f;
		}
	}

	public float MaxDamage
	{
		get
		{
			if (Damage != null)
			{
				return Damage.GetMaximum(Owner);
			}
			return 0f;
		}
	}

	public float AdjustDamage(float damage)
	{
		if (m_damageZero)
		{
			return 0f;
		}
		return damage * (m_damageMult + 1f) + m_damageAdd;
	}

	public void DamageAdd(float bonus)
	{
		m_damageAdd += bonus;
	}

	public void DamageMult(float mult)
	{
		if (mult == 0f)
		{
			m_damageZero = true;
		}
		else
		{
			m_damageMult += mult - 1f;
		}
	}

	public DamageInfo()
	{
	}

	public DamageInfo(GameObject target, CharacterStats.DefenseType defense, GenericAbility ability)
	{
		Target = target;
		DefendedBy = defense;
		Ability = ability;
	}

	public DamageInfo(GameObject target, float damage, AttackBase attack)
	{
		Target = target;
		DamageBase = damage;
		Attack = attack;
		if (attack != null)
		{
			DefendedBy = attack.DefendedBy;
			Damage = new DamagePacket();
			Damage.Copy(attack.DamageData);
			DamageType = attack.DamageData.Type;
		}
	}

	public void SimpleAttackReport(string reportFormat, string attacker, string defender, string effectsInParen, StringBuilder stringBuilder)
	{
		string text = Mathf.Max(0f, DTAdjustedDamage).ToString((DTAdjustedDamage < 1f) ? "#0.0" : "#0");
		stringBuilder.AppendCatchFormat(reportFormat, attacker, GUIUtils.GetCombatLogAttackRollString(HitType), defender, text, GUIUtils.GetCombatLogDamageTypeString(Damage.Type), effectsInParen);
	}

	public void SimpleAttackReportNoDamage(string reportFormat, string attacker, string defender, StringBuilder stringBuilder)
	{
		stringBuilder.AppendCatchFormat(reportFormat, attacker, GUIUtils.GetCombatLogAttackRollString(HitType), defender);
	}

	public void SimpleMissReport(string reportFormat, string attacker, string defender, StringBuilder stringBuilder)
	{
		stringBuilder.AppendCatchFormat(reportFormat, attacker, GUIUtils.GetCombatLogAttackRollString(HitType.MISS), defender);
	}

	public void GetToHitReport(GameObject attacker, GameObject defender, StringBuilder stringBuilder)
	{
		int num = AccuracyRating - DefenseRating;
		string hitConversionString = GetHitConversionString(attacker, defender);
		string text = (string.IsNullOrEmpty(hitConversionString) ? "" : GUIUtils.Format(1731, hitConversionString));
		string text2;
		string text3;
		if (Immune)
		{
			text2 = GUIUtils.GetText(2187);
			text3 = text2;
		}
		else
		{
			text2 = DefenseRating.ToString();
			text3 = num.ToString();
		}
		stringBuilder.AppendCatchFormat("{0}:{1} - {2}:{3} = {4}.", GUIUtils.GetText(427), AccuracyRating, GUIUtils.GetDefenseTypeShortString(DefendedBy), text2, text3);
		if (!Immune)
		{
			stringBuilder.Append(" ");
			stringBuilder.AppendCatchFormat("{0}:{1} {2} {3} = {4}{5}.", GUIUtils.GetText(426), RawRoll, (num < 0) ? " - " : " + ", Mathf.Abs(num), RawRoll + num, text);
		}
	}

	public void GetDamageReport(GameObject enemy, StringBuilder stringBuilder)
	{
		if (!(MaxDamage > 0f) || Damage.Type == DamagePacket.DamageType.None)
		{
			return;
		}
		string text = "";
		if (PostDtDamageMult != 1f)
		{
			text = " × " + GUIUtils.GetText(166) + ":" + PostDtDamageMult.ToString("#0.##");
		}
		string text2 = Mathf.Max(0f, DTAdjustedDamage).ToString("#0.0") + " " + GUIUtils.GetDamageTypeString(Damage.Type) + (IsMin ? GUIUtils.Format(1731, GUIUtils.GetText(57)) : "");
		string text3 = "";
		if (Damage != null)
		{
			for (int i = 0; i < ProcDamage.Length; i++)
			{
				if (ProcDamage[i] > 0f)
				{
					text3 = text3 + " + " + ProcDamage[i].ToString("#0.0") + " " + GUIUtils.GetDamageTypeString((DamagePacket.DamageType)i);
				}
			}
		}
		if (Damage.Type == DamagePacket.DamageType.Raw)
		{
			stringBuilder.Append(DamageAmount.ToString("#0.0") + " = " + text2);
			return;
		}
		string text4 = (float.IsPositiveInfinity(PreBypassDT) ? GUIUtils.GetText(2187) : Mathf.Max(0f, PreBypassDT).ToString("#0.0"));
		string text5 = GUIUtils.GetText(1622) + ":" + text4;
		float num = AttackerDTBypass + Attack.DTBypassTotal;
		if (num > 0f)
		{
			text5 = GUIUtils.Format(1731, text5 + " - " + num.ToString("#0.0"));
		}
		stringBuilder.Append(DamageAmount.ToString("#0.0") + " - " + text5 + text + " = " + text2 + text3 + ".");
	}

	public string GetHitConversionString(GameObject attacker, GameObject defender)
	{
		string empty = string.Empty;
		empty += GetHitTypeString(OriginalHitType);
		if (AttackerChangedToHitType != HitType.NONE)
		{
			string text = "";
			if ((bool)HitTypeChangeAbility)
			{
				text = GenericAbility.Name(HitTypeChangeAbility);
			}
			else
			{
				CharacterStats enemy = (defender ? defender.GetComponent<CharacterStats>() : null);
				text = attacker.GetComponent<CharacterStats>().GetHitTypeChangeDescription(enemy, OriginalHitType, AttackerChangedToHitType);
			}
			empty = empty + GUIUtils.Format(2249, text) + GetHitTypeString(AttackerChangedToHitType);
			if (text != GUIUtils.GetText(2248))
			{
				AddAttackEffect(text);
			}
		}
		if (HitType != AttackerChangedToHitType && HitType != OriginalHitType)
		{
			HitType hitType = AttackerChangedToHitType;
			if (hitType == HitType.NONE)
			{
				hitType = OriginalHitType;
			}
			string text2 = "";
			if ((bool)HitTypeChangeAbility)
			{
				text2 = GenericAbility.Name(HitTypeChangeAbility);
			}
			else
			{
				CharacterStats enemy2 = (defender ? defender.GetComponent<CharacterStats>() : null);
				text2 = defender.GetComponent<CharacterStats>().GetHitTypeChangeDescription(enemy2, hitType, HitType);
			}
			empty = empty + GUIUtils.Format(2249, text2) + GetHitTypeString(HitType);
			if (text2 != GUIUtils.GetText(2248))
			{
				AddAttackEffect(text2);
			}
		}
		return empty;
	}

	public string GetAdjustedDamageRangeString()
	{
		return GUIUtils.Format(445, (AdjustDamage(MinDamage) + MinDamageBonus).ToString("#0"), AdjustDamage(MaxDamage).ToString("#0"));
	}

	public void AddAttackEffect(string attackEffect)
	{
		if (string.IsNullOrEmpty(m_additionalAttackEffects))
		{
			m_additionalAttackEffects = attackEffect;
			return;
		}
		m_additionalAttackEffects += GUIUtils.Format(1730, attackEffect);
	}

	public string GetExtraAttackEffects()
	{
		if (!string.IsNullOrEmpty(m_additionalAttackEffects))
		{
			return GUIUtils.Format(1731, m_additionalAttackEffects);
		}
		return string.Empty;
	}

	public static string GetHitTypeString(HitType type)
	{
		return type switch
		{
			HitType.MISS => GUIUtils.GetText(54), 
			HitType.GRAZE => GUIUtils.GetText(55), 
			HitType.CRIT => GUIUtils.GetText(56), 
			HitType.HIT => GUIUtils.GetText(330), 
			_ => string.Empty, 
		};
	}
}

using UnityEngine;

public class GenericSpell : GenericAbility
{
	public int SpellLevel = 1;

	public CharacterStats.Class SpellClass = CharacterStats.Class.Wizard;

	[Tooltip("If set, this spell is disallowed from being written into a grimoire.")]
	public bool ProhibitFromGrimoire;

	public int CostToLearn => SpellLevel * EconomyManager.Instance.LearnSpellCostMultiplier;

	[Persistent]
	public bool IsFree { get; set; }

	[Persistent]
	public bool NeedsGrimoire { get; set; }

	public StatusEffect StatusEffectGrantingSpell { get; set; }

	public static bool IgnoreSpellLimits => GameState.Instance.IgnoreSpellLimits;

	protected override void CalculateWhyNotReady()
	{
		base.CalculateWhyNotReady();
		GameObject owner = Owner;
		if (NeedsGrimoire && !GameState.Instance.IgnoreInGrimoire && owner != null)
		{
			Equipment component = owner.GetComponent<Equipment>();
			if (component == null || component.CurrentItems == null || component.CurrentItems.Grimoire == null)
			{
				base.WhyNotReady = NotReadyValue.NotInGrimoire;
			}
			else
			{
				Grimoire component2 = component.CurrentItems.Grimoire.GetComponent<Grimoire>();
				if (component2 == null || !component2.HasSpell(this))
				{
					base.WhyNotReady = NotReadyValue.NotInGrimoire;
				}
			}
		}
		if (m_ownerStats != null)
		{
			if (m_ownerStats.EffectDisablesSpellcasting)
			{
				base.WhyNotReady = NotReadyValue.SpellCastingDisabled;
			}
			if (m_ownerStats.CurrentGrimoireCooldown > 0f && NeedsGrimoire)
			{
				base.WhyNotReady = NotReadyValue.GrimoireCooldown;
			}
			if (!IgnoreSpellLimits && !IsFree && CooldownType != CooldownMode.PerEncounter && owner != null && m_ownerStats.SpellCastCount[SpellLevel - 1] >= SpellMax.Instance.GetSpellCastMax(owner, SpellLevel))
			{
				base.WhyNotReady = NotReadyValue.AtMaxPer;
			}
		}
	}

	public override void ActivateCooldown()
	{
		if (m_ownerStats != null && !IgnoreSpellLimits && !IsFree && CooldownType != CooldownMode.PerEncounter)
		{
			m_ownerStats.SpellCastCount[SpellLevel - 1]++;
		}
		base.ActivateCooldown();
	}

	public override void RestoreCooldown()
	{
		if (CooldownType != CooldownMode.PerEncounter && m_ownerStats != null && !IgnoreSpellLimits && !IsFree && m_ownerStats.SpellCastCount[SpellLevel - 1] > 0)
		{
			m_ownerStats.SpellCastCount[SpellLevel - 1]--;
		}
		base.RestoreCooldown();
	}

	public override void IncrementMasteryLevel()
	{
		base.IncrementMasteryLevel();
		if (MasteryLevel > 0)
		{
			NeedsGrimoire = false;
			Cooldown = 0f;
		}
	}

	public override int UsesLeft()
	{
		if (Owner == null || m_ownerStats == null || CooldownType == CooldownMode.PerEncounter)
		{
			return base.UsesLeft();
		}
		int spellCastMax = SpellMax.Instance.GetSpellCastMax(Owner, SpellLevel);
		if (spellCastMax == int.MaxValue)
		{
			return base.UsesLeft();
		}
		if (IgnoreSpellLimits || IsFree)
		{
			return Mathf.Max(1, spellCastMax);
		}
		int result = 0;
		if (m_ownerStats.SpellCastCount[SpellLevel - 1] < spellCastMax)
		{
			result = spellCastMax - m_ownerStats.SpellCastCount[SpellLevel - 1];
		}
		return result;
	}

	protected override void ReportActivation(bool overridePassive)
	{
		if (!HideFromCombatLog)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(125), CharacterStats.NameColored(m_owner), GenericAbility.Name(this)), Color.white);
		}
	}

	public override string GetFrequencyString()
	{
		if (MasteryLevel > 0)
		{
			return base.GetFrequencyString();
		}
		return GUIUtils.Format(374, Ordinal.Get(SpellLevel), GUIUtils.Format(1588, GUIUtils.GetClassString(SpellClass, Gender.Neuter)));
	}
}

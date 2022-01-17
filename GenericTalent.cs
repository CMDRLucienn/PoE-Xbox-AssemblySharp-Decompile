using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GenericTalent : MonoBehaviour, ITooltipContent
{
	public class NameComparer : IEqualityComparer<GenericTalent>
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

		public bool Equals(GenericTalent a, GenericTalent b)
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

		public int GetHashCode(GenericTalent talent)
		{
			return talent.DisplayName.StringID;
		}
	}

	public enum TalentType
	{
		GrantNewAbility,
		ModExistingAbility
	}

	public enum TalentCategory
	{
		Undefined,
		Class,
		Offense,
		Defense,
		MixedOrUtility
	}

	[Serializable]
	public class SkillBonus
	{
		public CharacterStats.SkillType Skill;

		public int Bonus;
	}

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public DatabaseString Description = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public Texture2D Icon;

	public TalentCategory Category;

	public TalentType Type;

	public GenericAbility[] Abilities = new GenericAbility[1];

	public AbilityMod[] AbilityMods;

	public SkillBonus[] SkillBonuses;

	public static string Name(GameObject go, GameObject owner)
	{
		if (!go)
		{
			Debug.LogError("Tried to get the name of a null game object.");
			return "*NameError*";
		}
		GenericTalent component = go.GetComponent<GenericTalent>();
		if ((bool)component)
		{
			return component.Name(owner);
		}
		Debug.LogError("Tried to get the name of something that wasn't a generic talent (" + go.name + ")");
		return "*NameError*";
	}

	public static string Name(MonoBehaviour mb, GameObject owner)
	{
		return Name(mb.gameObject, owner);
	}

	public string Name(GameObject owner)
	{
		if ((bool)owner)
		{
			return StringUtility.Format(DisplayName, CharacterStats.Name(owner));
		}
		return DisplayName.GetText();
	}

	public int GetSkillAdjustment(CharacterStats.SkillType skill)
	{
		int num = 0;
		SkillBonus[] skillBonuses = SkillBonuses;
		foreach (SkillBonus skillBonus in skillBonuses)
		{
			if (skill == skillBonus.Skill)
			{
				num += skillBonus.Bonus;
			}
		}
		GenericAbility[] abilities = Abilities;
		for (int i = 0; i < abilities.Length; i++)
		{
			StatusEffectParams[] statusEffects = abilities[i].StatusEffects;
			foreach (StatusEffectParams statusEffectParams in statusEffects)
			{
				switch (statusEffectParams.AffectsStat)
				{
				case StatusEffect.ModifiedStat.Stealth:
					if (skill == CharacterStats.SkillType.Stealth)
					{
						num += (int)statusEffectParams.Value;
					}
					break;
				case StatusEffect.ModifiedStat.Athletics:
					if (skill == CharacterStats.SkillType.Athletics)
					{
						num += (int)statusEffectParams.Value;
					}
					break;
				case StatusEffect.ModifiedStat.Lore:
					if (skill == CharacterStats.SkillType.Lore)
					{
						num += (int)statusEffectParams.Value;
					}
					break;
				case StatusEffect.ModifiedStat.Mechanics:
					if (skill == CharacterStats.SkillType.Mechanics)
					{
						num += (int)statusEffectParams.Value;
					}
					break;
				case StatusEffect.ModifiedStat.Survival:
					if (skill == CharacterStats.SkillType.Survival)
					{
						num += (int)statusEffectParams.Value;
					}
					break;
				}
			}
		}
		return num;
	}

	public void Purchase(GameObject owner)
	{
		if (owner == null)
		{
			return;
		}
		CharacterStats component = owner.GetComponent<CharacterStats>();
		if (component == null || component.ActiveTalents.Contains(this, NameComparer.Instance))
		{
			return;
		}
		component.ActiveTalents.Add(this);
		if (Abilities != null)
		{
			switch (Type)
			{
			case TalentType.GrantNewAbility:
			{
				GenericAbility[] abilities = Abilities;
				foreach (GenericAbility ability in abilities)
				{
					component.InstantiateAbility(ability, GenericAbility.AbilityType.Talent);
				}
				break;
			}
			case TalentType.ModExistingAbility:
			{
				if (AbilityMods == null)
				{
					break;
				}
				GenericAbility[] abilities = Abilities;
				foreach (GenericAbility a in abilities)
				{
					foreach (GenericAbility activeAbility in component.ActiveAbilities)
					{
						if (GenericAbility.NameComparer.Instance.Equals(a, activeAbility))
						{
							AbilityMod[] abilityMods = AbilityMods;
							foreach (AbilityMod mod in abilityMods)
							{
								activeAbility.AddAbilityMod(mod, GenericAbility.AbilityType.Talent);
							}
						}
					}
				}
				break;
			}
			}
		}
		if (SkillBonuses != null)
		{
			SkillBonus[] skillBonuses = SkillBonuses;
			foreach (SkillBonus skillBonus in skillBonuses)
			{
				component.AdjustSkillBonus(skillBonus.Skill, skillBonus.Bonus);
			}
		}
	}

	public void Remove(GameObject owner)
	{
		if (owner == null)
		{
			return;
		}
		CharacterStats component = owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		component.ActiveTalents.Remove(this);
		if (Abilities != null)
		{
			switch (Type)
			{
			case TalentType.GrantNewAbility:
			{
				GenericAbility[] abilities = Abilities;
				foreach (GenericAbility ability in abilities)
				{
					component.RemoveAbility(ability);
				}
				break;
			}
			case TalentType.ModExistingAbility:
			{
				if (AbilityMods == null)
				{
					break;
				}
				GenericAbility[] abilities = Abilities;
				foreach (GenericAbility a in abilities)
				{
					foreach (GenericAbility activeAbility in component.ActiveAbilities)
					{
						if (GenericAbility.NameComparer.Instance.Equals(a, activeAbility))
						{
							AbilityMod[] abilityMods = AbilityMods;
							foreach (AbilityMod mod in abilityMods)
							{
								activeAbility.RemoveAbilityMod(mod, GenericAbility.AbilityType.Talent);
							}
						}
					}
				}
				break;
			}
			}
		}
		if (SkillBonuses != null)
		{
			SkillBonus[] skillBonuses = SkillBonuses;
			foreach (SkillBonus skillBonus in skillBonuses)
			{
				component.AdjustSkillBonus(skillBonus.Skill, -skillBonus.Bonus);
			}
		}
	}

	public void CheckNewAbility(GenericAbility ability)
	{
		if (Type != TalentType.ModExistingAbility || Abilities == null || AbilityMods == null)
		{
			return;
		}
		GenericAbility[] abilities = Abilities;
		foreach (GenericAbility a in abilities)
		{
			if (GenericAbility.NameComparer.Instance.Equals(a, ability))
			{
				AbilityMod[] abilityMods = AbilityMods;
				foreach (AbilityMod mod in abilityMods)
				{
					ability.AddAbilityMod(mod, GenericAbility.AbilityType.Talent);
				}
			}
		}
	}

	public bool ContainsSkillBonus(CharacterStats.SkillType skill)
	{
		return SkillBonuses.Any((SkillBonus sk) => sk.Skill == skill);
	}

	public int GetSkillBonus(CharacterStats.SkillType skill)
	{
		return SkillBonuses.Aggregate(0, (int res, SkillBonus sk) => res + sk.Bonus);
	}

	public string GetString(GameObject owner, StatusEffectFormatMode mode, bool onAnimalCompanion = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Type == TalentType.GrantNewAbility)
		{
			GenericAbility[] abilities = Abilities;
			foreach (GenericAbility genericAbility in abilities)
			{
				stringBuilder.AppendLine(GUIUtils.Format(1023, GenericAbility.Name(genericAbility)));
				stringBuilder.AppendLine(NGUITools.StripColorSymbols(genericAbility.GetStringFlat(summary: true, owner, mode, onAnimalCompanion)));
			}
		}
		else if (Type == TalentType.ModExistingAbility)
		{
			IEnumerable<GenericAbility> source = Abilities.Where((GenericAbility abil) => !abil.HideFromUi);
			if (source.Any())
			{
				stringBuilder.AppendGuiFormat(1022, string.Join(", ", source.Select((GenericAbility abil) => GenericAbility.Name(abil)).ToArray()));
				stringBuilder.AppendLine();
			}
			AbilityMod[] abilityMods = AbilityMods;
			for (int i = 0; i < abilityMods.Length; i++)
			{
				string @string = abilityMods[i].GetString(owner);
				if (!string.IsNullOrEmpty(@string))
				{
					stringBuilder.AppendLine("   " + @string);
				}
			}
			if (source.Count() == 1)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(GenericAbility.Name(source.First()));
				stringBuilder.AppendLine(source.First().GetStringFlat(summary: true, owner, mode, onAnimalCompanion));
			}
		}
		return stringBuilder.ToString().Trim();
	}

	public string GetTooltipContent(GameObject owner)
	{
		return Description.GetText();
	}

	public string GetTooltipName(GameObject owner)
	{
		return Name(owner);
	}

	public Texture GetTooltipIcon()
	{
		return Icon;
	}
}

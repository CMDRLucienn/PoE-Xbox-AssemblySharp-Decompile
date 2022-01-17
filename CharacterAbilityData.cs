using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterAbilityData : MonoBehaviour
{
	[Flags]
	public enum AbilitySearchFlag
	{
		None = 0,
		IncludeAlreadyOwnedAbilities = 1,
		OnlyAutoReceiveOnRequirementsMet = 2
	}

	[Serializable]
	public class AttributeScorePair
	{
		public CharacterStats.AttributeScoreType attribute;

		public int MinimumValue;
	}

	[Serializable]
	public class AbilityRequirements
	{
		public string DesignNote = "";

		public int MinimumLevel = 1;

		public CharacterStats.Class Class;

		public CharacterStats.Subrace Subrace;

		public CharacterStats.Background Background;

		public GenericAbility[] Abilities;

		public AttributeScorePair[] Attributes;
	}

	[Serializable]
	public class UnlockableAbility
	{
		public string DesignNote = "";

		public GenericAbility Ability;

		public AbilityRequirements[] RequirementSets;

		public bool AutoReceiveOnRequirementsMet = true;
	}

	[Serializable]
	public class UnlockableAbilitySet
	{
		public string DesignNote = "";

		public UnlockableAbility[] UnlockableAbilities;
	}

	public UnlockableAbilitySet[] ClassAblities;

	public UnlockableAbilitySet[] RaceAbilities;

	public UnlockableAbilitySet[] Talents;

	public static CharacterAbilityData Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'CharacterAbilityData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private static bool HasSearchFlag(AbilitySearchFlag flags, AbilitySearchFlag value)
	{
		return (flags & value) == value;
	}

	private static bool RequirementsMet(CharacterStats characterStats, AbilityRequirements abilityRequirement)
	{
		if (characterStats.Level < abilityRequirement.MinimumLevel)
		{
			return false;
		}
		if (abilityRequirement.Class != 0 && abilityRequirement.Class != characterStats.CharacterClass)
		{
			return false;
		}
		if (abilityRequirement.Subrace != 0 && abilityRequirement.Subrace != characterStats.CharacterSubrace)
		{
			return false;
		}
		if (abilityRequirement.Background != 0 && abilityRequirement.Background != characterStats.CharacterBackground)
		{
			return false;
		}
		IList<GenericAbility> activeAbilities = characterStats.ActiveAbilities;
		GenericAbility[] abilities = abilityRequirement.Abilities;
		foreach (GenericAbility value in abilities)
		{
			if (!activeAbilities.Contains(value, GenericAbility.NameComparer.Instance))
			{
				return false;
			}
		}
		AttributeScorePair[] attributes = abilityRequirement.Attributes;
		foreach (AttributeScorePair attributeScorePair in attributes)
		{
			if (attributeScorePair.MinimumValue < characterStats.GetAttributeScore(attributeScorePair.attribute))
			{
				return false;
			}
		}
		return true;
	}

	public static UnlockableAbility[] GetAbilities(GenericAbility.AbilityType abilityType, CharacterStats characterStats, AbilitySearchFlag searchFlags)
	{
		List<UnlockableAbility> list = new List<UnlockableAbility>();
		UnlockableAbilitySet[] array = null;
		array = abilityType switch
		{
			GenericAbility.AbilityType.Racial => Instance.RaceAbilities, 
			GenericAbility.AbilityType.Talent => Instance.Talents, 
			_ => Instance.ClassAblities, 
		};
		IList<GenericAbility> activeAbilities = characterStats.ActiveAbilities;
		UnlockableAbilitySet[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			UnlockableAbility[] unlockableAbilities = array2[i].UnlockableAbilities;
			foreach (UnlockableAbility unlockableAbility in unlockableAbilities)
			{
				if ((!HasSearchFlag(searchFlags, AbilitySearchFlag.IncludeAlreadyOwnedAbilities) && activeAbilities.Contains(unlockableAbility.Ability, GenericAbility.NameComparer.Instance)) || (HasSearchFlag(searchFlags, AbilitySearchFlag.OnlyAutoReceiveOnRequirementsMet) && !unlockableAbility.AutoReceiveOnRequirementsMet))
				{
					continue;
				}
				AbilityRequirements[] requirementSets = unlockableAbility.RequirementSets;
				foreach (AbilityRequirements abilityRequirement in requirementSets)
				{
					if (RequirementsMet(characterStats, abilityRequirement))
					{
						list.Add(unlockableAbility);
						break;
					}
				}
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}
}

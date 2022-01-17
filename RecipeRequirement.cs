using System;

[Serializable]
public class RecipeRequirement
{
	public enum RecipeRequirementType
	{
		Talent,
		Ability,
		Skill,
		Global,
		StrongholdUpgrade,
		PlayerMinimumLevel
	}

	public RecipeRequirementType Type;

	public StrongholdUpgrade.Type Upgrade;

	public string Tag;

	public int Value;

	public bool And;

	public static bool CheckRequirements(RecipeRequirement[] requirements)
	{
		if (requirements == null)
		{
			return true;
		}
		bool flag = false;
		bool flag2 = true;
		foreach (RecipeRequirement recipeRequirement in requirements)
		{
			if (flag)
			{
				if (!recipeRequirement.And)
				{
					flag = false;
				}
				continue;
			}
			flag2 = false;
			switch (recipeRequirement.Type)
			{
			case RecipeRequirementType.Talent:
			case RecipeRequirementType.Ability:
				flag2 = PartyHelper.PartyHasTalentOrAbility(recipeRequirement.Tag);
				break;
			case RecipeRequirementType.Skill:
				flag2 = PartyHelper.PartyHasSkill(recipeRequirement.Tag, recipeRequirement.Value);
				break;
			case RecipeRequirementType.Global:
				flag2 = GlobalVariables.Instance.GetVariable(recipeRequirement.Tag) == recipeRequirement.Value;
				break;
			case RecipeRequirementType.StrongholdUpgrade:
				flag2 = GameState.Stronghold.HasUpgrade(recipeRequirement.Upgrade);
				break;
			case RecipeRequirementType.PlayerMinimumLevel:
			{
				CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
				if (component != null)
				{
					flag2 = component.Level >= recipeRequirement.Value;
				}
				break;
			}
			}
			if (flag2 && !recipeRequirement.And)
			{
				break;
			}
			if (!flag2 && recipeRequirement.And)
			{
				flag = true;
			}
		}
		return flag2;
	}
}

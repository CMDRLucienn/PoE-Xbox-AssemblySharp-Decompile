using UnityEngine;

public class RecipeCategory : MonoBehaviour
{
	public ItemMod.EnchantCategory EnchantCategory;

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Recipes);

	public override bool Equals(object o)
	{
		if (o is RecipeCategory)
		{
			return ((RecipeCategory)o).DisplayName.Equals(DisplayName);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return DisplayName.GetHashCode();
	}
}

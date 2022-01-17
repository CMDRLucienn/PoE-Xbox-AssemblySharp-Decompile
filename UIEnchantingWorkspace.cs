public class UIEnchantingWorkspace : UICraftingWorkspace
{
	public UIEnchantingItemArea ItemArea;

	public override void LoadRecipe(Recipe recipe)
	{
		ItemArea.LoadItem(UICraftingManager.Instance.EnchantTarget);
		base.LoadRecipe(recipe);
	}
}

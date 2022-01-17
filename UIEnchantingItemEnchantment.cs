using UnityEngine;

public class UIEnchantingItemEnchantment : MonoBehaviour
{
	public UISprite SpriteCategory;

	public UILabel LblName;

	public UISprite EnchantingIcon;

	public UILabel EnchantingQty;

	public void LoadEnchantment(ItemMod mod)
	{
		LblName.text = mod.GetTooltipName(mod.gameObject);
		SpriteCategory.spriteName = UICraftingManager.GetCategorySpriteName(mod.ModEnchantCategory);
		SpriteCategory.gameObject.SetActive(!string.IsNullOrEmpty(SpriteCategory.spriteName));
		EnchantingIcon.color = UICraftingManager.Instance.ColorEnchantEnabled;
		EnchantingQty.text = mod.Cost.ToString();
	}
}

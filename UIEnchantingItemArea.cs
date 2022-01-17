using System.Collections.Generic;
using UnityEngine;

public class UIEnchantingItemArea : MonoBehaviour
{
	public UITexture TexEnchantingItem;

	public UISprite SpriteTotalEnchantIcon;

	public UILabel LblItemName;

	public UILabel LblItemQty;

	public UIEnchantingItemEnchantment RootEnchanchment;

	public UITable LayoutEnchantments;

	private const float MAX_TEX_SIZE = 96f;

	private List<UIEnchantingItemEnchantment> m_itemEnchantments = new List<UIEnchantingItemEnchantment>();

	public void LoadItem(Item itemToLoad)
	{
		if (itemToLoad == null)
		{
			return;
		}
		Equippable component = itemToLoad.GetComponent<Equippable>();
		if ((bool)component)
		{
			TexEnchantingItem.mainTexture = itemToLoad.GetIconLargeTexture();
			TexEnchantingItem.MakePixelPerfect();
			float num = 1f;
			if (TexEnchantingItem.transform.localScale.x > 96f)
			{
				num = 96f / TexEnchantingItem.transform.localScale.x;
			}
			if (TexEnchantingItem.transform.localScale.y > 96f)
			{
				num = Mathf.Min(96f / TexEnchantingItem.transform.localScale.y, num);
			}
			if (!Mathf.Approximately(num, 1f))
			{
				TexEnchantingItem.transform.localScale = TexEnchantingItem.transform.localScale * num;
			}
			LblItemName.text = itemToLoad.Name;
			SpriteTotalEnchantIcon.color = UICraftingManager.Instance.ColorEnchantEnabled;
			LblItemQty.text = GUIUtils.Format(451, component.TotalItemModValue(), ItemMod.MaximumModValue);
			ReloadEnchantments(component.Mods);
		}
	}

	private void ReloadEnchantments(List<ItemMod> allMods)
	{
		List<ItemMod> list = new List<ItemMod>();
		for (int i = 0; i < allMods.Count; i++)
		{
			if (allMods[i] != null && (allMods[i].Cost > 0 || allMods[i].ShowEvenIfCostZero))
			{
				list.Add(allMods[i]);
			}
		}
		while (m_itemEnchantments.Count < list.Count)
		{
			UIEnchantingItemEnchantment component = NGUITools.AddChild(LayoutEnchantments.gameObject, RootEnchanchment.gameObject).GetComponent<UIEnchantingItemEnchantment>();
			m_itemEnchantments.Add(component);
		}
		for (int j = 0; j < m_itemEnchantments.Count; j++)
		{
			if (j >= list.Count)
			{
				m_itemEnchantments[j].gameObject.SetActive(value: false);
				continue;
			}
			m_itemEnchantments[j].gameObject.SetActive(value: true);
			m_itemEnchantments[j].LoadEnchantment(list[j]);
		}
		LayoutEnchantments.Reposition();
	}
}

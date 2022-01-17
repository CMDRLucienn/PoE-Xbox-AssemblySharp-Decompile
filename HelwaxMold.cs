using System;
using UnityEngine;

public class HelwaxMold : Consumable
{
	protected override GenericAbility SetUpUse(GameObject owner)
	{
		if (!UIInventoryManager.Instance.WindowActive())
		{
			UIInventoryManager.Instance.ShowWindow();
		}
		InGameHUD.Instance.EnterHelwaxMode(this);
		UIGlobalInventory.Instance.CancelDrag();
		m_originalItem = this;
		return null;
	}

	public void TargetHelwax(Equippable existing)
	{
		if (!existing)
		{
			throw new ArgumentNullException("existing");
		}
		Equippable component = GameResources.Instantiate<GameObject>(existing.gameObject).GetComponent<Equippable>();
		component.Prefab = existing.Prefab;
		for (int num = component.AttachedItemMods.Count - 1; num >= 0; num--)
		{
			component.DestroyItemMod(component.AttachedItemMods[num]);
		}
		for (int i = 0; i < existing.AttachedItemMods.Count; i++)
		{
			component.AttachItemMod(existing.AttachedItemMods[i].Mod);
		}
		GameState.s_playerCharacter.GetComponent<StashInventory>().AddItem(component, 1);
		UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, base.Name, GUIUtils.Format(2333, component.Name));
		EndUse(null);
	}
}

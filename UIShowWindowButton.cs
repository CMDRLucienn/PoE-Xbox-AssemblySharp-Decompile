using UnityEngine;

public class UIShowWindowButton : MonoBehaviour
{
	public UIHudWindow Window;

	public bool Toggle;

	public bool QuestItems;

	public bool CraftingItems;

	public bool StashItems;

	private void OnClick()
	{
		if (UIInventoryManager.Instance.DraggingItem || UIInventoryItemReciever.RecievedThisFrame)
		{
			return;
		}
		if (QuestItems)
		{
			UIStashManager.Instance.ShowOnTab = UIInventoryFilter.ItemFilterType.QUEST;
			if (Toggle)
			{
				UIStashManager.Instance.Toggle();
			}
			else
			{
				UIStashManager.Instance.ShowWindow();
			}
		}
		else if (CraftingItems)
		{
			UIStashManager.Instance.ShowOnTab = UIInventoryFilter.ItemFilterType.INGREDIENTS;
			if (Toggle)
			{
				UIStashManager.Instance.Toggle();
			}
			else
			{
				UIStashManager.Instance.ShowWindow();
			}
		}
		else if (StashItems)
		{
			if (Toggle)
			{
				UIStashManager.Instance.Toggle();
			}
			else
			{
				UIStashManager.Instance.ShowWindow();
			}
		}
		else if (Toggle)
		{
			UIWindowManager.Instance.ToggleWindow(Window.GetType());
		}
		else
		{
			UIWindowManager.Instance.ShowWindow(Window.GetType());
		}
	}
}

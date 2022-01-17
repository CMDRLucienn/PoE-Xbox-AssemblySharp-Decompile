using UnityEngine;

public class UIInventoryListenerWidget : MonoBehaviour
{
	public enum Mode
	{
		STASH,
		CUSTOM
	}

	public Mode ListenTo;

	private BaseInventory m_Inventory;

	public UITweener OnChanged;

	private void Awake()
	{
		if (ListenTo == Mode.STASH)
		{
			StashInventory inventory = null;
			if (GameState.s_playerCharacter != null)
			{
				inventory = GameState.s_playerCharacter.GetComponent<StashInventory>();
			}
			Load(inventory);
		}
	}

	private void OnDestroy()
	{
		Unsubscribe();
	}

	private void Unsubscribe()
	{
		if ((bool)m_Inventory)
		{
			m_Inventory.OnChanged -= OnInventoryChanged;
		}
	}

	public void Load(BaseInventory inventory)
	{
		Unsubscribe();
		m_Inventory = inventory;
		if ((bool)m_Inventory)
		{
			m_Inventory.OnChanged += OnInventoryChanged;
		}
	}

	private void OnInventoryChanged(BaseInventory sender)
	{
		if ((bool)OnChanged)
		{
			OnChanged.Reset();
			OnChanged.Play(forward: true);
		}
	}
}

using UnityEngine;

public class UICraftingTrigger : MonoBehaviour
{
	private UIIsButton m_Button;

	private void Awake()
	{
		m_Button = GetComponent<UIIsButton>();
	}

	private void Update()
	{
		m_Button.enabled = !UIInventoryManager.Instance.DraggingItem && !GameState.InCombat;
	}

	private void OnClick()
	{
		if (base.enabled)
		{
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.EnchantMode = false;
			UICraftingManager.Instance.ShowWindow();
		}
	}
}

using System;
using UnityEngine;

public class UIConsoleTabs : MonoBehaviour
{
	public UIMultiSpriteImageButton DialogTab;

	public UIMultiSpriteImageButton CombatTab;

	private UIWidget[] m_DialogWidgets;

	private UIWidget[] m_CombatWidgets;

	public int TabDepthChange = 2;

	private bool m_Combat = true;

	private void Start()
	{
		UIMultiSpriteImageButton dialogTab = DialogTab;
		dialogTab.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(dialogTab.onClick, new UIEventListener.VoidDelegate(OnDialogClick));
		UIMultiSpriteImageButton combatTab = CombatTab;
		combatTab.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(combatTab.onClick, new UIEventListener.VoidDelegate(OnCombatClick));
		m_DialogWidgets = DialogTab.GetComponentsInChildren<UIWidget>(includeInactive: true);
		m_CombatWidgets = CombatTab.GetComponentsInChildren<UIWidget>(includeInactive: true);
		UIWindowManager.IncreaseSpriteDepthRecursive(CombatTab.gameObject, TabDepthChange);
		CombatTab.ForceHighlight(state: true);
		UIWidget[] dialogWidgets = m_DialogWidgets;
		for (int i = 0; i < dialogWidgets.Length; i++)
		{
			dialogWidgets[i].alpha = 0.75f;
		}
	}

	public void ForceShowDialogue()
	{
		OnDialogClick(null);
	}

	public void ForceShowCombat()
	{
		OnCombatClick(null);
	}

	private void OnDialogClick(GameObject go)
	{
		if (m_Combat)
		{
			m_Combat = false;
			UIWindowManager.IncreaseSpriteDepthRecursive(CombatTab.gameObject, -TabDepthChange);
			CombatTab.ForceHighlight(state: false);
			UIWindowManager.IncreaseSpriteDepthRecursive(DialogTab.gameObject, TabDepthChange);
			DialogTab.ForceHighlight(state: true);
			UIConsole.Instance.LogStateChanged(m_Combat);
			UIWidget[] dialogWidgets = m_DialogWidgets;
			for (int i = 0; i < dialogWidgets.Length; i++)
			{
				dialogWidgets[i].alpha = 1f;
			}
			dialogWidgets = m_CombatWidgets;
			for (int i = 0; i < dialogWidgets.Length; i++)
			{
				dialogWidgets[i].alpha = 0.75f;
			}
		}
	}

	private void OnCombatClick(GameObject go)
	{
		if (!m_Combat)
		{
			m_Combat = true;
			UIWindowManager.IncreaseSpriteDepthRecursive(CombatTab.gameObject, TabDepthChange);
			CombatTab.ForceHighlight(state: true);
			UIWindowManager.IncreaseSpriteDepthRecursive(DialogTab.gameObject, -TabDepthChange);
			DialogTab.ForceHighlight(state: false);
			UIConsole.Instance.LogStateChanged(m_Combat);
			UIWidget[] dialogWidgets = m_DialogWidgets;
			for (int i = 0; i < dialogWidgets.Length; i++)
			{
				dialogWidgets[i].alpha = 0.75f;
			}
			dialogWidgets = m_CombatWidgets;
			for (int i = 0; i < dialogWidgets.Length; i++)
			{
				dialogWidgets[i].alpha = 1f;
			}
		}
	}
}

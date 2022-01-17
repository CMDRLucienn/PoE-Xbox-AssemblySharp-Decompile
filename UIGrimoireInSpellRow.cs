using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIGrimoireInSpellRow : MonoBehaviour
{
	private List<UIGrimoireSpell> m_Spells;

	public UIGrimoireSpell RootSpell;

	public UIGrid Grid;

	public UIWidget AdditiveHighlight;

	private int m_SpellLevel;

	private GenericSpell m_RemoveSpell;

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Init()
	{
		if (m_Spells != null)
		{
			return;
		}
		m_Spells = new List<UIGrimoireSpell>();
		m_Spells.Add(RootSpell);
		for (int i = 1; i < 4; i++)
		{
			m_Spells.Add(NGUITools.AddChild(RootSpell.transform.parent.gameObject, RootSpell.gameObject).GetComponent<UIGrimoireSpell>());
		}
		foreach (UIGrimoireSpell spell in m_Spells)
		{
			UIEventListener uIEventListener = UIEventListener.Get(spell.Icon.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			UIEventListener uIEventListener2 = UIEventListener.Get(spell.Icon.gameObject);
			uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
		}
		UIGrimoireLevelButtons levelButtons = UIGrimoireManager.Instance.LevelButtons;
		levelButtons.LevelChanged = (UIGrimoireLevelButtons.OnLevelChanged)Delegate.Combine(levelButtons.LevelChanged, new UIGrimoireLevelButtons.OnLevelChanged(SelectedLevelChanged));
		Grid.Reposition();
	}

	private void SelectedLevelChanged(int level)
	{
		bool flag = level == m_SpellLevel;
		AdditiveHighlight.alpha = (flag ? 1 : 0);
	}

	private void OnChildRightClick(GameObject sender)
	{
		UIGrimoireSpell componentInParent = sender.GetComponentInParent<UIGrimoireSpell>();
		if (UIGrimoireManager.Instance.CanEditGrimoire)
		{
			UIItemInspectManager.ExamineLearn(componentInParent.Spell, UIGrimoireManager.Instance.SelectedCharacter.gameObject);
		}
		else
		{
			UIItemInspectManager.Examine(componentInParent.Spell, UIGrimoireManager.Instance.SelectedCharacter.gameObject);
		}
	}

	private void OnChildClick(GameObject sender)
	{
		UIGrimoireSpell componentInParent = sender.GetComponentInParent<UIGrimoireSpell>();
		CharacterStats selectedCharacter = UIGrimoireManager.Instance.SelectedCharacter;
		bool flag = selectedCharacter != null && selectedCharacter.ActiveAbilities.Contains(componentInParent.Spell, GenericAbility.NameComparer.Instance);
		if ((bool)componentInParent && !componentInParent.Disabled && InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.None)
		{
			UIGrimoireManager.Instance.LevelButtons.ChangeLevel(m_SpellLevel);
			if (!flag)
			{
				m_RemoveSpell = componentInParent.Spell;
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, GUIUtils.GetText(416), GUIUtils.Format(417, CharacterStats.Name(UIGrimoireManager.Instance.SelectedCharacter.gameObject), GenericAbility.Name(componentInParent.Spell)));
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnRemoveSpellEnd));
			}
			else
			{
				RemoveSpell(componentInParent.Spell);
			}
		}
	}

	private void OnRemoveSpellEnd(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			RemoveSpell(m_RemoveSpell);
		}
		m_RemoveSpell = null;
	}

	private void RemoveSpell(GenericSpell spell)
	{
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.RemoveSpellGrimoire);
		}
		UIGrimoireManager.Instance.LoadedGrimoire.Spells[m_SpellLevel - 1].SpellData.Remove(spell);
		UIGrimoireManager.Instance.CoolDownGrimoire();
		Reload(m_SpellLevel);
		if ((bool)UIGrimoireManager.Instance.SelectedCharacter)
		{
			PartyMemberAI component = UIGrimoireManager.Instance.SelectedCharacter.GetComponent<PartyMemberAI>();
			if ((bool)component)
			{
				component.StateManager.ClearQueuedStates();
				component.StateManager.CurrentState.BaseCancel();
			}
		}
		UIGrimoireManager.Instance.SpellsKnown.Reload(UIGrimoireManager.Instance.LevelButtons.CurrentLevel);
	}

	public void Mark(GenericSpell spell, bool state)
	{
		IEnumerable<UIGrimoireSpell> source = m_Spells.Where((UIGrimoireSpell sp) => sp.Spell == spell);
		if (source.Any())
		{
			source.First().Icon.GetComponent<UIImageButtonRevised>().SetOverrideHighlighted(state);
		}
	}

	public void Reload(int spellLevel)
	{
		m_SpellLevel = spellLevel;
		int i = 0;
		Grimoire loadedGrimoire = UIGrimoireManager.Instance.LoadedGrimoire;
		if ((bool)loadedGrimoire && spellLevel - 1 < loadedGrimoire.Spells.Length)
		{
			GenericSpell[] spellData = loadedGrimoire.Spells[spellLevel - 1].SpellData;
			foreach (GenericSpell genericSpell in spellData)
			{
				if (!(genericSpell == null))
				{
					if (i >= m_Spells.Count)
					{
						Debug.LogWarning("Grimoire has too many spells for UI (" + UIGrimoireManager.Instance.LoadedGrimoire.name + " S.L." + spellLevel + ")");
						break;
					}
					m_Spells[i].SetSpell(genericSpell);
					m_Spells[i].SetVisibility(val: true);
					m_Spells[i].SetSelected(i < 4 && spellLevel == UIGrimoireManager.Instance.LevelButtons.CurrentLevel);
					m_Spells[i].SetDisabled(GameState.InCombat || !UIGrimoireManager.Instance.CanEditGrimoire);
					i++;
				}
			}
		}
		for (; i < m_Spells.Count; i++)
		{
			m_Spells[i].SetVisibility(val: false);
		}
	}
}

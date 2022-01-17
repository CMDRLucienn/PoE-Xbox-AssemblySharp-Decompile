using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIGrimoireKnownSpells : MonoBehaviour
{
	public UIGrid Grid;

	public UIGrimoireSpell RootIcon;

	public UILabel None;

	private List<UIGrimoireSpell> elements;

	private void Start()
	{
		UIGrimoireLevelButtons levelButtons = UIGrimoireManager.Instance.LevelButtons;
		levelButtons.LevelChanged = (UIGrimoireLevelButtons.OnLevelChanged)Delegate.Combine(levelButtons.LevelChanged, new UIGrimoireLevelButtons.OnLevelChanged(OnLevelChanged));
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (elements == null)
		{
			elements = new List<UIGrimoireSpell>();
			RootIcon.gameObject.SetActive(value: false);
			elements.Add(RootIcon);
			Reload(UIGrimoireManager.Instance.LevelButtons.CurrentLevel);
		}
	}

	private void OnChildClick(GameObject sender)
	{
		UIGrimoireSpell sendSpell = sender.GetComponentInParent<UIGrimoireSpell>();
		if (sendSpell.Disabled || InGameHUD.Instance.CursorMode != 0)
		{
			return;
		}
		int currentLevel = UIGrimoireManager.Instance.LevelButtons.CurrentLevel;
		GenericSpell[] spellData = UIGrimoireManager.Instance.LoadedGrimoire.Spells[currentLevel - 1].SpellData;
		if (!sendSpell)
		{
			return;
		}
		if (!spellData.Where((GenericSpell sp) => GenericAbility.NameComparer.Instance.Equals(sendSpell.Spell, sp)).Any())
		{
			GenericSpell element = sendSpell.Spell;
			Persistence component = sendSpell.Spell.GetComponent<Persistence>();
			if ((bool)component)
			{
				GameObject gameObject = GameResources.LoadPrefab(component.Prefab, instantiate: false) as GameObject;
				if ((bool)gameObject)
				{
					element = gameObject.GetComponent<GenericSpell>();
				}
			}
			if (spellData.PushIfSpace(element))
			{
				if (GlobalAudioPlayer.Instance != null)
				{
					GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.AddSpellGrimoire);
				}
				UIGrimoireManager.Instance.CoolDownGrimoire();
			}
		}
		else
		{
			for (int num = spellData.Length - 1; num >= 0; num--)
			{
				if (GenericAbility.NameComparer.Instance.Equals(spellData[num], sendSpell.Spell))
				{
					spellData.RemoveAt(num);
				}
			}
		}
		Reload(UIGrimoireManager.Instance.LevelButtons.CurrentLevel);
		UIGrimoireManager.Instance.SpellsInGrimoire.Reload();
	}

	private void OnChildRightClick(GameObject sender)
	{
		UIItemInspectManager.Examine(sender.GetComponentInParent<UIGrimoireSpell>().Spell, null, dim: true);
	}

	private void OnChildHover(GameObject sender, bool over)
	{
		UIGrimoireSpell componentInParent = sender.GetComponentInParent<UIGrimoireSpell>();
		UIGrimoireManager.Instance.SpellsInGrimoire.Mark(componentInParent.Spell, over);
	}

	private void OnLevelChanged(int level)
	{
		Reload(level);
	}

	public void Reload(int spellLevel)
	{
		Init();
		CharacterStats selectedCharacter = UIGrimoireManager.Instance.SelectedCharacter;
		IEnumerable<GenericSpell> enumerable = from spell in selectedCharacter.ActiveAbilities.OfType<GenericSpell>()
			where spell.SpellLevel == spellLevel && !spell.ProhibitFromGrimoire && spell.SpellClass == CharacterStats.Class.Wizard && spell.MasteryLevel <= 0
			select spell;
		int num = 0;
		foreach (GenericSpell spell2 in enumerable)
		{
			num++;
			UIGrimoireSpell icon = GetIcon(num);
			icon.gameObject.SetActive(value: true);
			icon.SetSpell(spell2);
			bool flag = (bool)UIGrimoireManager.Instance.LoadedGrimoire && UIGrimoireManager.Instance.LoadedGrimoire.Spells[spell2.SpellLevel - 1].SpellData.Where((GenericSpell sp) => GenericAbility.NameComparer.Instance.Equals(sp, spell2)).Any();
			icon.SetHighlighted(flag);
			icon.SetDisabled(GameState.InCombat || !UIGrimoireManager.Instance.CanEditGrimoire || (!flag && (bool)UIGrimoireManager.Instance.LoadedGrimoire && UIGrimoireManager.Instance.LoadedGrimoire.IsLevelFull(spellLevel)));
		}
		None.gameObject.SetActive(num == 0);
		if (selectedCharacter.CharacterClass == CharacterStats.Class.Wizard)
		{
			None.GetComponent<GUIStringLabel>().SetString(411);
		}
		else
		{
			None.GetComponent<GUIStringLabel>().SetString(1790);
		}
		for (num++; num < elements.Count; num++)
		{
			elements[num].gameObject.SetActive(value: false);
		}
		Grid.Reposition();
	}

	private UIGrimoireSpell GetIcon(int index)
	{
		if (index < elements.Count)
		{
			return elements[index];
		}
		UIGrimoireSpell component = NGUITools.AddChild(RootIcon.transform.parent.gameObject, RootIcon.gameObject).GetComponent<UIGrimoireSpell>();
		component.gameObject.SetActive(value: true);
		UIEventListener uIEventListener = UIEventListener.Get(component.Icon.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(component.Icon.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnChildHover));
		UIEventListener uIEventListener3 = UIEventListener.Get(component.Icon.gameObject);
		uIEventListener3.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
		elements.Add(component);
		return component;
	}
}

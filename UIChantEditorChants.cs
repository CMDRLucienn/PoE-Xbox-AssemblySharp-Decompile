using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIChantEditorChants : UIParentSelectorListener
{
	public UIChantEditorChant RootChant;

	public UIGrid Grid;

	private List<UIChantEditorChant> m_Chants = new List<UIChantEditorChant>();

	private int m_Selected;

	public UIChantEditorChant SelectedChant => m_Chants[m_Selected];

	private void Awake()
	{
		RootChant.gameObject.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnSelectButton(GameObject sender)
	{
		UIChantEditorChant component = sender.transform.parent.parent.GetComponent<UIChantEditorChant>();
		SelectionChanged(component.Chant);
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if ((bool)stats)
		{
			LoadChants(stats);
			SelectFirst();
		}
	}

	public void ReloadChants()
	{
		LoadChants(ParentSelector.SelectedCharacter);
	}

	public void ClearChants()
	{
		for (int i = 0; i < 4; i++)
		{
			GetChant(i).LoadNew();
		}
	}

	public void LoadChants(CharacterStats character)
	{
		int num = 0;
		ClearChants();
		if ((bool)character)
		{
			IEnumerable<Chant> enumerable = character.ActiveAbilities.Where((GenericAbility a) => a is Chant).Cast<Chant>();
			foreach (Chant item in enumerable)
			{
				if (item.UiIndex < 0)
				{
					for (int i = 0; i < 4; i++)
					{
						bool flag = false;
						foreach (Chant item2 in enumerable)
						{
							if (item2.UiIndex == i)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							item.UiIndex = i;
							break;
						}
					}
					if (item.UiIndex < 0)
					{
						Debug.LogError("ERROR: UiIndex of chant '" + item.name + "' on '" + (item.Owner ? item.Owner.name : "NULL") + "' is out of bounds.");
					}
				}
				GetChant(item.UiIndex).Load(item);
				num++;
			}
		}
		Grid.Reposition();
	}

	public void SelectionChanged(UIChantEditorChant select)
	{
		for (int i = 0; i < m_Chants.Count; i++)
		{
			if (m_Chants[i] == select)
			{
				m_Chants[i].Button.ForceHighlight(setting: true);
				m_Selected = i;
			}
			else
			{
				m_Chants[i].Button.ForceHighlight(setting: false);
			}
		}
	}

	public void SelectionChanged(Chant select)
	{
		for (int i = 0; i < m_Chants.Count; i++)
		{
			if ((!select && i == 0) || ((bool)select && m_Chants[i].Chant == select))
			{
				m_Chants[i].Button.ForceHighlight(setting: true);
				m_Selected = i;
			}
			else
			{
				m_Chants[i].Button.ForceHighlight(setting: false);
			}
		}
	}

	public void SelectFirst()
	{
		SelectionChanged(GetChant(0).Chant);
	}

	private UIChantEditorChant GetChant(int index)
	{
		while (index >= m_Chants.Count)
		{
			UIChantEditorChant component = NGUITools.AddChild(RootChant.transform.parent.gameObject, RootChant.gameObject).GetComponent<UIChantEditorChant>();
			component.gameObject.SetActive(value: true);
			component.Owner = this;
			component.gameObject.name = "Chant" + index;
			component.Index = index;
			UIEventListener uIEventListener = UIEventListener.Get(component.Button.Collider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnSelectButton));
			m_Chants.Add(component);
		}
		m_Chants[index].Button.NumberLabel.text = TextUtils.IndexToAlphabet(index);
		return m_Chants[index];
	}
}

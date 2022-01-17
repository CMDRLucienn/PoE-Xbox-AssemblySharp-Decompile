using UnityEngine;

[RequireComponent(typeof(UIDropdownMenu))]
public class UICharacterAiScriptDropdown : UIParentSelectorListener
{
	public struct AiScriptDropdownItem
	{
		public PartyMemberSpellList Script;

		public int Index;

		public AiScriptDropdownItem(PartyMemberSpellList script, int index)
		{
			Script = script;
			Index = index;
		}

		public override string ToString()
		{
			if (Script == null)
			{
				return GUIUtils.GetText(343);
			}
			return Script.ToString();
		}
	}

	[Tooltip("Control whether this dropdown should propagate changes back to the AI itself.")]
	public bool Set = true;

	private PartyMemberAI m_SelectedAi;

	private UIDropdownMenu m_Dropdown;

	private AiScriptDropdownItem m_NoScriptItem = new AiScriptDropdownItem(null, -1);

	public int Setting { get; private set; }

	private void Awake()
	{
		m_Dropdown = GetComponent<UIDropdownMenu>();
		m_Dropdown.OnDropdownOptionChanged += OnDropdownOptionChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if (!stats)
		{
			m_Dropdown.SelectedItem = null;
			m_SelectedAi = null;
			Setting = 0;
			return;
		}
		m_SelectedAi = stats.GetComponent<PartyMemberAI>();
		PartyMemberSpellList[] spellLists = PartyMemberInstructionSetList.InstructionSetList.GetClassSpellList(stats.CharacterClass).SpellLists;
		object[] array = new object[spellLists.Length + 1];
		array[0] = m_NoScriptItem;
		for (int i = 0; i < spellLists.Length; i++)
		{
			array[i + 1] = new AiScriptDropdownItem(spellLists[i], i);
		}
		m_Dropdown.Options = array;
		if ((bool)m_SelectedAi && m_SelectedAi.InstructionSet != null)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] is AiScriptDropdownItem && ((AiScriptDropdownItem)array[j]).Index == m_SelectedAi.InstructionSetIndex)
				{
					m_Dropdown.SelectedItem = array[j];
					break;
				}
			}
			Setting = m_SelectedAi.InstructionSetIndex;
		}
		else
		{
			m_Dropdown.SelectedItem = m_NoScriptItem;
			Setting = -1;
		}
		if (Setting >= 0 && Setting < spellLists.Length)
		{
			UIAiCustomizerManager.Instance.SetScriptTooltip(spellLists[Setting].Description.GetText());
		}
	}

	private void OnDropdownOptionChanged(object selection)
	{
		Setting = ((AiScriptDropdownItem)selection).Index;
		if (Set)
		{
			m_SelectedAi.SetInstructionSetIndex(Setting);
		}
	}
}

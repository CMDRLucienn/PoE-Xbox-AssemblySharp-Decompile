using UnityEngine;

[RequireComponent(typeof(UIDropdownMenu))]
public class UICharacterAutoAttackDropdown : UIParentSelectorListener
{
	[Tooltip("Control whether this dropdown should propagate changes back to the AI itself.")]
	public bool Set = true;

	private AIController m_SelectedAi;

	private UIDropdownMenu m_Dropdown;

	private static object[] s_SettingList = new object[4]
	{
		new UICharacterAutoAttackDropdownData(AIController.AggressionType.Passive, 2118),
		new UICharacterAutoAttackDropdownData(AIController.AggressionType.DefendMyself, 2119),
		new UICharacterAutoAttackDropdownData(AIController.AggressionType.Defensive, 2120),
		new UICharacterAutoAttackDropdownData(AIController.AggressionType.Aggressive, 2121)
	};

	public AIController.AggressionType Setting { get; private set; }

	private void Awake()
	{
		m_Dropdown = GetComponent<UIDropdownMenu>();
		m_Dropdown.OnDropdownOptionChanged += OnDropdownOptionChanged;
		m_Dropdown.Options = s_SettingList;
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
			m_SelectedAi = null;
			Setting = AIController.AggressionType.Passive;
			return;
		}
		m_SelectedAi = GameUtilities.FindActiveAIController(stats.gameObject);
		if ((bool)m_SelectedAi)
		{
			for (int i = 0; i < s_SettingList.Length; i++)
			{
				if (((UICharacterAutoAttackDropdownData)s_SettingList[i]).Setting == m_SelectedAi.Aggression)
				{
					m_Dropdown.SelectedItem = s_SettingList[i];
					break;
				}
			}
			Setting = m_SelectedAi.Aggression;
		}
		else
		{
			m_Dropdown.SelectedItem = s_SettingList[0];
			Setting = AIController.AggressionType.Passive;
		}
	}

	private void OnDropdownOptionChanged(object selection)
	{
		Setting = ((UICharacterAutoAttackDropdownData)selection).Setting;
		if (Set)
		{
			m_SelectedAi.Aggression = ((UICharacterAutoAttackDropdownData)selection).Setting;
		}
	}
}

using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UIDropdownMenu))]
public class UIDropdownLanguage : MonoBehaviour
{
	private UIDropdownMenu m_Dropdown;

	private void Awake()
	{
		m_Dropdown = GetComponent<UIDropdownMenu>();
		m_Dropdown.Options = StringTableManager.Languages.Cast<object>().ToArray();
	}
}

using UnityEngine;

public class ComboBox
{
	private static bool m_forceToUnShow = false;

	private static int m_useControlID = -1;

	private static bool m_isAnyComboBoxDeployed = false;

	private bool m_isClickedComboButton;

	private int m_selectedItemIndex;

	private Rect m_rect;

	private GUIContent m_buttonContent;

	private GUIContent[] m_listContent;

	private string m_buttonStyle;

	private string m_boxStyle;

	private GUIStyle m_listStyle;

	public int SelectedItemIndex
	{
		get
		{
			return m_selectedItemIndex;
		}
		set
		{
			m_selectedItemIndex = value;
		}
	}

	public bool IsDeployed => m_isClickedComboButton;

	public static bool IsAnyComboBoxDeployed => m_isAnyComboBoxDeployed;

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
	{
		m_rect = rect;
		m_buttonContent = buttonContent;
		m_listContent = listContent;
		m_buttonStyle = "button";
		m_boxStyle = "box";
		m_listStyle = listStyle;
	}

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle)
	{
		m_rect = rect;
		m_buttonContent = buttonContent;
		m_listContent = listContent;
		m_buttonStyle = buttonStyle;
		m_boxStyle = boxStyle;
		m_listStyle = listStyle;
	}

	public ComboBox(GUIStyle listStyle)
	{
		m_buttonStyle = "button";
		m_boxStyle = "box";
		m_listStyle = listStyle;
	}

	public ComboBox(string buttonStyle, string boxStyle, GUIStyle listStyle)
	{
		m_buttonStyle = buttonStyle;
		m_boxStyle = boxStyle;
		m_listStyle = listStyle;
	}

	public void Set(Rect rect, GUIContent buttonContent, GUIContent[] listContent)
	{
		m_rect = rect;
		m_buttonContent = buttonContent;
		m_listContent = listContent;
	}

	public int Show(Rect rect, GUIContent buttonContent, GUIContent[] listContent)
	{
		m_rect = rect;
		m_buttonContent = buttonContent;
		m_listContent = listContent;
		return Show();
	}

	public int Show()
	{
		if (m_forceToUnShow)
		{
			if (m_isClickedComboButton)
			{
				m_isAnyComboBoxDeployed = false;
			}
			m_forceToUnShow = false;
			m_isClickedComboButton = false;
		}
		if (m_isAnyComboBoxDeployed && !IsDeployed)
		{
			GUI.Label(m_rect, m_buttonContent, m_buttonStyle);
			return m_selectedItemIndex;
		}
		bool flag = false;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		EventType typeForControl = Event.current.GetTypeForControl(controlID);
		if (typeForControl == EventType.MouseUp && m_isClickedComboButton)
		{
			flag = true;
		}
		if (GUI.Button(m_rect, m_buttonContent, m_buttonStyle))
		{
			if (IsDeployed)
			{
				m_isClickedComboButton = false;
				m_isAnyComboBoxDeployed = false;
				return -1;
			}
			if (m_isAnyComboBoxDeployed)
			{
				return -1;
			}
			if (m_useControlID == -1)
			{
				m_useControlID = controlID;
				m_isClickedComboButton = false;
				m_isAnyComboBoxDeployed = false;
			}
			if (m_useControlID != controlID)
			{
				m_forceToUnShow = true;
				m_useControlID = controlID;
			}
			m_isClickedComboButton = true;
			m_isAnyComboBoxDeployed = true;
		}
		if (m_isClickedComboButton)
		{
			Rect position = new Rect(m_rect.x, m_rect.y + m_listStyle.CalcHeight(m_listContent[0], 1f), m_rect.width, m_listStyle.CalcHeight(m_listContent[0], 1f) * (float)m_listContent.Length);
			GUI.Box(position, "", m_boxStyle);
			int num = GUI.SelectionGrid(position, m_selectedItemIndex, m_listContent, 1, m_listStyle);
			if (num != m_selectedItemIndex)
			{
				m_selectedItemIndex = num;
			}
		}
		if (flag)
		{
			m_isClickedComboButton = false;
			m_isAnyComboBoxDeployed = false;
		}
		return m_selectedItemIndex;
	}
}

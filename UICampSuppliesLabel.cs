using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UICampSuppliesLabel : MonoBehaviour
{
	private int m_LastSupplies = -1;

	private int m_LastMax = -1;

	public GUIDatabaseString FormatString;

	private UILabel m_Label;

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		if (!(GameState.s_playerCharacter != null))
		{
			return;
		}
		if (m_Label == null)
		{
			m_Label = GetComponent<UILabel>();
		}
		int campingSuppliesTotal = GameState.s_playerCharacter.Inventory.CampingSuppliesTotal;
		int stackMaximum = CampingSupplies.StackMaximum;
		if (m_LastSupplies != campingSuppliesTotal || m_LastMax != stackMaximum)
		{
			m_LastMax = stackMaximum;
			m_LastSupplies = campingSuppliesTotal;
			if (FormatString.IsValidString)
			{
				m_Label.text = GUIUtils.Format(FormatString.StringID, campingSuppliesTotal, stackMaximum);
			}
			else
			{
				m_Label.text = campingSuppliesTotal.ToString();
			}
		}
	}
}

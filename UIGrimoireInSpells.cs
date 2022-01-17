using UnityEngine;

public class UIGrimoireInSpells : MonoBehaviour
{
	public UIGrimoireInSpellRow RootRow;

	public UIGrid Grid;

	private UIGrimoireInSpellRow[] m_Rows;

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Rows == null)
		{
			m_Rows = new UIGrimoireInSpellRow[8];
			m_Rows[0] = RootRow;
			for (int i = 1; i < m_Rows.Length; i++)
			{
				m_Rows[i] = NGUITools.AddChild(RootRow.transform.parent.gameObject, RootRow.gameObject).GetComponent<UIGrimoireInSpellRow>();
			}
			for (int j = 0; j < m_Rows.Length; j++)
			{
				m_Rows[j].Init();
			}
			Grid.Reposition();
		}
	}

	public void Mark(GenericSpell spell, bool state)
	{
		m_Rows[spell.SpellLevel - 1].Mark(spell, state);
	}

	public void Reload()
	{
		Init();
		for (int i = 0; i < m_Rows.Length; i++)
		{
			m_Rows[i].Reload(i + 1);
		}
	}
}

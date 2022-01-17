using UnityEngine;

public class UICharacterSheetTalent : MonoBehaviour
{
	public UILabel Label;

	private GenericTalent m_Talent;

	private GameObject m_Owner;

	public float Height => Label.transform.localScale.y * Label.relativeSize.y;

	private void OnClick()
	{
		UIItemInspectManager.Examine(m_Talent, m_Owner);
	}

	public void SetTalent(GenericTalent talent, GameObject owner)
	{
		m_Talent = talent;
		m_Owner = owner;
		Label.text = m_Talent.Name(UICharacterSheetManager.Instance.SelectedCharacter.gameObject);
	}

	public void SetNone()
	{
		m_Talent = null;
		m_Owner = null;
		Label.text = GUIUtils.GetText(343);
	}
}

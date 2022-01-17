using UnityEngine;

public class UIPartyManagementParty : MonoBehaviour
{
	public UIPartyManagementIcon RootObject;

	private UIGrid m_Grid;

	public UILabel PartyCount;

	private UIPartyManagementIcon[] m_Icons;

	private int m_Size;

	public int PartySize => m_Size;

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
		if (m_Icons == null)
		{
			m_Grid = GetComponent<UIGrid>();
			m_Icons = new UIPartyManagementIcon[6];
			m_Icons[0] = RootObject;
			for (int i = 1; i < m_Icons.Length; i++)
			{
				m_Icons[i] = NGUITools.AddChild(RootObject.transform.parent.gameObject, RootObject.gameObject).GetComponent<UIPartyManagementIcon>();
			}
			m_Grid.Reposition();
		}
	}

	public void Reload()
	{
		Init();
		m_Size = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember && !UIPartyManager.Instance.PendingToBench.Contains(onlyPrimaryPartyMember.gameObject))
			{
				m_Icons[m_Size].SetPartyMember(onlyPrimaryPartyMember.gameObject);
				m_Size++;
			}
		}
		for (int i = 0; i < UIPartyManager.Instance.PendingToParty.Count; i++)
		{
			if (m_Size >= m_Icons.Length)
			{
				break;
			}
			GameObject gameObject = UIPartyManager.Instance.PendingToParty[i];
			if ((bool)gameObject)
			{
				m_Icons[m_Size].SetPartyMember(gameObject);
				m_Size++;
			}
		}
		for (int j = m_Size; j < m_Icons.Length; j++)
		{
			m_Icons[j].SetPartyMember(null);
		}
		PartyCount.text = m_Size + "/" + 6;
	}
}

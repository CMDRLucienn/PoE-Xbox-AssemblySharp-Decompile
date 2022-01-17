using System.Collections.Generic;
using UnityEngine;

public class UICharacterSheetTalents : UICharacterSheetContentLine
{
	public UICharacterSheetTalent RootTalent;

	private List<UICharacterSheetTalent> m_Talents;

	private float m_Ypos;

	public int LineHeight = 22;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Talents == null)
		{
			RootTalent.gameObject.SetActive(value: false);
			m_Talents = new List<UICharacterSheetTalent>();
			m_Talents.Add(RootTalent);
		}
	}

	public override void Load(CharacterStats stats)
	{
		Init();
		m_Ypos = RootTalent.transform.localPosition.y;
		int i = 0;
		LoadTalentsFrom(stats, ref i);
		PartyMemberAI partyMemberAI = (stats ? stats.GetComponent<PartyMemberAI>() : null);
		if ((bool)partyMemberAI)
		{
			PartyMemberAI partyMemberAI2 = PartyMemberAI.PartyMembers[partyMemberAI.MyAnimalSlot];
			if ((bool)partyMemberAI2)
			{
				LoadTalentsFrom(partyMemberAI2.GetComponent<CharacterStats>(), ref i);
			}
		}
		if (i == 0)
		{
			GetTalent(i).SetNone();
			i++;
		}
		for (; i < m_Talents.Count; i++)
		{
			m_Talents[i].gameObject.SetActive(value: false);
		}
	}

	private void LoadTalentsFrom(CharacterStats stats, ref int c)
	{
		if (stats == null)
		{
			return;
		}
		for (int i = 0; i < stats.ActiveTalents.Count; i++)
		{
			if (stats.ActiveTalents[i] == null)
			{
				Debug.LogError("Error: UICharacterSheetTalents sees null talent on character '" + stats.name + "'.");
				continue;
			}
			UICharacterSheetTalent talent = GetTalent(c);
			talent.SetTalent(stats.ActiveTalents[i], stats.gameObject);
			m_Ypos -= talent.Height;
			c++;
		}
	}

	private UICharacterSheetTalent GetTalent(int index)
	{
		UICharacterSheetTalent uICharacterSheetTalent = null;
		if (index < m_Talents.Count)
		{
			uICharacterSheetTalent = m_Talents[index];
		}
		else
		{
			UICharacterSheetTalent component = NGUITools.AddChild(RootTalent.transform.parent.gameObject, RootTalent.gameObject).GetComponent<UICharacterSheetTalent>();
			component.transform.localScale = RootTalent.transform.localScale;
			m_Talents.Add(component);
			uICharacterSheetTalent = component;
		}
		uICharacterSheetTalent.transform.localPosition = new Vector3(RootTalent.transform.localPosition.x, m_Ypos, RootTalent.transform.localPosition.z);
		uICharacterSheetTalent.gameObject.SetActive(value: true);
		return uICharacterSheetTalent;
	}
}

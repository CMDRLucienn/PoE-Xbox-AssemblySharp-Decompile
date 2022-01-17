using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICharacterSheetAbilities : UICharacterSheetContentLine
{
	public UICharacterSheetAbility RootAbility;

	public GameObject RootSection;

	private List<UICharacterSheetAbility> m_Abilities;

	private List<GameObject> m_Sections;

	public UITable Table;

	private int m_AbilityIndex;

	private int m_SectionIndex;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Abilities == null)
		{
			RootAbility.gameObject.SetActive(value: false);
			m_Abilities = new List<UICharacterSheetAbility>();
			m_Abilities.Add(RootAbility);
			RootSection.SetActive(value: false);
			m_Sections = new List<GameObject>();
			m_Sections.Add(RootSection);
		}
	}

	public override void Load(CharacterStats stats)
	{
		Init();
		m_AbilityIndex = 0;
		m_SectionIndex = 0;
		foreach (IGrouping<KeyValuePair<CharacterStats.Class, int>, GenericAbility> item in stats.ActiveAbilities.Where((GenericAbility a) => !a.HideFromUi && a.EffectType != GenericAbility.AbilityType.Talent).GroupBy(delegate(GenericAbility abil)
		{
			GenericSpell genericSpell = abil as GenericSpell;
			GenericCipherAbility genericCipherAbility = abil as GenericCipherAbility;
			if (abil.MasteryLevel > 0)
			{
				return new KeyValuePair<CharacterStats.Class, int>(genericSpell.SpellClass, 0);
			}
			if ((bool)genericSpell)
			{
				return new KeyValuePair<CharacterStats.Class, int>(genericSpell.SpellClass, genericSpell.SpellLevel);
			}
			return genericCipherAbility ? new KeyValuePair<CharacterStats.Class, int>(CharacterStats.Class.Cipher, genericCipherAbility.SpellLevel) : new KeyValuePair<CharacterStats.Class, int>(CharacterStats.Class.Undefined, -1);
		}))
		{
			string text = ((int)item.Key.Key).ToString("000") + item.Key.Value.ToString("000");
			if (item.Key.Key != 0)
			{
				GameObject section = GetSection();
				section.name = text + "aHeader";
				UILabel componentInChildren = section.GetComponentInChildren<UILabel>();
				if (componentInChildren != null)
				{
					if (item.Key.Value != 0)
					{
						componentInChildren.text = GUIUtils.FormatSpellLevel(item.Key.Key, item.Key.Value);
					}
					else
					{
						componentInChildren.text = GUIUtils.FormatMasteredLevel(item.Key.Key);
					}
				}
			}
			foreach (GenericAbility item2 in item)
			{
				UICharacterSheetAbility ability = GetAbility(Table.transform);
				ability.name = text + "b" + GetAbilitySortKey(item2).ToString("000") + item2.name;
				ability.SetAbility(item2);
			}
		}
		if (m_AbilityIndex == 0 && m_SectionIndex == 0)
		{
			GetAbility(Table.transform).SetNone();
		}
		while (m_AbilityIndex < m_Abilities.Count)
		{
			m_Abilities[m_AbilityIndex].gameObject.SetActive(value: false);
			m_AbilityIndex++;
		}
		while (m_SectionIndex < m_Sections.Count)
		{
			m_Sections[m_SectionIndex].gameObject.SetActive(value: false);
			m_SectionIndex++;
		}
		Table.Reposition();
	}

	private static int GetAbilitySortKey(GenericAbility ability)
	{
		Chant chant = ability as Chant;
		if ((bool)chant)
		{
			return chant.UiIndex + 1;
		}
		return 0;
	}

	public UICharacterSheetAbility GetAbility(Transform parent)
	{
		UICharacterSheetAbility uICharacterSheetAbility = null;
		if (m_AbilityIndex < m_Abilities.Count)
		{
			uICharacterSheetAbility = m_Abilities[m_AbilityIndex];
		}
		else
		{
			UICharacterSheetAbility component = NGUITools.AddChild(parent.gameObject, RootAbility.gameObject).GetComponent<UICharacterSheetAbility>();
			m_Abilities.Add(component);
			uICharacterSheetAbility = component;
		}
		m_AbilityIndex++;
		uICharacterSheetAbility.transform.parent = parent.transform;
		uICharacterSheetAbility.transform.localScale = RootAbility.transform.localScale;
		uICharacterSheetAbility.transform.localPosition = new Vector3(RootAbility.transform.localPosition.x, 0f, RootAbility.transform.localPosition.z);
		uICharacterSheetAbility.gameObject.SetActive(value: true);
		return uICharacterSheetAbility;
	}

	private GameObject GetSection()
	{
		GameObject gameObject = null;
		if (m_SectionIndex < m_Sections.Count)
		{
			gameObject = m_Sections[m_SectionIndex];
		}
		else
		{
			GameObject gameObject2 = NGUITools.AddChild(RootSection.transform.parent.gameObject, RootSection.gameObject);
			gameObject2.transform.localScale = RootSection.transform.localScale;
			m_Sections.Add(gameObject2);
			gameObject = gameObject2;
		}
		m_SectionIndex++;
		gameObject.transform.localPosition = new Vector3(RootSection.transform.localPosition.x, 0f, RootSection.transform.localPosition.z);
		gameObject.gameObject.SetActive(value: true);
		return gameObject;
	}
}

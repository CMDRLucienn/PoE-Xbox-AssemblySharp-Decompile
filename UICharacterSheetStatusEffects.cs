using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UICharacterSheetStatusEffects : UICharacterSheetContentLine
{
	public UICharacterSheetStatusEffect RootEffect;

	private List<UICharacterSheetStatusEffect> m_StatusEffects;

	public UITable Table;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_StatusEffects == null)
		{
			RootEffect.gameObject.SetActive(value: false);
			m_StatusEffects = new List<UICharacterSheetStatusEffect>();
			m_StatusEffects.Add(RootEffect);
		}
	}

	public static List<List<StatusEffect>> BundleEffectsForUI(IEnumerable<StatusEffect> input)
	{
		List<List<StatusEffect>> list = new List<List<StatusEffect>>();
		List<StatusEffect> list2 = new List<StatusEffect>();
		list2.AddRange(input);
		StatusEffectParams.CleanUp(list2);
		foreach (StatusEffect item in list2)
		{
			if ((!item.AppliedTriggered && !item.IsAura && !item.IsSuppressed) || ((bool)item.AbilityOrigin && item.AbilityOrigin.HideFromUi) || ((bool)item.AfflictionOrigin && item.AfflictionOrigin.HideFromUI) || item.Params.AffectsStat == StatusEffect.ModifiedStat.NoEffect || item.Params.HideFromUi || item.Params.IsInstantApplication || item.Params.OneHitUse)
			{
				continue;
			}
			bool flag = false;
			foreach (List<StatusEffect> item2 in list)
			{
				if (item2[0].BundlesWith(item))
				{
					flag = true;
					item2.Add(item);
					break;
				}
			}
			if (!flag)
			{
				List<StatusEffect> list3 = new List<StatusEffect>();
				list3.Add(item);
				list.Add(list3);
			}
		}
		foreach (List<StatusEffect> item3 in list)
		{
			item3.Sort(delegate(StatusEffect x, StatusEffect y)
			{
				if (x.IsSuppressed && !y.IsSuppressed)
				{
					return 1;
				}
				return (!x.IsSuppressed && y.IsSuppressed) ? (-1) : 0;
			});
		}
		return list;
	}

	public override void Load(CharacterStats stats)
	{
		Init();
		List<List<StatusEffect>> list = BundleEffectsForUI(stats.ActiveStatusEffects);
		int i;
		for (i = 0; i < list.Count; i++)
		{
			UICharacterSheetStatusEffect effect = GetEffect(i);
			effect.gameObject.SetActive(value: true);
			effect.gameObject.name = list[i][0].BundleName;
			UILabel label = effect.Label;
			StringBuilder stringBuilder = new StringBuilder(list[i][0].GetDisplayName());
			stringBuilder.Append(": ");
			if (list[i][0].IsAura && !list[i][0].AppliedTriggered)
			{
				stringBuilder.AppendGuiFormat(1731, GUIUtils.GetText(902));
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(StatusEffectParams.ListToString(list[i], stats, null, null, null, StatusEffectFormatMode.CharacterSheet, AttackBase.TargetType.All));
			label.text = stringBuilder.ToString();
		}
		if (i == 0)
		{
			UICharacterSheetStatusEffect effect2 = GetEffect(i);
			effect2.gameObject.SetActive(value: true);
			effect2.gameObject.name = "None";
			effect2.Label.text = GUIUtils.GetText(343);
			i++;
		}
		for (; i < m_StatusEffects.Count; i++)
		{
			m_StatusEffects[i].gameObject.SetActive(value: false);
		}
		Table.Reposition();
	}

	private UICharacterSheetStatusEffect GetEffect(int index)
	{
		UICharacterSheetStatusEffect uICharacterSheetStatusEffect = null;
		if (index < m_StatusEffects.Count)
		{
			uICharacterSheetStatusEffect = m_StatusEffects[index];
		}
		else
		{
			UICharacterSheetStatusEffect component = NGUITools.AddChild(Table.gameObject, RootEffect.gameObject).GetComponent<UICharacterSheetStatusEffect>();
			component.transform.localScale = RootEffect.transform.localScale;
			component.gameObject.name = index.ToString("000000");
			m_StatusEffects.Add(component);
			uICharacterSheetStatusEffect = component;
		}
		uICharacterSheetStatusEffect.transform.localPosition = new Vector3(RootEffect.transform.localPosition.x, 0f, RootEffect.transform.localPosition.z);
		return uICharacterSheetStatusEffect;
	}
}

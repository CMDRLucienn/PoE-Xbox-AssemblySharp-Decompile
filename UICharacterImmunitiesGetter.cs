using System.Collections.Generic;

public class UICharacterImmunitiesGetter : UIParentSelectorListener
{
	public UILabel Label;

	public bool DeactivateObjectWhenEmpty = true;

	private bool m_ExternalActivation = true;

	public bool ExternalActivation
	{
		get
		{
			return m_ExternalActivation;
		}
		set
		{
			m_ExternalActivation = value;
			UpdateActivation();
		}
	}

	private void Awake()
	{
		if (!Label)
		{
			Label = GetComponent<UILabel>();
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		List<string> list = new List<string>();
		if ((bool)stats)
		{
			for (int i = 0; i < stats.AfflictionImmunities.Count; i++)
			{
				if ((bool)stats.AfflictionImmunities[i])
				{
					list.Add(stats.AfflictionImmunities[i].Name());
				}
			}
			for (int j = 0; j < stats.ActiveStatusEffects.Count; j++)
			{
				StatusEffect statusEffect = stats.ActiveStatusEffects[j];
				if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.KeywordImmunity && statusEffect.Applied)
				{
					DatabaseString adjective = KeywordData.GetAdjective(statusEffect.Params.Keyword);
					if (adjective != null && !list.Contains(adjective.GetText()))
					{
						list.Add(adjective.GetText());
					}
				}
			}
		}
		list.Sort();
		if (list.Count > 0)
		{
			Label.text = GUIUtils.Format(2368, TextUtils.FuncJoin((string s) => s, list, GUIUtils.Comma()));
		}
		else
		{
			Label.text = "";
		}
		UpdateActivation();
	}

	private void UpdateActivation()
	{
		base.gameObject.SetActive((!DeactivateObjectWhenEmpty || !string.IsNullOrEmpty(Label.text)) && ExternalActivation);
	}
}

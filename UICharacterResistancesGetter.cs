using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UICharacterResistancesGetter : UIParentSelectorListener
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
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
		if ((bool)stats)
		{
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			for (int i = 0; i < stats.ActiveStatusEffects.Count; i++)
			{
				StatusEffect statusEffect = stats.ActiveStatusEffects[i];
				StatusEffectParams @params = statusEffect.Params;
				if (!statusEffect.Applied)
				{
					continue;
				}
				string text = "";
				if (@params.AffectsStat == StatusEffect.ModifiedStat.ResistKeyword)
				{
					DatabaseString adjective = KeywordData.GetAdjective(@params.Keyword);
					if (adjective != null)
					{
						text = adjective.GetText();
					}
				}
				else if (@params.AffectsStat == StatusEffect.ModifiedStat.ResistAffliction)
				{
					text = (@params.AfflictionPrefab ? @params.AfflictionPrefab.Name() : "");
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (dictionary2.ContainsKey(text))
					{
						dictionary2[text] += (int)statusEffect.CurrentAppliedValue;
					}
					else
					{
						dictionary2[text] = (int)statusEffect.CurrentAppliedValue;
					}
				}
			}
			foreach (KeyValuePair<string, int> item in dictionary2)
			{
				List<string> list;
				if (!dictionary.ContainsKey(item.Value))
				{
					list = new List<string>();
					dictionary[item.Value] = list;
				}
				else
				{
					list = dictionary[item.Value];
				}
				list.Add(item.Key);
			}
		}
		if (dictionary.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<int, List<string>> item2 in dictionary.OrderBy((KeyValuePair<int, List<string>> pair) => pair.Key))
			{
				if (item2.Value.Count > 0)
				{
					item2.Value.Sort();
					string value = TextUtils.FuncJoin((string s) => s, item2.Value, GUIUtils.Comma());
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(GUIUtils.Comma());
					}
					stringBuilder.AppendGuiFormat(1731, TextUtils.NumberBonus(item2.Key));
					stringBuilder.Append(' ');
					stringBuilder.Append(value);
				}
			}
			Label.text = GUIUtils.Format(2367, stringBuilder.ToString().Trim());
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

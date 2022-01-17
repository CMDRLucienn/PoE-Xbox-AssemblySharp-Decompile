using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStatusEffectBundleLine : MonoBehaviour
{
	public UITexture Icon;

	public UILabel Name;

	[Tooltip("Display the duration of the bundle?")]
	public bool ShowDuration = true;

	private List<StatusEffect> m_Bundle;

	private static string s_SuppressedColor = "[" + NGUITools.EncodeColor(Color.gray) + "]";

	private void Update()
	{
		if (ShowDuration)
		{
			RefreshDynamic();
		}
	}

	private void RefreshDynamic()
	{
		if (m_Bundle == null)
		{
			return;
		}
		string text = m_Bundle[0].GetDisplayName();
		if (m_Bundle.All((StatusEffect e) => e.IsSuppressed || e.IsSuspended))
		{
			text = s_SuppressedColor + text + GUIUtils.Format(1731, GUIUtils.GetText(379)) + "[-]";
		}
		else if (ShowDuration)
		{
			float num = m_Bundle.Max((StatusEffect eff) => eff.TimeLeft);
			if (num > 0f)
			{
				text += GUIUtils.Format(1731, GUIUtils.Format(211, num.ToString("#0.0")));
			}
		}
		int num2 = 0;
		for (int i = 0; i < m_Bundle.Count; i++)
		{
			if (m_Bundle[i].Params.AffectsStat == StatusEffect.ModifiedStat.GenericMarker)
			{
				num2++;
			}
		}
		if (num2 > 0)
		{
			text += GUIUtils.Format(1731, num2);
		}
		Name.text = text;
	}

	public void Load(List<StatusEffect> bundle)
	{
		base.gameObject.name = bundle[0].BundleName;
		m_Bundle = bundle;
		RefreshDynamic();
		Icon.mainTexture = bundle[0].GetDisplayIcon();
	}
}

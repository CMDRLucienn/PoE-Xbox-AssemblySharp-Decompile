using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIPartyMemberStatGetter : UIParentSelectorListener
{
	public enum Style
	{
		[Obsolete]
		Stamina_DEPRECATED,
		[Obsolete]
		Health_DEPRECATED,
		PrimaryDamage,
		SecondaryDamage,
		PrimaryAccuracy,
		SecondaryAccuracy,
		DamageThreshold,
		Defense,
		Concentration,
		Interrupt
	}

	public Style Stat;

	public CharacterStats.DefenseType Defense = CharacterStats.DefenseType.None;

	public DamagePacket.DamageType Damage = DamagePacket.DamageType.None;

	private CharacterStats m_SelectedStats;

	private Equipment m_SelectedEquipment;

	private UILabel m_Label;

	private UIImageButtonRevised m_ImageButton;

	private Color m_ImageButtonDefaultNeutral;

	private float m_LastFloat;

	private bool m_LastOverride;

	private BestiaryCertainty m_LastKnown = BestiaryCertainty.Exact;

	private static StringBuilder s_Builder = new StringBuilder();

	private static string s_UnknownColor = "[" + NGUITools.EncodeColor(Color.gray) + "]";

	private void OnEnable()
	{
		UpdateText();
	}

	private void Awake()
	{
		m_ImageButton = GetComponent<UIImageButtonRevised>();
		if ((bool)m_ImageButton)
		{
			m_ImageButtonDefaultNeutral = m_ImageButton.NeutralColor;
		}
	}

	private void Update()
	{
		UpdateText();
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_SelectedStats = stats;
		m_SelectedEquipment = (stats ? stats.GetComponent<Equipment>() : null);
		UpdateText();
	}

	private void UpdateText()
	{
		if (m_Label == null)
		{
			m_Label = GetComponent<UILabel>();
		}
		if (m_Label.alpha <= 0f)
		{
			return;
		}
		if (m_SelectedStats == null)
		{
			m_Label.text = "";
			return;
		}
		float num = m_LastFloat;
		bool flag = false;
		BestiaryCertainty known = m_LastKnown;
		switch (Stat)
		{
		case Style.PrimaryDamage:
			if ((bool)m_SelectedEquipment.PrimaryAttack)
			{
				DamageInfo damageInfo = new DamageInfo(null, 0f, m_SelectedEquipment.PrimaryAttack);
				m_SelectedStats.AdjustDamageForUi(damageInfo);
				m_Label.text = GUIUtils.Format(445, (damageInfo.AdjustDamage(damageInfo.MinDamage) + damageInfo.MinDamageBonus).ToString("#0"), damageInfo.AdjustDamage(damageInfo.MaxDamage).ToString("#0"));
			}
			else
			{
				m_Label.text = "";
			}
			break;
		case Style.SecondaryDamage:
			if ((bool)m_SelectedEquipment.SecondaryAttack)
			{
				DamageInfo damageInfo2 = new DamageInfo(null, 0f, m_SelectedEquipment.SecondaryAttack);
				m_SelectedStats.AdjustDamageForUi(damageInfo2);
				m_Label.text = GUIUtils.Format(445, (damageInfo2.AdjustDamage(damageInfo2.MinDamage) + damageInfo2.MinDamageBonus).ToString("#0"), damageInfo2.AdjustDamage(damageInfo2.MaxDamage).ToString("#0"));
			}
			else
			{
				m_Label.text = "";
			}
			break;
		case Style.PrimaryAccuracy:
			num = m_SelectedStats.CalculateAccuracyForUi(m_SelectedEquipment.PrimaryAttack, null, null);
			break;
		case Style.SecondaryAccuracy:
			num = m_SelectedStats.CalculateAccuracyForUi(m_SelectedEquipment.SecondaryAttack, null, null);
			break;
		case Style.DamageThreshold:
			num = m_SelectedStats.GetPerceivedDamThresh(Damage, isVeilPiercing: false, out known);
			break;
		case Style.Defense:
		{
			num = m_SelectedStats.GetPerceivedDefense(Defense, out known);
			flag = m_SelectedStats.TryGetRedirectDefense(Defense, null, null, isSecondary: false, out var defense);
			if (flag)
			{
				num = defense;
			}
			break;
		}
		case Style.Concentration:
			num = m_SelectedStats.ComputeConcentrationHelper();
			break;
		case Style.Interrupt:
			num = m_SelectedStats.ComputeInterruptHelper();
			break;
		}
		if (num == m_LastFloat && known == m_LastKnown && flag == m_LastOverride && !string.IsNullOrEmpty(m_Label.text))
		{
			return;
		}
		string val = ((!float.IsPositiveInfinity(num)) ? num.ToString("#0") : GUIUtils.GetText(2187));
		m_Label.text = FormatKnown(val, known);
		if (flag)
		{
			if ((bool)m_ImageButton)
			{
				m_ImageButton.SetNeutralColor(UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, UIGlobalColor.LinkStyle.WOOD, hovered: false, UIGlobalColor.LinkType.BUFF));
			}
		}
		else if ((bool)m_ImageButton)
		{
			m_ImageButton.SetNeutralColor(m_ImageButtonDefaultNeutral);
		}
		m_LastFloat = num;
		m_LastOverride = flag;
		m_LastKnown = known;
	}

	private static string FormatKnown(string val, BestiaryCertainty known)
	{
		s_Builder.Remove(0, s_Builder.Length);
		if (known <= BestiaryCertainty.Estimated)
		{
			s_Builder.Append(s_UnknownColor);
		}
		if (known == BestiaryCertainty.Unknown)
		{
			s_Builder.Append(GUIUtils.GetText(1980));
		}
		else
		{
			s_Builder.Append(val);
		}
		if (known == BestiaryCertainty.Estimated)
		{
			s_Builder.Append(GUIUtils.GetText(1980));
		}
		s_Builder.Append("[-]");
		return s_Builder.ToString();
	}
}

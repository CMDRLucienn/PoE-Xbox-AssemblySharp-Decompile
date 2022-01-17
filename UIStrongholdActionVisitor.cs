using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UIStrongholdActionVisitor : UIStrongholdExpandableItem
{
	public UITexture PortraitTexture;

	public UIWidget DilemmaIcon;

	public UIItemReadOnly OfferItem;

	public UILabel OfferPriceLabel;

	public GameObject OfferMoneyThing;

	public UILabel OfferMoneyLabel;

	public UILabel SecurityLabel;

	public UILabel PrestigeLabel;

	public GameObject StatsParent;

	public GameObject RightBar;

	private StrongholdVisitor m_Visitor;

	private StrongholdPrisonerData m_Prisoner;

	private StrongholdEvent m_Event;

	private bool m_Kidnapped;

	private bool m_IsEscort;

	private StoredCharacterInfo m_Escorter;

	public UILabel AssignmentLabel;

	public UIMultiSpriteImageButton Button1;

	public UIMultiSpriteImageButton Button2;

	private void Start()
	{
		UIMultiSpriteImageButton button = Button1;
		button.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button.onClick, new UIEventListener.VoidDelegate(OnButton1));
		UIMultiSpriteImageButton button2 = Button2;
		button2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button2.onClick, new UIEventListener.VoidDelegate(OnButton2));
		UIEventListener uIEventListener = UIEventListener.Get(DilemmaIcon);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnDilemmaTooltip));
	}

	private void OnDilemmaTooltip(GameObject sender, bool over)
	{
		if (over && m_Visitor != null)
		{
			UIActionBarTooltip.GlobalShow(DilemmaIcon, GUIUtils.Format(2405, m_Visitor.Name));
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	private void OnButton1(GameObject sender)
	{
		if ((bool)m_Escorter)
		{
			if (m_Event != null)
			{
				m_Event.EventAbandonedCompanion = m_Escorter;
			}
			UIStrongholdManager.Instance.Stronghold.AbortAndMakeAvailable(m_Escorter);
		}
		else if (m_Visitor != null)
		{
			switch (m_Visitor.VisitorType)
			{
			case StrongholdVisitor.Type.BadVisitor:
			case StrongholdVisitor.Type.Supplicant:
				UIStrongholdManager.Instance.Stronghold.PayOffVisitor(m_Visitor);
				break;
			case StrongholdVisitor.Type.PrestigiousVisitor:
				if (m_Kidnapped)
				{
					UIStrongholdManager.Instance.Stronghold.RansomVisitor(m_Visitor);
				}
				break;
			case StrongholdVisitor.Type.PrisonerRequest:
				UIStrongholdManager.Instance.Stronghold.RemovePrisoner(m_Prisoner.PrisonerName);
				break;
			case StrongholdVisitor.Type.RareItemMerchant:
				UIStrongholdManager.Instance.Stronghold.BuyItem(m_Visitor);
				break;
			}
		}
		else if (m_Prisoner != null)
		{
			UIStrongholdManager.Instance.Stronghold.RemovePrisoner(m_Prisoner.PrisonerName);
		}
	}

	private void OnButton2(GameObject sender)
	{
		switch (m_Visitor.VisitorType)
		{
		case StrongholdVisitor.Type.BadVisitor:
		case StrongholdVisitor.Type.Supplicant:
			if (GameState.Stronghold.FindAvailableCompanion() != null)
			{
				UIStrongholdCompanionPicker.Instance.ShowWindow();
				UIStrongholdCompanionPicker instance2 = UIStrongholdCompanionPicker.Instance;
				instance2.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Combine(instance2.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendEscort));
			}
			else
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(698));
			}
			break;
		case StrongholdVisitor.Type.PrestigiousVisitor:
			if (m_Kidnapped)
			{
				if (GameState.Stronghold.FindAvailableCompanion() != null)
				{
					UIStrongholdCompanionPicker.Instance.ShowWindow();
					UIStrongholdCompanionPicker instance = UIStrongholdCompanionPicker.Instance;
					instance.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Combine(instance.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendRescue));
				}
				else
				{
					UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(698));
				}
			}
			break;
		case StrongholdVisitor.Type.PrisonerRequest:
			if ((bool)m_Visitor.VisitorItem)
			{
				UIStrongholdManager.Instance.Stronghold.AcceptItem(m_Visitor);
			}
			else
			{
				UIStrongholdManager.Instance.Stronghold.AcceptMoney(m_Visitor);
			}
			break;
		case StrongholdVisitor.Type.RareItemMerchant:
			break;
		}
	}

	private void OnSendEscort(UIMessageBox.Result result, GameObject selected)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			UIStrongholdManager.Instance.Stronghold.EscortVisitor(m_Visitor, selected.GetComponent<StoredCharacterInfo>(), -1);
		}
		UIStrongholdCompanionPicker instance = UIStrongholdCompanionPicker.Instance;
		instance.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Remove(instance.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendEscort));
	}

	private void OnSendRescue(UIMessageBox.Result result, GameObject selected)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			UIStrongholdManager.Instance.Stronghold.RescueVisitor(m_Visitor, selected.GetComponent<StoredCharacterInfo>());
		}
		UIStrongholdCompanionPicker instance = UIStrongholdCompanionPicker.Instance;
		instance.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Remove(instance.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendRescue));
	}

	public override void Reload()
	{
		if (m_Prisoner != null)
		{
			SetPrisoner(m_Prisoner);
		}
		else if (m_Visitor != null)
		{
			Set(m_Visitor);
		}
	}

	public void SetKidnapped(StrongholdVisitor visitor)
	{
		m_Kidnapped = true;
		m_IsEscort = false;
		m_Escorter = null;
		Set(visitor);
	}

	public void SetKidnappedRescue(StrongholdVisitor visitor, StrongholdEvent evt)
	{
		m_Event = evt;
		m_Kidnapped = true;
		m_IsEscort = true;
		m_Escorter = (evt.EventCompanion ? evt.EventCompanion.gameObject.GetComponent<StoredCharacterInfo>() : null);
		Set(visitor);
	}

	public void SetRecall(StrongholdEvent sevent)
	{
		m_Event = sevent;
		m_Kidnapped = false;
		m_Escorter = sevent.EventCompanion;
		m_IsEscort = true;
		StrongholdVisitor visitor = (StrongholdVisitor)sevent.EventData;
		Set(visitor);
	}

	public void SetVisitor(StrongholdVisitor visitor)
	{
		m_Kidnapped = false;
		m_IsEscort = false;
		m_Escorter = null;
		Set(visitor);
	}

	public void SetPrisoner(StrongholdPrisonerData prisoner)
	{
		m_Prisoner = prisoner;
		RightBar.gameObject.SetActive(value: true);
		DilemmaIcon.alpha = 0f;
		m_Visitor = null;
		StrongholdVisitor[] visitors = UIStrongholdManager.Instance.Stronghold.Visitors;
		foreach (StrongholdVisitor strongholdVisitor in visitors)
		{
			if (strongholdVisitor.VisitorType == StrongholdVisitor.Type.PrisonerRequest && strongholdVisitor.AssociatedPrisoner == prisoner.PrisonerName)
			{
				m_Visitor = strongholdVisitor;
				break;
			}
		}
		AssignmentLabel.gameObject.SetActive(value: false);
		if (prisoner.PrisonerDescription != null && prisoner.PrisonerDescription.IsValidString)
		{
			SetDescriptionText(prisoner.PrisonerDescription.GetText());
		}
		else
		{
			SetDescriptionText("");
		}
		if (prisoner.PrisonerName == null)
		{
			NameLabel.text = "*NameError*";
		}
		else
		{
			NameLabel.text = prisoner.PrisonerName.GetText();
		}
		StatsParent.SetActive(value: false);
		Button1.gameObject.SetActive(value: true);
		Button1.Label.text = GUIUtils.GetText(894);
		if (m_Visitor != null)
		{
			if (m_Visitor.VisitorType != StrongholdVisitor.Type.PrisonerRequest)
			{
				Debug.LogError("Prisoner visitor is not a Request.");
			}
			SetItem(m_Visitor.VisitorItem, m_Visitor.MoneyValue);
			Button2.gameObject.SetActive(value: true);
			if ((bool)m_Visitor.VisitorItem)
			{
				Button2.Label.text = GUIUtils.GetText(787);
			}
			else
			{
				Button2.Label.text = GUIUtils.GetText(786);
			}
		}
		else
		{
			Button2.gameObject.SetActive(value: false);
			SetItem(null, 0);
		}
		UIGrid[] componentsInChildren = GetComponentsInChildren<UIGrid>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reposition();
		}
	}

	private void Set(StrongholdVisitor visitor)
	{
		m_Visitor = visitor;
		m_Prisoner = null;
		NameLabel.text = visitor.Name;
		if ((bool)PortraitTexture)
		{
			PortraitTexture.mainTexture = Portrait.GetTextureSmall(visitor.VisitorPrefab);
		}
		bool flag = m_Visitor.HasUnresolvedDilemma();
		RightBar.gameObject.SetActive(!flag || m_Kidnapped);
		DilemmaIcon.alpha = (flag ? 1f : 0f);
		if ((bool)visitor.VisitorItem)
		{
			SetItem(visitor.VisitorItem, visitor.MoneyValue);
		}
		else
		{
			SetItem(null, 0);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (m_IsEscort)
		{
			Gender gender = (m_Escorter ? m_Escorter.Gender : Gender.Neuter);
			AssignmentLabel.gameObject.SetActive(value: true);
			if ((bool)m_Escorter)
			{
				AssignmentLabel.text = GUIUtils.Format(gender, 707, CharacterStats.Name(m_Escorter));
			}
			else
			{
				AssignmentLabel.text = "";
			}
			Button1.gameObject.SetActive(m_Event.EventDataInt < 0);
			Button1.Label.text = GUIUtils.GetText(677);
			Button2.gameObject.SetActive(value: false);
			string text = GUIUtils.GetText(m_Kidnapped ? 771 : 772, gender);
			bool flag2 = m_Event.EventDataInt >= 0 && m_Event.EventDataInt < visitor.SpecialEscorts.Length;
			if (flag2 && visitor.SpecialEscorts[m_Event.EventDataInt].EscortDescription.IsValidString)
			{
				text = visitor.SpecialEscorts[m_Event.EventDataInt].EscortDescription.GetText(gender);
			}
			if ((bool)m_Escorter || flag2)
			{
				stringBuilder.AppendLine(StringUtility.Format(text, CharacterStats.Name(m_Escorter), visitor.VisitorPrefab.Name()));
			}
			else
			{
				stringBuilder.AppendLine(StrongholdUtils.GetText(229));
			}
			stringBuilder.AppendLine(StrongholdUtils.Format(CharacterStats.GetGender(m_Escorter), 59, new EternityTimeInterval((int)m_Event.Time).FormatNonZero(2)));
		}
		else
		{
			AssignmentLabel.gameObject.SetActive(value: false);
			stringBuilder.Append(GUIUtils.GetStrongholdVisitorString(visitor.VisitorType, CharacterStats.GetGender(visitor.VisitorPrefab)));
			stringBuilder.Append(". ");
			if (visitor.IsAffectingStats())
			{
				StatsParent.SetActive(value: true);
				SecurityLabel.text = TextUtils.NumberBonus(visitor.GetSecurityAdjustment());
				PrestigeLabel.text = TextUtils.NumberBonus(visitor.GetPrestigeAdjustment());
			}
			else
			{
				StatsParent.SetActive(value: false);
			}
			if (visitor.VisitDuration > 0)
			{
				stringBuilder.Append(StrongholdUtils.Format(CharacterStats.GetGender(visitor.VisitorPrefab), 60, new EternityTimeInterval((int)visitor.TimeToLeave).FormatNonZero(2)));
			}
			List<string> list = new List<string>();
			if (visitor.SupplicantPrestigeAdjustment != 0)
			{
				string text2 = GUIUtils.Format(654, TextUtils.NumberBonus(visitor.SupplicantPrestigeAdjustment), TextUtils.NumberBonus(0));
				text2 = GUIUtils.Format(1665, text2, new EternityTimeInterval((int)(UIStrongholdManager.Instance.Stronghold.DaysToGTU(visitor.SupplicantEffectsDuration) * (float)WorldTime.Instance.SecondsPerDay)));
				list.Add(text2);
			}
			if (visitor.SupplicantReputationAdjustment != null && visitor.SupplicantReputationAdjustment.factionName != 0)
			{
				list.Add(visitor.SupplicantReputationAdjustment.ToString());
			}
			stringBuilder.AppendLine();
			if (list.Count > 0)
			{
				stringBuilder.Append(GUIUtils.Format(1823, TextUtils.FuncJoin((string s) => s, list, GUIUtils.Comma())));
				stringBuilder.Append(". ");
			}
			if (!flag && (m_Visitor.VisitorType == StrongholdVisitor.Type.BadVisitor || m_Visitor.VisitorType == StrongholdVisitor.Type.Supplicant))
			{
				stringBuilder.Append(GUIUtils.GetText(782));
				stringBuilder.Append(": ");
				stringBuilder.Append(GUIUtils.Format(466, m_Visitor.MoneyValue));
				stringBuilder.Append(". ");
			}
			switch (m_Visitor.VisitorType)
			{
			case StrongholdVisitor.Type.BadVisitor:
			case StrongholdVisitor.Type.Supplicant:
				Button1.gameObject.SetActive(value: true);
				Button1.Label.text = GUIUtils.GetText(782);
				Button2.gameObject.SetActive(value: true);
				Button2.Label.text = GUIUtils.GetText(783);
				break;
			case StrongholdVisitor.Type.PrestigiousVisitor:
				if (m_Kidnapped)
				{
					Button1.gameObject.SetActive(value: true);
					Button1.Label.text = GUIUtils.GetText(784);
					Button2.gameObject.SetActive(value: true);
					Button2.Label.text = GUIUtils.GetText(785);
				}
				else
				{
					Button1.gameObject.SetActive(value: false);
					Button2.gameObject.SetActive(value: false);
				}
				break;
			case StrongholdVisitor.Type.PrisonerRequest:
				Button1.gameObject.SetActive(value: true);
				Button1.Label.text = GUIUtils.GetText(786);
				Button2.gameObject.SetActive(m_Visitor.VisitorItem != null);
				Button2.Label.text = GUIUtils.GetText(787);
				break;
			case StrongholdVisitor.Type.RareItemMerchant:
				Button1.gameObject.SetActive(value: true);
				Button1.Label.text = GUIUtils.GetText(788);
				Button2.gameObject.SetActive(value: false);
				if (m_Visitor != null && (bool)m_Visitor.VisitorItem && !flag)
				{
					stringBuilder.Append(GUIUtils.Format(790, m_Visitor.VisitorItem.Name));
				}
				break;
			}
		}
		if (m_Kidnapped && !m_Escorter)
		{
			stringBuilder.Append(GUIUtils.Format(770, GUIUtils.Format(294, visitor.MoneyValue, CharacterStats.GetGender(visitor.VisitorPrefab))));
		}
		else if (m_Visitor.HasUnresolvedDilemma() && m_Visitor.HasDilemmaArrivalString())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(m_Visitor.FormatArrivalString());
		}
		SetDescriptionText(stringBuilder.ToString().Trim());
		UIGrid[] componentsInChildren = GetComponentsInChildren<UIGrid>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reposition();
		}
	}

	private void SetItem(Item item, int price)
	{
		OfferItem.LoadItem(item);
		if ((bool)item)
		{
			OfferItem.gameObject.SetActive(value: true);
			if (price > 0)
			{
				OfferPriceLabel.text = GUIUtils.Format(466, price);
			}
			else
			{
				OfferPriceLabel.text = "";
			}
			OfferMoneyThing.SetActive(value: false);
		}
		else if (price > 0)
		{
			OfferMoneyThing.SetActive(value: true);
			OfferMoneyLabel.text = GUIUtils.Format(466, price);
			OfferItem.gameObject.SetActive(value: false);
		}
		else
		{
			OfferMoneyThing.SetActive(value: false);
			OfferItem.gameObject.SetActive(value: false);
		}
	}
}

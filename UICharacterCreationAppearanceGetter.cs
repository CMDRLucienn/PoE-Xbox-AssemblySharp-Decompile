using System;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UICharacterCreationAppearanceGetter : UICharacterCreationElement
{
	public UICharacterCreationAppearanceSetter.AppearanceType AppearanceType;

	public string Format = "{0} - {1}/{2}";

	private UILabel m_Label;

	private static readonly int[] AppearanceTypeIDs = new int[4] { 340, 341, 342, 320 };

	private bool CanUseVariationZero(UICharacterCreationAppearanceSetter.AppearanceType appearanceType)
	{
		if (appearanceType == UICharacterCreationAppearanceSetter.AppearanceType.HAIR || appearanceType == UICharacterCreationAppearanceSetter.AppearanceType.FACIAL_HAIR)
		{
			return true;
		}
		return false;
	}

	protected override void Start()
	{
		base.Start();
		m_Label = GetComponent<UILabel>();
	}

	public void OnEnable()
	{
		SignalValueChanged(ValueType.All);
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (type != ValueType.Portrait && type != ValueType.BodyPart && type != ValueType.All)
		{
			return;
		}
		if (!m_Label)
		{
			Start();
		}
		if (AppearanceType == UICharacterCreationAppearanceSetter.AppearanceType.PORTRAIT)
		{
			if (UICharacterCustomizeManager.PortraitOptions == null || UICharacterCustomizeManager.PortraitOptions.Count == 0)
			{
				UICharacterCustomizeManager.LoadPortraitCache();
			}
			m_Label.text = StringUtility.Format(Format, UICharacterCreationAppearanceSetter.s_pendingPortraitIndex + 1, UICharacterCustomizeManager.PortraitOptions.Count);
			return;
		}
		int modelVariation = base.Owner.Character.GetModelVariation(AppearanceType);
		int totalModelVariations = UICharacterCreationManager.GetTotalModelVariations(base.Owner.Character, AppearanceType);
		bool flag = CanUseVariationZero(AppearanceType);
		if (totalModelVariations == 0 || ((base.Owner.Character.Race == CharacterStats.Race.Godlike || base.Owner.Character.Subrace == CharacterStats.Subrace.Wild_Orlan) && AppearanceType == UICharacterCreationAppearanceSetter.AppearanceType.HAIR))
		{
			m_Label.text = StringUtility.Format("{0} - {1}", GUIUtils.GetText(AppearanceTypeIDs[(int)AppearanceType]), GUIUtils.GetText(343));
		}
		else
		{
			m_Label.text = StringUtility.Format(Format, GUIUtils.GetText(AppearanceTypeIDs[(int)AppearanceType]), Math.Min(modelVariation, totalModelVariations) + (flag ? 1 : 0), totalModelVariations + (flag ? 1 : 0));
		}
	}
}

using UnityEngine;

public class UICharacterCreationAppearanceSetter : UICharacterCreationElement
{
	public enum AppearanceType
	{
		HAIR,
		FACIAL_HAIR,
		HEAD,
		PORTRAIT
	}

	public int Adjustment = 1;

	public AppearanceType Type;

	public static int s_pendingPortraitIndex = -1;

	public void OnEnable()
	{
		if (Type == AppearanceType.PORTRAIT)
		{
			if (UICharacterCustomizeManager.PortraitOptions == null || UICharacterCustomizeManager.PortraitOptions.Count == 0)
			{
				UICharacterCustomizeManager.LoadPortraitCache();
			}
			s_pendingPortraitIndex = ((base.Owner.Character.PortraitIndex == -1) ? UICharacterCustomizeManager.GetPortraitIndexFor(base.Owner.Character.Gender, base.Owner.Character.Race) : base.Owner.Character.PortraitIndex);
			SetPortrait(s_pendingPortraitIndex);
		}
		if (Type == AppearanceType.FACIAL_HAIR || Type == AppearanceType.HAIR || Type == AppearanceType.HEAD)
		{
			int totalModelVariations = UICharacterCreationManager.GetTotalModelVariations(base.Owner.Character, Type);
			UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.enabled = totalModelVariations > 1;
			}
			UISprite component2 = GetComponent<UISprite>();
			if ((bool)component2)
			{
				component2.color = new Color(component2.color.r, component2.color.g, component2.color.b, (totalModelVariations > 1) ? 1f : 0.3f);
			}
			UILabel[] componentsInChildren = base.gameObject.transform.parent.GetComponentsInChildren<UILabel>(includeInactive: true);
			foreach (UILabel uILabel in componentsInChildren)
			{
				uILabel.color = new Color(uILabel.color.r, uILabel.color.g, uILabel.color.b, (totalModelVariations > 1) ? 1f : 0.3f);
			}
			base.Owner.SignalValueChanged(ValueType.BodyPart);
		}
	}

	private void SetPortrait(int index)
	{
		base.Owner.Character.CharacterPortraitSmallPath = UICharacterCustomizeManager.PortraitOptions[index];
		base.Owner.Character.CharacterPortraitLargePath = UICharacterCustomizeManager.PortraitOptions[index].Replace("sm.", "lg.");
		base.Owner.SignalValueChanged(ValueType.Portrait);
	}

	private void OnClick()
	{
		int totalModelVariations = UICharacterCreationManager.GetTotalModelVariations(base.Owner.Character, Type);
		switch (Type)
		{
		case AppearanceType.FACIAL_HAIR:
			base.Owner.Character.FacialHairModelVariation = Wrap0(base.Owner.Character.FacialHairModelVariation + Adjustment, totalModelVariations);
			base.Owner.SignalValueChanged(ValueType.BodyPart);
			break;
		case AppearanceType.HAIR:
			if (base.Owner.Character.Race != CharacterStats.Race.Godlike)
			{
				base.Owner.Character.HairModelVariation = Wrap0(base.Owner.Character.HairModelVariation + Adjustment, totalModelVariations);
			}
			base.Owner.SignalValueChanged(ValueType.BodyPart);
			break;
		case AppearanceType.HEAD:
			base.Owner.Character.HeadModelVariation = Wrap1(base.Owner.Character.HeadModelVariation + Adjustment, totalModelVariations);
			if (base.Owner.Character.Race == CharacterStats.Race.Godlike)
			{
				base.Owner.Character.HairModelVariation = base.Owner.Character.HeadModelVariation;
			}
			base.Owner.SignalValueChanged(ValueType.BodyPart);
			break;
		case AppearanceType.PORTRAIT:
			s_pendingPortraitIndex = Wrap0(s_pendingPortraitIndex + Adjustment, UICharacterCustomizeManager.PortraitOptions.Count - 1);
			SetPortrait(s_pendingPortraitIndex);
			break;
		}
	}

	private int Wrap0(int val, int max)
	{
		max++;
		if (max == 0)
		{
			return 0;
		}
		return (val + max) % max;
	}

	private int Wrap1(int val, int max)
	{
		return (val - 1 + max) % max + 1;
	}
}

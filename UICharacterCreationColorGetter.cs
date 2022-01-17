using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UICharacterCreationColorGetter : UICharacterCreationElement
{
	public enum ColorType
	{
		SKIN,
		HAIR,
		MAJOR,
		MINOR
	}

	public ColorType Color;

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Color || type == ValueType.All)
		{
			switch (Color)
			{
			case ColorType.SKIN:
				GetComponent<UIWidget>().color = base.Owner.Character.SkinColor;
				break;
			case ColorType.HAIR:
				GetComponent<UIWidget>().color = base.Owner.Character.HairColor;
				break;
			case ColorType.MAJOR:
				GetComponent<UIWidget>().color = base.Owner.Character.MajorColor;
				break;
			case ColorType.MINOR:
				GetComponent<UIWidget>().color = base.Owner.Character.MinorColor;
				break;
			}
		}
	}
}

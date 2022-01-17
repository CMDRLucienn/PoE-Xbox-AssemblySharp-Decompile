using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class UICharacterCreationEnumGetter : UICharacterCreationElement
{
	public static string[] SpritePrefixes = new string[5] { "ICO_SEX_", "ICO_CLASS_", "ICO_RACE_", "ICO_SUBRACE_", "ICO_CULTURE_" };

	public UICharacterCreationEnumSetter.EnumType SetType;

	public static string SpriteForCharacter(UICharacterCreationEnumSetter.EnumType enumType, UICharacterCreationManager.Character character)
	{
		string text = "";
		switch (enumType)
		{
		case UICharacterCreationEnumSetter.EnumType.CLASS:
			text = character.Class.ToString();
			break;
		case UICharacterCreationEnumSetter.EnumType.GENDER:
			text = character.Gender.ToString();
			break;
		case UICharacterCreationEnumSetter.EnumType.RACE:
			text = character.Race.ToString();
			break;
		case UICharacterCreationEnumSetter.EnumType.SUBRACE:
			text = character.Subrace.ToString();
			break;
		case UICharacterCreationEnumSetter.EnumType.CULTURE:
			text = UICharacterCreationEnumSetter.s_PendingCulture.ToString();
			break;
		default:
			return "";
		}
		return SpritePrefixes[(int)enumType] + text.ToLower();
	}

	public static string SpriteForValue(UICharacterCreationEnumSetter.EnumType enumType, int val)
	{
		string text = "";
		switch (enumType)
		{
		case UICharacterCreationEnumSetter.EnumType.CLASS:
		{
			CharacterStats.Class @class = (CharacterStats.Class)val;
			text = @class.ToString();
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.GENDER:
		{
			Gender gender = (Gender)val;
			text = gender.ToString();
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.RACE:
		{
			CharacterStats.Race race = (CharacterStats.Race)val;
			text = race.ToString();
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.SUBRACE:
		{
			CharacterStats.Subrace subrace = (CharacterStats.Subrace)val;
			text = subrace.ToString();
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.CULTURE:
		{
			CharacterStats.Culture culture = (CharacterStats.Culture)val;
			text = culture.ToString();
			break;
		}
		default:
			return "";
		}
		return SpritePrefixes[(int)enumType] + text.ToLower();
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Class || type == ValueType.Gender || type == ValueType.Race || type == ValueType.Subrace || type == ValueType.Culture || type == ValueType.All)
		{
			string text = SpriteForCharacter(SetType, base.Owner.Character);
			GetComponent<UISprite>().spriteName = text;
			GetComponent<UISprite>().MakePixelPerfect();
			UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.resetSprites();
				component.normalSprite = text;
			}
		}
	}
}

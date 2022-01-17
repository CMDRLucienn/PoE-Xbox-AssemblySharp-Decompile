public class UICharacterCreationAttributeRecommendation : UICharacterCreationElement
{
	public CharacterStats.AttributeScoreType AttributeType;

	public UISprite Sprite;

	public override void SignalValueChanged(ValueType valueType)
	{
		if ((valueType != ValueType.Class && valueType != ValueType.All) || !Sprite)
		{
			return;
		}
		if (GameState.Mode.Expert)
		{
			Sprite.enabled = false;
			return;
		}
		CharacterStats.AttributeScoreType[] array = UICharacterCreationEnumSetter.GodTierAttributes[(int)base.Owner.Character.Class];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == AttributeType)
			{
				Sprite.enabled = true;
				Sprite.spriteName = "goldStar";
				Sprite.MakePixelPerfect();
				return;
			}
		}
		array = UICharacterCreationEnumSetter.ProTierAttributes[(int)base.Owner.Character.Class];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == AttributeType)
			{
				Sprite.enabled = true;
				Sprite.spriteName = "silverStar";
				Sprite.MakePixelPerfect();
				return;
			}
		}
		Sprite.enabled = false;
	}
}

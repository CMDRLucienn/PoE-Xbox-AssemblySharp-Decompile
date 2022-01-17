public struct UICharacterAutoAttackDropdownData
{
	public AIController.AggressionType Setting;

	public int StringID;

	public UICharacterAutoAttackDropdownData(AIController.AggressionType setting, int stringID)
	{
		Setting = setting;
		StringID = stringID;
	}

	public override string ToString()
	{
		return GUIUtils.GetText(StringID);
	}
}

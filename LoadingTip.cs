using System;

[Serializable]
public class LoadingTip
{
	public DatabaseString Tip = new DatabaseString(DatabaseString.StringTableType.LoadingTips);

	public string Text => Tip.ToString();
}

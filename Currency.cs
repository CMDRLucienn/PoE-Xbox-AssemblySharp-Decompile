using System.Text;
using UnityEngine;

public class Currency : Item
{
	public override string GetString(GameObject owner)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GUIUtils.GetText(1499));
		stringBuilder.Append(": ");
		stringBuilder.AppendGuiFormat(466, Value);
		string @string = base.GetString(owner);
		if (!string.IsNullOrEmpty(@string))
		{
			stringBuilder.AppendLine();
		}
		stringBuilder.Append(@string);
		return stringBuilder.ToString();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class JournalBiographyData : ScriptableObject
{
	[Serializable]
	public class BackgroundDesc
	{
		public CharacterStats.Background Key;

		public BackstoryDatabaseString Text = new BackstoryDatabaseString(-1);
	}

	[GlobalVariableString]
	public string SpecificBackgroundGlobal;

	[GlobalVariableString]
	public string LeftBecauseGlobal;

	[GlobalVariableString]
	public string FutureMotivationGlobal;

	public BackgroundDesc[] Backgrounds;

	public BackstoryDatabaseString[] SpecificBackgrounds;

	public BackstoryDatabaseString[] LeftBecause;

	public BackstoryDatabaseString[] FutureMotivations;

	public string GetBiographyString(CharacterStats.Background background)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerable<BackgroundDesc> source = Backgrounds.Where((BackgroundDesc e) => e.Key == background);
		if (source.Any())
		{
			stringBuilder.Append(source.First().Text.GetText().Trim());
		}
		else
		{
			Debug.LogError("No description found for Background '" + background.ToString() + "'.");
		}
		int variable = GlobalVariables.Instance.GetVariable(SpecificBackgroundGlobal);
		if (variable < SpecificBackgrounds.Length)
		{
			string text = SpecificBackgrounds[variable].GetText();
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append("\n\n");
				stringBuilder.Append(text + " ");
			}
		}
		else if (variable != 0)
		{
			Debug.LogError("Specific Background index '" + variable + "' is out of bounds.");
		}
		int variable2 = GlobalVariables.Instance.GetVariable(LeftBecauseGlobal);
		if (variable2 >= 0)
		{
			if (variable2 < LeftBecause.Length)
			{
				string text2 = LeftBecause[variable2].GetText();
				if (!string.IsNullOrEmpty(text2))
				{
					stringBuilder.Append(text2);
				}
			}
			else if (variable2 != 0)
			{
				Debug.LogError("Left Because index '" + variable2 + "' is out of bounds.");
			}
		}
		int variable3 = GlobalVariables.Instance.GetVariable(FutureMotivationGlobal);
		if (variable3 >= 0)
		{
			if (variable3 < FutureMotivations.Length)
			{
				string text3 = FutureMotivations[variable3].GetText();
				if (!string.IsNullOrEmpty(text3))
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(text3);
				}
			}
			else if (variable3 != 0)
			{
				Debug.LogError("Future Motivation index '" + variable3 + "' is out of bounds.");
			}
		}
		return Conversation.ReplacePlayerTokens(stringBuilder.ToString()).Trim();
	}
}

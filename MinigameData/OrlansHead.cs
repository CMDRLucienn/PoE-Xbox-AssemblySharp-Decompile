using System.Collections.Generic;

namespace MinigameData;

public static class OrlansHead
{
	public enum Result
	{
		MISS,
		FUR,
		SKIN,
		EAR,
		MOUTH,
		EYE,
		NOSE,
		COUNT
	}

	public enum Approach
	{
		EARS,
		NOSE
	}

	public static Dictionary<Result, int> ResultValues = new Dictionary<Result, int>
	{
		{
			Result.MISS,
			0
		},
		{
			Result.FUR,
			2
		},
		{
			Result.SKIN,
			1
		},
		{
			Result.EAR,
			7
		},
		{
			Result.MOUTH,
			5
		},
		{
			Result.EYE,
			5
		},
		{
			Result.NOSE,
			10
		}
	};

	private static int[][] s_ApproachResultCutoffs = new int[2][]
	{
		new int[7] { 20, 40, 55, 75, 85, 95, 100 },
		new int[7] { 20, 35, 50, 65, 80, 90, 100 }
	};

	public static Result[] ContestantResults = new Result[2];

	public static int[] Scores = new int[2];

	public static int RoundCount = 0;

	public static void DoRoll(Contestant contestant, CharacterStats thrower, Approach approach)
	{
		int num = OEIRandom.DieRoll(100);
		if ((bool)thrower)
		{
			num += (thrower.Perception - 10) * 2;
			num += (thrower.Dexterity - 10) * 2;
		}
		for (int i = 0; i < 7; i++)
		{
			if (num < s_ApproachResultCutoffs[(int)approach][i])
			{
				ContestantResults[(int)contestant] = (Result)i;
				Scores[(int)contestant] += ResultValues[ContestantResults[(int)contestant]];
				return;
			}
		}
		if (approach == Approach.EARS)
		{
			ContestantResults[(int)contestant] = Result.EAR;
		}
		else
		{
			ContestantResults[(int)contestant] = Result.NOSE;
		}
		Scores[(int)contestant] += ResultValues[ContestantResults[(int)contestant]];
	}

	public static void Reset()
	{
		for (int i = 0; i < ContestantResults.Length; i++)
		{
			Scores[i] = 0;
		}
		RoundCount = 0;
	}

	public static bool WinnerIs(Contestant c)
	{
		return Scores[(int)c] > Scores[(int)(1 - c)];
	}

	public static bool IsTied()
	{
		return Scores[0] == Scores[1];
	}

	public static string ReplaceTokens(string text)
	{
		if (!text.Contains("OrlansHeadGame"))
		{
			return text;
		}
		text = text.Replace("[OrlansHeadGame_PlayerLastResult]", GUIUtils.GetOrlanResultString(ContestantResults[0]));
		text = text.Replace("[OrlansHeadGame_PlayerLastScore]", ResultValues[ContestantResults[0]].ToString());
		text = text.Replace("[OrlansHeadGame_PlayerTotalScore]", Scores[0].ToString());
		text = text.Replace("[OrlansHeadGame_OpponentLastResult]", GUIUtils.GetOrlanResultString(ContestantResults[1]));
		text = text.Replace("[OrlansHeadGame_OpponentLastScore]", ResultValues[ContestantResults[1]].ToString());
		text = text.Replace("[OrlansHeadGame_OpponentTotalScore]", Scores[1].ToString());
		return text;
	}
}

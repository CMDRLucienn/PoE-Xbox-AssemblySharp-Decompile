using System;

namespace MinigameData;

public static class Dozens
{
	public enum Result
	{
		TALKERS,
		THE_SAINT,
		WITNESSES,
		TOWERS,
		GODHAMMERS,
		THE_DOZENS
	}

	public class RollData : IComparable<RollData>
	{
		public int[] DiceValues = new int[3];

		public Result ResultType;

		public int ResultValue;

		public RollData()
		{
			Reroll();
		}

		public void Reroll()
		{
			for (int i = 0; i < DiceValues.Length; i++)
			{
				DiceValues[i] = OEIRandom.DieRoll(6);
			}
			Array.Sort(DiceValues);
			if (DiceValues[0] == 4 && DiceValues[1] == 4 && DiceValues[2] == 4)
			{
				ResultType = Result.THE_DOZENS;
				ResultValue = -1;
			}
			else if (DiceValues[0] == DiceValues[1] && DiceValues[1] == DiceValues[2])
			{
				ResultType = Result.GODHAMMERS;
				ResultValue = DiceValues[0];
			}
			else if (DiceValues[0] + DiceValues[1] + DiceValues[2] == 12)
			{
				ResultType = Result.TOWERS;
				ResultValue = DiceValues[2];
			}
			else if (DiceValues[0] == DiceValues[1])
			{
				ResultType = Result.WITNESSES;
				ResultValue = DiceValues[2];
			}
			else if (DiceValues[1] == DiceValues[2])
			{
				ResultType = Result.WITNESSES;
				ResultValue = DiceValues[0];
			}
			else if (DiceValues[0] == 1 && DiceValues[1] == 2 && DiceValues[2] == 3)
			{
				ResultType = Result.THE_SAINT;
				ResultValue = -1;
			}
			else
			{
				ResultType = Result.TALKERS;
				ResultValue = -1;
			}
		}

		public string GetNumsString()
		{
			return TextUtils.FuncJoin((int i) => i.ToString(), DiceValues, " ");
		}

		public string GetDescString()
		{
			if (ResultValue >= 0)
			{
				return GUIUtils.Format(2142, GUIUtils.GetDozensResultString(ResultType), ResultValue);
			}
			return GUIUtils.GetDozensResultString(ResultType);
		}

		public int CompareTo(RollData other)
		{
			if (ResultType != other.ResultType)
			{
				return ResultType - other.ResultType;
			}
			return ResultValue - other.ResultValue;
		}

		public override bool Equals(object obj)
		{
			if (obj is RollData other)
			{
				return CompareTo(other) == 0;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ResultType.GetHashCode();
		}
	}

	public static RollData[] ContestantRolls = new RollData[2]
	{
		new RollData(),
		new RollData()
	};

	public static void DoRoll(Contestant c)
	{
		ContestantRolls[(int)c].Reroll();
	}

	public static bool WinnerIs(Contestant c)
	{
		return ContestantRolls[(int)c].CompareTo(ContestantRolls[(int)(1 - c)]) > 0;
	}

	public static bool IsTied()
	{
		return ContestantRolls[0].CompareTo(ContestantRolls[1]) == 0;
	}

	public static string ReplaceTokens(string text)
	{
		if (!text.Contains("DozensGame"))
		{
			return text;
		}
		text = text.Replace("[DozensGame_LastPlayerRollNums]", ContestantRolls[0].GetNumsString());
		text = text.Replace("[DozensGame_LastOpponentRollNums]", ContestantRolls[1].GetNumsString());
		text = text.Replace("[DozensGame_LastPlayerRollDesc]", ContestantRolls[0].GetDescString());
		text = text.Replace("[DozensGame_LastOpponentRollDesc]", ContestantRolls[1].GetDescString());
		string dozensResultString = GUIUtils.GetDozensResultString(ContestantRolls[0].ResultType);
		string dozensResultString2 = GUIUtils.GetDozensResultString(ContestantRolls[1].ResultType);
		text = text.Replace("[DozensGame_PlayerFinalDesc]", dozensResultString);
		text = text.Replace("[DozensGame_OpponentFinalDesc]", dozensResultString2);
		return text;
	}
}

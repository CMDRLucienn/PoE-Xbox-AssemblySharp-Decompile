using System;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class Reputation
{
	public enum ChangeStrength
	{
		None = 0,
		VeryMinor = 1,
		Minor = 2,
		Average = 4,
		Major = 6,
		VeryMajor = 8
	}

	public enum Axis
	{
		Positive,
		Negative
	}

	public enum RankType
	{
		Default,
		Good,
		Bad,
		Mixed
	}

	public string EditorName = string.Empty;

	public FactionName FactionID;

	public FactionDatabaseString Name = new FactionDatabaseString();

	public int Scale = 50;

	[ExcludeFromSerialization]
	public Texture2D DisplayImage;

	public int Positive;

	public int Negative;

	public TitleStringSet TitleStrings { get; set; }

	public FactionDatabaseString SerializedName
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	[ExcludeFromSerialization]
	public int GoodRank
	{
		get
		{
			float reputationPct = GetReputationPct(Axis.Positive);
			float reputationPct2 = GetReputationPct(Axis.Negative);
			if (reputationPct < float.Epsilon)
			{
				return 0;
			}
			if (reputationPct >= 1f && reputationPct2 < 0.15f)
			{
				return 5;
			}
			if (reputationPct >= 1f && reputationPct2 < 0.5f)
			{
				return 4;
			}
			if (reputationPct > 0.5f && reputationPct2 < 0.15f)
			{
				return 3;
			}
			if (reputationPct > 0.5f && reputationPct2 < 0.5f)
			{
				return 2;
			}
			if (reputationPct > 0.15f && reputationPct2 < 0.15f)
			{
				return 1;
			}
			return 0;
		}
	}

	[ExcludeFromSerialization]
	public int BadRank
	{
		get
		{
			float reputationPct = GetReputationPct(Axis.Positive);
			float reputationPct2 = GetReputationPct(Axis.Negative);
			if (reputationPct2 < float.Epsilon)
			{
				return 0;
			}
			if (reputationPct2 >= 1f && reputationPct < 0.15f)
			{
				return 5;
			}
			if (reputationPct2 >= 1f && reputationPct < 0.5f)
			{
				return 4;
			}
			if (reputationPct2 > 0.5f && reputationPct < 0.15f)
			{
				return 3;
			}
			if (reputationPct2 > 0.5f && reputationPct < 0.5f)
			{
				return 2;
			}
			if (reputationPct2 > 0.15f && reputationPct < 0.15f)
			{
				return 1;
			}
			return 0;
		}
	}

	[ExcludeFromSerialization]
	public DatabaseString[] MixedTitles => TitleStrings.MixedTitles;

	[ExcludeFromSerialization]
	public DatabaseString[] MixedDescription => TitleStrings.MixedDescription;

	[ExcludeFromSerialization]
	public DatabaseString[] GoodTitles => TitleStrings.GoodTitles;

	[ExcludeFromSerialization]
	public DatabaseString[] GoodDescription => TitleStrings.GoodDescription;

	[ExcludeFromSerialization]
	public DatabaseString[] BadTitles => TitleStrings.BadTitles;

	[ExcludeFromSerialization]
	public DatabaseString[] BadDescription => TitleStrings.BadDescription;

	[ExcludeFromSerialization]
	public float ScaleMultiplier => (float)Scale / 100f;

	[ExcludeFromSerialization]
	public string Title
	{
		get
		{
			RankType rankType = RankType.Default;
			int num = GetRank(out rankType) - 1;
			if (num < 0)
			{
				return StringTableManager.GetText(DatabaseString.StringTableType.Factions, 0);
			}
			return rankType switch
			{
				RankType.Bad => TitleStrings.BadTitles[num].GetText(), 
				RankType.Default => TitleStrings.DefaultTitles[num].GetText(), 
				RankType.Good => TitleStrings.GoodTitles[num].GetText(), 
				RankType.Mixed => TitleStrings.MixedTitles[num].GetText(), 
				_ => string.Empty, 
			};
		}
	}

	[ExcludeFromSerialization]
	public string TitleDescription
	{
		get
		{
			RankType rankType = RankType.Default;
			int num = GetRank(out rankType) - 1;
			return rankType switch
			{
				RankType.Bad => TitleStrings.BadDescription[num].GetText(), 
				RankType.Default => TitleStrings.DefaultDescription[num].GetText(), 
				RankType.Good => TitleStrings.GoodDescription[num].GetText(), 
				RankType.Mixed => TitleStrings.MixedDescription[num].GetText(), 
				_ => string.Empty, 
			};
		}
	}

	public void OnStart()
	{
		if (ReputationManager.Instance != null)
		{
			TitleStrings = ReputationManager.Instance.DefaultTitles;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is Reputation reputation)
		{
			return reputation.FactionID == FactionID;
		}
		if (obj is FactionName)
		{
			return FactionID == (FactionName)obj;
		}
		if (obj is string)
		{
			return FactionID.ToString().ToLower() == (obj as string).ToLower();
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return FactionID.GetHashCode();
	}

	public void AddReputation(Axis axis, ChangeStrength strength)
	{
		AddReputation(axis, (int)strength);
	}

	protected void AddReputation(Axis axis, int amount)
	{
		if (amount != 0)
		{
			switch (axis)
			{
			case Axis.Negative:
			{
				string text2 = " (" + GUIUtils.GetReputationChangeStrengthString((ChangeStrength)amount) + ")";
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1765), Name) + text2, Color.green);
				Negative += amount;
				break;
			}
			case Axis.Positive:
			{
				string text = " (" + GUIUtils.GetReputationChangeStrengthString((ChangeStrength)amount) + ")";
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1766), Name) + text, Color.green);
				Positive += amount;
				break;
			}
			}
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.REPUTATION_GAINED);
		}
	}

	public float GetReputationPct(Axis axis)
	{
		return axis switch
		{
			Axis.Positive => (float)Positive / (float)Scale, 
			Axis.Negative => (float)Negative / (float)Scale, 
			_ => 0f, 
		};
	}

	private bool Between(float value, float min, float max)
	{
		if (value >= min)
		{
			return value < max;
		}
		return false;
	}

	public int GetRank(out RankType rankType)
	{
		float reputationPct = GetReputationPct(Axis.Positive);
		float reputationPct2 = GetReputationPct(Axis.Negative);
		if (reputationPct < float.Epsilon && reputationPct2 < float.Epsilon)
		{
			rankType = RankType.Default;
			return 0;
		}
		if (reputationPct < 0.15f && reputationPct2 < 0.15f)
		{
			rankType = RankType.Default;
			return 1;
		}
		if (Between(reputationPct, 0.15f, 0.5f) && Between(reputationPct2, 0.15f, 0.5f))
		{
			rankType = RankType.Mixed;
			return 1;
		}
		if (Between(reputationPct, 0.5f, 1f) && Between(reputationPct2, 0.5f, 1f))
		{
			rankType = RankType.Mixed;
			return 2;
		}
		if (Between(reputationPct, 0.5f, 1f) && reputationPct2 >= 1f)
		{
			rankType = RankType.Mixed;
			return 3;
		}
		if (reputationPct >= 1f && Between(reputationPct2, 0.5f, 1f))
		{
			rankType = RankType.Mixed;
			return 3;
		}
		if (reputationPct >= 1f && reputationPct2 >= 1f)
		{
			rankType = RankType.Mixed;
			return 4;
		}
		if (Between(reputationPct, 0.15f, 0.5f) && reputationPct2 < 0.15f)
		{
			rankType = RankType.Good;
			return 1;
		}
		if (Between(reputationPct, 0.5f, 1f) && Between(reputationPct2, 0.15f, 0.5f))
		{
			rankType = RankType.Good;
			return 2;
		}
		if (Between(reputationPct, 0.5f, 1f) && reputationPct2 < 0.15f)
		{
			rankType = RankType.Good;
			return 3;
		}
		if (reputationPct >= 1f && Between(reputationPct2, 0.15f, 0.5f))
		{
			rankType = RankType.Good;
			return 4;
		}
		if (reputationPct >= 1f && reputationPct2 < 0.15f)
		{
			rankType = RankType.Good;
			return 5;
		}
		if (reputationPct < 0.15f && Between(reputationPct2, 0.15f, 0.5f))
		{
			rankType = RankType.Bad;
			return 1;
		}
		if (Between(reputationPct, 0.15f, 0.5f) && Between(reputationPct2, 0.5f, 1f))
		{
			rankType = RankType.Bad;
			return 2;
		}
		if (reputationPct < 0.15f && Between(reputationPct2, 0.5f, 1f))
		{
			rankType = RankType.Bad;
			return 3;
		}
		if (Between(reputationPct, 0.15f, 0.5f) && reputationPct2 >= 1f)
		{
			rankType = RankType.Bad;
			return 4;
		}
		if (reputationPct < 0.15f && reputationPct2 >= 1f)
		{
			rankType = RankType.Bad;
			return 5;
		}
		Debug.LogError("ERROR: Reputation.GetRank fell through to default case.");
		rankType = RankType.Default;
		return 0;
	}

	public Reputation Clone()
	{
		return MemberwiseClone() as Reputation;
	}
}

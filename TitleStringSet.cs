using System;

[Serializable]
public class TitleStringSet
{
	public const int MAX_TITLES = 5;

	public FactionDatabaseString[] DefaultTitles;

	public FactionDatabaseString[] DefaultDescription;

	public FactionDatabaseString[] MixedTitles;

	public FactionDatabaseString[] MixedDescription;

	public FactionDatabaseString[] GoodTitles;

	public FactionDatabaseString[] GoodDescription;

	public FactionDatabaseString[] BadTitles;

	public FactionDatabaseString[] BadDescription;

	public FactionDatabaseString[] SerializedDefaultTitles { get; set; }

	public FactionDatabaseString[] SerializedDefaultDescription { get; set; }

	public FactionDatabaseString[] SerializedMixedTitles { get; set; }

	public FactionDatabaseString[] SerializedMixedDescription { get; set; }

	public FactionDatabaseString[] SerializedGoodTitles { get; set; }

	public FactionDatabaseString[] SerializedGoodDescription { get; set; }

	public FactionDatabaseString[] SerializedBadTitles { get; set; }

	public FactionDatabaseString[] SerializedBadDescription { get; set; }

	public TitleStringSet()
	{
		DefaultTitles = new FactionDatabaseString[2];
		DefaultDescription = new FactionDatabaseString[2];
		MixedTitles = new FactionDatabaseString[5];
		MixedDescription = new FactionDatabaseString[5];
		GoodTitles = new FactionDatabaseString[5];
		GoodDescription = new FactionDatabaseString[5];
		BadTitles = new FactionDatabaseString[5];
		BadDescription = new FactionDatabaseString[5];
		for (int i = 0; i < 5; i++)
		{
			MixedTitles[i] = new FactionDatabaseString(i + 5);
			GoodTitles[i] = new FactionDatabaseString(i + 10);
			BadTitles[i] = new FactionDatabaseString(i + 15);
		}
	}
}

using System;
using UnityEngine.Serialization;

[Serializable]
public class DifficultySettings
{
	public bool Easy = true;

	public bool Normal = true;

	public bool Hard = true;

	[EnumFlags]
	[FormerlySerializedAs("Scale")]
	public DifficultyScaling.Scaler RequiresAnyOf;

	public bool AppearsInBaseDifficulty(GameDifficulty difficulty)
	{
		return difficulty switch
		{
			GameDifficulty.Easy => Easy, 
			GameDifficulty.Normal => Normal, 
			GameDifficulty.Hard => Hard, 
			_ => throw new ArgumentException("DifficultySettings doesn't have data for difficulty '" + difficulty.ToString() + "'.", "difficulty"), 
		};
	}
}

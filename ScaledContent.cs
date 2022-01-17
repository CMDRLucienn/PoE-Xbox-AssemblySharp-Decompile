using UnityEngine;

public class ScaledContent : MonoBehaviour
{
	[EnumFlags]
	[Tooltip("The scalers that will apply to this object, if they are enabled.")]
	public DifficultyScaling.Scaler Scalers;

	public static implicit operator DifficultyScaling.Scaler(ScaledContent content)
	{
		if ((bool)content)
		{
			return content.Scalers;
		}
		return (DifficultyScaling.Scaler)0;
	}
}

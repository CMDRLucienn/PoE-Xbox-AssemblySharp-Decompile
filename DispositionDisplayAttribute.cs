using System;
using OEIFormats.FlowCharts;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Obsidian Tools can't call methods into the Unity DLL, so this method of handling conditional display will not work.")]
public class DispositionDisplayAttribute : ConditionalStatDisplayAttribute
{
	public DispositionDisplayAttribute(string paramTypeName, string paramValueName)
		: base(paramTypeName, paramValueName)
	{
	}

	public override bool ShouldShowDisplayString(ConditionalCall call, bool isPlayerResponse)
	{
		if (!isPlayerResponse)
		{
			return GameState.Option.DisplayPersonalityReputationIndicators;
		}
		return true;
	}
}

using UnityEngine;

public interface ITooltipContent
{
	string GetTooltipContent(GameObject owner);

	string GetTooltipName(GameObject owner);

	Texture GetTooltipIcon();
}

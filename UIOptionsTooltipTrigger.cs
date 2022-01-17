using UnityEngine;

public class UIOptionsTooltipTrigger : MonoBehaviour
{
	public GUIDatabaseString TooltipString;

	private void OnHover(bool over)
	{
		if (over)
		{
			UIOptionsTooltip.Show(TooltipString.GetText());
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}

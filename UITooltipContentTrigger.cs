using UnityEngine;

public class UITooltipContentTrigger : MonoBehaviour
{
	public ITooltipContent Tooltippable;

	public MonoBehaviour Inspectable;

	private void OnTooltip(bool over)
	{
		if (over && Tooltippable != null)
		{
			UIAbilityTooltip.GlobalShow(GetComponent<UIWidget>(), Tooltippable);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	private void OnRightClick()
	{
		if ((bool)Inspectable)
		{
			UIItemInspectManager.Examine(Inspectable);
		}
	}

	public void Set(ITooltipContent tool, MonoBehaviour inspect)
	{
		Tooltippable = tool;
		Inspectable = inspect;
	}

	public void Clear()
	{
		Set(null, null);
	}
}

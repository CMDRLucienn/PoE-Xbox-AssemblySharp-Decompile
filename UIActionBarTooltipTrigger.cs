using UnityEngine;

public class UIActionBarTooltipTrigger : MonoBehaviour
{
	public DatabaseString Text = new DatabaseString(DatabaseString.StringTableType.Gui);

	public UIWidget Widget;

	public string OverrideText { get; set; }

	private void Start()
	{
		Widget = Widget ?? GetComponent<UIWidget>();
	}

	private void OnTooltip(bool over)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (over)
		{
			string text = "";
			if (!string.IsNullOrEmpty(OverrideText))
			{
				text = OverrideText;
			}
			else if (Text != null)
			{
				text = Text.GetText();
			}
			UIActionBarTooltip.GlobalShow(Widget ? Widget : GetComponent<UIWidget>(), text);
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}
}

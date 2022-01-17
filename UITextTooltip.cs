using UnityEngine;

public class UITextTooltip : MonoBehaviour
{
	public UILabel TitleLabel;

	public UILabel ContentLabel;

	public UIPanel Panel;

	public UIAnchor Anchor;

	public static UITextTooltip Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Anchor = GetComponent<UIAnchor>();
		Anchor.enabled = true;
		Panel.alpha = 0f;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void Show(UIWidget anchor, string title, string content)
	{
		Anchor.widgetContainer = anchor;
		TitleLabel.text = title;
		ContentLabel.text = content;
		Panel.alpha = 1f;
	}

	public void Hide()
	{
		Panel.alpha = 0f;
	}
}

public class UIPartyPortraitIconTooltip : UIBaseTooltip
{
	public UILabel Label;

	private static UIPartyPortraitIconTooltip s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	protected override void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void SetText(string text)
	{
		Label.text = text;
	}

	public static void GlobalShow(UIWidget button, string text)
	{
		if (s_Instance != null)
		{
			s_Instance.Show(button, text);
		}
	}

	public static void GlobalHide()
	{
		if (s_Instance != null)
		{
			s_Instance.Hide();
		}
	}
}

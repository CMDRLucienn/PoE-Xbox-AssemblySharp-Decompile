public class UIMemorialManager : UIHudWindow
{
	private static UIMemorialManager m_Instance;

	public MemorialContainer SourceContainer;

	public UIMemorialNamesWindow NamesWindow;

	public static UIMemorialManager Instance => m_Instance;

	private void Awake()
	{
		m_Instance = this;
	}

	protected override void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Show()
	{
		base.gameObject.SetActive(value: true);
		foreach (MemorialContainer.Memorial memorial in SourceContainer.m_Memorials)
		{
			NamesWindow.AddMemorialName(memorial.Name, memorial.Description);
		}
		NamesWindow.NameGrid.Reposition();
		NamesWindow.SelectEntry(0);
	}

	protected override bool Hide(bool forced)
	{
		NamesWindow.Clear();
		return base.Hide(forced);
	}
}

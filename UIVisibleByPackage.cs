using UnityEngine;

public class UIVisibleByPackage : MonoBehaviour
{
	[EnumFlags]
	public ProductConfiguration.Package Packages;

	public bool Invert;

	private UIWidget m_Widget;

	private UIPanel m_Panel;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Panel = GetComponent<UIPanel>();
	}

	private void Update()
	{
		bool flag = Invert != ((ProductConfiguration.ActivePackage & Packages) != 0);
		if ((bool)m_Widget)
		{
			m_Widget.alpha = (flag ? 1f : 0f);
		}
		if ((bool)m_Panel)
		{
			m_Panel.alpha = (flag ? 1f : 0f);
		}
	}
}

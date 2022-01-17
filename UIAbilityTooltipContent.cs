using UnityEngine;

public class UIAbilityTooltipContent : MonoBehaviour
{
	public UITexture Icon;

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public GameObject DefendedByParent;

	public UIAbilityTooltipLeaderIcons LeaderIcons;

	private UIAnchor m_TitleAnchor;

	private UITable[] m_Tables;

	public ITooltipContent Data { get; set; }

	public GameObject Owner { get; set; }

	private void Awake()
	{
		m_Tables = GetComponentsInChildren<UITable>();
	}

	public void Refresh()
	{
		Icon.mainTexture = Data.GetTooltipIcon();
		Icon.transform.parent.gameObject.SetActive(Icon.mainTexture != null);
		if (!m_TitleAnchor)
		{
			m_TitleAnchor = TitleLabel.GetComponent<UIAnchor>();
		}
		if (Icon.transform.parent.gameObject.activeSelf)
		{
			TitleLabel.lineWidth = 205;
			m_TitleAnchor.side = UIAnchor.Side.Right;
			m_TitleAnchor.pixelOffset.x = 6f;
		}
		else
		{
			TitleLabel.lineWidth = 259;
			m_TitleAnchor.side = UIAnchor.Side.Left;
			m_TitleAnchor.pixelOffset.x = 0f;
		}
		if (TitleLabel != null)
		{
			TitleLabel.text = Data.GetTooltipName(Owner);
			TitleLabel.gameObject.SetActive(!string.IsNullOrEmpty(TitleLabel.text));
		}
		if (DescLabel != null)
		{
			string content = Data.GetTooltipContent(Owner);
			if ((bool)LeaderIcons)
			{
				LeaderIcons.Load(ref content);
				DefendedByParent.SetActive(!LeaderIcons.Empty);
			}
			DescLabel.text = content;
			DescLabel.gameObject.SetActive(!string.IsNullOrEmpty(DescLabel.text));
		}
		UIWidgetUtils.UpdateDependents(base.gameObject, 1);
		UITable[] tables = m_Tables;
		for (int i = 0; i < tables.Length; i++)
		{
			tables[i].Reposition();
		}
	}
}

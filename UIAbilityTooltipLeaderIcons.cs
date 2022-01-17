using System.Collections.Generic;
using UnityEngine;

public class UIAbilityTooltipLeaderIcons : MonoBehaviour
{
	public UISprite RootSprite;

	private UIGrid m_Grid;

	private List<UISprite> m_Icons = new List<UISprite>();

	public float Width => m_Grid.cellWidth * (float)m_Grid.ChildCount;

	public bool Empty => m_Grid.ChildCount == 0;

	private void Awake()
	{
		m_Grid = GetComponent<UIGrid>();
		RootSprite.gameObject.SetActive(value: false);
		m_Icons.Add(RootSprite);
	}

	public void Load(ref string content)
	{
		int num = 0;
		while (content.StartsWith("<"))
		{
			int i;
			for (i = 0; i < content.Length && content[i] != '>'; i++)
			{
			}
			string text = content.Substring(1, i - 1);
			content = content.Substring(i + 1);
			UISprite icon = GetIcon(num++);
			icon.gameObject.SetActive(value: true);
			icon.spriteName = text + "_42px";
			icon.color = UIPartyMemberStatIconGetter.GetIconColor(text);
		}
		for (int j = num; j < m_Icons.Count; j++)
		{
			m_Icons[j].gameObject.SetActive(value: false);
		}
		m_Grid.Reposition();
	}

	private UISprite GetIcon(int index)
	{
		while (index >= m_Icons.Count)
		{
			UISprite component = NGUITools.AddChild(base.gameObject, RootSprite.gameObject).GetComponent<UISprite>();
			component.transform.localScale = RootSprite.transform.localScale;
			component.gameObject.SetActive(value: false);
			m_Icons.Add(component);
		}
		return m_Icons[index];
	}
}

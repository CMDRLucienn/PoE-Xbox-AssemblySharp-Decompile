using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIAbilityTooltip : MonoBehaviour
{
	protected const int DEFAULT_ICON_SIZE = 42;

	private static UIAbilityTooltip m_instance;

	public UIWidget Background;

	public UITable ContentTable;

	public UIAbilityTooltipContent ContentTemplate;

	private UILikeContext m_LikeContext;

	private UIStretchToContents m_BackgroundStretch;

	private UIPanel m_Panel;

	private UIAnchor m_Anchor;

	private bool m_DoShow;

	private IEnumerable<ITooltipContent> m_Data;

	private List<UIAbilityTooltipContent> m_Content = new List<UIAbilityTooltipContent>();

	public static bool VisibleThisFrame;

	public static bool IgnoreHide;

	private void Awake()
	{
		m_instance = this;
	}

	private void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
		}
		m_Content.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (!m_Panel)
		{
			ContentTemplate.gameObject.SetActive(value: false);
			m_Panel = GetComponent<UIPanel>();
			m_Anchor = GetComponent<UIAnchor>();
			m_LikeContext = GetComponentInChildren<UILikeContext>();
			m_BackgroundStretch = Background.GetComponent<UIStretchToContents>();
			Hide();
		}
	}

	private void Update()
	{
		if (m_DoShow)
		{
			if (m_Data != null)
			{
				RefreshContent();
			}
			m_Panel.alpha = 1f;
		}
		if ((bool)m_Anchor.widgetContainer && (!m_Anchor.widgetContainer.isVisible || !m_Anchor.widgetContainer.gameObject.activeInHierarchy))
		{
			Hide();
		}
	}

	private void LateUpdate()
	{
		IgnoreHide = false;
		if (m_Panel != null)
		{
			VisibleThisFrame = m_Panel.alpha > 0f;
		}
	}

	private void RefreshContent()
	{
		foreach (UIAbilityTooltipContent item in m_Content)
		{
			if (item.gameObject.activeInHierarchy)
			{
				item.Refresh();
			}
		}
		ContentTable.Reposition();
		UIWidgetUtils.UpdateDependents(base.gameObject, 1);
		m_BackgroundStretch.DoUpdate();
		m_Panel.Refresh();
	}

	public void SetHorizontal()
	{
		m_instance.ContentTable.columns = int.MaxValue;
	}

	public void Show(UIWidget element, IEnumerable<ITooltipContent> data, UIWidget.Pivot prefer, float iconSize = 42f)
	{
		Show(element, data, prefer, null, iconSize);
	}

	public void Show(Vector3 position, IEnumerable<ITooltipContent> data, UIWidget.Pivot prefer, GameObject owner, float iconSize = 42f)
	{
		if (m_Data == null || !data.SequenceEqual(m_Data))
		{
			Init();
			m_Anchor.enabled = false;
			position.z = m_Anchor.transform.position.z;
			m_Anchor.widgetContainer = null;
			m_Anchor.transform.position = position;
			Show(data, prefer, owner, iconSize);
		}
	}

	public void Show(UIWidget element, IEnumerable<ITooltipContent> data, UIWidget.Pivot prefer, GameObject owner, float iconSize = 42f)
	{
		if (m_Data == null || !data.SequenceEqual(m_Data))
		{
			Init();
			m_Anchor.enabled = true;
			m_Anchor.widgetContainer = element;
			Show(data, prefer, owner, iconSize);
		}
	}

	public void Show(IEnumerable<ITooltipContent> data, UIWidget.Pivot prefer, GameObject owner, float iconSize = 42f)
	{
		if (m_Data != null && data.SequenceEqual(m_Data))
		{
			return;
		}
		Init();
		m_LikeContext.Prefer = prefer;
		m_DoShow = true;
		m_Panel.alpha = 0f;
		if (UIWindowManager.Instance != null)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z = UIWindowManager.Instance.NextMessageBoxDepth(null);
			base.transform.localPosition = localPosition;
		}
		m_Data = data;
		int i = 0;
		foreach (ITooltipContent datum in m_Data)
		{
			if (datum != null)
			{
				UIAbilityTooltipContent uIAbilityTooltipContent = null;
				if (i >= m_Content.Count)
				{
					GameObject obj = NGUITools.AddChild(ContentTable.gameObject, ContentTemplate.gameObject);
					uIAbilityTooltipContent = obj.GetComponent<UIAbilityTooltipContent>();
					obj.name = i.ToString("000") + ".Content";
					m_Content.Add(uIAbilityTooltipContent);
				}
				else
				{
					uIAbilityTooltipContent = m_Content[i];
				}
				uIAbilityTooltipContent.gameObject.SetActive(value: true);
				uIAbilityTooltipContent.Data = datum;
				uIAbilityTooltipContent.Owner = owner;
				uIAbilityTooltipContent.Icon.transform.localScale = iconSize * Vector3.one;
				i++;
			}
		}
		for (; i < m_Content.Count; i++)
		{
			m_Content[i].gameObject.SetActive(value: false);
		}
	}

	public void Hide()
	{
		if (!IgnoreHide)
		{
			Init();
			ContentTable.columns = 1;
			m_Panel.alpha = 0f;
			m_DoShow = false;
			m_Data = null;
		}
	}

	public static void GlobalShow(UIWidget element, params ITooltipContent[] data)
	{
		UIAbilityTooltipManager.Instance.Show(0, element, data);
	}

	public static void GlobalShow(Vector3 position, params ITooltipContent[] data)
	{
		UIAbilityTooltipManager.Instance.Show(0, position, null, UIWidget.Pivot.BottomLeft, data);
	}

	public static void GlobalShow(UIWidget element, GameObject owner, params ITooltipContent[] data)
	{
		UIAbilityTooltipManager.Instance.Show(0, element, owner, UIWidget.Pivot.BottomLeft, data);
	}

	public static void GlobalShow(UIWidget element, UIWidget.Pivot prefer, GameObject owner, params ITooltipContent[] data)
	{
		UIAbilityTooltipManager.Instance.Show(0, element, owner, prefer, data);
	}

	public static void GlobalShow(UIWidget element, int iconSize, params ITooltipContent[] data)
	{
		UIAbilityTooltipManager.Instance.Show(0, element, iconSize, data);
	}

	public static void GlobalHide()
	{
		UIAbilityTooltipManager.Instance.HideAll();
	}
}

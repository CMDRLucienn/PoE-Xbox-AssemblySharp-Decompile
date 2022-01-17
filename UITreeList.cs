using UnityEngine;

public class UITreeList : MonoBehaviour
{
	public delegate void SelectedItemChangedEvent(UITreeList sender, UITreeListItem selected);

	private UITreeListItem m_SelectedItem;

	public UITreeListItem RootItem;

	public UIAnchor SelectionPointer;

	public UIDraggablePanel ParentPanel;

	public bool AllowArrowkeyNav = true;

	public static GUIDatabaseString NoChildrenDefaultString = new GUIDatabaseString(329);

	private int m_Reposition;

	public UITreeListItem SelectedItem
	{
		get
		{
			return m_SelectedItem;
		}
		set
		{
			if (!(value != m_SelectedItem))
			{
				return;
			}
			if ((bool)m_SelectedItem)
			{
				m_SelectedItem.NotifyDeselect();
			}
			m_SelectedItem = value;
			if (this.OnSelectedItemChanged != null)
			{
				this.OnSelectedItemChanged(this, m_SelectedItem);
			}
			if ((bool)m_SelectedItem)
			{
				m_SelectedItem.NotifySelect();
			}
			if (m_SelectedItem == null)
			{
				SelectionPointer.gameObject.SetActive(value: false);
				SelectionPointer.widgetContainer = null;
				return;
			}
			SelectionPointer.gameObject.SetActive(value: true);
			SelectionPointer.widgetContainer = m_SelectedItem.DisplayLabel;
			UITweener componentInChildren = SelectionPointer.GetComponentInChildren<UITweener>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Reset();
				componentInChildren.Play(forward: true);
			}
			SelectedItem.ExpandUpward();
			ParentPanel.ScrollObjectInView(m_SelectedItem.gameObject, 20f);
		}
	}

	public event SelectedItemChangedEvent OnSelectedItemChanged;

	private void Awake()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		SelectionPointer.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (m_Reposition > 0)
		{
			Reposition();
		}
		if ((bool)SelectedItem)
		{
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				Navigate(-1);
			}
			else if (Input.GetKeyUp(KeyCode.DownArrow))
			{
				Navigate(1);
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				SelectedItem.Expand();
			}
			else if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				SelectedItem.Contract();
			}
		}
	}

	private void OnLanguageChanged(Language lang)
	{
	}

	public void Navigate(int move)
	{
		SelectedItem.Navigate(move);
	}

	public void Reposition()
	{
		RootItem.RepositionRecursive();
		m_Reposition--;
	}

	public void Load(ITreeListContent content)
	{
		ParentPanel.ResetPosition();
		RootItem.Load(content, defaultExpand: true);
		m_Reposition = 2;
		ParentPanel.ResetPosition();
	}

	public void ToggleItemSelection(ITreeListContent item)
	{
		if (m_SelectedItem.Data == item)
		{
			Deselect();
		}
		else
		{
			SelectItem(item);
		}
	}

	public void SelectItem(ITreeListContent item)
	{
		if (item == null || !RootItem.SelectItem(item))
		{
			SelectedItem = null;
		}
	}

	public void Deselect()
	{
		SelectedItem = null;
	}
}

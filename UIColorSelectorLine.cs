using System;
using UnityEngine;

public class UIColorSelectorLine : MonoBehaviour
{
	public delegate void ColorChanged(Color color);

	public UISprite Background;

	public UISprite RootIcon;

	public UISprite SelectionIcon;

	[HideInInspector]
	public GameObject CurrentTarget;

	private UISprite[] m_Icons;

	private bool m_Shown;

	public ColorChanged OnColorChanged;

	private void Start()
	{
		if (!m_Shown)
		{
			base.gameObject.SetActive(value: false);
		}
		Init();
		UIEventListener uIEventListener = UIEventListener.Get(RootIcon.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnIconClick));
	}

	private void Init()
	{
		if (m_Icons == null)
		{
			m_Icons = new UISprite[1];
			m_Icons[0] = RootIcon;
		}
	}

	private void FocusSelectionHighlight(GameObject go)
	{
		if ((bool)SelectionIcon)
		{
			SelectionIcon.GetComponent<UIAnchor>().widgetContainer = go.GetComponent<UISprite>();
		}
	}

	private void OnIconClick(GameObject go)
	{
		OnColorChanged(go.GetComponent<UISprite>().color);
		FocusSelectionHighlight(go);
		GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
	}

	private void LateUpdate()
	{
		Vector3 point = InGameUILayout.NGUICamera.ScreenToWorldPoint(GameInput.MousePosition);
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
		bool flag = vector.x > 0f && vector.y < 0f && vector.x < Background.transform.localScale.x && vector.y > 0f - Background.transform.localScale.y;
		if ((GameInput.GetMouseButtonDown(0, setHandled: true) && !flag) || (flag && GameInput.GetControlDown(MappedControl.CLOSE_WINDOW)))
		{
			Hide();
		}
	}

	public void Show(ColorList list, Color current)
	{
		if (list != null)
		{
			m_Shown = true;
			base.gameObject.SetActive(value: true);
			InitIcons(list.colors.Length);
			for (int i = 0; i < list.colors.Length; i++)
			{
				m_Icons[i].color = list.GetSortedColor(i);
				if (current == m_Icons[i].color)
				{
					FocusSelectionHighlight(m_Icons[i].gameObject);
				}
			}
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void InitIcons(int count)
	{
		Init();
		if (m_Icons.Length < count)
		{
			UISprite[] array = new UISprite[count];
			m_Icons.CopyTo(array, 0);
			for (int i = m_Icons.Length; i < count; i++)
			{
				array[i] = UnityEngine.Object.Instantiate(RootIcon);
				array[i].transform.parent = RootIcon.transform.parent;
				array[i].transform.localScale = RootIcon.transform.localScale;
				array[i].transform.localPosition = Vector3.zero;
				array[i].transform.localRotation = Quaternion.identity;
				UIImageButtonRevised componentInChildren = array[i].GetComponentInChildren<UIImageButtonRevised>();
				if (componentInChildren != null)
				{
					componentInChildren.resetSprites();
				}
				UIEventListener uIEventListener = UIEventListener.Get(array[i].gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnIconClick));
			}
			m_Icons = array;
		}
		for (int j = 0; j < m_Icons.Length; j++)
		{
			if (j < count)
			{
				m_Icons[j].gameObject.SetActive(value: true);
			}
			else
			{
				m_Icons[j].gameObject.SetActive(value: false);
			}
		}
		UIGrid componentInChildren2 = GetComponentInChildren<UIGrid>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.Reposition();
		}
	}
}

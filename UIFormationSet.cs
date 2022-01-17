using System;
using UnityEngine;

public class UIFormationSet : MonoBehaviour
{
	private static UIFormationSet s_Instance;

	public UIGrid Grid;

	public UIWidget Background;

	private UIFormationButton[] m_Buttons;

	private bool m_MouseOver;

	public static UIFormationSet Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		Init();
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		m_MouseOver = false;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!m_MouseOver && (GameInput.GetMouseButtonDown(0, setHandled: false) || GameInput.GetMouseButtonDown(1, setHandled: false)))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnChildHover(GameObject sender, bool over)
	{
		m_MouseOver = over;
	}

	private void OnChildClick(GameObject sender)
	{
		base.gameObject.SetActive(value: false);
	}

	private void Init()
	{
		m_Buttons = Grid.GetComponentsInChildren<UIFormationButton>();
		for (int i = 0; i < m_Buttons.Length; i++)
		{
			int numStandardSets = UIFormationsManager.Instance.NumStandardSets;
			if (i < numStandardSets)
			{
				m_Buttons[i].TooltipText = GUIUtils.GetText(UIFormationsManager.Instance.StandardFormationNameIds[i]);
			}
			else
			{
				m_Buttons[i].TooltipText = GUIUtils.GetText(1712);
			}
			m_Buttons[i].SetFormation(i);
			m_Buttons[i].gameObject.name += i;
		}
		Grid.Reposition();
		UIFormationButton[] buttons = m_Buttons;
		foreach (UIFormationButton obj in buttons)
		{
			UIEventListener uIEventListener = UIEventListener.Get(obj.Collider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			UIEventListener uIEventListener2 = UIEventListener.Get(obj.Collider.gameObject);
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnChildHover));
		}
		Background.transform.localScale += new Vector3((m_Buttons.Length - 1) * 49, 0f, 0f);
	}
}

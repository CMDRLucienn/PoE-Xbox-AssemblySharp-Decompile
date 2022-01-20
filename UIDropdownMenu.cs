using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDropdownMenu : MonoBehaviour
{
	public delegate void DropdownOptionChanged(object option);

	[Tooltip("Label for showing the currently selected item.")]
	public UILabel SelectedText;

	public UIDropdownItem OptionRootObject;

	public UIGrid OptionGrid;

	public UITable OptionTable;

	private List<UIDropdownItem> m_OptionObjects = new List<UIDropdownItem>();

	[HideInInspector]
	public object[] Options;

	public string[] CustomOptions;

	private bool m_Enabled;

	[Tooltip("Collider for the unexpanded box.")]
	public GameObject BaseCollider;

	public Transform ArrowPivot;

	public UIImageButtonRevised Arrow;

	public GameObject DropdownParent;

	public UIWidget DropdownBackground;

	private bool m_DropdownOpen;

	private bool m_DropdownOpenNewThisFrame;

	private object m_Selected;

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			m_Enabled = value;
			RefreshDisabled();
		}
	}

	public object SelectedItem
	{
		get
		{
			return m_Selected;
		}
		set
		{
			m_Selected = value;
			RefreshSelected();
		}
	}

	public event DropdownOptionChanged OnDropdownOptionChanged;

	private void Start()
	{
		if (BaseCollider != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(BaseCollider);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnBoxClicked));
			UIEventListener uIEventListener2 = UIEventListener.Get(BaseCollider);
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnBoxHover));
		}
		if (Arrow != null)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(Arrow.gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnBoxClicked));
		}
		m_OptionObjects.Add(OptionRootObject);
		UIEventListener uIEventListener4 = UIEventListener.Get(OptionRootObject.Collider ? OptionRootObject.Collider : OptionRootObject.gameObject);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnDropdownClicked));
		if (CustomOptions.Length != 0 && Options == null)
		{
			Options = new object[CustomOptions.Length];
			CustomOptions.CopyTo(Options, 0);
		}
		RefreshDropdown();
		RefreshSelected();
	}

	private void OnEnable()
	{
		RefreshDropdown();
	}

	private void OnDisable()
	{
		m_DropdownOpen = false;
		RefreshDropdown();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void LateUpdate()
	{
		if (m_DropdownOpen && !m_DropdownOpenNewThisFrame && Input.GetMouseButtonUp(0))
		{
			m_DropdownOpen = false;
			RefreshDropdown();
		}
		m_DropdownOpenNewThisFrame = false;
	}

	private void OnBoxHover(GameObject go, bool hover)
	{
		if ((bool)Arrow)
		{
			Arrow.ForceHover(hover);
		}
		UIImageButtonRevised uIImageButtonRevised = (SelectedText ? SelectedText.GetComponent<UIImageButtonRevised>() : null);
		if ((bool)uIImageButtonRevised)
		{
			uIImageButtonRevised.ForceHover(hover);
		}
	}

	private void OnBoxClicked(GameObject go)
	{
		m_DropdownOpen = !m_DropdownOpen;
		if (m_DropdownOpen)
		{
			m_DropdownOpenNewThisFrame = true;
		}
		RefreshDropdown();
	}

	private void OnDropdownClicked(GameObject sender)
	{
		UIDropdownItem componentInParent = sender.GetComponentInParent<UIDropdownItem>();
		if ((bool)componentInParent && !componentInParent.Enabled)
		{
			return;
		}
		m_DropdownOpen = false;
		int num = m_OptionObjects.IndexOf(componentInParent);
		if (Options != null && num < Options.Length && num >= 0)
		{
			m_Selected = Options[num];
			if (this.OnDropdownOptionChanged != null)
			{
				this.OnDropdownOptionChanged(m_Selected);
			}
			RefreshSelected();
			RefreshDropdown();
		}
	}

	public void ForceShowObject(object obj)
	{
		m_Selected = obj;
		RefreshSelected();
	}

	public void Enable()
	{
		Enabled = true;
	}

	public void Disable()
	{
		Enabled = false;
	}

	public void SetOptionEnabled(int index, bool state)
	{
		UIDropdownItem option = GetOption(index);
		option.Enabled = state;
		UIIsButton component = option.GetComponent<UIIsButton>();
		if ((bool)component)
		{
			component.enabled = state;
		}
	}

	public void RefreshDropdown()
	{
		if (m_DropdownOpen && Options != null && Options.Length != 0)
		{
			NGUITools.SetActive(DropdownParent, state: true);
			int num = 0;
			object[] options = Options;
			foreach (object obj in options)
			{
				UIDropdownItem option = GetOption(num);
				option.gameObject.SetActive(value: true);
				option.SetContent(obj, SelectedItem == obj);
				num++;
			}
			for (int j = num; j < m_OptionObjects.Count; j++)
			{
				m_OptionObjects[j].gameObject.SetActive(value: false);
			}
			if (Arrow != null && ArrowPivot != null)
			{
				ArrowPivot.localRotation = Quaternion.Euler(0f, 0f, 180f);
			}
			if ((bool)OptionGrid)
			{
				OptionGrid.Reposition();
			}
			if ((bool)OptionTable)
			{
				OptionTable.Reposition();
			}
		}
		else
		{
			NGUITools.SetActive(DropdownParent, state: false);
			if (Arrow != null && ArrowPivot != null)
			{
				ArrowPivot.localRotation = Quaternion.identity;
			}
		}
	}

	public void RefreshSelected()
	{
		if (SelectedItem == null)
		{
			SelectedText.text = "";
		}
		else
		{
			SelectedText.text = SelectedItem.ToString();
		}
		UIDropdownItem component = SelectedText.GetComponent<UIDropdownItem>();
		if ((bool)component)
		{
			component.SetContent(SelectedItem, selected: true);
		}
	}

	private void RefreshDisabled()
	{
		Color color = (Enabled ? Color.white : Color.gray);
		SelectedText.color = color;
	}

	public UIDropdownItem GetOption(int index)
	{
		while (index >= m_OptionObjects.Count)
		{
			GameObject obj = NGUITools.AddChild(OptionRootObject.transform.parent.gameObject, OptionRootObject.gameObject);
			UIDropdownItem component = obj.GetComponent<UIDropdownItem>();
			obj.transform.position = new Vector3(0f, 0f, OptionRootObject.transform.position.z);
			m_OptionObjects.Add(component);
			UIEventListener uIEventListener = UIEventListener.Get(component.Collider ? component.Collider : component.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnDropdownClicked));
		}
		return m_OptionObjects[index];
	}
}

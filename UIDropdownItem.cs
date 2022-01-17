using UnityEngine;

public class UIDropdownItem : MonoBehaviour
{
	public UILabel Label;

	public GameObject Collider;

	public GameObject ShowWhenSelected;

	protected object m_Content;

	private bool m_Enabled = true;

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			m_Enabled = value;
		}
	}

	public void SetContent(object obj, bool selected)
	{
		if (!Label)
		{
			Label = GetComponent<UILabel>();
		}
		if ((bool)ShowWhenSelected)
		{
			ShowWhenSelected.gameObject.SetActive(selected);
		}
		Label.text = obj.ToString();
		m_Content = obj;
		NotifyContentChanged();
	}

	public virtual void NotifyContentChanged()
	{
	}
}

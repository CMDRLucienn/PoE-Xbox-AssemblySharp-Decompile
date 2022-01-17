using System.Collections.Generic;
using UnityEngine;

public class UIOptionsControlCategory : MonoBehaviour
{
	public UILabel ExpandLabel;

	public UILabel Title;

	public UIGrid Grid;

	public UIOptionsControl RootControl;

	private List<UIOptionsControl> m_Controls;

	private MappedCategory m_Category;

	private bool m_Expanded = true;

	public bool Expanded
	{
		get
		{
			return m_Expanded;
		}
		set
		{
			m_Expanded = value;
			Grid.gameObject.SetActive(m_Expanded);
			if (m_Expanded)
			{
				ExpandLabel.text = "-";
			}
			else
			{
				ExpandLabel.text = "+";
			}
		}
	}

	public float Height
	{
		get
		{
			if (m_Expanded)
			{
				return (float)Grid.transform.childCount * Grid.cellHeight - Grid.transform.localPosition.y;
			}
			return Grid.transform.localPosition.y;
		}
	}

	private void OnEnable()
	{
		SetText();
	}

	public void Set(MappedCategory category)
	{
		m_Category = category;
		SetText();
		Init();
	}

	private void SetText()
	{
		Title.text = MappedInput.CategoryNames[(int)m_Category].GetText();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Controls != null)
		{
			return;
		}
		m_Controls = new List<UIOptionsControl>();
		if (MappedInput.CategorizedControls.Length > (int)m_Category)
		{
			MappedControl[] array = MappedInput.CategorizedControls[(int)m_Category];
			for (int i = 0; i < array.Length; i++)
			{
				UIOptionsControl component = NGUITools.AddChild(RootControl.transform.parent.gameObject, RootControl.gameObject).GetComponent<UIOptionsControl>();
				component.Set(array[i]);
				component.gameObject.name = i.ToString("0000");
				m_Controls.Add(component);
			}
		}
		RootControl.gameObject.SetActive(value: false);
		GameUtilities.Destroy(RootControl.gameObject);
		Grid.Reposition();
		Reload();
	}

	public void Reload()
	{
		foreach (UIOptionsControl control in m_Controls)
		{
			control.Reload();
		}
	}
}

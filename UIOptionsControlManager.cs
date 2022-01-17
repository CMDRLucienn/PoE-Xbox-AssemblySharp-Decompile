using System.Collections.Generic;
using UnityEngine;

public class UIOptionsControlManager : MonoBehaviour
{
	public static int AltKeyCount = 2;

	public float Spacing = 30f;

	public UIOptionsControlCategory RootCategory;

	private List<UIOptionsControlCategory> m_Categories;

	public UIDraggablePanel Panel;

	public UIWidget Padder;

	private float m_CategoryYStart;

	private void Start()
	{
		Init();
		Reposition();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Categories == null)
		{
			m_Categories = new List<UIOptionsControlCategory>();
			m_CategoryYStart = RootCategory.transform.localPosition.y;
			Panel.ResetPosition();
			for (int i = 0; i < 5; i++)
			{
				UIOptionsControlCategory component = NGUITools.AddChild(RootCategory.transform.parent.gameObject, RootCategory.gameObject).GetComponent<UIOptionsControlCategory>();
				component.Set((MappedCategory)i);
				component.gameObject.SetActive(value: true);
				m_Categories.Add(component);
			}
			RootCategory.gameObject.SetActive(value: false);
			GameUtilities.Destroy(RootCategory.gameObject);
			Reposition();
			Panel.ResetPosition();
		}
	}

	public void Reload(MappedControl ctrl)
	{
		Init();
		foreach (UIOptionsControlCategory category in m_Categories)
		{
			category.Reload();
		}
	}

	public void Reposition()
	{
		float num = m_CategoryYStart;
		foreach (UIOptionsControlCategory category in m_Categories)
		{
			category.transform.localPosition = new Vector3(category.transform.localPosition.x, num, category.transform.localPosition.z);
			num -= category.Height + Spacing;
		}
		num -= 50f;
		Padder.transform.localPosition = new Vector3(Padder.transform.localPosition.x, num, Padder.transform.localPosition.y);
	}
}

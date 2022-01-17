using System.Collections.Generic;
using UnityEngine;

public class UIStoreInnPage : MonoBehaviour
{
	public UIGrid Grid;

	public UIStoreInnRow RootRow;

	public GameObject NoRooms;

	private List<UIStoreInnRow> m_Rows;

	public UIDraggablePanel DragPanel;

	public GameObject Backgrounds;

	private void OnDisable()
	{
		Backgrounds.SetActive(value: false);
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		Reload();
		Backgrounds.SetActive(value: true);
	}

	private void Init()
	{
		if (m_Rows == null)
		{
			m_Rows = new List<UIStoreInnRow>();
			RootRow.gameObject.SetActive(value: false);
		}
	}

	public void Set(Inn inn)
	{
		Init();
		DragPanel.ResetPosition();
		int i = 0;
		Inn.InnRoom[] rooms = inn.Rooms;
		foreach (Inn.InnRoom room in rooms)
		{
			GetRow(i).Set(room, inn);
			i++;
		}
		NoRooms.SetActive(i == 0);
		for (; i < m_Rows.Count; i++)
		{
			m_Rows[i].Set(null, null);
			m_Rows[i].gameObject.SetActive(value: false);
		}
		Grid.Reposition();
		DragPanel.ResetPosition();
	}

	public void Reload()
	{
		Init();
		foreach (UIStoreInnRow row in m_Rows)
		{
			row.Reload();
		}
		Grid.Reposition();
	}

	private UIStoreInnRow GetRow(int index)
	{
		if (index < m_Rows.Count)
		{
			m_Rows[index].gameObject.SetActive(value: true);
			return m_Rows[index];
		}
		UIStoreInnRow component = NGUITools.AddChild(RootRow.transform.parent.gameObject, RootRow.gameObject).GetComponent<UIStoreInnRow>();
		component.gameObject.SetActive(value: true);
		m_Rows.Add(component);
		return component;
	}
}

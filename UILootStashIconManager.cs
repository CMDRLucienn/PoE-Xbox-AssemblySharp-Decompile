using System.Collections.Generic;
using UnityEngine;

public class UILootStashIconManager : MonoBehaviour
{
	public UILootStashIcon RootIcon;

	private Queue<UILootStashIcon> m_Queue = new Queue<UILootStashIcon>();

	private List<UILootStashIcon> m_Active = new List<UILootStashIcon>();

	private void Start()
	{
		RootIcon.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		m_Queue.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Clear()
	{
		for (int num = m_Active.Count - 1; num >= 0; num--)
		{
			m_Active[num].End();
		}
		m_Active.Clear();
	}

	public void Looted(Item item)
	{
		UILootStashIcon icon = GetIcon();
		m_Active.Add(icon);
		icon.Begin(item);
	}

	public void RecycleIcon(UILootStashIcon icon)
	{
		m_Active.Remove(icon);
		m_Queue.Enqueue(icon);
	}

	private UILootStashIcon GetIcon()
	{
		if (m_Queue.Count > 0)
		{
			return m_Queue.Dequeue();
		}
		return NGUITools.AddChild(base.gameObject, RootIcon.gameObject).GetComponent<UILootStashIcon>();
	}
}

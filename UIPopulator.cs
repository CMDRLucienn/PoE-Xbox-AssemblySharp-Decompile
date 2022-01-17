using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIPopulator : MonoBehaviour
{
	public GameObject RootPopulatedObject;

	[Tooltip("The clone's parent by default is the Root's parent.")]
	public GameObject ParentObject;

	protected List<GameObject> m_Clones = new List<GameObject>();

	private UITable m_table;

	private UIGrid m_grid;

	public bool Empty
	{
		get
		{
			if (m_Clones.Count != 0)
			{
				return !m_Clones[0].activeSelf;
			}
			return true;
		}
	}

	protected virtual bool ResetTransform => true;

	protected virtual void Awake()
	{
		Init();
		NGUITools.SetActive(RootPopulatedObject, state: true);
		NGUITools.SetActive(RootPopulatedObject, state: false);
	}

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected virtual void Init()
	{
		if (ParentObject == null)
		{
			ParentObject = RootPopulatedObject.transform.parent.gameObject;
		}
		if (m_table == null)
		{
			m_table = NGUITools.FindInParents<UITable>(ParentObject);
		}
		if (m_grid == null)
		{
			m_grid = NGUITools.FindInParents<UIGrid>(ParentObject);
		}
	}

	protected virtual void InitClone(GameObject clone)
	{
	}

	protected void Populate(int items)
	{
		Init();
		int i;
		for (i = 0; i < items; i++)
		{
			ActivateClone(i);
		}
		for (; i < m_Clones.Count; i++)
		{
			if (m_Clones[i] != null)
			{
				m_Clones[i].SetActive(value: false);
			}
		}
		if ((bool)m_grid)
		{
			m_grid.repositionNow = true;
		}
		if ((bool)m_table)
		{
			m_table.repositionNow = true;
		}
	}

	protected virtual GameObject ActivateClone(int index)
	{
		GameObject gameObject = null;
		gameObject = ((index >= m_Clones.Count || !(m_Clones[index] != null)) ? AddClone(index, RootPopulatedObject, ParentObject) : m_Clones[index]);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: true);
		}
		return gameObject;
	}

	protected virtual void RemoveClone(int index)
	{
		if (index < m_Clones.Count && m_Clones[index] != null)
		{
			GameUtilities.Destroy(m_Clones[index]);
			m_Clones.RemoveAt(index);
		}
	}

	protected GameObject AddClone(int index, GameObject prefab, GameObject parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		gameObject.layer = parent.layer;
		gameObject.transform.parent = parent.transform;
		if (ResetTransform)
		{
			gameObject.transform.localPosition = prefab.transform.localPosition;
			gameObject.transform.localScale = prefab.transform.localScale;
		}
		gameObject.name += index.ToString("D3");
		while (m_Clones.Count < index)
		{
			m_Clones.Add(null);
		}
		InitClone(gameObject);
		m_Clones.Insert(index, gameObject);
		return gameObject;
	}

	public GameObject GetCloneWithComponent<T>(IEquatable<T> component) where T : Component
	{
		foreach (GameObject clone in m_Clones)
		{
			T component2 = clone.GetComponent<T>();
			if ((UnityEngine.Object)component2 != (UnityEngine.Object)null && component.Equals(component2))
			{
				return clone;
			}
		}
		return null;
	}
}

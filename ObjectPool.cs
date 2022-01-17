using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : iMemoryPool where T : class
{
	private List<T> m_pool;

	private GameObject m_prefab;

	private int m_allocatedObjects;

	private int m_highWaterMark;

	public int NumAllocatedObjects => m_allocatedObjects;

	public ObjectPool(int capacity)
	{
		FillPool(capacity);
	}

	public ObjectPool(int capacity, GameObject prefab)
	{
		FillPool(capacity, prefab);
	}

	private void FillPool(int capacity)
	{
		m_pool = new List<T>(capacity);
		if (typeof(T) == typeof(GameObject))
		{
			Debug.LogError("Programmer Error: ObjectPool constructor used isn't valid for GameObjects. Use the constructor and provide a prefab!");
			return;
		}
		for (int i = 0; i < capacity; i++)
		{
			m_pool.Add((T)Activator.CreateInstance(typeof(T)));
		}
	}

	private void FillPool(int capacity, GameObject prefab)
	{
		m_pool = new List<T>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			m_pool.Add(UnityEngine.Object.Instantiate(prefab) as T);
		}
	}

	public void Free(object obj)
	{
		Free(obj as T);
	}

	public object GenericAllocate()
	{
		return Allocate();
	}

	public T Allocate()
	{
		m_allocatedObjects++;
		if (m_allocatedObjects > m_highWaterMark)
		{
			m_highWaterMark = m_allocatedObjects;
		}
		if (m_pool.Count == 0)
		{
			Debug.LogWarning("Object pool for type " + typeof(T).ToString() + " wasn't given a large enough capacity, doubling allocations!");
			if (m_prefab != null)
			{
				FillPool(m_allocatedObjects, m_prefab);
			}
			else
			{
				FillPool(m_allocatedObjects);
			}
		}
		T val = m_pool[m_pool.Count - 1];
		m_pool.RemoveAt(m_pool.Count - 1);
		if (val is GameObject)
		{
			(val as GameObject).SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
			(val as GameObject).SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
			(val as GameObject).SendMessage("Start", SendMessageOptions.DontRequireReceiver);
		}
		return val;
	}

	public void Free(T obj)
	{
		if (obj != null)
		{
			m_allocatedObjects--;
			if (obj is GameObject)
			{
				(obj as GameObject).SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);
				(obj as GameObject).SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
			}
			m_pool.Add(obj);
		}
	}
}

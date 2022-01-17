using System;
using System.Collections;
using UnityEngine;

public class MultiTypeObjectPool
{
	private Hashtable m_pools = new Hashtable();

	private int m_poolSize = 32;

	public MultiTypeObjectPool(Type[] types, int poolSize)
	{
		m_poolSize = poolSize;
		FillPools(types);
	}

	public MultiTypeObjectPool(int initialTypeCount, int poolSize)
	{
		m_pools = new Hashtable(initialTypeCount);
		m_poolSize = poolSize;
	}

	public void FillPools(Type[] types)
	{
		foreach (Type t in types)
		{
			CreatePool(t);
		}
	}

	private void CreatePool(Type t)
	{
		object value = Activator.CreateInstance(typeof(ObjectPool<>).MakeGenericType(t), m_poolSize);
		m_pools.Add(t, value);
	}

	public T Allocate<T>() where T : class
	{
		if (!m_pools.ContainsKey(typeof(T)))
		{
			CreatePool(typeof(T));
		}
		if (!(m_pools[typeof(T)] is ObjectPool<T> objectPool))
		{
			return null;
		}
		return objectPool.Allocate();
	}

	public void Free(object obj)
	{
		Type type = obj.GetType();
		if (!m_pools.ContainsKey(type))
		{
			Debug.LogWarning("Programming Error: Someone is trying to free an object of type " + type.ToString() + " but no pools have been created for that type!");
		}
		else if (m_pools[type] is iMemoryPool iMemoryPool2)
		{
			iMemoryPool2.Free(obj);
		}
	}
}

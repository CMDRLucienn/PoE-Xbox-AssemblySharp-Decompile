using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("Miscellaneous/Instance ID")]
public class InstanceID : MonoBehaviour
{
	private static Dictionary<Guid, GameObject> s_activeObjects = new Dictionary<Guid, GameObject>();

	public Guid m_Guid = Guid.Empty;

	public string UniqueID = string.Empty;

	[Persistent]
	public Guid Guid
	{
		get
		{
			if (m_Guid == Guid.Empty)
			{
				if (!string.IsNullOrEmpty(UniqueID))
				{
					m_Guid = new Guid(UniqueID);
				}
				else
				{
					m_Guid = Guid.NewGuid();
					UniqueID = m_Guid.ToString();
				}
			}
			return m_Guid;
		}
		set
		{
			if (m_Guid == value)
			{
				return;
			}
			if (!(this is CompanionInstanceID) && m_Guid != Guid.Empty && s_activeObjects.ContainsKey(m_Guid))
			{
				if (s_activeObjects[m_Guid] != null && s_activeObjects[m_Guid] != base.gameObject)
				{
					Debug.LogError(s_activeObjects[m_Guid].name + " was replaced by " + base.gameObject.name + ". They have the same guid!");
				}
				s_activeObjects.Remove(m_Guid);
			}
			m_Guid = value;
			UniqueID = value.ToString();
			if (s_activeObjects.ContainsKey(m_Guid))
			{
				s_activeObjects[Guid] = base.gameObject;
			}
			else
			{
				s_activeObjects.Add(m_Guid, base.gameObject);
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (!s_activeObjects.ContainsKey(Guid))
		{
			s_activeObjects.Add(Guid, base.gameObject);
		}
		else if (s_activeObjects[Guid] == null)
		{
			s_activeObjects[Guid] = base.gameObject;
		}
		else if (s_activeObjects[Guid] != base.gameObject)
		{
			Debug.LogError(base.gameObject.name + " was attempting to replace by " + s_activeObjects[m_Guid].name + ". They have the same guid!");
		}
	}

	protected virtual void OnDisable()
	{
		s_activeObjects.Remove(Guid);
	}

	protected virtual void Awake()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			Guid = new Guid(UniqueID);
		}
	}

	protected virtual void Start()
	{
		Load();
	}

	public virtual void Load()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			Guid = new Guid(UniqueID);
		}
	}

	public static void ResetActiveList()
	{
		s_activeObjects.Clear();
	}

	public static GameObject GetObjectByID(Guid id)
	{
		if (s_activeObjects.ContainsKey(id))
		{
			return s_activeObjects[id];
		}
		return null;
	}

	public static GameObject GetObjectByID(InstanceID p)
	{
		if (s_activeObjects.ContainsKey(p.Guid))
		{
			return s_activeObjects[p.Guid];
		}
		return null;
	}

	public static GameObject GetObjectByID(GameObject obj)
	{
		InstanceID component = obj.GetComponent<InstanceID>();
		if (component != null && s_activeObjects.ContainsKey(component.Guid))
		{
			return s_activeObjects[component.Guid];
		}
		return null;
	}

	public static bool ObjectIsActive(Guid id)
	{
		if (s_activeObjects.ContainsKey(id))
		{
			return s_activeObjects[id] != null;
		}
		return false;
	}

	public static bool ObjectIsActive(GameObject obj)
	{
		InstanceID component = obj.GetComponent<InstanceID>();
		if ((bool)component)
		{
			if (s_activeObjects.ContainsKey(component.Guid))
			{
				return s_activeObjects[component.Guid] != null;
			}
			return false;
		}
		return false;
	}

	public static void AddSpecialObjectID(GameObject obj, Guid id)
	{
		if (!s_activeObjects.ContainsKey(id))
		{
			s_activeObjects.Add(id, obj);
		}
		else
		{
			s_activeObjects[id] = obj;
		}
	}

	public static void RemoveSpecialObjectID(Guid id)
	{
		if (s_activeObjects.ContainsKey(id))
		{
			s_activeObjects.Remove(id);
		}
	}
}

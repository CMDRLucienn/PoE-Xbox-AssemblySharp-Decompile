using System;
using System.Collections.Generic;
using System.Reflection;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class ObjectPersistencePacket
{
	private string m_objectName = string.Empty;

	private string m_levelName = string.Empty;

	private string m_prefabResource = string.Empty;

	private Guid m_objectID = Guid.Empty;

	private Vector3 m_location = Vector3.zero;

	private Quaternion m_rotation = Quaternion.identity;

	private bool m_mobile;

	private bool m_global;

	private Dictionary<Type, ComponentPersistencePacket> m_components = new Dictionary<Type, ComponentPersistencePacket>(32);

	public string ObjectName
	{
		get
		{
			return m_objectName;
		}
		set
		{
			m_objectName = value;
		}
	}

	public string LevelName
	{
		get
		{
			return m_levelName;
		}
		set
		{
			m_levelName = value;
		}
	}

	public string PrefabResource
	{
		get
		{
			return m_prefabResource;
		}
		set
		{
			m_prefabResource = value;
		}
	}

	public bool Mobile
	{
		get
		{
			return m_mobile;
		}
		set
		{
			m_mobile = value;
		}
	}

	public bool Global
	{
		get
		{
			return m_global;
		}
		set
		{
			m_global = value;
		}
	}

	public Guid GUID
	{
		get
		{
			return m_objectID;
		}
		set
		{
			m_objectID = value;
		}
	}

	public string ObjectID
	{
		get
		{
			return m_objectID.ToString();
		}
		set
		{
			m_objectID = new Guid(value);
		}
	}

	public Vector3 Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public Vector3 Rotation
	{
		get
		{
			return m_rotation.eulerAngles;
		}
		set
		{
			m_rotation.eulerAngles = value;
		}
	}

	public bool Packed { get; set; }

	public bool LoadManually { get; set; }

	public string Parent { get; set; }

	[ExcludeFromSerialization]
	public Dictionary<Type, ComponentPersistencePacket> Components
	{
		get
		{
			return m_components;
		}
		set
		{
			m_components = value;
		}
	}

	public ComponentPersistencePacket[] ComponentPackets
	{
		get
		{
			List<ComponentPersistencePacket> list = new List<ComponentPersistencePacket>(m_components.Values.Count);
			list.AddRange(m_components.Values);
			return list.ToArray();
		}
		set
		{
			m_components.Clear();
			foreach (ComponentPersistencePacket componentPersistencePacket in value)
			{
				if (componentPersistencePacket != null)
				{
					Type type = Type.GetType(componentPersistencePacket.TypeString);
					if (type != null)
					{
						m_components.Add(type, componentPersistencePacket);
					}
				}
			}
		}
	}

	public GameObject CreateObject(bool mobile)
	{
		GameObject gameObject = null;
		if (!string.IsNullOrEmpty(PrefabResource))
		{
			UnityEngine.Object @object = GameResources.LoadPrefab(PrefabResource, instantiate: true);
			gameObject = ((!(@object is MonoBehaviour)) ? (@object as GameObject) : (@object as MonoBehaviour).gameObject);
		}
		if (gameObject != null)
		{
			RegisterPendingMobile(gameObject, mobile);
		}
		if (gameObject == null)
		{
			gameObject = InstanceID.GetObjectByID(GUID);
		}
		if (gameObject == null && mobile)
		{
			UnityEngine.Object object2 = GameResources.LoadPrefab(ObjectName, instantiate: true);
			gameObject = ((!(object2 is MonoBehaviour)) ? (object2 as GameObject) : (object2 as MonoBehaviour).gameObject);
			if (gameObject != null)
			{
				UIDebug.Instance.LogOnScreenWarning(ObjectName + " doesn't have an associated prefab, but a prefab was found matching the object name.", UIDebug.Department.Programming, 10f);
				RegisterPendingMobile(gameObject, mobile);
			}
		}
		if (gameObject == null && mobile)
		{
			gameObject = new GameObject(ObjectName);
			foreach (ComponentPersistencePacket value in Components.Values)
			{
				Persistence persistence = (gameObject.AddComponent(value.ComponentType) as MonoBehaviour) as Persistence;
				if (mobile && (bool)persistence)
				{
					persistence.GUID = m_objectID;
					persistence.Mobile = true;
					persistence.CreateFromPrefabFailed = true;
				}
			}
			if (gameObject.GetComponent<StoredCharacterInfo>() == null)
			{
				UIDebug.Instance.LogOnScreenWarning(ObjectName + " doesn't have an associated prefab. Trying to recreate from known data, this object may not work correctly.", UIDebug.Department.Programming, 10f);
			}
		}
		return gameObject;
	}

	private void RegisterPendingMobile(GameObject go, bool mobile)
	{
		Persistence component = go.GetComponent<Persistence>();
		component.GUID = m_objectID;
		if (mobile && !PersistenceManager.MobileObjects.ContainsKey(component.GUID))
		{
			PersistenceManager.PendingMobileObjects.Add(this);
		}
	}

	public void SaveObject(object obj)
	{
		LevelName = GameState.ApplicationLoadedLevelName;
		ObjectID = Guid.NewGuid().ToString();
		ObjectName = obj.ToString();
		List<FieldInfo> list = new List<FieldInfo>();
		PersistenceManager.GetAllFields(obj.GetType(), list);
		foreach (FieldInfo item in list)
		{
			object[] customAttributes = item.GetCustomAttributes(typeof(Persistent), inherit: true);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				Persistent obj2 = customAttributes[0] as Persistent;
				Type type = obj.GetType();
				object val = obj2.PackObject(item.GetValue(obj));
				AddVariable(type, item.Name, val);
			}
		}
		List<PropertyInfo> list2 = new List<PropertyInfo>();
		PersistenceManager.GetAllProperties(obj.GetType(), list2);
		foreach (PropertyInfo item2 in list2)
		{
			object[] customAttributes2 = item2.GetCustomAttributes(typeof(Persistent), inherit: true);
			if (customAttributes2 != null && customAttributes2.Length != 0)
			{
				Persistent obj3 = customAttributes2[0] as Persistent;
				Type type2 = obj.GetType();
				object val2 = obj3.PackObject(item2.GetValue(obj, null));
				AddVariable(type2, item2.Name, val2);
			}
		}
	}

	public void SaveData(GameObject savedObj)
	{
		LevelName = GameState.ApplicationLoadedLevelName;
		ObjectID = Guid.NewGuid().ToString();
		ObjectName = savedObj.name;
		Location = savedObj.transform.position;
		Rotation = savedObj.transform.rotation.eulerAngles;
		object[] components = savedObj.GetComponents<MonoBehaviour>();
		object[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				continue;
			}
			List<FieldInfo> list = new List<FieldInfo>();
			PersistenceManager.GetAllFields(array[i].GetType(), list);
			foreach (FieldInfo item in list)
			{
				object[] customAttributes = item.GetCustomAttributes(typeof(Persistent), inherit: true);
				if (customAttributes != null && customAttributes.Length != 0)
				{
					Persistent obj = customAttributes[0] as Persistent;
					Type type = array[i].GetType();
					object val = obj.PackObject(item.GetValue(array[i]));
					AddVariable(type, item.Name, val);
				}
			}
			List<PropertyInfo> list2 = new List<PropertyInfo>();
			PersistenceManager.GetAllProperties(array[i].GetType(), list2);
			foreach (PropertyInfo item2 in list2)
			{
				object[] customAttributes2 = item2.GetCustomAttributes(typeof(Persistent), inherit: true);
				if (customAttributes2 != null && customAttributes2.Length != 0)
				{
					Persistent obj2 = customAttributes2[0] as Persistent;
					Type type2 = array[i].GetType();
					object val2 = obj2.PackObject(item2.GetValue(array[i], null));
					AddVariable(type2, item2.Name, val2);
				}
			}
		}
	}

	public void PartiallyRestoreObject(ref GameObject go)
	{
		go.transform.position = Location;
		Quaternion rotation = default(Quaternion);
		rotation.eulerAngles = Rotation;
		go.transform.rotation = rotation;
	}

	public void RestoreBasicObject(ref object obj)
	{
		foreach (ComponentPersistencePacket value in Components.Values)
		{
			GameObject gameObject = obj as GameObject;
			if (gameObject == null && obj is MonoBehaviour)
			{
				gameObject = (obj as MonoBehaviour).gameObject;
			}
			if (gameObject.GetComponent(value.ComponentType) == null)
			{
				gameObject.AddComponent(value.ComponentType);
			}
			foreach (string key in value.Variables.Keys)
			{
				FieldInfo fieldInfo = PersistenceManager.FindField(value.ComponentType, key);
				if (fieldInfo != null)
				{
					Type fieldType = fieldInfo.FieldType;
					object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(Persistent), inherit: true);
					Persistent persistent = null;
					if (customAttributes.Length != 0)
					{
						persistent = customAttributes[0] as Persistent;
					}
					object obj2 = value.Variables[key];
					if (persistent != null)
					{
						persistent.FieldType = fieldType;
						obj2 = persistent.UnpackObject(obj2);
					}
					try
					{
						if (obj2 is MonoBehaviour && fieldType == typeof(GameObject))
						{
							obj2 = (obj2 as MonoBehaviour).gameObject;
						}
						else if (obj2 is GameObject && fieldType.IsSubclassOf(typeof(MonoBehaviour)))
						{
							obj2 = (obj2 as GameObject).GetComponent(fieldType);
						}
						else if (obj2 is MonoBehaviour && obj2.GetType() != fieldType && fieldType.IsSubclassOf(typeof(MonoBehaviour)))
						{
							obj2 = (obj2 as MonoBehaviour).GetComponent(fieldType);
						}
						fieldInfo.SetValue(obj, obj2);
					}
					catch (Exception ex)
					{
						Debug.LogError("Loading " + key + " into " + obj.ToString() + " failed. Message: " + ex.Message);
					}
					continue;
				}
				PropertyInfo propertyInfo = PersistenceManager.FindProperty(value.ComponentType, key);
				if (!(propertyInfo != null))
				{
					continue;
				}
				object[] customAttributes2 = propertyInfo.GetCustomAttributes(typeof(Persistent), inherit: true);
				Persistent persistent2 = null;
				if (customAttributes2.Length != 0)
				{
					persistent2 = customAttributes2[0] as Persistent;
				}
				object obj3 = value.Variables[key];
				Type propertyType = propertyInfo.PropertyType;
				if (persistent2 != null)
				{
					persistent2.FieldType = propertyType;
					obj3 = persistent2.UnpackObject(obj3);
				}
				try
				{
					if (obj3 is MonoBehaviour && propertyType == typeof(GameObject))
					{
						obj3 = (obj3 as MonoBehaviour).gameObject;
					}
					else if (obj3 is GameObject && propertyType.IsSubclassOf(typeof(MonoBehaviour)))
					{
						obj3 = (obj3 as GameObject).GetComponent(propertyType);
					}
					propertyInfo.SetValue(obj, obj3, null);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Loading " + key + " into " + obj.ToString() + " failed. Message: " + ex2.Message);
				}
			}
		}
	}

	public void RestoreGUID(ref GameObject go)
	{
		if (Components.ContainsKey(typeof(InstanceID)))
		{
			RestorePacket(ref go, Components[typeof(InstanceID)]);
		}
	}

	public void RestoreObject(ref GameObject go)
	{
		go.transform.position = Location;
		Quaternion rotation = default(Quaternion);
		rotation.eulerAngles = Rotation;
		go.transform.rotation = rotation;
		AIPackageController component = go.GetComponent<AIPackageController>();
		if (component != null)
		{
			component.RecordRetreatPosition(Location);
		}
		if (Components.ContainsKey(typeof(InstanceID)))
		{
			RestorePacket(ref go, Components[typeof(InstanceID)]);
		}
		foreach (ComponentPersistencePacket value in Components.Values)
		{
			RestorePacket(ref go, value);
		}
	}

	private void RestorePacket(ref GameObject go, ComponentPersistencePacket packet)
	{
		if (go == null || packet == null)
		{
			return;
		}
		Component component = go.GetComponent(packet.ComponentType);
		if (component == null)
		{
			component = go.AddComponent(packet.ComponentType);
		}
		foreach (string key in packet.Variables.Keys)
		{
			FieldInfo fieldInfo = PersistenceManager.FindField(packet.ComponentType, key);
			if (fieldInfo != null)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(Persistent), inherit: true);
				Persistent persistent = null;
				if (customAttributes.Length != 0)
				{
					persistent = customAttributes[0] as Persistent;
				}
				object obj = packet.Variables[key];
				if (persistent != null)
				{
					persistent.FieldType = fieldInfo.FieldType;
					obj = persistent.UnpackObject(obj);
				}
				try
				{
					if (obj is GameObject && fieldInfo.FieldType != typeof(GameObject))
					{
						obj = (obj as GameObject).GetComponent(fieldInfo.FieldType);
					}
					fieldInfo.SetValue(component, obj);
				}
				catch (Exception ex)
				{
					Debug.LogError("Loading " + key + " into " + component.name + " failed. Message: " + ex.Message);
				}
				continue;
			}
			PropertyInfo propertyInfo = PersistenceManager.FindProperty(packet.ComponentType, key);
			if (propertyInfo != null)
			{
				object[] customAttributes2 = propertyInfo.GetCustomAttributes(typeof(Persistent), inherit: true);
				Persistent persistent2 = null;
				if (customAttributes2.Length != 0)
				{
					persistent2 = customAttributes2[0] as Persistent;
				}
				object obj2 = packet.Variables[key];
				if (persistent2 != null)
				{
					persistent2.FieldType = propertyInfo.PropertyType;
					obj2 = persistent2.UnpackObject(obj2);
				}
				if (obj2 is GameObject && propertyInfo.PropertyType != typeof(GameObject))
				{
					obj2 = (obj2 as GameObject).GetComponent(propertyInfo.PropertyType);
				}
				else if (obj2 != null && obj2.GetType() != propertyInfo.PropertyType && obj2.GetType().IsAssignableFrom(propertyInfo.PropertyType))
				{
					obj2 = obj2 as Equippable[];
				}
				try
				{
					propertyInfo.SetValue(component, obj2, null);
				}
				catch (Exception ex2)
				{
					Debug.LogError(string.Concat("Unable to set value for ", ObjectName, ".", packet.ComponentType, ".", propertyInfo.Name, " value was ", obj2.ToString(), "\n", ex2.ToString()));
				}
			}
		}
	}

	private object ConvertFromString(FieldInfo field, string valAsString)
	{
		if (field.FieldType.BaseType == typeof(Enum))
		{
			return Enum.Parse(field.FieldType, valAsString);
		}
		if (field.FieldType == typeof(int))
		{
			return IntUtils.ParseInvariant(valAsString);
		}
		if (field.FieldType == typeof(float))
		{
			return FloatUtils.ParseInvariant(valAsString);
		}
		if (field.FieldType == typeof(bool))
		{
			return BoolUtils.ParseInvariant(valAsString);
		}
		if (field.FieldType == typeof(uint))
		{
			return UIntUtils.ParseInvariant(valAsString);
		}
		return null;
	}

	public void AddVariable(Type component, string name, object val)
	{
		if (Components.ContainsKey(component) && Components[component] == null)
		{
			Components.Remove(component);
		}
		if (Components.ContainsKey(component))
		{
			ComponentPersistencePacket componentPersistencePacket = Components[component];
			if (componentPersistencePacket == null)
			{
				Debug.LogError("Null packet in the save system!!! Tried to get component " + component.ToString());
			}
			componentPersistencePacket.AddVariable(name, val);
		}
		else
		{
			ComponentPersistencePacket componentPersistencePacket2 = new ComponentPersistencePacket();
			componentPersistencePacket2.ComponentType = component;
			componentPersistencePacket2.AddVariable(name, val);
			Components.Add(component, componentPersistencePacket2);
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is ObjectPersistencePacket)
		{
			return ObjectID == (obj as ObjectPersistencePacket).ObjectID;
		}
		if (obj is string)
		{
			return ObjectID == obj as string;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return ObjectID.GetHashCode();
	}

	public override string ToString()
	{
		return ObjectName;
	}
}

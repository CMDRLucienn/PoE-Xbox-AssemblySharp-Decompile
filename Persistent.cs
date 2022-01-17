using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Polenter.Serialization.Core;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Persistent : Attribute
{
	public enum ConversionType
	{
		Normal,
		Binary,
		ObjectPacket,
		GUIDLink
	}

	public ConversionType ConversionMethod { get; set; }

	public Type FieldType { get; set; }

	public Persistent()
	{
		ConversionMethod = ConversionType.Normal;
	}

	public Persistent(ConversionType convertType)
	{
		ConversionMethod = convertType;
	}

	public object PackObject(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (ConversionMethod == ConversionType.Normal)
		{
			return obj;
		}
		if (ConversionMethod == ConversionType.GUIDLink)
		{
			return PackGUIDLink(obj);
		}
		if (ConversionMethod == ConversionType.ObjectPacket)
		{
			return PackUnityObject(obj);
		}
		return BinarySerialize(obj);
	}

	private object PackGUIDLink(object obj)
	{
		if (obj is IEnumerable)
		{
			return PackGUIDLinkCollection(obj);
		}
		if (obj == null || (obj is UnityEngine.Object && obj as UnityEngine.Object == null))
		{
			return null;
		}
		GameObject gameObject = obj as GameObject;
		if (gameObject == null && obj is MonoBehaviour)
		{
			gameObject = (obj as MonoBehaviour).gameObject;
		}
		if (gameObject == null)
		{
			if (obj != null)
			{
				Debug.LogError(obj.ToString() + " can't be saved as a GUIDLink!");
			}
			return null;
		}
		InstanceID component = gameObject.GetComponent<InstanceID>();
		if (component == null)
		{
			Debug.LogError(gameObject.name + " doesn't have an InstanceID component! It can't be saved as a GUIDLink.", gameObject);
			return null;
		}
		return component.Guid;
	}

	private object PackGUIDLinkCollection(object obj)
	{
		if (!(obj is IEnumerable))
		{
			PackGUIDLink(obj);
		}
		IEnumerable obj2 = obj as IEnumerable;
		List<Guid> list = new List<Guid>();
		foreach (object item in obj2)
		{
			object obj3 = PackGUIDLink(item);
			if (obj3 != null)
			{
				list.Add((Guid)obj3);
			}
			else
			{
				list.Add(Guid.Empty);
			}
		}
		return list;
	}

	private object PackCollection(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is IDictionary)
		{
			Debug.LogError("Dictionary's of Unity Game Objects are not supported by the save system. Programmer: Please send keys/values as separate arrays.");
			return obj;
		}
		if (obj is IEnumerable)
		{
			IEnumerable obj2 = obj as IEnumerable;
			List<ObjectPersistencePacket> list = new List<ObjectPersistencePacket>();
			{
				foreach (object item in obj2)
				{
					UnityEngine.Object @object = item as UnityEngine.Object;
					if (@object == null)
					{
						list.Add(PackCustomObject(item));
					}
					else
					{
						list.Add(PackUnityObject(@object) as ObjectPersistencePacket);
					}
				}
				return list;
			}
		}
		Debug.LogWarning("Packet doesn't know how to deal with collection of type " + obj.GetType().ToString());
		return obj;
	}

	private object PackUnityObject(object obj)
	{
		if (obj is IEnumerable)
		{
			return PackCollection(obj);
		}
		GameObject gameObject = obj as GameObject;
		if (gameObject == null && obj is MonoBehaviour)
		{
			gameObject = (obj as MonoBehaviour).gameObject;
		}
		if (gameObject == null)
		{
			return PackCustomObject(obj);
		}
		ObjectPersistencePacket objectPersistencePacket = new ObjectPersistencePacket();
		objectPersistencePacket.SaveData(gameObject);
		return objectPersistencePacket;
	}

	private ObjectPersistencePacket PackCustomObject(object obj)
	{
		ObjectPersistencePacket objectPersistencePacket = new ObjectPersistencePacket();
		objectPersistencePacket.SaveObject(obj);
		return objectPersistencePacket;
	}

	private byte[] BinarySerialize(object obj)
	{
		if (obj is UnityEngine.Object)
		{
			Debug.LogError(obj.ToString() + " is trying to be binary serialized. This will very likely fail.", obj as UnityEngine.Object);
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.GetBuffer();
	}

	public object UnpackObject(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (ConversionMethod == ConversionType.Normal)
		{
			if (obj is string && FieldType.IsSubclassOf(typeof(MonoBehaviour)))
			{
				return UnpackPrefab(obj as string);
			}
			return obj;
		}
		if (ConversionMethod == ConversionType.ObjectPacket)
		{
			return UnpackUnityObject(obj, FieldType);
		}
		if (ConversionMethod == ConversionType.GUIDLink)
		{
			return UnpackGUIDLink(obj);
		}
		byte[] array = (byte[])obj;
		if (array == null)
		{
			return obj;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream serializationStream = new MemoryStream(array);
		return binaryFormatter.Deserialize(serializationStream);
	}

	private object UnpackPrefab(string name)
	{
		return GameResources.LoadPrefab(name, instantiate: false);
	}

	private object UnpackGUIDLink(object obj)
	{
		if (obj is IEnumerable)
		{
			return UnpackGUIDCollection(obj);
		}
		Guid guid = (Guid)obj;
		if (obj == null)
		{
			return obj;
		}
		if (guid == Guid.Empty)
		{
			return null;
		}
		GameObject gameObject = InstanceID.GetObjectByID(guid);
		if (gameObject == null)
		{
			ObjectPersistencePacket packet = PersistenceManager.GetPacket(guid);
			if (packet != null && (!packet.Packed || StoredCharacterInfo.RestoringPackedCharacter))
			{
				gameObject = PersistenceManager.RestorePackedObject(guid);
			}
		}
		return gameObject;
	}

	public object UnpackGUIDCollection(object obj)
	{
		if (!(obj is List<Guid>))
		{
			return UnpackGUIDLink(obj);
		}
		List<Guid> list = obj as List<Guid>;
		object obj2 = Tools.CreateInstance(FieldType);
		MethodInfo method = obj2.GetType().GetMethod("Add");
		if (method != null)
		{
			Type type = obj2.GetType().GetGenericArguments()[0];
			{
				foreach (Guid item in list)
				{
					object obj3 = null;
					try
					{
						obj3 = UnpackGUIDLink(item);
						GameObject gameObject = obj3 as GameObject;
						if (gameObject != null && type.IsSubclassOf(typeof(MonoBehaviour)))
						{
							obj3 = gameObject.GetComponent(type);
						}
						method.Invoke(obj2, new object[1] { obj3 });
					}
					catch (Exception exception)
					{
						Debug.LogError(("Error adding " + obj3 == null) ? string.Empty : obj3.ToString());
						Debug.LogException(exception);
					}
				}
				return obj2;
			}
		}
		Debug.LogError("Unpack GUID Collection currently only supports lists. Failed unpacking type " + obj2.GetType().ToString());
		return obj2;
	}

	private object UnpackCustomObject(object obj, Type objType)
	{
		if (!(obj is ObjectPersistencePacket objectPersistencePacket))
		{
			return obj;
		}
		object obj2 = Tools.CreateInstance(objType);
		objectPersistencePacket.RestoreBasicObject(ref obj2);
		return obj2;
	}

	private object UnpackUnityObject(object obj, Type objType)
	{
		if (obj is IEnumerable)
		{
			return UnpackCollection(obj);
		}
		ObjectPersistencePacket objectPersistencePacket = obj as ObjectPersistencePacket;
		if (obj == null)
		{
			Debug.LogError("Programming Error: Trying to unpack a Unity object that wasn't in a persistence packet!");
			return null;
		}
		if (!objType.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			return UnpackCustomObject(obj, FieldType);
		}
		GameObject go = null;
		if (objectPersistencePacket.PrefabResource != string.Empty)
		{
			go = GameResources.LoadPrefab(objectPersistencePacket.PrefabResource, instantiate: false) as GameObject;
		}
		if (go == null)
		{
			go = new GameObject(objectPersistencePacket.ObjectName);
			Debug.LogWarning(objectPersistencePacket.ObjectName + " didn't have a prefab resource. Object may not be restored correctly.", go);
		}
		objectPersistencePacket.RestoreObject(ref go);
		return go;
	}

	private object UnpackCollection(object obj)
	{
		if (!(obj is List<ObjectPersistencePacket> list))
		{
			Debug.LogError("Programmer Error: Collection was of type " + obj.GetType().ToString() + " instead of a List<ObjectPersistencePacket>!");
			return null;
		}
		object obj2 = Tools.CreateInstance(FieldType);
		MethodInfo method = obj2.GetType().GetMethod("Add");
		if (method != null)
		{
			Type type = obj2.GetType().GetGenericArguments()[0];
			{
				foreach (ObjectPersistencePacket item in list)
				{
					object obj3 = null;
					try
					{
						if (type.IsSubclassOf(typeof(UnityEngine.Object)))
						{
							obj3 = UnpackUnityObject(item, type);
							if (obj3.GetType() != type)
							{
								obj3 = (obj3 as GameObject).GetComponent(type);
							}
						}
						else
						{
							obj3 = UnpackCustomObject(item, type);
						}
						method.Invoke(obj2, new object[1] { obj3 });
					}
					catch (Exception exception)
					{
						Debug.LogError(("Error adding " + obj3 == null) ? string.Empty : obj3.ToString());
						Debug.LogException(exception);
					}
				}
				return obj2;
			}
		}
		Debug.LogError("Packet doesn't know how to repack " + FieldType.ToString());
		return obj2;
	}
}

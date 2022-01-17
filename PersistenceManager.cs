using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Polenter.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistenceManager
{
	public static Dictionary<Guid, ObjectPersistencePacket> PersistentObjects = new Dictionary<Guid, ObjectPersistencePacket>(2048);

	public static string s_oldTempSavePath = Path.Combine(Application.persistentDataPath, "PreviousGame");

	public static string s_tempSavePath = Path.Combine(Application.persistentDataPath, "CurrentGame");

	public static string s_mobileObjPath = Path.Combine(s_tempSavePath, "MobileObjects.save");

	public static Dictionary<Guid, ObjectPersistencePacket> MobileObjects = new Dictionary<Guid, ObjectPersistencePacket>();

	public static List<ObjectPersistencePacket> PendingMobileObjects = new List<ObjectPersistencePacket>();

	private static Type s_TrapType = typeof(Trap);

	private static Type s_PersistenceType = typeof(Persistence);

	private static Type s_PartyMemberAIType = typeof(PartyMemberAI);

	private static Type s_AIPackageControllerType = typeof(AIPackageController);

	public static bool LevelHasData => File.Exists(GetLevelFilePath(SceneManager.GetActiveScene().name));

	public static event EventHandler OnLoadObjects;

	public static void SaveAndDestroyObject(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		Persistence component = obj.GetComponent<Persistence>();
		if (component == null)
		{
			Debug.LogError("SaveAndDestroyObject was told to save an object with no persistence component!");
			return;
		}
		ObjectPersistencePacket objectPersistencePacket = SaveObject(component);
		if (objectPersistencePacket != null)
		{
			objectPersistencePacket.Packed = true;
		}
		GameUtilities.Destroy(obj);
	}

	public static GameObject RestorePackedObject(Guid id)
	{
		ObjectPersistencePacket objectPersistencePacket = null;
		if (MobileObjects.ContainsKey(id))
		{
			objectPersistencePacket = MobileObjects[id];
		}
		else if (PersistentObjects.ContainsKey(id))
		{
			objectPersistencePacket = PersistentObjects[id];
		}
		if (objectPersistencePacket == null)
		{
			return null;
		}
		if (!objectPersistencePacket.Mobile)
		{
			return null;
		}
		objectPersistencePacket.Packed = false;
		return objectPersistencePacket.CreateObject(mobile: true);
	}

	public static void ModifySavedValue(Guid id, Type component, string variable, object newValue)
	{
		ObjectPersistencePacket objectPersistencePacket = null;
		if (MobileObjects.ContainsKey(id))
		{
			objectPersistencePacket = MobileObjects[id];
		}
		else if (PersistentObjects.ContainsKey(id))
		{
			objectPersistencePacket = PersistentObjects[id];
		}
		if (objectPersistencePacket == null)
		{
			Debug.LogError("Tried to modify saved object that doesn't exist!");
			return;
		}
		ComponentPersistencePacket componentPersistencePacket = null;
		if (objectPersistencePacket.Components.ContainsKey(component))
		{
			componentPersistencePacket = objectPersistencePacket.Components[component];
		}
		if (componentPersistencePacket == null)
		{
			Debug.LogError("Component of type " + component.ToString() + " doesn't exist in " + objectPersistencePacket.ObjectName);
		}
		else if (!componentPersistencePacket.Variables.ContainsKey(variable))
		{
			Debug.LogError("Variable " + variable + " isn't in packet " + component.ToString());
		}
		else
		{
			componentPersistencePacket.Variables[variable] = newValue;
		}
	}

	public static object GetSavedValue(Guid id, Type component, string variable)
	{
		ObjectPersistencePacket objectPersistencePacket = null;
		if (MobileObjects.ContainsKey(id))
		{
			objectPersistencePacket = MobileObjects[id];
		}
		else if (PersistentObjects.ContainsKey(id))
		{
			objectPersistencePacket = PersistentObjects[id];
		}
		if (objectPersistencePacket == null)
		{
			Debug.LogError("Tried to modify saved object that doesn't exist!");
			return null;
		}
		ComponentPersistencePacket componentPersistencePacket = null;
		if (objectPersistencePacket.Components.ContainsKey(component))
		{
			componentPersistencePacket = objectPersistencePacket.Components[component];
		}
		if (componentPersistencePacket == null)
		{
			Debug.LogError("Component of type " + component.ToString() + " doesn't exist in " + objectPersistencePacket.ObjectName);
			return null;
		}
		if (!componentPersistencePacket.Variables.ContainsKey(variable))
		{
			Debug.LogError("Variable " + variable + " isn't in packet " + component.ToString());
			return null;
		}
		return componentPersistencePacket.Variables[variable];
	}

	public static ObjectPersistencePacket SaveObject(Persistence persistence)
	{
		ObjectPersistencePacket objectPersistencePacket = null;
		Guid gUID = persistence.GUID;
		if (PersistentObjects.ContainsKey(gUID))
		{
			objectPersistencePacket = PersistentObjects[gUID];
			PersistentObjects.Remove(gUID);
		}
		else if (MobileObjects.ContainsKey(gUID))
		{
			objectPersistencePacket = MobileObjects[gUID];
			MobileObjects.Remove(gUID);
		}
		if (objectPersistencePacket == null)
		{
			objectPersistencePacket = new ObjectPersistencePacket();
		}
		if (persistence.SaveObject(objectPersistencePacket))
		{
			if (objectPersistencePacket.Mobile)
			{
				try
				{
					MobileObjects.Add(gUID, objectPersistencePacket);
					return objectPersistencePacket;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
					if ((bool)UIDebug.Instance)
					{
						UIDebug.Instance.LogOnScreenWarning("Trying to add '" + objectPersistencePacket.ObjectName + "' to MobileObjects when packet already exists! Packet is in both Mobile and Persistence object lists!", UIDebug.Department.Programming, 10f);
					}
					MobileObjects[gUID] = objectPersistencePacket;
					PersistentObjects.Remove(gUID);
					return objectPersistencePacket;
				}
			}
			try
			{
				PersistentObjects.Add(gUID, objectPersistencePacket);
				return objectPersistencePacket;
			}
			catch (Exception ex2)
			{
				Debug.LogError(ex2.ToString());
				if ((bool)UIDebug.Instance)
				{
					UIDebug.Instance.LogOnScreenWarning("Trying to add '" + objectPersistencePacket.ObjectName + "' to PersistentObjects when packet already exists! Packet is in both Mobile and Persistence object lists!", UIDebug.Department.Programming, 10f);
				}
				PersistentObjects[gUID] = objectPersistencePacket;
				MobileObjects.Remove(gUID);
				return objectPersistencePacket;
			}
		}
		return null;
	}

	public static void SaveComponentForObject(GameObject gameObj, Type componentType)
	{
		if (gameObj == null)
		{
			return;
		}
		Persistence component = gameObj.GetComponent<Persistence>();
		if (component == null)
		{
			return;
		}
		ObjectPersistencePacket packet = GetPacket(component);
		if (packet == null || packet.Components[componentType] == null)
		{
			return;
		}
		Component component2 = gameObj.GetComponent(componentType);
		if (!component2)
		{
			return;
		}
		List<FieldInfo> list = new List<FieldInfo>();
		GetAllFields(componentType, list);
		foreach (FieldInfo item in list)
		{
			object[] customAttributes = item.GetCustomAttributes(typeof(Persistent), inherit: true);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				object val = (customAttributes[0] as Persistent).PackObject(item.GetValue(component2));
				packet.AddVariable(componentType, item.Name, val);
			}
		}
		List<PropertyInfo> list2 = new List<PropertyInfo>();
		GetAllProperties(componentType, list2);
		foreach (PropertyInfo item2 in list2)
		{
			object[] customAttributes2 = item2.GetCustomAttributes(typeof(Persistent), inherit: true);
			if (customAttributes2 != null && customAttributes2.Length != 0)
			{
				object val2 = (customAttributes2[0] as Persistent).PackObject(item2.GetValue(component2, null));
				packet.AddVariable(componentType, item2.Name, val2);
			}
		}
	}

	public static void SaveGame()
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Persistence));
		for (int i = 0; i < array.Length; i++)
		{
			SaveObject((Persistence)array[i]);
		}
		SaveLevelDataToFile();
		SaveMobileDataToFile();
	}

	public static string GetLevelFilePath(string levelName)
	{
		string text = s_tempSavePath;
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return Path.Combine(text, levelName + ".lvl");
	}

	private static void SaveMobileDataToFile()
	{
		string text = s_mobileObjPath + ".backup";
		if (File.Exists(text) && File.Exists(s_mobileObjPath))
		{
			File.Delete(text);
		}
		if (File.Exists(s_mobileObjPath))
		{
			File.Move(s_mobileObjPath, text);
		}
		bool flag = true;
		using (FileStream stream = new FileStream(s_mobileObjPath, FileMode.OpenOrCreate))
		{
			SharpSerializer binaryXMLSerializer = GameResources.GetBinaryXMLSerializer();
			binaryXMLSerializer.Serialize(MobileObjects.Count, stream);
			binaryXMLSerializer.PropertyProvider.AttributesToIgnore.Add(typeof(XmlIgnoreAttribute));
			foreach (ObjectPersistencePacket value in MobileObjects.Values)
			{
				try
				{
					binaryXMLSerializer.Serialize(value, stream);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error saving mobile object " + value.ObjectName + ".\n" + ex.ToString());
					flag = false;
				}
			}
		}
		if (!flag)
		{
			if (File.Exists(s_mobileObjPath))
			{
				File.Delete(s_mobileObjPath);
			}
			if (File.Exists(text))
			{
				File.Move(text, s_mobileObjPath);
			}
			if ((bool)UIDebug.Instance)
			{
				UIDebug.Instance.LogOnScreenWarning("PersistenceManager.SaveMobileDataToFile failed! Save game likely corrupted!", UIDebug.Department.Programming, 10f);
			}
		}
		else if (File.Exists(text))
		{
			File.Delete(text);
		}
	}

	public static void CleanupInvalidMobileData()
	{
		List<Guid> list = new List<Guid>();
		foreach (ObjectPersistencePacket value2 in MobileObjects.Values)
		{
			if (value2 == null || value2.Components == null)
			{
				continue;
			}
			if (value2.Components.ContainsKey(s_TrapType) && value2.Components.ContainsKey(s_PersistenceType))
			{
				Dictionary<string, object> variables = value2.Components[s_TrapType].Variables;
				if (variables == null || !variables.ContainsKey("Owner") || variables["Owner"] == null)
				{
					continue;
				}
				variables = value2.Components[s_PersistenceType].Variables;
				if (variables != null && variables.ContainsKey("m_objDestroyed") && (bool)variables["m_objDestroyed"])
				{
					list.Add(value2.GUID);
					continue;
				}
			}
			ComponentPersistencePacket value = null;
			if (!value2.Components.TryGetValue(s_PartyMemberAIType, out value) && !value2.Components.TryGetValue(s_AIPackageControllerType, out value))
			{
				continue;
			}
			Dictionary<string, object> variables2 = value.Variables;
			if (variables2 != null && variables2.ContainsKey("SummonType"))
			{
				object obj = variables2["SummonType"];
				if (obj != null && (AIController.AISummonType)obj == AIController.AISummonType.Summoned)
				{
					list.Add(value2.GUID);
				}
			}
		}
		foreach (Guid item in list)
		{
			MobileObjects.Remove(item);
		}
	}

	public static void LoadMobileObjects()
	{
		if (GameState.NumSceneLoads == 0)
		{
			string path = s_mobileObjPath;
			if (!File.Exists(path))
			{
				return;
			}
			MobileObjects.Clear();
			List<ObjectPersistencePacket> list = new List<ObjectPersistencePacket>(5000);
			using (FileStream fileStream = new FileStream(path, FileMode.Open))
			{
				SharpSerializer binaryXMLSerializer = GameResources.GetBinaryXMLSerializer();
				int num = (int)binaryXMLSerializer.Deserialize(fileStream);
				for (int i = 0; i < num; i++)
				{
					try
					{
						if (binaryXMLSerializer.Deserialize(fileStream) is ObjectPersistencePacket item)
						{
							list.Add(item);
						}
						else
						{
							Debug.LogError("OPP is null");
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("Object load error! " + ex.ToString());
					}
				}
				fileStream.Close();
			}
			foreach (ObjectPersistencePacket item2 in list)
			{
				MobileObjects.Add(item2.GUID, item2);
			}
			CleanupInvalidMobileData();
		}
		List<Persistence> list2 = new List<Persistence>();
		int num2 = 0;
		foreach (ObjectPersistencePacket value in MobileObjects.Values)
		{
			bool flag = string.IsNullOrEmpty(value.LevelName);
			if (flag)
			{
				num2++;
			}
			if (!(value.Global || flag) && !(value.LevelName == SceneManager.GetActiveScene().name))
			{
				continue;
			}
			GameObject objectByID = InstanceID.GetObjectByID(value.GUID);
			if (objectByID == null && !value.Packed)
			{
				objectByID = value.CreateObject(mobile: true);
				Persistence component = objectByID.GetComponent<Persistence>();
				if ((bool)component)
				{
					list2.Add(component);
				}
			}
			else if (objectByID != null)
			{
				Persistence persistence = null;
				persistence = objectByID.GetComponent<Persistence>();
				if (persistence == null)
				{
					Debug.LogError(value.ObjectName + " doesn't have a persistence component, yet was saved in mobile objects. Something isn't right.");
					persistence = objectByID.AddComponent<Persistence>();
				}
				list2.Add(persistence);
			}
		}
		if (num2 > 0)
		{
			Debug.LogWarning("Restored " + num2 + " mobile packets with an empty LevelName.");
		}
		foreach (Persistence item3 in list2)
		{
			if (item3.GetComponent<Equippable>() != null)
			{
				item3.Load();
			}
		}
		foreach (Persistence item4 in list2)
		{
			if (item4.GetComponent<Equippable>() == null)
			{
				item4.Load();
			}
		}
		foreach (ObjectPersistencePacket pendingMobileObject in PendingMobileObjects)
		{
			if (MobileObjects.ContainsKey(pendingMobileObject.GUID))
			{
				MobileObjects[pendingMobileObject.GUID] = pendingMobileObject;
			}
			else
			{
				MobileObjects.Add(pendingMobileObject.GUID, pendingMobileObject);
			}
		}
		PendingMobileObjects.Clear();
	}

	private static void SaveLevelDataToFile()
	{
		string levelFilePath = GetLevelFilePath(GameState.ApplicationLoadedLevelName);
		if (File.Exists(levelFilePath))
		{
			File.Delete(levelFilePath);
		}
		using FileStream fileStream = new FileStream(levelFilePath, FileMode.OpenOrCreate);
		SharpSerializer binaryXMLSerializer = GameResources.GetBinaryXMLSerializer();
		List<ObjectPersistencePacket> list = new List<ObjectPersistencePacket>();
		list.AddRange(PersistentObjects.Values);
		binaryXMLSerializer.Serialize(list.Count, fileStream);
		binaryXMLSerializer.PropertyProvider.AttributesToIgnore.Add(typeof(XmlIgnoreAttribute));
		foreach (ObjectPersistencePacket item in list)
		{
			try
			{
				binaryXMLSerializer.Serialize(item, fileStream);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error saving level object " + item.ObjectName + ". " + ex.ToString());
			}
		}
		fileStream.Close();
	}

	public static void LevelLoaded()
	{
		try
		{
			string levelFilePath = GetLevelFilePath(GameState.ApplicationLoadedLevelName);
			PersistentObjects.Clear();
			PendingMobileObjects.Clear();
			if (File.Exists(levelFilePath))
			{
				using FileStream fileStream = new FileStream(levelFilePath, FileMode.Open);
				SharpSerializer binaryXMLSerializer = GameResources.GetBinaryXMLSerializer();
				int num = (int)binaryXMLSerializer.Deserialize(fileStream);
				for (int i = 0; i < num; i++)
				{
					try
					{
						if (binaryXMLSerializer.Deserialize(fileStream) is ObjectPersistencePacket objectPersistencePacket)
						{
							PersistentObjects.Add(objectPersistencePacket.GUID, objectPersistencePacket);
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("Object load error! " + ex.ToString());
					}
				}
				fileStream.Close();
			}
			foreach (ObjectPersistencePacket value in PersistentObjects.Values)
			{
				if (value.LoadManually)
				{
					value.CreateObject(mobile: false);
				}
			}
			LoadMobileObjects();
			InstanceID[] array = UnityEngine.Object.FindObjectsOfType<InstanceID>();
			for (int j = 0; j < array.Length; j++)
			{
				Persistence component = array[j].GetComponent<Persistence>();
				if (!(component == null) && PersistentObjects.ContainsKey(component.GUID))
				{
					if (PersistentObjects[component.GUID].Packed)
					{
						GameUtilities.Destroy(component.gameObject);
						Debug.LogWarning("LevelLoadedDestroy " + component.gameObject.name);
					}
					else
					{
						component.ResetForLoad();
					}
				}
			}
			if (PersistenceManager.OnLoadObjects != null)
			{
				PersistenceManager.OnLoadObjects(null, EventArgs.Empty);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			GameState.ReturnToMainMenuFromError();
		}
	}

	public static ObjectPersistencePacket GetPacket(Guid guid)
	{
		ObjectPersistencePacket value = null;
		if (MobileObjects.TryGetValue(guid, out value) || PersistentObjects.TryGetValue(guid, out value))
		{
			return value;
		}
		return null;
	}

	public static ObjectPersistencePacket GetPacket(Persistence obj)
	{
		return GetPacket(obj.GUID);
	}

	public static void ClearComponentPacket(GameObject gameObj, Type componentType)
	{
		if (gameObj == null)
		{
			return;
		}
		Persistence component = gameObj.GetComponent<Persistence>();
		if (!(component == null))
		{
			ObjectPersistencePacket packet = GetPacket(component);
			if (packet != null && packet.Components.ContainsKey(componentType) && packet.Components[componentType] != null)
			{
				packet.Components[componentType] = null;
			}
		}
	}

	public static void RemoveObject(Guid guid, bool removeEvenIfPacked)
	{
		if (MobileObjects.ContainsKey(guid) && (!MobileObjects[guid].Packed || removeEvenIfPacked))
		{
			MobileObjects.Remove(guid);
		}
		if (PersistentObjects.ContainsKey(guid))
		{
			PersistentObjects.Remove(guid);
		}
	}

	public static void RemoveObject(Persistence p)
	{
		RemoveObject(p.GUID, removeEvenIfPacked: false);
	}

	public static void ClearTempData()
	{
		try
		{
			if (Directory.Exists(s_tempSavePath))
			{
				try
				{
					Directory.Move(s_tempSavePath, s_oldTempSavePath);
				}
				catch (IOException exception)
				{
					Debug.LogError("Failed to move savegame temp directory.");
					Debug.LogException(exception);
					if (Directory.Exists(s_tempSavePath))
					{
						FileUtility.DeleteDirectory(s_tempSavePath, block: true);
					}
				}
				if (Directory.Exists(s_oldTempSavePath))
				{
					FileUtility.DeleteDirectory(s_oldTempSavePath, block: false);
				}
			}
		}
		catch (IOException ex)
		{
			Debug.LogError("FATAL: Failed to delete save-game temp directory.");
			throw ex;
		}
		if (!Directory.Exists(s_tempSavePath))
		{
			Directory.CreateDirectory(s_tempSavePath).Attributes |= FileAttributes.Hidden;
		}
		MobileObjects.Clear();
		PersistentObjects.Clear();
	}

	public static void GetAllFields(Type t, List<FieldInfo> list)
	{
		if (!(t == null))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			list.AddRange(t.GetFields(bindingAttr));
			GetAllFields(t.BaseType, list);
		}
	}

	public static void GetAllProperties(Type t, List<PropertyInfo> list)
	{
		if (!(t == null))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			list.AddRange(t.GetProperties(bindingAttr));
			GetAllProperties(t.BaseType, list);
		}
	}

	public static FieldInfo FindField(Type t, string name)
	{
		if (t == null)
		{
			return null;
		}
		BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo field = t.GetField(name, bindingAttr);
		if (field != null)
		{
			return field;
		}
		return FindField(t.BaseType, name);
	}

	public static PropertyInfo FindProperty(Type t, string name)
	{
		if (t == null)
		{
			return null;
		}
		BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		PropertyInfo property = t.GetProperty(name, bindingAttr);
		if (property != null)
		{
			return property;
		}
		return FindProperty(t.BaseType, name);
	}
}

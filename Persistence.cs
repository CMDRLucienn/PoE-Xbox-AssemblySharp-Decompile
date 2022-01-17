using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InstanceID))]
public class Persistence : MonoBehaviour
{
	public enum AssetBundleExportPackage
	{
		None,
		Base,
		X1,
		X2,
		X4
	}

	public AssetBundleExportPackage ExportPackage;

	public bool Mobile;

	public bool TemplateOnly;

	public string Prefab = string.Empty;

	public bool GlobalObject;

	public bool m_isPrefab;

	private bool m_unloadsBetweenLevels = true;

	private bool m_loaded;

	[Persistent]
	private bool m_objDestroyed;

	private bool m_destroyOnStart;

	private bool m_hasSaved;

	private bool m_hookedToEvents;

	public static Guid DebugCheckGuid = Guid.Empty;

	public static string DebugCheckName = "head";

	[Persistent]
	public bool IsActive
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool CreateFromPrefabFailed { get; set; }

	public bool IsNew { get; private set; }

	public bool IsPrefab
	{
		get
		{
			return m_isPrefab;
		}
		set
		{
			m_isPrefab = value;
		}
	}

	public Guid GUID
	{
		get
		{
			InstanceID component = GetComponent<InstanceID>();
			if (component == null)
			{
				Debug.LogError(base.gameObject.name + " doesn't have an instance ID!", base.gameObject);
				return Guid.Empty;
			}
			return component.Guid;
		}
		set
		{
			InstanceID component = GetComponent<InstanceID>();
			if (component != null)
			{
				component.Guid = value;
			}
		}
	}

	public string UniqueID
	{
		get
		{
			return GUID.ToString();
		}
		set
		{
			GUID = new Guid(value);
		}
	}

	public bool PackageIsValid => (AssetBundlePackageToProductPackage(ExportPackage) & ProductConfiguration.ActivePackage) != 0;

	public bool ImmediateRestore { get; set; }

	[Persistent]
	public bool UnloadsBetweenLevels
	{
		get
		{
			return m_unloadsBetweenLevels;
		}
		set
		{
			if (!value)
			{
				m_loaded = true;
			}
			m_unloadsBetweenLevels = value;
			if (!value && !GlobalObject)
			{
				GameState.PersistAcrossSceneLoadsTracked(base.gameObject);
			}
		}
	}

	private void OnEnable()
	{
		if (!m_hookedToEvents)
		{
			PersistenceManager.OnLoadObjects += PersistenceManager_OnLoadObjects;
			m_hookedToEvents = true;
		}
	}

	private void OnDestroy()
	{
		if (m_hookedToEvents)
		{
			PersistenceManager.OnLoadObjects -= PersistenceManager_OnLoadObjects;
			m_hookedToEvents = false;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Awake()
	{
		m_isPrefab = false;
	}

	private void Start()
	{
		if (!m_hasSaved && !m_loaded && !GameState.IsRestoredLevel)
		{
			Load();
		}
		Health component = GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath += HandleOnDeath;
		}
	}

	private void HandleOnDeath(GameObject myObject, GameEventArgs args)
	{
		if (GetComponent<Health>().ShouldDecay)
		{
			SetForDestroy();
		}
	}

	public void SetForDestroy()
	{
		m_objDestroyed = true;
		if (!GameState.IsLoading)
		{
			SaveObject();
		}
	}

	public void ResetForLoad()
	{
		m_loaded = false;
	}

	public void Restored()
	{
		if (m_objDestroyed)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (m_destroyOnStart)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	public void Load()
	{
		if (base.gameObject == null)
		{
			return;
		}
		GameObject go = base.gameObject;
		if (m_loaded && !GameState.LoadedGame)
		{
			ObjectPersistencePacket packet = PersistenceManager.GetPacket(GUID);
			if (packet != null && packet.Packed)
			{
				m_destroyOnStart = true;
			}
		}
		else
		{
			if (m_isPrefab || (!UnloadsBetweenLevels && !GameState.LoadedGame && GameState.IsRestoredLevel))
			{
				return;
			}
			ObjectPersistencePacket packet2 = PersistenceManager.GetPacket(this);
			if (!UnloadsBetweenLevels && !GameState.LoadedGame)
			{
				m_loaded = true;
			}
			if (packet2 == null)
			{
				IsNew = true;
				RestoreObject(base.gameObject);
				return;
			}
			m_loaded = true;
			packet2.RestoreObject(ref go);
			if (TemplateOnly && Mobile && packet2.LevelName != SceneManager.GetActiveScene().name)
			{
				GameUtilities.Destroy(base.gameObject);
				return;
			}
			RestoreObject(base.gameObject);
			if (m_objDestroyed)
			{
				m_destroyOnStart = true;
			}
		}
	}

	private void RestoreObject(GameObject restoredObj)
	{
		RestoreObject(restoredObj, delayed: true);
	}

	private void RestoreObject(GameObject restoredObj, bool delayed)
	{
		object[] components = restoredObj.GetComponents<MonoBehaviour>();
		object[] array = components;
		ConditionalToggle conditionalToggle = null;
		components = array;
		for (int i = 0; i < components.Length; i++)
		{
			MonoBehaviour monoBehaviour = (MonoBehaviour)components[i];
			if (monoBehaviour == null)
			{
				continue;
			}
			if (monoBehaviour is ConditionalToggle)
			{
				if (conditionalToggle != null)
				{
					if (UIDebug.Instance != null)
					{
						UIDebug.Instance.LogOnScreenWarning(string.Concat("Object ", conditionalToggle.gameObject, " has more than one ConditionalToggle component. This is very dangerous!"), UIDebug.Department.Design, 10f);
					}
					else
					{
						Debug.Log(string.Concat("Object ", conditionalToggle.gameObject, " has more than one ConditionalToggle component. This is very dangerous!"));
					}
				}
				conditionalToggle = monoBehaviour as ConditionalToggle;
				continue;
			}
			MethodInfo method = monoBehaviour.GetType().GetMethod("Restored");
			if (method != null)
			{
				if (ImmediateRestore)
				{
					method.Invoke(monoBehaviour, null);
				}
				else
				{
					monoBehaviour.Invoke("Restored", 0f);
				}
			}
		}
		if (conditionalToggle != null)
		{
			MemberInfo[] member = conditionalToggle.GetType().GetMember("Restored");
			foreach (MemberInfo memberInfo in member)
			{
				conditionalToggle.Invoke(memberInfo.Name, 0f);
			}
		}
	}

	public void SaveObject()
	{
		PersistenceManager.SaveObject(this);
	}

	public bool SaveObject(ObjectPersistencePacket packet)
	{
		m_hasSaved = true;
		bool flag = Mobile;
		string parent = "none";
		bool flag2 = base.gameObject.GetComponent<CharacterStats>() != null;
		GameObject gameObject = base.gameObject;
		Persistence persistence = this;
		while (gameObject != null && gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			persistence = gameObject.GetComponent<Persistence>();
			if ((bool)persistence)
			{
				flag = persistence.Mobile;
				parent = persistence.name;
			}
			if ((bool)gameObject.GetComponent<CharacterStats>())
			{
				flag2 = true;
				if (persistence == null)
				{
					flag = false;
				}
			}
		}
		if (flag2 && persistence != null && persistence.m_objDestroyed && flag && (bool)gameObject.GetComponent<Container>())
		{
			flag = false;
		}
		Equippable component = GetComponent<Equippable>();
		if (!flag && component != null && component.Location != 0)
		{
			return false;
		}
		if (!flag && GetComponent<GenericAbility>() != null && flag2 && GetComponent<Consumable>() == null)
		{
			return false;
		}
		packet.LevelName = GameState.ApplicationLoadedLevelName;
		packet.ObjectID = GUID.ToString();
		packet.PrefabResource = Prefab;
		packet.ObjectName = base.gameObject.name;
		packet.Mobile = flag;
		packet.LoadManually = Mobile && !flag;
		packet.Parent = parent;
		packet.Global = GlobalObject;
		packet.Location = base.gameObject.transform.position;
		packet.Rotation = base.gameObject.transform.rotation.eulerAngles;
		object[] components = base.gameObject.GetComponents<MonoBehaviour>();
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
					packet.AddVariable(type, item.Name, val);
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
					packet.AddVariable(type2, item2.Name, val2);
				}
			}
		}
		return true;
	}

	private void PersistenceManager_OnLoadObjects(object sender, EventArgs e)
	{
		Load();
	}

	public static ProductConfiguration.Package AssetBundlePackageToProductPackage(AssetBundleExportPackage package)
	{
		return package switch
		{
			AssetBundleExportPackage.Base => ProductConfiguration.Package.BaseGame, 
			AssetBundleExportPackage.X1 => ProductConfiguration.Package.Expansion1, 
			AssetBundleExportPackage.X2 => ProductConfiguration.Package.Expansion2, 
			AssetBundleExportPackage.X4 => ProductConfiguration.Package.Expansion4, 
			_ => ProductConfiguration.Package.BaseGame, 
		};
	}
}

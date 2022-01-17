using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OEIFormats.GlobalVariables;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
	private class QueuedVariableChange
	{
		public string Name { get; set; }

		public int Value { get; set; }
	}

	[Persistent]
	private Hashtable m_data = new Hashtable(2048);

	private List<QueuedVariableChange> m_queuedVariableChanges = new List<QueuedVariableChange>();

	public static GlobalVariables Instance { get; private set; }

	public static GlobalVariables ToolInstance => Instance;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GlobalVariables' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		m_queuedVariableChanges.Clear();
		LoadGlobalsFromData();
		OutputGlobalsToFile();
	}

	private void LoadGlobalsFromData()
	{
		ProductConfiguration.Package[] array = (ProductConfiguration.Package[])Enum.GetValues(typeof(ProductConfiguration.Package));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == ProductConfiguration.Package.BackerBeta)
			{
				continue;
			}
			string text = ProductConfiguration.PackageDataFolders[i] + "/design/global/game.globalvariables";
			text = Application.dataPath + Path.DirectorySeparatorChar + text;
			if (!File.Exists(text))
			{
				continue;
			}
			foreach (GlobalVariablesData.GlobalVariable globalVariable in GlobalVariablesData.Load(text).GlobalVariables)
			{
				if (!m_data.ContainsKey(globalVariable.Tag))
				{
					m_data.Add(globalVariable.Tag, globalVariable.InitialValue);
				}
			}
		}
	}

	public void Restored()
	{
		LoadGlobalsFromData();
		OutputGlobalsToFile();
		Debug.Log("Global variables restored");
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_queuedVariableChanges.Count <= 0)
		{
			return;
		}
		lock (m_queuedVariableChanges)
		{
			foreach (QueuedVariableChange queuedVariableChange in m_queuedVariableChanges)
			{
				SetVariable(queuedVariableChange.Name, queuedVariableChange.Value);
			}
			m_queuedVariableChanges.Clear();
		}
	}

	public void QueueVariable(string name, int val)
	{
		QueuedVariableChange queuedVariableChange = new QueuedVariableChange();
		queuedVariableChange.Name = name;
		queuedVariableChange.Value = val;
		lock (m_queuedVariableChanges)
		{
			m_queuedVariableChanges.Add(queuedVariableChange);
		}
	}

	public void SetVariable(string name, int val)
	{
		if (m_data.ContainsKey(name))
		{
			m_data[name] = val;
		}
		else
		{
			m_data.Add(name, val);
		}
		QuestManager.Instance.TriggerGlobalVariableEvent(name, (int)m_data[name]);
	}

	public bool IsValid(string variableName)
	{
		return m_data.ContainsKey(variableName);
	}

	public int GetVariable(GlobalVariableString gv)
	{
		if (gv != null)
		{
			return GetVariable(gv.Name);
		}
		return -1;
	}

	public int GetVariable(string name)
	{
		if (m_data.ContainsKey(name))
		{
			return (int)m_data[name];
		}
		return -1;
	}

	public IEnumerable<string> GetAllVariableNames()
	{
		foreach (string key in m_data.Keys)
		{
			yield return key;
		}
	}

	public void OutputGlobalsToFile()
	{
	}

	public void OutputGlobalsToFile(string filename)
	{
	}

	public void ImportGlobalsFromFile(string filename)
	{
	}
}

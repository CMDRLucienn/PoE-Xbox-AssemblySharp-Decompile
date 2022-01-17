using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[AddComponentMenu("Miscellaneous/Data Manager")]
public class DataManager : MonoBehaviour
{
	private class DataFile
	{
		public string TargetNamespace;

		public string SortingVariable;

		public string[] SortNames;

		public string[] VariableNames;

		public string[,] Values;
	}

	private static DataFile[] s_dataFiles;

	public static DataManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'DataManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		if (s_dataFiles == null)
		{
			LoadData();
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void LoadData()
	{
		if (s_dataFiles != null)
		{
			return;
		}
		UnityEngine.Object[] array = Resources.LoadAll("Data/Tables");
		s_dataFiles = new DataFile[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == "ExperienceBackerBeta")
			{
				continue;
			}
			string text = array[i].ToString();
			DataFile dataFile = new DataFile();
			List<string> list = new List<string>();
			list.AddRange(text.Split('\n'));
			list.RemoveAt(list.Count - 1);
			Debug.Log("Loading table: " + array[i].name + "\n" + text);
			string[] array2 = list.ToArray();
			if (array2.Length == 0)
			{
				Debug.LogWarning("Invalid file in data folder!");
				continue;
			}
			string[] array3 = array2[0].Split(';');
			dataFile.TargetNamespace = array3[0];
			dataFile.SortingVariable = array3[1];
			array3[2] = array3[2].Substring(1, array3[2].Length - 1);
			array3[2] = array3[2].TrimEnd('\r');
			dataFile.VariableNames = array3[2].Split(',');
			dataFile.SortNames = new string[array2.Length - 1];
			dataFile.Values = new string[array2.Length - 1, dataFile.VariableNames.Length];
			for (int j = 1; j < array2.Length; j++)
			{
				array2[j] = array2[j].TrimEnd('\r');
				string[] array4 = array2[j].Split(',');
				dataFile.SortNames[j - 1] = array4[0];
				for (int k = 0; k < dataFile.VariableNames.Length; k++)
				{
					dataFile.Values[j - 1, k] = array4[k + 1];
				}
			}
			s_dataFiles[i] = dataFile;
		}
		Debug.Log(array.Length + " data files parsed!");
	}

	public static void AdjustFromData(ref object component)
	{
		AdjustFromData(ref component, component.GetType().ToString());
	}

	public static void AdjustFromData(ref object component, string componentType)
	{
		if (s_dataFiles == null)
		{
			LoadData();
		}
		DataFile[] array = s_dataFiles;
		foreach (DataFile dataFile in array)
		{
			if (dataFile != null && componentType == dataFile.TargetNamespace)
			{
				AlterData(dataFile, ref component);
			}
		}
	}

	private static void AlterData(DataFile data, ref object component)
	{
		FieldInfo[] fields = component.GetType().GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!(fieldInfo.Name == data.SortingVariable))
			{
				continue;
			}
			string text = fieldInfo.GetValue(component).ToString();
			int num = -1;
			for (int j = 0; j < data.SortNames.Length; j++)
			{
				if (data.SortNames[j] == text)
				{
					num = j;
					break;
				}
			}
			if (num == -1)
			{
				continue;
			}
			for (int k = 0; k < data.VariableNames.Length; k++)
			{
				bool flag = false;
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if (!string.IsNullOrEmpty(data.Values[num, k]) && fieldInfo2.Name == data.VariableNames[k])
					{
						if (fieldInfo2.FieldType == typeof(int))
						{
							fieldInfo2.SetValue(component, IntUtils.ParseInvariant(data.Values[num, k]));
						}
						else if (fieldInfo2.FieldType == typeof(float))
						{
							fieldInfo2.SetValue(component, FloatUtils.ParseInvariant(data.Values[num, k]));
						}
						else
						{
							Debug.LogError("Data table manager doesn't support data types of " + fieldInfo2.DeclaringType.ToString() + ". " + data.VariableNames[k] + " can't be supported!");
						}
						flag = true;
						break;
					}
				}
				if (!flag && data.VariableNames[k].ToString() != "Display")
				{
					LogError("DataManager could not find field '" + data.VariableNames[k] + "' on '" + component.GetType().Name + "'", component);
				}
			}
		}
	}

	public static T GetValueFromData<T>(ref object component, string componentType, string memberName)
	{
		if (s_dataFiles == null)
		{
			LoadData();
		}
		DataFile[] array = s_dataFiles;
		foreach (DataFile dataFile in array)
		{
			if (dataFile != null && componentType == dataFile.TargetNamespace)
			{
				return GetData<T>(dataFile, ref component, memberName);
			}
		}
		return (T)Convert.ChangeType(0, typeof(T));
	}

	private static T GetData<T>(DataFile data, ref object component, string memberName)
	{
		FieldInfo[] fields = component.GetType().GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!(fieldInfo.Name == data.SortingVariable))
			{
				continue;
			}
			string text = fieldInfo.GetValue(component).ToString();
			int num = -1;
			for (int j = 0; j < data.SortNames.Length; j++)
			{
				if (data.SortNames[j] == text)
				{
					num = j;
					break;
				}
			}
			if (num == -1)
			{
				continue;
			}
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo2 in array2)
			{
				if (!(fieldInfo2.Name == memberName))
				{
					continue;
				}
				for (int l = 0; l < data.VariableNames.Length; l++)
				{
					if (!string.IsNullOrEmpty(data.Values[num, l]) && fieldInfo2.Name == data.VariableNames[l])
					{
						if (fieldInfo2.FieldType == typeof(int))
						{
							return (T)Convert.ChangeType(IntUtils.ParseInvariant(data.Values[num, l]), typeof(T));
						}
						if (fieldInfo2.FieldType == typeof(float))
						{
							return (T)Convert.ChangeType(FloatUtils.ParseInvariant(data.Values[num, l]), typeof(T));
						}
						Debug.LogError("Data table manager doesn't support data types of " + fieldInfo2.DeclaringType.ToString() + ". " + data.VariableNames[l] + " can't be supported!");
					}
				}
			}
		}
		return (T)Convert.ChangeType(0, typeof(T));
	}

	private static void LogError(string error, object source)
	{
		if (source is UnityEngine.Object)
		{
			Debug.LogError(error, source as UnityEngine.Object);
		}
		else
		{
			Debug.LogError(error);
		}
	}
}

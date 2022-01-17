using System.IO;
using UnityEngine;

public class LevelStartWrapperEnter : MonoBehaviour
{
	private static StreamWriter logFile;

	private static LevelStartWrapperEnter s_instance;

	private void Awake()
	{
		WriteLineToLog("AWAKE: " + GetType().ToString() + ", GameObject = " + base.gameObject);
	}

	private void Start()
	{
		WriteLineToLog("START: " + GetType().ToString() + ", GameObject = " + base.gameObject);
		if (!(s_instance != null))
		{
			s_instance = this;
			WriteLineToLog("PRIMARY LEVELSTARTWRAPPERENTER OWNED BY: " + base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
		if (logFile != null)
		{
			logFile.Close();
			logFile = null;
		}
	}

	public static void WriteLineToLog(string line)
	{
		if (logFile != null)
		{
			logFile.WriteLine(line);
			logFile.Flush();
		}
	}
}

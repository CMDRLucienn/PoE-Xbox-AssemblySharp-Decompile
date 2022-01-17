using System.IO;
using UnityEngine;

public class GameInstanceLocker : MonoBehaviour
{
	private static string LockFilePath;

	private static FileStream lockFile;

	private void Awake()
	{
		LockFilePath = Application.dataPath + Path.DirectorySeparatorChar + "instance.lock";
		if (!new FileInfo(LockFilePath).Exists)
		{
			Debug.Log("Creating lock file at '" + LockFilePath + "'\n");
			lockFile = new FileStream(LockFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			return;
		}
		Debug.Log("Opening lock file at '" + LockFilePath + "'\n");
		try
		{
			lockFile = new FileStream(LockFilePath, FileMode.Open, FileAccess.Write, FileShare.None);
		}
		catch (IOException ex)
		{
			Debug.LogError("Lock file already opened at location '" + LockFilePath + "'. Closing application.\n" + ex.Message);
			Application.Quit();
		}
	}

	private void OnApplicationQuit()
	{
		if (lockFile != null)
		{
			lockFile.Close();
			lockFile = null;
		}
	}
}

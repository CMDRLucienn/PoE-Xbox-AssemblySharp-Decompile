using System.IO;
using System.Threading;
using UnityEngine;

public static class FileUtility
{
	public static void RemoveReadOnly(string filename)
	{
		if (File.Exists(filename) && (File.GetAttributes(filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
		{
			FileAttributes attributes = File.GetAttributes(filename);
			attributes &= ~FileAttributes.ReadOnly;
			File.SetAttributes(filename, attributes);
		}
	}

	public static void CreateDirectory(string path)
	{
		if (!Directory.Exists(path))
		{
			try
			{
				Directory.CreateDirectory(path);
			}
			catch
			{
			}
		}
	}

	public static bool DeleteFile(string filename)
	{
		try
		{
			RemoveReadOnly(filename);
			File.Delete(filename);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static void DeleteDirectory(string path, bool block)
	{
		DeleteDirectoryHelper(path);
		if (!block)
		{
			return;
		}
		int num = 3000;
		int num2 = 30;
		while (Directory.Exists(path))
		{
			Thread.Sleep(num2);
			num -= num2;
			if (num <= 0)
			{
				Debug.LogError("Timed out waiting for Directory to delete ('" + path + "')");
				break;
			}
		}
	}

	private static void DeleteDirectoryHelper(string path)
	{
		string[] files = Directory.GetFiles(path);
		foreach (string path2 in files)
		{
			File.SetAttributes(path2, FileAttributes.Normal);
			File.Delete(path2);
		}
		files = Directory.GetDirectories(path);
		for (int i = 0; i < files.Length; i++)
		{
			DeleteDirectoryHelper(files[i]);
		}
		Directory.Delete(path);
	}
}

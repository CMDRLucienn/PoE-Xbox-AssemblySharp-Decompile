using System;
using System.IO;
using UnityEngine;

[Serializable]
public class FilenameObject
{
	public string Filename;

	public UnityEngine.Object Asset;

	public string ExtensionFilter;

	public FilenameObject(string extensionFilter)
	{
		Filename = string.Empty;
		ExtensionFilter = extensionFilter;
	}

	public void Start()
	{
		string overridePath = GameResources.GetOverridePath(Path.GetFileName(Filename));
		if (!string.IsNullOrEmpty(overridePath))
		{
			Filename = overridePath;
		}
	}
}

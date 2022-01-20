// IEMod.Helpers.PathHelper
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Patchwork.Attributes;

[PatchedByType("IEMod.Helpers.PathHelper")]
[NewType(null, null)]
public static class PathHelper
{
	private static readonly string _curDir = Environment.CurrentDirectory;

	private static readonly bool _isWindows = Environment.OSVersion.VersionString.Contains("Windows");

	public static bool IsCaseSensitive
	{
		[PatchedByMember("System.Boolean IEMod.Helpers.PathHelper::get_IsCaseSensitive()")]
		get
		{
			return _isWindows;
		}
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::Combine(System.String[])")]
	public static string Combine(params string[] paths)
	{
		return paths.Aggregate("", Path.Combine);
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::GetAbsolutePath(System.String)")]
	public static string GetAbsolutePath(string relativePath)
	{
		if (Path.IsPathRooted(relativePath))
		{
			return relativePath;
		}
		string path = Combine(_curDir, relativePath);
		return Path.GetFullPath(path);
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::GetRelativePath(System.String,System.String)")]
	public static string GetRelativePath(string fromPath, string toPath)
	{
		if (string.IsNullOrEmpty(fromPath))
		{
			throw new ArgumentNullException("fromPath");
		}
		if (string.IsNullOrEmpty(toPath))
		{
			throw new ArgumentNullException("toPath");
		}
		if (!Path.IsPathRooted(toPath))
		{
			return toPath;
		}
		Uri uri = new Uri(fromPath);
		Uri uri2 = new Uri(toPath, UriKind.RelativeOrAbsolute);
		if (uri.Scheme != uri2.Scheme)
		{
			return toPath;
		}
		Uri uri3 = uri.MakeRelativeUri(uri2);
		string text = Uri.UnescapeDataString(uri3.ToString());
		if (uri2.Scheme.ToUpperInvariant() == "FILE")
		{
			text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}
		return text;
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::GetRelativePath(System.String)")]
	public static string GetRelativePath(string path)
	{
		return GetRelativePath(_curDir, path);
	}

	[PatchedByMember("System.Boolean IEMod.Helpers.PathHelper::Equal(System.String,System.String)")]
	public static bool Equal(string path1, string path2)
	{
		return string.Equals(path1, path2, IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::ChangeExtension(System.String,System.Func`2<System.String,System.String>)")]
	public static string ChangeExtension(string path, Func<string, string> selector)
	{
		string extension = Path.GetExtension(path);
		return Path.ChangeExtension(path, selector(extension));
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<System.String> IEMod.Helpers.PathHelper::Components(System.String)")]
	public static IEnumerable<string> Components(string path)
	{
		return path.Split(Path.DirectorySeparatorChar);
	}

	[PatchedByMember("System.String IEMod.Helpers.PathHelper::GetUserFriendlyPath(System.String)")]
	public static string GetUserFriendlyPath(string path)
	{
		List<string> list = Components(path).ToList();
		int count = list.Count;
		if (count <= 5)
		{
			return path;
		}
		list.RemoveRange(2, list.Count - 4);
		list.Insert(2, "...");
		return Combine(list.ToArray());
	}
}

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static class WindowsPathHelper
{
	[ComImport]
	[Guid("3aa7af7e-9b36-420c-a8e3-f77d4674a488")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IKnownFolder
	{
		void GetId_Stub();

		void GetCategory_Stub();

		void GetShellItem_Stub();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetPath([In] uint flags, [MarshalAs(UnmanagedType.LPWStr)] out string path);

		void SetPath_Stub();

		void GetLocation_Stub();

		void GetFolderType_Stub();

		void GetRedirectionCapabilities_Stub();

		void GetFolderDefinition_Stub();
	}

	[ComImport]
	[Guid("8be2d872-86aa-4d47-b776-32cca40c7018")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IKnownFolderManager
	{
		void FolderIdFromCsidl_Stub();

		void FolderIdToCsidl_Stub();

		void GetFolderIds_Stub();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetFolder([In] ref Guid folderId, [MarshalAs(UnmanagedType.Interface)] out IKnownFolder knownFolder);

		void GetFolderByName_Stub();

		void RegisterFolder_Stub();

		void UnregisterFolder_Stub();

		void FindFolderFromPath_Stub();

		void FindFolderFromIDList_Stub();

		void Redirect_Stub();
	}

	[ComImport]
	[Guid("4df0c730-df9d-4ae3-9153-aa6b82e9795a")]
	internal class KnownFolderManagerImpl
	{
	}

	private const int MAX_PATH = 260;

	private const int CSIDL_PERSONAL = 5;

	private const string CLSID_KnownFolderManager = "4df0c730-df9d-4ae3-9153-aa6b82e9795a";

	private const string IID_IKnownFolder = "3aa7af7e-9b36-420c-a8e3-f77d4674a488";

	private const string IID_IKnownFolderManager = "8be2d872-86aa-4d47-b776-32cca40c7018";

	private static readonly Guid FOLDERID_SavedGames = new Guid(1281110783u, 48029, 17328, 181, 180, 45, 114, 229, 78, 170, 164);

	[DllImport("Shell32", CharSet = CharSet.Unicode)]
	private static extern int SHGetSpecialFolderPath(IntPtr ownerWindowHandle, StringBuilder path, int folderId, int createFlag);

	public static string GetSaveGameDirectory()
	{
		if (isAtLeastWindowsVersion(6, 0))
		{
			try
			{
				string knownFolder = GetKnownFolder(FOLDERID_SavedGames);
				if (!string.IsNullOrEmpty(knownFolder))
				{
					return knownFolder;
				}
			}
			catch (Exception)
			{
			}
		}
		try
		{
			string specialFolder = GetSpecialFolder(5);
			if (!string.IsNullOrEmpty(specialFolder))
			{
				string text = Path.Combine(specialFolder, "My Games");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				return text;
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static string GetSpecialFolder(int folderId)
	{
		StringBuilder stringBuilder = new StringBuilder(260);
		if (SHGetSpecialFolderPath(IntPtr.Zero, stringBuilder, folderId, 1) == 0)
		{
			throw new Win32Exception("Could not query special folder path");
		}
		return stringBuilder.ToString();
	}

	public static string GetKnownFolder(Guid knownFolderId)
	{
		(((new KnownFolderManagerImpl() ?? throw new COMException("Could not create instance of known folder manager coclass")) as IKnownFolderManager) ?? throw new COMException("Could not query known folder manager interface")).GetFolder(ref knownFolderId, out var knownFolder);
		if (knownFolder == null)
		{
			throw new COMException("Could not query known folder");
		}
		knownFolder.GetPath(0u, out var path);
		return path;
	}

	public static bool isAtLeastWindowsVersion(int major, int minor)
	{
		Version version = Environment.OSVersion.Version;
		if (version.Major <= major)
		{
			if (version.Major == major)
			{
				return version.Minor >= minor;
			}
			return false;
		}
		return true;
	}
}

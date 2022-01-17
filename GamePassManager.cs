using System;
using UnityEngine;
using XGamingRuntime;

public class GamePassManager
{
	private static GamePassManager s_instance;

	private bool m_bInitialized;

	public static bool AppUsesGamePass;

	private XblContextHandle C_Handle;

	private XUserHandle m_userHandle;

	private XUserLocalId m_userLocalID;

	private string m_primaryServiceConfigID;

	private ulong m_userID;

	private uint m_TitleId;

	private XPackageDetails[] m_PackageDetails;

	public static GamePassManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new GamePassManager();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	public XblContextHandle ContextHandle => C_Handle;

	public XUserHandle UserHandle => m_userHandle;

	public XUserLocalId UserLocalId => m_userLocalID;

	public string PrimaryServiceConfigID => m_primaryServiceConfigID;

	public ulong UserID => m_userID;

	public uint TitleId => m_TitleId;

	public static void LogHR(string s, int hr)
	{
		Debug.Log(string.Format("{0} -- hr=0x{1}", s, hr.ToString("X8")));
	}

	public static void SetAchievement(string achievementTag)
	{
		try
		{
			if (AppUsesGamePass && Initialized && s_instance != null)
			{
				((XboxOneAchievementManager)XboxOneNativeWrapper.Instance.GetAchievementHandler()).AwardAchievement(achievementTag);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void SetAchievementProgress(string achievementTag, uint currentProgress)
	{
		try
		{
			if (AppUsesGamePass && Initialized && s_instance != null)
			{
				((XboxOneAchievementManager)XboxOneNativeWrapper.Instance.GetAchievementHandler()).UpdateAchievement(achievementTag, currentProgress);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void ClearAchievement(string achievementTag)
	{
		Debug.LogError("Impossible to clear achievements!");
	}

	public void InitializeUserIDs(XUserHandle userHandle)
	{
		SDK.XGameGetXboxTitleId(out m_TitleId);
		m_primaryServiceConfigID = "00000000-0000-0000-0000-0000" + m_TitleId.ToString("x");
		Debug.LogFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : m_primaryServiceConfigID: {0}", m_primaryServiceConfigID);
		Debug.Log("---------GAMEPASS: GamePassManager.InitializeUserIDs : Initializing XBL");
		int num = SDK.XBL.XblInitialize(m_primaryServiceConfigID);
		Debug.LogFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : XblInitialize completed hr: {0}", num);
		m_userHandle = userHandle;
		Debug.Log("---------GAMEPASS: GamePassManager.InitializeUserIDs : Creating XBL Context");
		num = SDK.XBL.XblContextCreateHandle(m_userHandle, out C_Handle);
		Debug.LogErrorFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : Created XBL Context hr: {0}", num.ToString("X8"));
		SDK.XUserGetId(UserHandle, out m_userID);
		SDK.XUserGetLocalId(UserHandle, out m_userLocalID);
		Debug.LogErrorFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : m_userID {0}, m_userLocalID {1}", m_userID, m_userLocalID);
		num = SDK.XBL.XblSocialManagerAddLocalUser(UserHandle, XblSocialManagerExtraDetailLevel.All);
		Debug.LogFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : XblSocialManagerAddLocalUser hr: {0}", num.ToString("X8"));
		num = SDK.XPackageEnumeratePackages(XPackageKind.Content, XPackageEnumerationScope.ThisAndRelated, out m_PackageDetails);
		Debug.LogFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : Enumerated hr: {0}", num.ToString("X8"));
		if (num >= 0)
		{
			Debug.LogFormat($"---------GAMEPASS: GamePassManager.InitializeUserIDs : Found {m_PackageDetails.Length.ToString()} packages");
			XPackageDetails[] packageDetails = m_PackageDetails;
			foreach (XPackageDetails xPackageDetails in packageDetails)
			{
				Debug.LogFormat($"   {xPackageDetails.DisplayName} - {xPackageDetails.PackageIdentifier}");
			}
		}
		m_bInitialized = true;
		AppUsesGamePass = true;
		if (m_bInitialized)
		{
			Debug.Log("---------GAMEPASS: GamePassManager.InitializeUserIDs : Initialized User IDs");
		}
	}

	public void Reset()
	{
		m_bInitialized = false;
	}

	public void ReEnumerate()
	{
		int num = SDK.XPackageEnumeratePackages(XPackageKind.Content, XPackageEnumerationScope.ThisAndRelated, out m_PackageDetails);
		Debug.LogFormat("---------GAMEPASS: GamePassManager.InitializeUserIDs : Enumerated hr: {0}", num.ToString("X8"));
		if (num >= 0)
		{
			Debug.LogFormat($"---------GAMEPASS: GamePassManager.InitializeUserIDs : Found {m_PackageDetails.Length.ToString()} packages");
			XPackageDetails[] packageDetails = m_PackageDetails;
			foreach (XPackageDetails xPackageDetails in packageDetails)
			{
				Debug.LogFormat($"   {xPackageDetails.DisplayName} - {xPackageDetails.PackageIdentifier}");
			}
		}
	}

	public bool IsPackageAvailable(string PackageStoreId)
	{
		bool result = false;
		XPackageDetails[] packageDetails = m_PackageDetails;
		for (int i = 0; i < packageDetails.Length; i++)
		{
			if (packageDetails[i].StoreId == PackageStoreId)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public string GetPackageIdentifierFromStoreId(string PackageStoreId)
	{
		string result = "";
		XPackageDetails[] packageDetails = m_PackageDetails;
		foreach (XPackageDetails xPackageDetails in packageDetails)
		{
			if (xPackageDetails.StoreId == PackageStoreId)
			{
				result = xPackageDetails.PackageIdentifier;
				break;
			}
		}
		return result;
	}
}

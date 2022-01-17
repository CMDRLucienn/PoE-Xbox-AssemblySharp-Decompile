using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XGamingRuntime;

public class XboxOneNativeWrapper : MonoBehaviour
{
	public struct GamePassUser
	{
		public string UID;

		public string onlineID;

		public bool primary;

		public GamePassUser(string UserID, string OID, bool isPrimary)
		{
			UID = UserID;
			onlineID = OID;
			primary = isPrimary;
		}
	}

	public delegate void ContentCopiedDelegate(bool Success);

	public readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

	private static IAchievementHandler m_AchievementHandler = new XboxOneAchievementManager();

	private static XboxOneHeroStatsWrapper m_HeroStatsWrapper = new XboxOneHeroStatsWrapper();

	private static XboxOnePresenceManager m_PresenceManager = new XboxOnePresenceManager();

	private ulong m_userID;

	private static XRegistrationToken m_SignInRegistrationToken;

	private static XRegistrationToken registerInstallationToken;

	private const int E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED = -1994108670;

	private XUserHandle m_UserHandle;

	private string m_Gamertag;

	private bool SignOutDetected;

	public static ContentCopiedDelegate ContentCopied;

	public XPackageInstallationMonitorHandle royalEditionInstallationMonitor;

	public static XPackageInstalledCallback royalEditionInstallCallback;

	private bool fullScreenOriginalSetting;

	public static XboxOneNativeWrapper Instance { get; private set; }

	public void InstallationChange(XPackageDetails details)
	{
		RoyalEditionButton.isRoyalEditionProgressComplete = true;
	}

	private void Refresh()
	{
	}

	public void Initialize()
	{
		int num = SDK.XGameRuntimeInitialize();
		Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.Initialize : XGameRuntimeInitialize hr: {0}", num.ToString("X8"));
		if (num >= 0)
		{
			GamePassManager.AppUsesGamePass = true;
			SDK.XUserRegisterForChangeEvent(OnPlayerChangeState, out m_SignInRegistrationToken);
			Debug.Log("---------GAMEPASS: XboxOneNativeWrapper.Initialize : Register Change Event for user");
		}
		else
		{
			Debug.LogErrorFormat("---------GAMEPASS: XboxOneNativeWrapper.Initialize : Could not initialize XGameRuntime, error: {0}", num.ToString("X8"));
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameState.PersistAcrossSceneLoadsUntracked(this);
		SignOutDetected = false;
	}

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'XboxOneNativeWrapper' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	public void Start()
	{
		royalEditionInstallCallback = (XPackageInstalledCallback)Delegate.Combine(royalEditionInstallCallback, new XPackageInstalledCallback(InstallationChange));
		Initialize();
		SDK.XPackageRegisterPackageInstalled(royalEditionInstallCallback, out registerInstallationToken);
		royalEditionInstallCallback = (XPackageInstalledCallback)Delegate.Combine(royalEditionInstallCallback, new XPackageInstalledCallback(InstallationChange));
	}

	public void OnDestroy()
	{
		SDK.XUserUnregisterForChangeEvent(m_SignInRegistrationToken);
		royalEditionInstallCallback = (XPackageInstalledCallback)Delegate.Remove(royalEditionInstallCallback, new XPackageInstalledCallback(InstallationChange));
		SDK.XPackageUnregisterPackageInstalled(registerInstallationToken);
		SDK.XGameRuntimeUninitialize();
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void AddUser(bool CanSignSilently = false)
	{
		int options = (CanSignSilently ? 1 : 0);
		Debug.Log("---------GAMEPASS: XboxOneNativeWrapper.AddUser : Starting AddUser...");
		fullScreenOriginalSetting = Screen.fullScreen;
		Screen.fullScreen = false;
		SDK.XUserAddAsync((XUserAddOptions)options, AddUserComplete);
	}

	private void AddUserComplete(int hresult, XUserHandle userHandle)
	{
		Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.AddUserComplete : Adduser completed: {0}", hresult);
		Screen.fullScreen = fullScreenOriginalSetting;
		if (hresult >= 0)
		{
			SDK.XUserGetGamertag(userHandle, out m_Gamertag);
			int num = SDK.XUserGetId(userHandle, out m_userID);
			m_UserHandle = userHandle;
			Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.SignInSilentlyComplete : GetId: {0}", num.ToString("X8"));
			if (num == -1994108670)
			{
				SDK.XUserResolveIssueWithUiUtf16Async(userHandle, null, ResolveIssueCompleted);
			}
			else
			{
				InitAll();
			}
		}
	}

	private void InitAll()
	{
		Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.AddUserComplete : Added user: {0} {1}", m_Gamertag, m_userID.ToString());
		GamePassManager.Instance.InitializeUserIDs(m_UserHandle);
		GameUtilities.CheckForExpansions();
		SaveGameInfo.RecacheSaveGameInfo();
		m_AchievementHandler.Initialize();
		m_HeroStatsWrapper.Initialize();
		m_PresenceManager.Initialize();
	}

	private void ResolveIssueCompleted(int hresult)
	{
		Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.ResolveIssueCompleted : Resolve issue completed: {0}", hresult);
		if (hresult >= 0)
		{
			int num = SDK.XUserGetId(m_UserHandle, out m_userID);
			Debug.LogFormat("---------GAMEPASS: XboxOneNativeWrapper.ResolveIssueCompleted : GetId: {0}", num);
			if (num >= 0)
			{
				InitAll();
			}
			else
			{
				AddUser();
			}
		}
		else
		{
			AddUser();
		}
	}

	private void OnPlayerChangeState(XUserLocalId userLocalId, XUserChangeEvent eventType)
	{
		Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.OnPlayerChangeState : userLocalId = " + userLocalId.value + " --- eventType = " + eventType);
		switch (eventType)
		{
		case XUserChangeEvent.SignedIn:
			Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.OnPlayerChangeState : User SignedIn : userLocalId = " + userLocalId.value);
			if (!GamePassManager.Initialized)
			{
				AddUser();
			}
			break;
		case XUserChangeEvent.SignedOut:
			if (GamePassManager.Instance.UserLocalId.Equals(userLocalId))
			{
				Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.OnPlayerChangeState : User SignedOut : userLocalId = " + userLocalId.value);
				SignOutDetected = true;
			}
			break;
		}
	}

	public IAchievementHandler GetAchievementHandler()
	{
		return m_AchievementHandler;
	}

	public void Update()
	{
		SDK.XTaskQueueDispatch();
		if (SignOutDetected)
		{
			Debug.Log("---------GAMEPASS: XboxOneNativeWrapper.Update SignOutDetected!!!!!");
			if (!GameState.IsLoading)
			{
				GamePassManager.Instance.Reset();
				SaveGameInfo.WaitUntilSafeToSaveLoad();
				GameState.LoadMainMenu(fadeOut: true);
				SignOutDetected = false;
			}
		}
		else if (GamePassManager.Initialized)
		{
			m_AchievementHandler.Update();
			m_HeroStatsWrapper.Update();
			m_PresenceManager.Update();
		}
	}

	public void SetPresence(string presenceId, string[] tokenIds = null)
	{
		Debug.Log("---------GAMEPASS: XboxOneNativeWrapper.SetPresence : presenceId = " + presenceId + " --- tokenIds = " + tokenIds);
		m_PresenceManager.SetPresence(presenceId, tokenIds);
	}

	public void CopyDLCPackageContent(string PackageStoreId, string TargetPath)
	{
		StartCoroutine(CopyDLCPackageContentCoroutine(PackageStoreId, TargetPath));
	}

	public IEnumerator CopyDLCPackageContentCoroutine(string PackageStoreId, string TargetPath)
	{
		bool Result = false;
		try
		{
			if (GamePassManager.Instance.IsPackageAvailable(PackageStoreId))
			{
				string packageIdentifierFromStoreId = GamePassManager.Instance.GetPackageIdentifierFromStoreId(PackageStoreId);
				if (packageIdentifierFromStoreId != "")
				{
					int num = SDK.XPackageMount(packageIdentifierFromStoreId, out var PackageHandle);
					if (num >= 0)
					{
						string MountPath = "";
						num = SDK.XPackageGetMountPath(PackageHandle, out MountPath);
						if (num >= 0)
						{
							DirectoryInfo[] directories = new DirectoryInfo(MountPath).GetDirectories();
							DirectoryInfo[] array = directories;
							foreach (DirectoryInfo directoryInfo in array)
							{
								string sourceDirName = MountPath + "/" + directoryInfo.Name;
								string destDirName = TargetPath + "/" + directoryInfo.Name;
								yield return StartCoroutine(DirectoryCopy(sourceDirName, destDirName));
							}
							SDK.XPackageCloseMountHandle(PackageHandle);
							Result = true;
						}
						else
						{
							Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.CopyDLCPackageContentCoroutine : Couldn't retrieve mount path hr: " + num.ToString("X8"));
							SDK.XPackageCloseMountHandle(PackageHandle);
							Result = false;
						}
					}
					else
					{
						Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.CopyDLCPackageContentCoroutine : Couldn't mount package hr: " + num.ToString("X8"));
						SDK.XPackageCloseMountHandle(PackageHandle);
						Result = false;
					}
				}
				else
				{
					Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.CopyDLCPackageContentCoroutine : Couldn't find package identifier");
					Result = false;
				}
			}
			else
			{
				Debug.LogError("---------GAMEPASS: XboxOneNativeWrapper.CopyDLCPackageContentCoroutine : The requested package isn't available");
				Result = false;
			}
		}
		finally
		{
			if (ContentCopied != null)
			{
				ContentCopied(Result);
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				GameCursor.LockCursor = false;
			}
		}
	}

	private IEnumerator DirectoryCopy(string sourceDirName, string destDirName)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] dirs = directoryInfo.GetDirectories();
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite: true);
			yield return null;
		}
		DirectoryInfo[] array2 = dirs;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
			yield return DirectoryCopy(directoryInfo2.FullName, destDirName2);
		}
	}
}

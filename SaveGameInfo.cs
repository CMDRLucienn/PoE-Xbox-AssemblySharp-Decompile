using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class SaveGameInfo
{
	public enum SizeStyle
	{
		Full,
		DataOnly
	}

	public const int CurrentSaveVersion = 2;

	public const int MinimumCompatibleSaveVersion = 1;

	public const int MaximumCompatibleSaveVersion = 2;

	public string PlayerName = string.Empty;

	public string MapName = string.Empty;

	public string SceneTitle = string.Empty;

	public int SceneTitleId = -1;

	public int Chapter;

	public int RealtimePlayDurationSeconds;

	public int PlaytimeSeconds;

	public bool TrialOfIron;

	public DateTime RealTimestamp;

	public Guid SessionID;

	public string FileName = string.Empty;

	public int SaveVersion = 2;

	public bool GameComplete;

	public string UserSaveName = string.Empty;

	public GameDifficulty Difficulty;

	public ProductConfiguration.Package ActivePackages = ProductConfiguration.Package.BaseGame;

	public static string SAVE_FILENAME = "saveinfo.xml";

	[ExcludeFromSerialization]
	public byte[][] PartyPortraitsRawData = new byte[30][];

	private Texture2D[] mPartyPortraits = new Texture2D[30];

	[ExcludeFromSerialization]
	public byte[] ScreenshotRawData;

	private Texture2D mScreenshot;

	private static object s_cachedSaveInfoLock = new object();

	private static bool s_cachedSaveInfoLoaded = false;

	private static Thread s_cachingThread = null;

	private static bool s_abortThreadSignaled = false;

	private static object s_onCachingCompleteLock = new object();

	private static List<SaveGameInfo> s_cachedSaveGameInfo = new List<SaveGameInfo>();

	private static Thread s_savingThread = null;

	private static SaveGameInfo s_saveGameInfo = null;

	private static string s_saveGameFileName = "";

	private static object s_savingLock = new object();

	public const string QUICK_SAVE = "quicksave";

	public const string AUTO_SAVE = "autosave";

	public const string POINT_OF_NO_RETURN_SAVE = "noreturnsave";

	public const string GAME_COMPLETE_SAVE = "gamecomplete";

	public const string BUG_FILE = "bug.savegame";

	public const string SAVE_EXTENSION = ".savegame";

	public EternityTimeInterval Playtime => new EternityTimeInterval(PlaytimeSeconds);

	[ExcludeFromSerialization]
	public Texture2D[] PartyPortraits
	{
		get
		{
			for (int i = 0; i < mPartyPortraits.Length; i++)
			{
				if (mPartyPortraits[i] == null && PartyPortraitsRawData[i] != null)
				{
					mPartyPortraits[i] = new Texture2D(32, 41);
					mPartyPortraits[i].LoadImage(PartyPortraitsRawData[i]);
					PartyPortraitsRawData[i] = null;
				}
			}
			return mPartyPortraits;
		}
	}

	[ExcludeFromSerialization]
	public Texture2D Screenshot
	{
		get
		{
			if (mScreenshot == null && ScreenshotRawData != null)
			{
				mScreenshot = new Texture2D(150, 84);
				mScreenshot.LoadImage(ScreenshotRawData);
				ScreenshotRawData = null;
			}
			return mScreenshot;
		}
	}

	public static List<SaveGameInfo> CachedSaveGameInfo
	{
		get
		{
			if (s_savingThread != null)
			{
				s_savingThread.Join();
			}
			lock (s_cachedSaveInfoLock)
			{
				for (int num = s_cachedSaveGameInfo.Count - 1; num >= 0; num--)
				{
					if (!GameResources.SaveFileExists(s_cachedSaveGameInfo[num].FileName))
					{
						s_cachedSaveGameInfo.RemoveAt(num);
					}
				}
				return s_cachedSaveGameInfo;
			}
		}
	}

	private static event EventHandler s_onSaveCachingComplete;

	public static event EventHandler OnSaveCachingComplete
	{
		add
		{
			lock (s_onCachingCompleteLock)
			{
				s_onSaveCachingComplete += value;
			}
		}
		remove
		{
			lock (s_onCachingCompleteLock)
			{
				s_onSaveCachingComplete -= value;
			}
		}
	}

	public static void WaitUntilSafeToSaveLoad()
	{
		if (s_cachingThread != null)
		{
			s_cachingThread.Join();
		}
		if (s_savingThread != null)
		{
			s_savingThread.Join();
		}
	}

	public static void CancelRunningThreads()
	{
		if (s_cachingThread != null)
		{
			s_abortThreadSignaled = true;
			while (s_cachingThread.IsAlive)
			{
				Thread.Sleep(10);
			}
			s_cachingThread = null;
		}
	}

	public static void RecacheSaveGameInfo()
	{
		WaitUntilSafeToSaveLoad();
		foreach (SaveGameInfo item in s_cachedSaveGameInfo)
		{
			item.Cleanup();
		}
		s_cachedSaveGameInfo.Clear();
		s_cachedSaveInfoLoaded = false;
		CacheSaveGameInfo();
	}

	public static void CacheSaveGameInfo()
	{
		if (!s_cachedSaveInfoLoaded && !string.IsNullOrEmpty(GameResources.TemporaryCachePath) && !string.IsNullOrEmpty(GameResources.PersistentDataPath))
		{
			s_cachingThread = new Thread(CacheSaveGameInfoThreadFunc);
			s_cachingThread.IsBackground = true;
			s_cachingThread.Start();
		}
	}

	public bool IsAutoSave()
	{
		return FileName.Contains(" autosave");
	}

	public bool IsQuickSave()
	{
		return Path.GetFileNameWithoutExtension(FileName).EndsWith("quicksave");
	}

	public bool IsBug()
	{
		return FileName.Equals("bug.savegame");
	}

	public bool IsPointOfNoReturnSave()
	{
		return Path.GetFileNameWithoutExtension(FileName).EndsWith("noreturnsave");
	}

	public void RemoveFileNameFlags()
	{
		RemoveFileNameSuffix("autosave");
		RemoveFileNameSuffix("quicksave");
		if (!FileName.EndsWith(".savegame"))
		{
			FileName = FileName.Trim() + ".savegame";
		}
	}

	private bool RemoveFileNameSuffix(string searchString)
	{
		int num = FileName.LastIndexOf(searchString);
		if (num >= 0 && num + searchString.Length <= FileName.Length)
		{
			FileName = FileName.Remove(num, searchString.Length);
			return true;
		}
		return false;
	}

	private static void BackupSaveGames()
	{
		try
		{
			string saveGamePath = GameResources.SaveGamePath;
			string text = Path.Combine(saveGamePath, "2.0 Save Games Backup");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
				string[] files = Directory.GetFiles(saveGamePath, "*.savegame");
				for (int i = 0; i < files.Length; i++)
				{
					string fileName = Path.GetFileName(files[i]);
					File.Copy(Path.Combine(saveGamePath, fileName), Path.Combine(text, fileName));
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static void CacheSaveGameInfoThreadFunc()
	{
		try
		{
			lock (s_cachedSaveInfoLock)
			{
				BackupSaveGames();
				SaveGameInfo[] saveFileInfo = GameResources.GetSaveFileInfo(SizeStyle.Full, ref s_abortThreadSignaled);
				foreach (SaveGameInfo saveGameInfo in saveFileInfo)
				{
					if (!s_abortThreadSignaled)
					{
						bool flag = saveGameInfo.SaveVersion >= 1 && saveGameInfo.SaveVersion <= 2;
						bool flag2 = (saveGameInfo.ActivePackages & ProductConfiguration.ActivePackage) == saveGameInfo.ActivePackages;
						if (!saveGameInfo.GameComplete && flag && flag2)
						{
							s_cachedSaveGameInfo.Add(saveGameInfo);
						}
						continue;
					}
					break;
				}
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			Debug.LogError("Error loading save game cache!");
		}
		if (!s_abortThreadSignaled)
		{
			lock (s_onCachingCompleteLock)
			{
				if (SaveGameInfo.s_onSaveCachingComplete != null)
				{
					SaveGameInfo.s_onSaveCachingComplete(null, null);
				}
			}
		}
		s_cachedSaveInfoLoaded = true;
	}

	public static bool SaveGameExists()
	{
		bool flag = false;
		if (Monitor.TryEnter(s_cachedSaveInfoLock))
		{
			try
			{
				return s_cachedSaveGameInfo.Count > 0;
			}
			finally
			{
				Monitor.Exit(s_cachedSaveInfoLock);
			}
		}
		return false;
	}

	public static bool SaveCachingComplete()
	{
		bool flag = false;
		if (Monitor.TryEnter(s_cachedSaveInfoLock))
		{
			try
			{
				return s_cachedSaveInfoLoaded;
			}
			finally
			{
				Monitor.Exit(s_cachedSaveInfoLock);
			}
		}
		return false;
	}

	public static void CleanCache()
	{
		if (!s_cachedSaveInfoLoaded)
		{
			return;
		}
		foreach (SaveGameInfo item in s_cachedSaveGameInfo)
		{
			item?.Cleanup();
		}
		s_cachedSaveGameInfo.Clear();
	}

	public void Cleanup()
	{
		if (mScreenshot != null)
		{
			GameUtilities.Destroy(mScreenshot);
		}
		mScreenshot = null;
		for (int i = 0; i < mPartyPortraits.Length; i++)
		{
			if (mPartyPortraits[i] != null)
			{
				GameUtilities.Destroy(mPartyPortraits[i]);
			}
			mPartyPortraits[i] = null;
		}
	}

	public static SaveGameInfo Save(string path, string userString, string newFileName)
	{
		SaveGameInfo saveGameInfo = new SaveGameInfo();
		string text = string.Empty;
		if (GameState.Instance.CurrentMap != null)
		{
			text = GameState.Instance.CurrentMap.DisplayName.GetText();
			saveGameInfo.SceneTitleId = GameState.Instance.CurrentMap.DisplayName.StringID;
		}
		saveGameInfo.PlayerName = CharacterStats.Name(GameState.s_playerCharacter.gameObject);
		saveGameInfo.MapName = GameState.ApplicationLoadedLevelName;
		if (string.IsNullOrEmpty(text))
		{
			text = saveGameInfo.MapName;
		}
		saveGameInfo.SceneTitle = text;
		if (GlobalVariables.Instance.IsValid("n_Current_Act"))
		{
			saveGameInfo.Chapter = GlobalVariables.Instance.GetVariable("n_Current_Act");
		}
		EternityTimeInterval eternityTimeInterval = WorldTime.Instance.CurrentTime - WorldTime.Instance.AdventureStart;
		saveGameInfo.PlaytimeSeconds = eternityTimeInterval.TotalSeconds();
		saveGameInfo.RealtimePlayDurationSeconds = Mathf.RoundToInt(WorldTime.Instance.RealWorldPlayTime);
		saveGameInfo.TrialOfIron = GameState.Mode.TrialOfIron;
		if (!string.IsNullOrEmpty(userString))
		{
			saveGameInfo.UserSaveName = userString;
		}
		saveGameInfo.RealTimestamp = DateTime.Now;
		saveGameInfo.SessionID = GameState.s_playerCharacter.SessionID;
		saveGameInfo.GameComplete = GameState.GameComplete;
		saveGameInfo.Difficulty = GameState.Instance.Difficulty;
		saveGameInfo.ActivePackages = ProductConfiguration.ActivePackage;
		saveGameInfo.ActivePackages &= ~ProductConfiguration.Package.Expansion4;
		saveGameInfo.ActivePackages &= ~ProductConfiguration.Package.RoyalEdition;
		SerializeSaveGameInfo(saveGameInfo, path);
		string text2 = Path.Combine(Application.temporaryCachePath, "screenshot.png");
		string text3 = Path.Combine(path, "screenshot.png");
		if (File.Exists(text2))
		{
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			saveGameInfo.ScreenshotRawData = File.ReadAllBytes(text2);
			File.Move(text2, text3);
		}
		for (int i = 0; i < 30; i++)
		{
			string path2 = Path.Combine(path, i + ".png");
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
		}
		int num = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember == null)
			{
				continue;
			}
			try
			{
				string path3 = Path.Combine(path, onlyPrimaryPartyMember.Slot + ".png");
				Portrait component = onlyPrimaryPartyMember.GetComponent<Portrait>();
				if (component != null && component.TextureSmall != null)
				{
					Texture2D texture2D = GameUtilities.ResizeTexture(component.TextureSmall, 32, 41);
					byte[] array = texture2D.EncodeToPNG();
					saveGameInfo.PartyPortraitsRawData[num++] = array;
					File.WriteAllBytes(path3, array);
					GameUtilities.Destroy(texture2D);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Debug.LogError("Error saving '" + onlyPrimaryPartyMember.name + "' portrait.");
			}
		}
		if (!saveGameInfo.GameComplete)
		{
			CachedSaveGameInfo.Add(saveGameInfo);
		}
		lock (s_savingLock)
		{
			s_saveGameFileName = newFileName;
			s_saveGameInfo = saveGameInfo;
		}
		s_savingThread = new Thread(WriteSaveFileToDiskThread);
		s_savingThread.IsBackground = false;
		s_savingThread.Start();
		return saveGameInfo;
	}

	public static void WriteSaveFileToDiskThread()
	{
		lock (s_savingLock)
		{
			using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
			{
				zipFile.AddDirectory(PersistenceManager.s_tempSavePath);
				zipFile.Save(Path.Combine(GameResources.SaveGamePath, s_saveGameFileName));
				s_saveGameInfo.FileName = s_saveGameFileName;
			}
			s_saveGameInfo = null;
			s_saveGameFileName = "";
		}
	}

	public static bool SerializeSaveGameInfo(SaveGameInfo info, string path)
	{
		SharpSerializer textXMLSerializer = GameResources.GetTextXMLSerializer();
		string text = Path.Combine(path, SAVE_FILENAME);
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		textXMLSerializer.Serialize(info, text);
		return true;
	}

	public static SaveGameInfo Load(string filePath, SizeStyle saveGameInfoStyle)
	{
		SharpSerializer textXMLSerializer = GameResources.GetTextXMLSerializer();
		SaveGameInfo saveGameInfo = null;
		try
		{
			saveGameInfo = textXMLSerializer.Deserialize(Path.Combine(filePath, SAVE_FILENAME)) as SaveGameInfo;
		}
		catch (Exception ex)
		{
			Debug.Log("TextXML Parse Failed. Attempting binary... File: " + filePath + " Reason: " + ex.ToString());
		}
		if (saveGameInfo == null)
		{
			textXMLSerializer = new SharpSerializer(binarySerialization: true);
			saveGameInfo = textXMLSerializer.Deserialize(Path.Combine(filePath, SAVE_FILENAME)) as SaveGameInfo;
			if (saveGameInfo == null)
			{
				return null;
			}
		}
		if (saveGameInfoStyle == SizeStyle.DataOnly)
		{
			return saveGameInfo;
		}
		string path = Path.Combine(filePath, "screenshot.png");
		if (File.Exists(path))
		{
			saveGameInfo.ScreenshotRawData = File.ReadAllBytes(path);
		}
		for (int i = 0; i < 30; i++)
		{
			string path2 = Path.Combine(filePath, i + ".png");
			if (File.Exists(path2))
			{
				saveGameInfo.PartyPortraitsRawData[i] = File.ReadAllBytes(path2);
			}
		}
		return saveGameInfo;
	}

	public static string GetSaveFileName()
	{
		string text = "session";
		string text2 = "worldtime";
		if ((bool)GameState.s_playerCharacter)
		{
			text = GameState.s_playerCharacter.SessionID.ToString();
		}
		else
		{
			Debug.LogError("GameState.s_playerCharacter is NULL when attempting to save.");
		}
		if ((bool)WorldTime.Instance)
		{
			text2 = WorldTime.Instance.CurrentTime.TotalSeconds.ToString();
		}
		else
		{
			Debug.LogError("WorldTime.Instance is NULL when attempting to save.");
		}
		string text3 = text + " " + text2 + " ";
		if ((bool)GameState.Instance && GameState.Instance.CurrentMap != null)
		{
			text3 += GameState.Instance.CurrentMap.DisplayName.GetText().Replace(" ", "");
		}
		text3 = Regex.Replace(text3, "[^A-Za-z0-9 ]", "");
		return text3 + ".savegame";
	}

	public static string GetOldAutosaveFileName()
	{
		return GetSpecialSaveFileName("autosave");
	}

	public static string GetAutosaveFileName()
	{
		return GetSpecialSaveFileName("autosave_" + GameState.Instance.AutosaveCycleNumber);
	}

	public static string GetLastAutosaveFileName()
	{
		int num = GameState.Instance.AutosaveCycleNumber - 1;
		if (num < 0)
		{
			num += GameState.Option.MaxAutosaves;
		}
		return GetSpecialSaveFileName("autosave_" + num);
	}

	public static string GetQuicksaveFileName()
	{
		return GetSpecialSaveFileName("quicksave");
	}

	public static string GetPointOfNoReturnSaveFileName()
	{
		return GetSpecialSaveFileName("noreturnsave");
	}

	public static string GetGameCompleteSaveFileName()
	{
		return GetSpecialSaveFileName("gamecomplete");
	}

	private static string GetSpecialSaveFileName(string saveType)
	{
		string text;
		if ((bool)GameState.s_playerCharacter)
		{
			text = GameState.s_playerCharacter.SessionID.ToString();
		}
		else
		{
			Debug.LogError("GameState.s_playerCharacter is NULL when attempting to save (" + saveType + ").");
			text = "NULL";
		}
		return text + " " + saveType + ".savegame";
	}
}

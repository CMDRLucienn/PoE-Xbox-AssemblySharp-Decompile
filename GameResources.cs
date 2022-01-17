using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using OEICommon;
using Polenter.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameResources
{
	public class ConversationVOAssetRefCount
	{
		public VOAsset[] VOAssets;

		public int RefCount;
	}

	public class AssetBundleRefCount
	{
		public AssetBundle Bundle;

		public int RefCount;
	}

	public delegate void LoadedSave();

	public static Type[] SupportedTypes = new Type[2]
	{
		typeof(Recipe),
		typeof(ItemMod)
	};

	public static string BasePath = "assetbundles";

	public static string DataPath = BasePath + "/prefabs";

	public static string ObjectBundlePath = BasePath + "/prefabs/objectbundle";

	public static string HDTextureBundlePath = BasePath + "/art/hd";

	public static string VOPath = BasePath + "/vo";

	public static string PreloadPath = "resources/preload";

	public static string OverridePath = "assetbundles/override";

	public static string AssetTableResourcesPath = "resources/data/assetbundletable/prefabassettable.txt";

	public static string ScenePath = OverridePath + "/scene";

	public static string PX1Path = BasePath + "/px1.unity3d";

	public static string PX2Path = BasePath + "/px2.unity3d";

	public static string PX4Path = BasePath + "/px4.unity3d";

	private const string VOAssetFolderPath = "Audio/Vocalization/VO Assets";

	public UnityEngine.Object obj;

	private static string s_lastFailedPath = string.Empty;

	private static bool s_loading = false;

	private static AssetBundle s_currentScene = null;

	private static Dictionary<string, ConversationVOAssetRefCount> s_loadedDialogAudio = new Dictionary<string, ConversationVOAssetRefCount>();

	private static Dictionary<string, WeakReference> s_loadedPrefabs = new Dictionary<string, WeakReference>(15000);

	private static Dictionary<string, AssetBundleRequest> s_asyncPrefabs = new Dictionary<string, AssetBundleRequest>(100);

	private static Dictionary<string, AssetBundle> s_asyncAssetBundles = new Dictionary<string, AssetBundle>(100);

	private static string sPersistentDataPath = "";

	private static List<string> s_clearPrefabList = new List<string>();

	private static string sTemporaryCachePath = "";

	public static string PersistentDataPath
	{
		get
		{
			if (string.IsNullOrEmpty(sPersistentDataPath))
			{
				sPersistentDataPath = Application.persistentDataPath;
			}
			return sPersistentDataPath;
		}
	}

	public static string SaveGamePath
	{
		get
		{
			string text = PersistentDataPath;
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				text = ((!GamePassManager.Initialized) ? Path.Combine(WindowsPathHelper.GetSaveGameDirectory(), "Pillars of Eternity/_LOCAL_") : Path.Combine(path2: (GamePassManager.Instance.UserID == 0L) ? "Pillars of Eternity/_LOCAL_" : ("Pillars of Eternity/" + GamePassManager.Instance.UserID), path1: WindowsPathHelper.GetSaveGameDirectory()));
			}
			else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			{
				text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library/Application Support/Pillars of Eternity/Saved Games");
			}
			else if (Application.platform == RuntimePlatform.LinuxPlayer)
			{
				string text2 = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrEmpty(text2))
				{
					text2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				}
				text = Path.Combine(text2, "PillarsOfEternity/SavedGames");
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}
	}

	public static string TemporaryCachePath
	{
		get
		{
			if (string.IsNullOrEmpty(sTemporaryCachePath))
			{
				sTemporaryCachePath = Application.temporaryCachePath;
			}
			return sTemporaryCachePath;
		}
	}

	public static event LoadedSave OnPreSaveGame;

	public static event LoadedSave OnLoadedSave;

	public static event LoadedSave OnPreloadGame;

	public static Type GetSupportedType<T>()
	{
		return GetSupportedType(typeof(T));
	}

	public static Type GetSupportedType(Type testType)
	{
		Type type = testType;
		while (type != null)
		{
			Type[] supportedTypes = SupportedTypes;
			for (int i = 0; i < supportedTypes.Length; i++)
			{
				if (supportedTypes[i] == type)
				{
					return type;
				}
			}
			type = type.BaseType;
		}
		return null;
	}

	public static void ClearPrefabReferences(Type type)
	{
		foreach (KeyValuePair<string, WeakReference> s_loadedPrefab in s_loadedPrefabs)
		{
			if (s_loadedPrefab.Value != null)
			{
				UnityEngine.Object @object = s_loadedPrefab.Value.Target as UnityEngine.Object;
				if (@object != null && @object.GetType() == type)
				{
					s_clearPrefabList.Add(s_loadedPrefab.Key);
				}
			}
			else
			{
				s_clearPrefabList.Add(s_loadedPrefab.Key);
			}
		}
		foreach (string s_clearPrefab in s_clearPrefabList)
		{
			s_loadedPrefabs.Remove(s_clearPrefab);
		}
		s_clearPrefabList.Clear();
	}

	public static void ClearPrefabReference(string loadedPrefab)
	{
		foreach (KeyValuePair<string, WeakReference> s_loadedPrefab in s_loadedPrefabs)
		{
			if (s_loadedPrefab.Key == loadedPrefab)
			{
				s_loadedPrefabs.Remove(loadedPrefab);
				break;
			}
		}
	}

	public static void ClearPrefabReference(UnityEngine.Object loadedPrefab)
	{
		foreach (KeyValuePair<string, WeakReference> s_loadedPrefab in s_loadedPrefabs)
		{
			if (s_loadedPrefab.Value != null)
			{
				UnityEngine.Object @object = s_loadedPrefab.Value.Target as UnityEngine.Object;
				if (@object != null && @object == loadedPrefab)
				{
					s_clearPrefabList.Add(s_loadedPrefab.Key);
				}
			}
			else
			{
				s_clearPrefabList.Add(s_loadedPrefab.Key);
			}
		}
		foreach (string s_clearPrefab in s_clearPrefabList)
		{
			s_loadedPrefabs.Remove(s_clearPrefab);
		}
		s_clearPrefabList.Clear();
	}

	public static void ClearPrefabReferences()
	{
		s_loadedPrefabs.Clear();
		Caching.ClearCache();
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	public static void TakeScreenShot()
	{
		int num = 84;
		int num2 = 150;
		string path = Path.Combine(TemporaryCachePath, "screenshot.png");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		List<Camera> list = new List<Camera>();
		list.AddRange(Camera.main.GetComponentsInChildren<Camera>(includeInactive: true));
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			if (list[num3].name.Contains("GammaCamera"))
			{
				list.RemoveAt(num3);
			}
		}
		list.Sort(PE_GameRender.CameraDepthComparer.Instance);
		RenderTexture renderTexture = new RenderTexture(num2, num, 24);
		Texture2D texture2D = new Texture2D(num2, num, TextureFormat.RGB24, mipChain: false);
		foreach (Camera item in list)
		{
			if (item.targetTexture == null && item.name != "Camera_DynamicNormals" && item.name != "Camera_DynamicDepth" && item.name != "Camera_DynamicAlbedo")
			{
				item.targetTexture = renderTexture;
			}
		}
		Shader.SetGlobalInt("_FlipYForRenderTextureUV", 1);
		foreach (Camera item2 in list)
		{
			if (item2.gameObject.activeInHierarchy && item2.enabled && !(item2.name == "GUI") && !(item2.name == "InGameUIPass") && !(item2.name == "PostProcess") && !item2.name.Contains("WatcherFatigueCamera") && !item2.name.Contains("SoulMemoryCamera") && !item2.name.Contains("GammaCamera"))
			{
				item2.Render();
			}
		}
		Shader.SetGlobalInt("_FlipYForRenderTextureUV", 0);
		foreach (Camera item3 in list)
		{
			if (item3.targetTexture == renderTexture)
			{
				item3.targetTexture = null;
				item3.ResetAspect();
			}
		}
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num2, num), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		renderTexture.DiscardContents(discardColor: true, discardDepth: true);
		GameUtilities.Destroy(renderTexture);
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(path, bytes);
		GameUtilities.Destroy(texture2D);
	}

	private static void LoadLastSaveOnFadeEnd()
	{
		GameInput.DisableInput = false;
		UICamera.DisableSelectionInput = false;
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadLastSaveOnFadeEnd));
		bool flag = false;
		while (!flag)
		{
			IEnumerable<SaveGameInfo> enumerable = SaveGameInfo.CachedSaveGameInfo.OrderByDescending((SaveGameInfo sgi) => sgi?.RealTimestamp ?? DateTime.Now);
			if (enumerable == null || !enumerable.Any())
			{
				break;
			}
			flag = LoadGame(enumerable.First().FileName);
		}
		if (!flag)
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
		}
	}

	public static void LoadLastGame(bool fadeOut)
	{
		if (fadeOut)
		{
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.MusicAndFx);
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadLastSaveOnFadeEnd));
			GameInput.DisableInput = true;
			UICamera.DisableSelectionInput = true;
			return;
		}
		bool flag = false;
		while (!flag)
		{
			IEnumerable<SaveGameInfo> enumerable = SaveGameInfo.CachedSaveGameInfo.OrderByDescending((SaveGameInfo sgi) => sgi?.RealTimestamp ?? DateTime.Now);
			if (enumerable == null || !enumerable.Any())
			{
				break;
			}
			LoadGame(enumerable.First().FileName);
		}
		if (!flag)
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
		}
	}

	public static bool SaveGameExists()
	{
		return SaveGameInfo.SaveGameExists();
	}

	public static bool SaveGameExists(string filename)
	{
		return SaveGameInfo.CachedSaveGameInfo.Find((SaveGameInfo sv) => sv.FileName == filename) != null;
	}

	public static void WriteTrialOfIronReadme()
	{
		string path = Path.Combine(SaveGamePath, "_trial_of_iron_read_me.txt");
		if (!File.Exists(path) && (bool)GameState.Instance && (bool)GameState.Instance.TrialOfIronReadme)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				binaryWriter.Write(GameState.Instance.TrialOfIronReadme.bytes);
			}
		}
	}

	public static bool SaveGame(string filename)
	{
		return SaveGame(filename, string.Empty);
	}

	public static bool SaveGame(string filename, string userString)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		if (GameState.InCombat)
		{
			return false;
		}
		if (GameState.IsLoading)
		{
			return false;
		}
		if (GameResources.OnPreSaveGame != null)
		{
			GameResources.OnPreSaveGame();
		}
		if ((bool)QuestManager.Instance)
		{
			QuestManager.Instance.Update();
		}
		string temporaryCachePath = TemporaryCachePath;
		if (!Directory.Exists(temporaryCachePath))
		{
			Directory.CreateDirectory(temporaryCachePath);
		}
		try
		{
			TakeScreenShot();
		}
		catch (Exception)
		{
		}
		try
		{
			FogOfWar.Save();
		}
		catch (Exception)
		{
		}
		BuildSaveFile(filename, userString);
		GameState.LoadedFileName = filename;
		return true;
	}

	public static bool LoadGame(string filename)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		if (!File.Exists(Path.Combine(SaveGamePath, filename)))
		{
			return false;
		}
		if (GameState.CanTrialOfIronQuitSave())
		{
			GameState.TrialOfIronSave();
		}
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.StopAllMusic();
			MusicManager.Instance.UnloadMusic();
		}
		if ((bool)TimeController.Instance)
		{
			TimeController.Instance.Paused = false;
		}
		if (GameResources.OnPreloadGame != null)
		{
			GameResources.OnPreloadGame();
		}
		for (int num = PartyMemberAI.PartyMembers.Length - 1; num >= 0; num--)
		{
			if (!(PartyMemberAI.PartyMembers[num] == null))
			{
				if (PartyMemberAI.PartyMembers[num].StateManager != null)
				{
					PartyMemberAI.PartyMembers[num].StateManager.AbortStateStack();
				}
				GameState.DestroyTrackedObjectImmediate(PartyMemberAI.PartyMembers[num].gameObject);
			}
		}
		if ((bool)GameState.Stronghold)
		{
			GameState.Stronghold.DestroyStoredCompanions();
		}
		GameState.CleanupPersistAcrossSceneLoadObjectsOfType(typeof(AIController));
		GameState.LoadedGame = true;
		GameState.LoadedFileName = filename;
		GameInput.HandleAllClicks();
		SaveGameInfo saveGameInfo = LoadSaveFile(filename, SaveGameInfo.SizeStyle.DataOnly);
		if (saveGameInfo == null)
		{
			return false;
		}
		GameState.BeginLevelUnload(saveGameInfo.MapName);
		GameState.Instance.Difficulty = saveGameInfo.Difficulty;
		GameState.LoadLevel(saveGameInfo.MapName);
		if (GameResources.OnLoadedSave != null)
		{
			GameResources.OnLoadedSave();
		}
		return true;
	}

	public static void DeleteSavedGame(string filename)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		try
		{
			SaveGameInfo saveGameInfo = SaveGameInfo.CachedSaveGameInfo.Find((SaveGameInfo sv) => sv != null && sv.FileName == filename);
			if (saveGameInfo != null)
			{
				SaveGameInfo.CachedSaveGameInfo.Remove(saveGameInfo);
				saveGameInfo.Cleanup();
			}
			if (!filename.EndsWith(".savegame"))
			{
				filename += ".savegame";
			}
			File.Delete(Path.Combine(SaveGamePath, filename));
		}
		catch (Exception message)
		{
			Debug.Log("Error while delete save file:");
			Debug.Log(message);
		}
	}

	public static VOAsset[] LoadDialogueAudio(string dialogFilename)
	{
		if (dialogFilename == null || dialogFilename == "")
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dialogFilename);
		string text = "Audio/Vocalization/VO Assets/" + fileNameWithoutExtension;
		if (s_loadedDialogAudio.ContainsKey(fileNameWithoutExtension))
		{
			ConversationVOAssetRefCount conversationVOAssetRefCount = s_loadedDialogAudio[fileNameWithoutExtension];
			conversationVOAssetRefCount.RefCount++;
			return conversationVOAssetRefCount.VOAssets;
		}
		VOAsset[] array = Resources.LoadAll<VOAsset>(text);
		if (array == null)
		{
			Debug.LogError("Failed to load VO assets from path \"" + text + "\"");
			return null;
		}
		ConversationVOAssetRefCount conversationVOAssetRefCount2 = new ConversationVOAssetRefCount();
		conversationVOAssetRefCount2.VOAssets = array;
		conversationVOAssetRefCount2.RefCount++;
		s_loadedDialogAudio.Add(fileNameWithoutExtension, conversationVOAssetRefCount2);
		return conversationVOAssetRefCount2.VOAssets;
	}

	public static void UnloadDialogueAudio(string dialogFilename)
	{
		if (dialogFilename == null || dialogFilename == "")
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dialogFilename);
		if (!s_loadedDialogAudio.ContainsKey(fileNameWithoutExtension))
		{
			return;
		}
		ConversationVOAssetRefCount conversationVOAssetRefCount = s_loadedDialogAudio[fileNameWithoutExtension];
		conversationVOAssetRefCount.RefCount--;
		if (conversationVOAssetRefCount.RefCount > 0)
		{
			return;
		}
		VOAsset[] vOAssets = s_loadedDialogAudio[fileNameWithoutExtension].VOAssets;
		if (vOAssets != null)
		{
			for (int i = 0; i < vOAssets.Length; i++)
			{
				Resources.UnloadAsset(vOAssets[i]);
			}
		}
		else
		{
			Debug.LogWarning("Conversation VOAssets were null when unloading for conversation \"" + fileNameWithoutExtension + "\"");
		}
		s_loadedDialogAudio.Remove(fileNameWithoutExtension);
	}

	public static VOAsset GetDialogueAudio(string dialogName, int node, bool useFemaleVersion)
	{
		if (dialogName == null || dialogName == "")
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dialogName);
		string text = fileNameWithoutExtension + "_" + node.ToString("0000").ToLowerInvariant();
		string text2 = "Audio/Vocalization/VO Assets/" + fileNameWithoutExtension + "/" + text;
		if (useFemaleVersion && StringTableManager.CurrentLanguage.EnumLanguage == OEICommon.Language.english)
		{
			text2 += "_fem";
			text += "_fem";
		}
		VOAsset result = null;
		if (s_loadedDialogAudio.ContainsKey(fileNameWithoutExtension))
		{
			VOAsset[] vOAssets = s_loadedDialogAudio[fileNameWithoutExtension].VOAssets;
			for (int i = 0; i < vOAssets.Length; i++)
			{
				if (vOAssets[i].name.ToLowerInvariant() == text)
				{
					result = vOAssets[i];
					break;
				}
			}
		}
		return result;
	}

	public static bool ShouldUseFemaleVersion(string dialogueName, int node)
	{
		if (StringTableManager.PlayerGender != Gender.Female)
		{
			return false;
		}
		return StringTableManager.FemaleVersionExists(dialogueName, node);
	}

	public static void BuildSaveFile(string name, string userString)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		string fname = name;
		if (!fname.EndsWith(".savegame"))
		{
			fname += ".savegame";
		}
		SaveGameInfo saveGameInfo = SaveGameInfo.CachedSaveGameInfo.Find((SaveGameInfo sv) => sv.FileName == fname);
		if (saveGameInfo != null)
		{
			SaveGameInfo.CachedSaveGameInfo.Remove(saveGameInfo);
			saveGameInfo.Cleanup();
		}
		PersistenceManager.SaveGame();
		SaveGameInfo.Save(PersistenceManager.s_tempSavePath, userString, fname);
	}

	public static void UpdateUserStringSaveFile(SaveGameInfo saveInfoToUpdate, string newUserString)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		saveInfoToUpdate.UserSaveName = newUserString;
		string text = Path.Combine(SaveGamePath, saveInfoToUpdate.FileName);
		if (!File.Exists(text))
		{
			return;
		}
		try
		{
			string text2 = Path.Combine(TemporaryCachePath, "loadedSave.zip");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Copy(text, text2);
			string text3 = Path.Combine(sTemporaryCachePath, "TempSaveData/");
			using ZipFile zipFile = new ZipFile(text2, Encoding.UTF8);
			zipFile.ExtractSelectedEntries(SaveGameInfo.SAVE_FILENAME, "", text3, ExtractExistingFileAction.OverwriteSilently);
			SaveGameInfo saveGameInfo = SaveGameInfo.Load(text3, SaveGameInfo.SizeStyle.DataOnly);
			saveGameInfo.UserSaveName = newUserString;
			SaveGameInfo.SerializeSaveGameInfo(saveGameInfo, text3);
			zipFile.UpdateFile(Path.Combine(text3, SaveGameInfo.SAVE_FILENAME), "");
			if (saveInfoToUpdate.IsAutoSave() || saveInfoToUpdate.IsBug() || saveInfoToUpdate.IsQuickSave())
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				saveInfoToUpdate.RemoveFileNameFlags();
			}
			zipFile.Save(Path.Combine(SaveGamePath, saveInfoToUpdate.FileName));
		}
		catch (Exception ex)
		{
			Debug.Log(string.Concat("Error updating save game file: ", saveInfoToUpdate, " Reason: ", ex));
		}
	}

	public static SaveGameInfo LoadSaveFile(string filename, SaveGameInfo.SizeStyle sizeStyle)
	{
		SaveGameInfo.WaitUntilSafeToSaveLoad();
		string text = Path.Combine(SaveGamePath, filename);
		if (!File.Exists(text))
		{
			return null;
		}
		PersistenceManager.ClearTempData();
		try
		{
			string text2 = Path.Combine(TemporaryCachePath, "loadedSave.zip");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Copy(text, text2);
			using ZipFile zipFile = new ZipFile(text2, Encoding.UTF8);
			zipFile.ExtractAll(PersistenceManager.s_tempSavePath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError(text + " ZIP FILE IS CORRUPT!");
			return null;
		}
		SaveGameInfo saveGameInfo = SaveGameInfo.Load(PersistenceManager.s_tempSavePath, sizeStyle);
		saveGameInfo.FileName = Path.GetFileName(filename);
		if (string.IsNullOrEmpty(saveGameInfo.MapName))
		{
			for (int i = 0; i < WorldMap.Maps.Length; i++)
			{
				if (WorldMap.Maps[i].DisplayName.StringID == saveGameInfo.SceneTitleId)
				{
					saveGameInfo.MapName = WorldMap.Maps[i].SceneName;
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(saveGameInfo.MapName))
		{
			throw new Exception("Could not determine what map to load from SaveGameInfo.");
		}
		return saveGameInfo;
	}

	public static bool SaveFileExists(string saveName)
	{
		return File.Exists(Path.Combine(SaveGamePath, saveName));
	}

	public static SaveGameInfo[] GetSaveFileInfo(SaveGameInfo.SizeStyle sizeStyle, ref bool abortSignal)
	{
		string[] files = Directory.GetFiles(SaveGamePath, "*.savegame");
		List<SaveGameInfo> list = new List<SaveGameInfo>();
		string text = Path.Combine(TemporaryCachePath, "TempSaveData");
		string text2 = Path.Combine(TemporaryCachePath, "loadedSave.zip");
		string[] array = files;
		foreach (string text3 in array)
		{
			if (abortSignal)
			{
				break;
			}
			string text4 = Path.Combine(SaveGamePath, text3);
			if (Directory.Exists(text))
			{
				FileUtility.DeleteDirectory(text, block: true);
			}
			Directory.CreateDirectory(text);
			try
			{
				if (File.Exists(text2))
				{
					File.SetAttributes(text2, FileAttributes.Normal);
					File.Delete(text2);
				}
				File.Copy(text4, text2);
				using ZipFile zipFile = new ZipFile(text2, Encoding.UTF8);
				zipFile.ExtractSelectedEntries("name = saveinfo.xml OR name = *.png", null, text);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Debug.LogError("ZIP FILE WAS CORRUPT! " + text4);
				continue;
			}
			try
			{
				SaveGameInfo saveGameInfo = SaveGameInfo.Load(text, sizeStyle);
				saveGameInfo.FileName = Path.GetFileName(text3);
				if (saveGameInfo != null)
				{
					list.Add(saveGameInfo);
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				Debug.LogError("Error deserializing save game. Skipping.");
			}
		}
		return list.ToArray();
	}

	public static string GetBundlePath(Type objType)
	{
		if (objType == typeof(UnityEngine.Object))
		{
			string text = Path.Combine(Application.dataPath, ObjectBundlePath);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}
		bool flag = false;
		for (int i = 0; i < SupportedTypes.Length; i++)
		{
			if (SupportedTypes[i] == objType)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogError("Cannot find bundle type " + objType.ToString());
			return string.Empty;
		}
		string text2 = string.Concat(objType);
		text2 = text2.ToLower();
		string text3 = Path.Combine(Application.dataPath, DataPath);
		string path = Path.Combine(Application.dataPath, OverridePath);
		if (!Directory.Exists(text3))
		{
			Directory.CreateDirectory(text3);
		}
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] directories = Directory.GetDirectories(Path.Combine(Application.dataPath, BasePath));
		int num = 0;
		string path2 = string.Empty;
		string[] array = directories;
		foreach (string text4 in array)
		{
			if (text4.Contains("override_") && File.Exists(Path.Combine(text4, text2)))
			{
				int val = 0;
				if (IntUtils.TryParseInvariant(text4.Substring(text4.Length - 2, 2), out val) && val > num)
				{
					num = val;
					path2 = text4;
				}
			}
		}
		string text5 = Path.Combine(path2, text2);
		if (File.Exists(text5))
		{
			return text5;
		}
		return Path.Combine(text3, text2);
	}

	public static string GetOverridePath(string relativePath, string filename)
	{
		string result = string.Empty;
		string[] directories = Directory.GetDirectories(Path.Combine(Application.dataPath, BasePath));
		int num = 0;
		string[] array = directories;
		foreach (string text in array)
		{
			if (text.Contains("override_") && SearchForFile(text + relativePath, filename, out var path))
			{
				int val = 0;
				if (IntUtils.TryParseInvariant(text.Substring(text.Length - 2, 2), out val) && val > num)
				{
					num = val;
					result = path;
				}
			}
		}
		if (num == 0)
		{
			string path2 = OverridePath + relativePath;
			path2 = Path.Combine(path2, Path.GetFileName(filename));
			if (File.Exists(path2))
			{
				return path2;
			}
			return filename;
		}
		return result;
	}

	public static string GetOverridePath(string filename)
	{
		filename = filename.ToLower();
		string path = Path.Combine(Application.dataPath, BasePath);
		if (!Directory.Exists(path))
		{
			return string.Empty;
		}
		string[] directories = Directory.GetDirectories(path);
		int num = 0;
		string result = string.Empty;
		string[] array = directories;
		foreach (string text in array)
		{
			if (text.Contains("override_") && SearchForFile(text, filename, out var path2))
			{
				int val = 0;
				if (IntUtils.TryParseInvariant(text.Substring(text.Length - 2, 2), out val) && val > num)
				{
					num = val;
					result = path2;
				}
			}
		}
		if (num == 0 && SearchForFile(OverridePath, filename, out var path3))
		{
			result = Path.Combine(OverridePath, path3);
		}
		return result;
	}

	public static void Cleanup()
	{
		if (s_currentScene != null)
		{
			s_currentScene.Unload(unloadAllLoadedObjects: true);
		}
		s_currentScene = null;
	}

	public static IEnumerator LoadScene(string name, bool loadFromSaveFile)
	{
		if (s_loading)
		{
			yield break;
		}
		yield return null;
		yield return null;
		try
		{
			GameUtilities.CreateInGameGlobalPrefabObject();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			GameState.ReturnToMainMenuFromError();
			yield break;
		}
		yield return null;
		if (loadFromSaveFile)
		{
			try
			{
				string loadedFileName = GameState.LoadedFileName;
				GameState.Instance.Reset(GameState.ResetStyle.LoadedGame);
				GameState.LoadedGame = true;
				GameState.IsRestoredLevel = true;
				GameState.IsLoading = true;
				GameState.LoadedFileName = loadedFileName;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				GameState.ReturnToMainMenuFromError();
				yield break;
			}
		}
		try
		{
			if (s_currentScene != null)
			{
				s_currentScene.Unload(unloadAllLoadedObjects: true);
			}
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
			GameState.ReturnToMainMenuFromError();
			yield break;
		}
		s_loading = true;
		try
		{
			Debug.Log("-------- TRANSITION TO EMPTY --------");
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0f, AudioFadeMode.Fx);
			SceneManager.LoadScene("oei_scene_transition");
			Resources.UnloadUnusedAssets();
			GC.Collect();
			Debug.Log("-------- TRANSITION TO EMPTY COMPLETE --------");
		}
		catch (Exception exception4)
		{
			s_loading = false;
			Debug.LogException(exception4);
			GameState.ReturnToMainMenuFromError();
			yield break;
		}
		yield return null;
		string overridePath = GetOverridePath(name);
		if (!string.IsNullOrEmpty(overridePath) && File.Exists(overridePath) && !AssetBundle.LoadFromFile(overridePath))
		{
			string uri = TextUtils.LiteEscapeUrl("file://" + overridePath);
			using UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri);
			yield return assetBundleRequest.SendWebRequest();
			if (assetBundleRequest.isNetworkError || assetBundleRequest.isHttpError)
			{
				Debug.LogError(assetBundleRequest.error);
				GameState.ReturnToMainMenuFromError();
				yield break;
			}
			s_currentScene = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);
		}
		try
		{
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0f, AudioFadeMode.Fx);
			if (GameUtilities.StreamedSceneAssetBundle != null)
			{
				GameUtilities.StreamedSceneAssetBundle.Unload(unloadAllLoadedObjects: false);
				GameUtilities.Destroy(GameUtilities.StreamedSceneAssetBundle);
				GameUtilities.StreamedSceneAssetBundle = null;
			}
			if ((name.ToLower().Contains("px1") || name.ToLower().Contains("px2")) && !Application.isEditor)
			{
				string text = Path.GetFileNameWithoutExtension(name.ToLower()) ?? "";
				GameUtilities.StreamedSceneAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, BasePath + Path.DirectorySeparatorChar + text));
				if (GameUtilities.StreamedSceneAssetBundle == null)
				{
					Debug.LogError("Could not load Expansion Asset Bundle.");
					throw new Exception();
				}
			}
			SceneManager.LoadScene(name);
			Debug.Log("-------- LEVEL LOAD COMPLETE --------        Level = " + name + ". \n\n");
			s_loading = false;
		}
		catch (Exception exception5)
		{
			s_loading = false;
			Debug.LogException(exception5);
			GameState.ReturnToMainMenuFromError();
		}
	}

	public static void HandlePreloadItems()
	{
		if (!Directory.Exists(PreloadPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(PreloadPath, "*");
		foreach (string obj in files)
		{
			UnityEngine.Object @object = GameObject.Find(Path.GetFileNameWithoutExtension(obj));
			if (@object != null)
			{
				Resources.UnloadAsset(@object);
			}
			if ((bool)LoadPrefab<UnityEngine.Object>(obj, string.Empty, instantiate: false))
			{
				Debug.Log(@object.ToString() + " loaded");
			}
		}
	}

	public void LoadBundle<T>(string path, bool suppressErrors = false) where T : UnityEngine.Object
	{
		obj = null;
		obj = Resources.Load(path, typeof(T));
		if (obj == null && path != s_lastFailedPath)
		{
			if (!suppressErrors)
			{
				Debug.LogError("Asset not found at path: " + path);
			}
			s_lastFailedPath = path;
		}
	}

	public static T Instantiate<T>(UnityEngine.Object file) where T : UnityEngine.Object
	{
		Transform transform = null;
		if (file is GameObject)
		{
			transform = (file as GameObject).transform;
		}
		else if (file is MonoBehaviour)
		{
			transform = (file as MonoBehaviour).transform;
		}
		if (transform != null)
		{
			return Instantiate<T>(file, transform.position, transform.rotation);
		}
		return Instantiate<T>(file, Vector3.zero, Quaternion.identity);
	}

	public static T Instantiate<T>(UnityEngine.Object file, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
	{
		T val = null;
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			val = LoadPrefab<T>(file.name, instantiate: false);
		}
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			val = file as T;
		}
		T val2 = UnityEngine.Object.Instantiate(val, position, rotation);
		GameObject gameObject = null;
		if (val2 is GameObject)
		{
			gameObject = val2 as GameObject;
		}
		else if (val2 is MonoBehaviour)
		{
			gameObject = (val2 as MonoBehaviour).gameObject;
		}
		if (gameObject != null && !gameObject.activeSelf)
		{
			Debug.LogError("GameResources.Instantiate(): Instantiated " + gameObject.name + " from " + file.name + ", which is marked Disabled. This will cause duplicate GUID issues!");
		}
		if (file != null)
		{
			if (file is GameObject)
			{
				if (InstanceID.ObjectIsActive(file as GameObject))
				{
					InstanceID component = (val2 as GameObject).GetComponent<InstanceID>();
					if (!(component is CompanionInstanceID))
					{
						component.Guid = Guid.NewGuid();
					}
				}
			}
			else if (file is MonoBehaviour && InstanceID.ObjectIsActive((file as MonoBehaviour).gameObject))
			{
				InstanceID component2 = (val2 as MonoBehaviour).GetComponent<InstanceID>();
				if (!(component2 is CompanionInstanceID))
				{
					component2.Guid = Guid.NewGuid();
				}
			}
		}
		return val2;
	}

	public static UnityEngine.Object[] LoadAllPrefabs<T>() where T : UnityEngine.Object
	{
		try
		{
			UnityEngine.Object[] array = null;
			Type supportedType = GetSupportedType<T>();
			string bundlePath = GetBundlePath(supportedType);
			if (string.IsNullOrEmpty(bundlePath))
			{
				Debug.LogError(string.Concat(supportedType, " cannot be loaded because it's type is not in a supported asset bundle!"));
				return null;
			}
			AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
			if (assetBundle == null)
			{
				Debug.LogError("Error trying to load " + bundlePath);
				return null;
			}
			array = assetBundle.LoadAllAssets();
			assetBundle.Unload(unloadAllLoadedObjects: false);
			GameUtilities.Destroy(assetBundle);
			if (array == null)
			{
				Debug.Log(string.Concat("No assets of type ", supportedType, " found in asset bundle. Use the Build -> Export Prefabs button and try again."));
			}
			return array;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError(string.Concat("Error trying to load ", GetSupportedType<T>(), "."));
		}
		return null;
	}

	public static T[] LoadAllPrefabsWithComponent<T>() where T : Component
	{
		try
		{
			GameObject[] array = null;
			Type supportedType = GetSupportedType<T>();
			string bundlePath = GetBundlePath(supportedType);
			if (string.IsNullOrEmpty(bundlePath))
			{
				Debug.LogError(string.Concat(supportedType, " cannot be loaded because it's type is not in a supported asset bundle!"));
				return null;
			}
			AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
			if (assetBundle == null)
			{
				Debug.LogError("Error trying to load " + bundlePath);
				return null;
			}
			array = assetBundle.LoadAllAssets<GameObject>();
			assetBundle.Unload(unloadAllLoadedObjects: false);
			GameUtilities.Destroy(assetBundle);
			if (array == null)
			{
				Debug.Log(string.Concat("No assets of type ", supportedType, " found in asset bundle. Use the Build -> Export Prefabs button and try again."));
			}
			List<T> list = new List<T>();
			for (int i = 0; i < array.Length; i++)
			{
				T component = array[i].GetComponent<T>();
				if ((bool)(UnityEngine.Object)component)
				{
					list.Add(component);
				}
			}
			return list.ToArray();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError(string.Concat("Error trying to load ", GetSupportedType<T>(), "."));
		}
		return null;
	}

	public static UnityEngine.Object LoadPrefab(string prefabPath, bool instantiate)
	{
		string text = Path.Combine(Application.dataPath, ObjectBundlePath);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(prefabPath);
		return LoadPrefab<UnityEngine.Object>(text, fileNameWithoutExtension, instantiate);
	}

	public static T LoadPrefab<T>(string filename, string assetName, bool instantiate) where T : UnityEngine.Object
	{
		assetName = assetName.Replace("(Clone)", "");
		filename = filename.Replace("(Clone)", "");
		filename.Replace(Application.dataPath, "Assets");
		try
		{
			string text = Path.GetFileName(filename);
			if (string.IsNullOrEmpty(text) || !text.Contains(assetName))
			{
				text = assetName.ToLower() ?? "";
				filename = Path.Combine(filename, text);
			}
			else if (!text.Contains(""))
			{
				filename = filename ?? "";
				text = text ?? "";
			}
			UnityEngine.Object @object = null;
			string text2 = filename;
			string text3 = Path.Combine(Path.Combine(Application.dataPath, ObjectBundlePath), text.ToLower());
			if (File.Exists(text3))
			{
				text2 = text3;
			}
			if (string.IsNullOrEmpty(text2))
			{
				Debug.LogError(assetName + " cannot be loaded because its type is not in a supported asset bundle!");
				return null;
			}
			string text4 = Path.GetFileNameWithoutExtension(text2).ToLower();
			AssetBundle assetBundle = null;
			T val = null;
			if (s_loadedPrefabs.ContainsKey(text4))
			{
				WeakReference weakReference = s_loadedPrefabs[text4];
				if (weakReference == null)
				{
					s_loadedPrefabs.Remove(text4);
				}
				else if (weakReference.Target as UnityEngine.Object == null)
				{
					s_loadedPrefabs.Remove(text4);
				}
			}
			if (s_loadedPrefabs.ContainsKey(text4))
			{
				val = s_loadedPrefabs[text4].Target as T;
			}
			else
			{
				assetBundle = AssetBundle.LoadFromFile(text2);
				if (assetBundle == null)
				{
					Debug.LogError("Error trying to load " + assetName + " from " + text2);
					return null;
				}
				UnityEngine.Object mainAsset = GetMainAsset(assetBundle, text2);
				val = ((!(mainAsset == null)) ? (mainAsset as T) : (assetBundle.LoadAsset(assetName, typeof(T)) as T));
				PrefabRedirect prefabRedirect = mainAsset as PrefabRedirect;
				if ((bool)prefabRedirect)
				{
					val = LoadPrefab<T>(prefabRedirect.TargetAssetName, instantiate: false);
				}
				WeakReference value = new WeakReference(val);
				s_loadedPrefabs.Add(text4, value);
			}
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				Debug.LogError(assetName + " is missing from the " + text4 + " asset bundle. Use the Build -> Export Prefabs button and try again.");
				if ((bool)assetBundle)
				{
					assetBundle.Unload(unloadAllLoadedObjects: false);
					GameUtilities.Destroy(assetBundle);
				}
				return null;
			}
			@object = ((!instantiate) ? val : Instantiate<T>(val));
			if (@object == null)
			{
				Debug.LogError(assetName + " is missing from the " + text4 + " asset bundle. Use the Build -> Export Prefabs button and try again.");
			}
			val = null;
			if ((bool)assetBundle)
			{
				assetBundle.Unload(unloadAllLoadedObjects: false);
				GameUtilities.Destroy(assetBundle);
			}
			return @object as T;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Error trying to load " + assetName + ".");
		}
		return null;
	}

	public static UnityEngine.Object LoadPrefab(string assetName, Type bundleType, bool instantiate)
	{
		return LoadPrefab(assetName, ObjectBundlePath, bundleType, instantiate);
	}

	public static UnityEngine.Object LoadPrefab(string assetName, string bundlePath, Type bundleType, bool instantiate)
	{
		UnityEngine.Object @object = null;
		assetName = assetName.Replace("(Clone)", "");
		try
		{
			Type supportedType = GetSupportedType(bundleType);
			string text = Path.Combine(Path.Combine(Application.dataPath, bundlePath), assetName.ToLower() ?? "");
			if (!File.Exists(text))
			{
				if (supportedType == null)
				{
					Debug.LogError(assetName + " cannot be loaded because the asset bundle type is not supported!");
					return null;
				}
				text = GetBundlePath(supportedType);
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogError(assetName + " cannot be loaded because it's type is not in a supported asset bundle!");
					return null;
				}
			}
			AssetBundle assetBundle = null;
			UnityEngine.Object object2 = null;
			string text2 = Path.GetFileNameWithoutExtension(text).ToLower();
			if (s_loadedPrefabs.ContainsKey(text2))
			{
				WeakReference weakReference = s_loadedPrefabs[text2];
				if (weakReference == null)
				{
					s_loadedPrefabs.Remove(text2);
				}
				else if (weakReference.Target as UnityEngine.Object == null)
				{
					s_loadedPrefabs.Remove(text2);
				}
			}
			if (s_loadedPrefabs.ContainsKey(text2))
			{
				object2 = s_loadedPrefabs[text2].Target as UnityEngine.Object;
			}
			else
			{
				assetBundle = AssetBundle.LoadFromFile(text);
				UnityEngine.Object mainAsset = GetMainAsset(assetBundle, text);
				if (assetBundle == null)
				{
					Debug.LogError("Error trying to load " + assetName + " from " + text);
					return null;
				}
				if (!string.IsNullOrEmpty(assetName))
				{
					object2 = assetBundle.LoadAsset(assetName, bundleType);
					if (!object2)
					{
						object2 = mainAsset;
					}
				}
				else
				{
					object2 = mainAsset;
				}
				PrefabRedirect prefabRedirect = mainAsset as PrefabRedirect;
				if ((bool)prefabRedirect)
				{
					object2 = LoadPrefab(prefabRedirect.TargetAssetName, bundlePath, bundleType, instantiate: false);
				}
				WeakReference value = new WeakReference(object2);
				s_loadedPrefabs.Add(text2, value);
			}
			if (object2 == null)
			{
				Debug.LogError(assetName + " is missing from the " + text2 + " asset bundle. Use the Build -> Export Prefabs button and try again.");
				if ((bool)assetBundle)
				{
					assetBundle.Unload(unloadAllLoadedObjects: false);
					GameUtilities.Destroy(assetBundle);
				}
				return null;
			}
			@object = ((!instantiate) ? object2 : UnityEngine.Object.Instantiate(object2));
			object2 = null;
			if ((bool)assetBundle)
			{
				assetBundle.Unload(unloadAllLoadedObjects: false);
				GameUtilities.Destroy(assetBundle);
			}
			if (@object == null)
			{
				Debug.LogError(string.Concat(assetName, " is missing from the ", supportedType, " asset bundle. Use the Build -> Export Prefabs button and try again."));
			}
			return @object;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Error trying to load " + assetName + ".");
		}
		return null;
	}

	private static UnityEngine.Object GetMainAsset(AssetBundle bundle, string fullBundlePath)
	{
		if (File.Exists(fullBundlePath + ".mainasset"))
		{
			StreamReader streamReader = new StreamReader(fullBundlePath + ".mainasset");
			UnityEngine.Object result = bundle.LoadAsset(streamReader.ReadLine().ToLower());
			streamReader.Close();
			return result;
		}
		Debug.Log("No .mainasset file for " + fullBundlePath);
		return null;
	}

	private static string GetMainAssetPath(AssetBundle bundle, string fullBundlePath)
	{
		if (File.Exists(fullBundlePath + ".mainasset"))
		{
			StreamReader streamReader = new StreamReader(fullBundlePath + ".mainasset");
			string result = streamReader.ReadLine();
			streamReader.Close();
			return result;
		}
		Debug.Log("No .mainasset file for " + fullBundlePath);
		return null;
	}

	public static T LoadPrefab<T>(string assetName, bool instantiate) where T : UnityEngine.Object
	{
		UnityEngine.Object @object = LoadPrefab(assetName, typeof(T), instantiate);
		if (@object is GameObject && typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
		{
			@object = (@object as GameObject).GetComponent(typeof(T));
		}
		return @object as T;
	}

	public static Texture LoadTextureBundle(string assetName)
	{
		return LoadPrefab(assetName, HDTextureBundlePath, typeof(Texture), instantiate: false) as Texture;
	}

	public static bool IsPrefabLoaded(string assetName)
	{
		string key = Path.GetFileNameWithoutExtension(Path.Combine(Application.dataPath + Path.DirectorySeparatorChar + ObjectBundlePath, assetName.ToLower() ?? "")).ToLower();
		if (!s_loadedPrefabs.ContainsKey(key))
		{
			return false;
		}
		WeakReference weakReference = s_loadedPrefabs[key];
		if (weakReference == null || weakReference.Target as UnityEngine.Object == null)
		{
			s_loadedPrefabs.Remove(key);
			return false;
		}
		return true;
	}

	public static void StopPrefabAsyncRequest(string assetName, AssetBundleRequest request)
	{
		string key = Path.GetFileNameWithoutExtension(Path.Combine(Application.dataPath + Path.DirectorySeparatorChar + ObjectBundlePath, assetName.ToLower() ?? "")).ToLower();
		AssetBundle assetBundle = null;
		if (s_asyncPrefabs.ContainsKey(key))
		{
			s_asyncPrefabs.Remove(key);
			assetBundle = s_asyncAssetBundles[key];
			s_asyncAssetBundles.Remove(key);
			if ((bool)assetBundle)
			{
				assetBundle.Unload(unloadAllLoadedObjects: false);
				GameUtilities.Destroy(assetBundle);
			}
		}
	}

	public static UnityEngine.Object LoadPrefabFromAsyncRequest(string assetName, AssetBundleRequest request, Type bundleType, bool instantiate)
	{
		if (request.isDone)
		{
			string key = Path.GetFileNameWithoutExtension(Path.Combine(Application.dataPath + Path.DirectorySeparatorChar + ObjectBundlePath, assetName.ToLower() ?? "")).ToLower();
			if (s_loadedPrefabs.ContainsKey(key))
			{
				WeakReference weakReference = s_loadedPrefabs[key];
				if (weakReference != null && weakReference.Target as UnityEngine.Object != null)
				{
					return LoadPrefab(assetName, bundleType, instantiate);
				}
			}
			UnityEngine.Object @object = request.asset;
			AssetBundle assetBundle = null;
			if (s_asyncPrefabs.ContainsKey(key))
			{
				s_asyncPrefabs.Remove(key);
				assetBundle = s_asyncAssetBundles[key];
				s_asyncAssetBundles.Remove(key);
				WeakReference value = new WeakReference(@object);
				s_loadedPrefabs.Add(key, value);
				if (instantiate)
				{
					@object = UnityEngine.Object.Instantiate(@object);
				}
				if ((bool)assetBundle)
				{
					assetBundle.Unload(unloadAllLoadedObjects: false);
					GameUtilities.Destroy(assetBundle);
				}
				return @object;
			}
			Debug.LogError(assetName + " not found in async asset bundle list.");
			return LoadPrefab(assetName, bundleType, instantiate);
		}
		return null;
	}

	public static T LoadPrefabFromAsyncRequest<T>(string assetName, AssetBundleRequest request, bool instantiate) where T : UnityEngine.Object
	{
		if (!request.isDone)
		{
			return null;
		}
		UnityEngine.Object @object = LoadPrefabFromAsyncRequest(assetName, request, typeof(T), instantiate);
		if (@object == null)
		{
			return null;
		}
		if (@object is GameObject && typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
		{
			@object = (@object as GameObject).GetComponent(typeof(T));
		}
		return @object as T;
	}

	public static AssetBundleRequest LoadPrefabAsync<T>(string assetName) where T : UnityEngine.Object
	{
		return LoadPrefabAsync(assetName, typeof(T));
	}

	public static AssetBundleRequest LoadPrefabAsync(string assetName, Type bundleType)
	{
		while (assetName.Contains("(Clone)"))
		{
			assetName = assetName.Remove(assetName.IndexOf("(Clone)"), 7);
		}
		try
		{
			string text = Path.Combine(Application.dataPath + Path.DirectorySeparatorChar + ObjectBundlePath, assetName.ToLower() ?? "");
			if (!File.Exists(text))
			{
				Debug.LogError(assetName + " cannot be loaded because it's type is not in a supported asset bundle!");
				return null;
			}
			AssetBundle assetBundle = null;
			AssetBundleRequest assetBundleRequest = null;
			string text2 = Path.GetFileNameWithoutExtension(text).ToLower();
			if (s_asyncPrefabs.ContainsKey(text2) && s_asyncPrefabs[text2] == null)
			{
				Debug.LogError(text2 + " asset bundle was deleted.");
				s_asyncPrefabs.Remove(text2);
				s_asyncAssetBundles.Remove(text2);
			}
			if (s_asyncPrefabs.ContainsKey(text2))
			{
				assetBundleRequest = s_asyncPrefabs[text2];
			}
			else
			{
				assetBundle = AssetBundle.LoadFromFile(text);
				if (assetBundle == null)
				{
					Debug.LogError("Error trying to load " + assetName + " from " + text);
					return null;
				}
				if (!string.IsNullOrEmpty(assetName))
				{
					string mainAssetPath = GetMainAssetPath(assetBundle, text);
					assetBundleRequest = assetBundle.LoadAssetAsync(mainAssetPath);
					s_asyncPrefabs.Add(text2, assetBundleRequest);
					s_asyncAssetBundles.Add(text2, assetBundle);
				}
			}
			if (assetBundleRequest == null)
			{
				Debug.LogError(assetName + " is missing from the " + text2 + " asset bundle. Use the Build -> Export Prefabs button and try again.");
				if ((bool)assetBundle)
				{
					assetBundle.Unload(unloadAllLoadedObjects: true);
					GameUtilities.Destroy(assetBundle);
				}
				return null;
			}
			return assetBundleRequest;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Error trying to load " + assetName + ".");
		}
		return null;
	}

	public static void RemoveNullEntriesFromPrefabCache()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, WeakReference> s_loadedPrefab in s_loadedPrefabs)
		{
			if (s_loadedPrefab.Value != null)
			{
				if (s_loadedPrefab.Value.Target as UnityEngine.Object == null)
				{
					list.Add(s_loadedPrefab.Key);
				}
			}
			else
			{
				list.Add(s_loadedPrefab.Key);
			}
		}
		foreach (string item in list)
		{
			s_loadedPrefabs.Remove(item);
		}
	}

	public static SharpSerializer GetTextXMLSerializer()
	{
		return new SharpSerializer(new SharpSerializerXmlSettings
		{
			IncludeAssemblyVersionInTypeName = false,
			IncludeCultureInTypeName = false,
			IncludePublicKeyTokenInTypeName = false
		});
	}

	public static SharpSerializer GetBinaryXMLSerializer()
	{
		return new SharpSerializer(new SharpSerializerBinarySettings
		{
			IncludeAssemblyVersionInTypeName = false,
			IncludeCultureInTypeName = false,
			IncludePublicKeyTokenInTypeName = false
		});
	}

	public static bool SearchForFile(string startPath, string filename, out string path)
	{
		if (File.Exists(startPath + "/" + filename))
		{
			path = startPath + "/" + filename;
			return true;
		}
		if (Directory.Exists(startPath))
		{
			string[] directories = Directory.GetDirectories(startPath);
			for (int i = 0; i < directories.Length; i++)
			{
				if (SearchForFile(directories[i], filename, out path))
				{
					return true;
				}
			}
		}
		path = string.Empty;
		return false;
	}

	public static string GetCrashBuddyRegionAreaArguments()
	{
		string empty = string.Empty;
		string text = "global";
		empty = ((Application.isPlaying || !(s_currentScene == null)) ? SceneManager.GetActiveScene().path : SceneManager.GetActiveScene().path);
		if (string.IsNullOrEmpty(empty))
		{
			return string.Empty;
		}
		empty = Path.GetFileNameWithoutExtension(empty).ToLower();
		if (empty.StartsWith("ar_00"))
		{
			text = "dyrford";
		}
		else if (empty.StartsWith("ar_01"))
		{
			text = "dyrford";
		}
		else if (empty.StartsWith("ar_02"))
		{
			text = "defiance bay";
		}
		else if (empty.StartsWith("ar_03"))
		{
			text = "defiance bay";
		}
		else if (empty.StartsWith("ar_04"))
		{
			text = "defiance bay";
		}
		else if (empty.StartsWith("ar_05"))
		{
			text = "defiance bay";
		}
		else if (empty.StartsWith("ar_06"))
		{
			text = "stronghold";
		}
		else if (empty.StartsWith("ar_07"))
		{
			text = "gilded vale";
		}
		else if (empty.StartsWith("ar_08"))
		{
			text = "wilderness";
		}
		else if (empty.StartsWith("ar_09"))
		{
			text = "global";
		}
		else if (empty.StartsWith("ar_10"))
		{
			text = "od nua";
		}
		else if (empty.StartsWith("ar_11"))
		{
			text = "twin elms";
		}
		else if (empty.StartsWith("ar_12"))
		{
			text = "twin elms";
		}
		else if (empty.StartsWith("ar_13"))
		{
			text = "twin elms";
		}
		else if (empty.StartsWith("ar_14"))
		{
			text = "twin elms";
		}
		if (empty.StartsWith("px1"))
		{
			text = ((!empty.StartsWith("px1_0305") && !empty.StartsWith("px1_0306")) ? "the white march" : "concelhauts");
		}
		if (empty.StartsWith("px2"))
		{
			text = "the white march II";
		}
		string text2 = "-region \"" + text + "\" -area \"" + empty + "\" -buildnum " + buildnum.BUILD_NUMBER + " -autoupdatepath \"\\\\oeitools\\oeitools\\crash buddy\"";
		if (ConversationManager.Instance != null)
		{
			FlowChartPlayer activeConversationForHUD = ConversationManager.Instance.GetActiveConversationForHUD();
			if (activeConversationForHUD != null)
			{
				Conversation conversation = activeConversationForHUD.CurrentFlowChart as Conversation;
				string text3 = string.Empty;
				if (conversation != null)
				{
					text3 = "\"" + conversation.GetNodeText(activeConversationForHUD, conversation.GetNode(activeConversationForHUD.CurrentNodeID)) + "\"";
				}
				string text4 = "1) Load scene " + empty + "\n2) Play conversation " + activeConversationForHUD.CurrentFlowChart.Filename + "\n3) Navigate to node " + activeConversationForHUD.CurrentNodeID + " - " + text3 + "\n4) ";
				text2 = text2 + " -reprosteps \"" + text4 + "\"";
			}
		}
		return text2;
	}
}

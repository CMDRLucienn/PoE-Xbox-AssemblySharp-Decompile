using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using UnityEngine;

public class CrashSender
{
	public delegate void AddCrashFileDelegate(string sCrashDumpDirectory);

	public static string ConfigFilename = string.Empty;

	private string DumpDirectory = string.Empty;

	private string ScreenShotFilename = string.Empty;

	private string Arguments = string.Empty;

	private bool IsCrashBuddyQueued;

	private static List<AddCrashFileDelegate> s_cAddCrashFileHandlers = new List<AddCrashFileDelegate>();

	public static void AddCrashFileHandler(AddCrashFileDelegate cHandler)
	{
		s_cAddCrashFileHandlers.Add(cHandler);
	}

	public static void RemoveCrashFileHandler(AddCrashFileDelegate cHandler)
	{
		s_cAddCrashFileHandlers.Remove(cHandler);
	}

	public CrashSender()
	{
		if (s_cAddCrashFileHandlers.Count != 0)
		{
			return;
		}
		AddCrashFileHandler(delegate(string sDumpDirectory)
		{
			string value = "";
			using StreamWriter streamWriter = new StreamWriter(Path.Combine(sDumpDirectory, "tool_console.txt"));
			streamWriter.Write(value);
		});
	}

	public void Update()
	{
		if (IsCrashBuddyQueued && (File.Exists(ScreenShotFilename) || (Application.isEditor && !Application.isPlaying)))
		{
			LaunchCrashBuddy(DumpDirectory, Arguments);
			IsCrashBuddyQueued = false;
			DumpDirectory = string.Empty;
			ScreenShotFilename = string.Empty;
			Arguments = string.Empty;
		}
	}

	private static string GetCrashBuddyInstallKey()
	{
		return "HKEY_CURRENT_USER\\Software\\Obsidian Entertainment\\Crash Buddy\\Environment";
	}

	private static string GetLagacyCrashBuddyInstallKey()
	{
		return string.Empty;
	}

	private static string GetCrashBuddyInstallValue()
	{
		return "InstallDir";
	}

	public static bool IsCrashBuddyInstalled()
	{
		string text = Registry.GetValue(GetCrashBuddyInstallKey(), GetCrashBuddyInstallValue(), null) as string;
		if (string.IsNullOrEmpty(text))
		{
			text = Registry.GetValue(GetLagacyCrashBuddyInstallKey(), GetCrashBuddyInstallValue(), null) as string;
		}
		if (!string.IsNullOrEmpty(text) && Directory.Exists(text))
		{
			return true;
		}
		return false;
	}

	public void TriggerBug(string arguments)
	{
		Arguments = arguments;
		BugHandling();
	}

	public void TriggerExportGlobals()
	{
		string text = "Output Global Variables";
		bool flag = false;
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("TriggerExportGlobals - Error creating directory " + text + ": " + ex.Message);
				return;
			}
		}
		try
		{
			if (GlobalVariables.Instance != null)
			{
				GlobalVariables.Instance.OutputGlobalsToFile(Path.Combine(text, "global_variables_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"));
			}
		}
		catch (Exception ex2)
		{
			UnityEngine.Debug.LogWarning("TriggerExportGlobals - Error writing global variables: " + ex2.Message);
			flag = true;
		}
		try
		{
			if (QuestManager.Instance != null)
			{
				QuestManager.Instance.OutputQuestsToFile(Path.Combine(text, "quests_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"));
			}
		}
		catch (Exception ex3)
		{
			UnityEngine.Debug.LogWarning("TriggerExportGlobals - Error writing quests: " + ex3.Message);
			flag = true;
		}
		try
		{
			GameUtilities.LogProgrammingDebug(Path.Combine(text, "programming_debug_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"));
		}
		catch (Exception ex4)
		{
			UnityEngine.Debug.LogWarning("TriggerExportGlobals - Error writing programming debug: " + ex4.Message);
			flag = true;
		}
		if (!flag)
		{
			UnityEngine.Debug.Log("TriggerExportGlobals - Global variables written to " + text);
		}
	}

	public static void WriteBuildVersionToDirectory(string sDumpDirectory)
	{
		try
		{
			using FileStream stream = new FileStream(Path.Combine(sDumpDirectory, "build_version.txt"), FileMode.Create, FileAccess.Write, FileShare.None);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
		}
		catch
		{
		}
	}

	private void TakeScreenshotsForBug(string sDumpDirectory)
	{
		try
		{
			if (GlobalVariables.Instance != null)
			{
				GlobalVariables.Instance.OutputGlobalsToFile(Path.Combine(sDumpDirectory, "global_variables.txt"));
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error writing global variables: " + ex.Message);
		}
		try
		{
			if (QuestManager.Instance != null)
			{
				QuestManager.Instance.OutputQuestsToFile(Path.Combine(sDumpDirectory, "quests.txt"));
			}
		}
		catch (Exception ex2)
		{
			UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error writing quests: " + ex2.Message);
		}
		try
		{
			GameUtilities.LogProgrammingDebug(Path.Combine(sDumpDirectory, "programming_debug.txt"));
		}
		catch (Exception ex3)
		{
			UnityEngine.Debug.LogWarning("TriggerExportGlobals - Error writing programming debug: " + ex3.Message);
		}
		string text = "c:\\Documents and Settings\\" + Environment.UserName + "\\Local Settings\\Application Data\\Unity\\Editor\\Editor.log";
		string[] files = Directory.GetFiles(Environment.CurrentDirectory, "output_log.txt", SearchOption.AllDirectories);
		if (files.Length != 0)
		{
			try
			{
				File.Copy(files[0], Path.Combine(sDumpDirectory, "output_log.txt"));
			}
			catch (Exception ex4)
			{
				UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error copying game log: " + ex4.Message);
			}
		}
		text = "trenton_Data\\output_log.txt";
		if (File.Exists(text))
		{
			try
			{
				File.Copy(text, Path.Combine(sDumpDirectory, "game_log.txt"));
			}
			catch (Exception ex5)
			{
				UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error copying game log: " + ex5.Message);
			}
		}
		string text2 = Path.Combine(sDumpDirectory, "assert_screenshot.png");
		try
		{
			GameResources.BuildSaveFile("bug.savegame", string.Empty);
		}
		catch (Exception ex6)
		{
			UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error creating save file: " + ex6.Message);
		}
		try
		{
			File.Copy(Path.Combine(GameResources.SaveGamePath, "bug.savegame"), Path.Combine(sDumpDirectory, "bug.savegame"), overwrite: true);
		}
		catch (Exception ex7)
		{
			UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error copying save file: " + ex7.Message);
		}
		try
		{
			string text3 = Path.Combine(GameResources.SaveGamePath, SaveGameInfo.GetLastAutosaveFileName());
			if (File.Exists(text3))
			{
				File.Copy(text3, Path.Combine(sDumpDirectory, SaveGameInfo.GetLastAutosaveFileName()), overwrite: true);
			}
		}
		catch (Exception ex8)
		{
			UnityEngine.Debug.LogWarning("TakeScreenshotsForBug - Error copying autosave file: " + ex8.Message);
		}
		try
		{
			ScreenCapture.CaptureScreenshot(text2);
			ScreenShotFilename = text2;
		}
		catch (Exception ex9)
		{
			UnityEngine.Debug.LogWarning("CaptureScreenshot Failed: " + ex9.Message);
		}
	}

	private void BugHandling()
	{
		if (!IsCrashBuddyInstalled())
		{
			UnityEngine.Debug.LogWarning("Could not create a bug report because Crash Buddy is not installed.");
			return;
		}
		try
		{
			GetCrashDumpPathAndCreateIfNecessary(out var sDumpDirectory, out var _);
			WriteBuildVersionToDirectory(sDumpDirectory);
			try
			{
				foreach (AddCrashFileDelegate s_cAddCrashFileHandler in s_cAddCrashFileHandlers)
				{
					try
					{
						s_cAddCrashFileHandler(sDumpDirectory);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			if (!Application.isEditor || Application.isPlaying)
			{
				TakeScreenshotsForBug(sDumpDirectory);
			}
			DumpDirectory = sDumpDirectory;
			IsCrashBuddyQueued = true;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning("Crash Buddy Setup Failed: " + ex.Message);
		}
	}

	private void GetCrashDumpPathAndCreateIfNecessary(out string sDumpDirectory, out string sDumpID)
	{
		string tempPath = Path.GetTempPath();
		string text = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.ModuleName, null);
		text = text.Replace(" ", "");
		List<char> list = new List<char>();
		char[] invalidPathChars = Path.GetInvalidPathChars();
		foreach (char item in invalidPathChars)
		{
			list.Add(item);
		}
		invalidPathChars = Path.GetInvalidFileNameChars();
		foreach (char item2 in invalidPathChars)
		{
			if (!list.Contains(item2))
			{
				list.Add(item2);
			}
		}
		foreach (char item3 in list)
		{
			text = text.Replace(new string(item3, 1), "");
		}
		string text2 = Path.ChangeExtension(Path.Combine(tempPath, text), null);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		DateTime now = DateTime.Now;
		string text3 = Environment.UserName;
		if (string.IsNullOrEmpty(text3))
		{
			text3 = "unknown_user";
		}
		string text4 = now.Year.ToString("0000");
		string text5 = now.Month.ToString("00");
		string text6 = now.Day.ToString("00");
		string text7 = now.Hour.ToString("00");
		string text8 = now.Minute.ToString("00");
		string text9 = now.Second.ToString("00");
		string text10 = Process.GetCurrentProcess().Id.ToString();
		string text11 = Thread.CurrentThread.ManagedThreadId.ToString();
		sDumpID = $"{text3}-{text}-{text4}{text5}{text6}-{text7}{text8}{text9}-{text10}-{text11}";
		sDumpDirectory = Path.Combine(text2, sDumpID);
		if (!Directory.Exists(sDumpDirectory))
		{
			Directory.CreateDirectory(sDumpDirectory);
		}
	}

	public void LaunchCrashBuddy(string sDumpDirectory, string arguments)
	{
		string empty = string.Empty;
		string text = Registry.GetValue(GetCrashBuddyInstallKey(), GetCrashBuddyInstallValue(), null) as string;
		if (string.IsNullOrEmpty(text))
		{
			text = Registry.GetValue(GetLagacyCrashBuddyInstallKey(), GetCrashBuddyInstallValue(), null) as string;
		}
		if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
		{
			return;
		}
		empty = Path.Combine(text, "CrashBuddy.exe");
		if (string.IsNullOrEmpty(empty) || !File.Exists(empty))
		{
			return;
		}
		try
		{
			Process process = new Process();
			ConfigFilename = Path.Combine(Environment.CurrentDirectory, "Assets\\Data\\Debug\\trenton.crashbuddyconfig");
			if (!File.Exists(ConfigFilename))
			{
				ConfigFilename = Path.Combine(Environment.CurrentDirectory, "PillarsOfEternity_Data\\Data\\Debug\\trenton.crashbuddyconfig");
			}
			string arguments2 = "-configfile \"" + ConfigFilename + "\" -dir \"" + sDumpDirectory + "\" " + arguments;
			ProcessStartInfo processStartInfo = new ProcessStartInfo(empty, arguments2);
			processStartInfo.CreateNoWindow = true;
			processStartInfo.ErrorDialog = false;
			processStartInfo.UseShellExecute = false;
			processStartInfo.WorkingDirectory = Path.GetDirectoryName(empty);
			process.StartInfo = processStartInfo;
			process.Start();
			if (!Screen.fullScreen)
			{
				process.WaitForExit();
			}
		}
		catch
		{
		}
	}
}

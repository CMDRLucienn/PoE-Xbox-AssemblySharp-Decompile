using UnityEngine;

[AddComponentMenu("Miscellaneous/Crash Logging")]
public class CrashLogging : MonoBehaviour
{
	private CrashSender CrashSender;

	private static bool IsBugCommandQueued = false;

	private static bool IsExportGlobalsCommandQueued = false;

	private static string Arguments = string.Empty;

	public CrashLogging()
	{
		CrashSender = new CrashSender();
	}

	public static void QueueBugCommand(string arguments)
	{
		IsBugCommandQueued = true;
		Arguments = arguments;
	}

	public static void DequeueBugCommand()
	{
		IsBugCommandQueued = false;
	}

	public static void QueueExportGlobalsCommand()
	{
		IsExportGlobalsCommandQueued = true;
	}

	public static void DequeueExportGlobalsCommand()
	{
		IsExportGlobalsCommandQueued = false;
	}

	public void Update()
	{
		if (IsBugCommandQueued)
		{
			CrashSender.TriggerBug(Arguments);
			IsBugCommandQueued = false;
		}
		if (IsExportGlobalsCommandQueued)
		{
			CrashSender.TriggerExportGlobals();
			IsExportGlobalsCommandQueued = false;
		}
		CrashSender.Update();
	}
}

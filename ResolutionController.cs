using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ResolutionController : MonoBehaviour
{
	public enum DisplayModes
	{
		Unknown,
		Fullscreen,
		Borderless,
		Windowed
	}

	public struct Position
	{
		public int X;

		public int Y;

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return $"{X:F1}, {Y:F1}";
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Position position))
			{
				return false;
			}
			if (X == position.X)
			{
				return Y == position.Y;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator ==(Position position1, Position position2)
		{
			return object.Equals(position1, position2);
		}

		public static bool operator !=(Position position1, Position position2)
		{
			return !object.Equals(position1, position2);
		}
	}

	public static class Flags
	{
		public static void Set<T>(ref T mask, T flag) where T : struct
		{
			int num = (int)(object)mask;
			int num2 = (int)(object)flag;
			mask = (T)(object)(num | num2);
		}

		public static void Unset<T>(ref T mask, T flag) where T : struct
		{
			int num = (int)(object)mask;
			int num2 = (int)(object)flag;
			mask = (T)(object)(num & ~num2);
		}

		public static void Toggle<T>(ref T mask, T flag) where T : struct
		{
			if (Contains(mask, flag))
			{
				Unset(ref mask, flag);
			}
			else
			{
				Set(ref mask, flag);
			}
		}

		public static bool Contains<T>(T mask, T flag) where T : struct
		{
			return Contains((int)(object)mask, (int)(object)flag);
		}

		public static bool Contains(int mask, int flag)
		{
			return (mask & flag) != 0;
		}
	}

	public class WindowHandler
	{
		public struct RECT
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		private const int WS_BORDER = 8388608;

		private const int WS_CAPTION = 12582912;

		private const int WS_CHILD = 1073741824;

		private const int WS_CHILDWINDOW = 1073741824;

		private const int WS_CLIPCHILDREN = 33554432;

		private const int WS_CLIPSIBLINGS = 67108864;

		private const int WS_DISABLED = 134217728;

		private const int WS_DLGFRAME = 4194304;

		private const int WS_GROUP = 131072;

		private const int WS_HSCROLL = 1048576;

		private const int WS_ICONIC = 536870912;

		private const int WS_MAXIMIZE = 16777216;

		private const int WS_MAXIMIZEBOX = 65536;

		private const int WS_MINIMIZE = 536870912;

		private const int WS_MINIMIZEBOX = 131072;

		private const int WS_OVERLAPPED = 0;

		private const int WS_OVERLAPPEDWINDOW = 13565952;

		private const int WS_POPUP = int.MinValue;

		private const int WS_POPUPWINDOW = -2138570752;

		private const int WS_SIZEBOX = 262144;

		private const int WS_SYSMENU = 524288;

		private const int WS_TABSTOP = 65536;

		private const int WS_THICKFRAME = 262144;

		private const int WS_TILED = 0;

		private const int WS_TILEDWINDOW = 13565952;

		private const int WS_VISIBLE = 268435456;

		private const int WS_VSCROLL = 2097152;

		private const int WS_EX_DLGMODALFRAME = 1;

		private const int WS_EX_CLIENTEDGE = 512;

		private const int WS_EX_STATICEDGE = 131072;

		private const int SWP_FRAMECHANGED = 32;

		private const int SWP_NOMOVE = 2;

		private const int SWP_NOSIZE = 1;

		private const int SWP_NOZORDER = 4;

		private const int SWP_NOOWNERZORDER = 512;

		private const int SWP_SHOWWINDOW = 64;

		private const int SWP_NOSENDCHANGING = 1024;

		private const int GWL_STYLE = -16;

		private const int GWL_EXSTYLE = -20;

		private string _title;

		private int _borderWidth;

		private int _captionHeight;

		private IntPtr Window => FindWindowByCaption(IntPtr.Zero, _title);

		private IntPtr Desktop => GetDesktopWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong", SetLastError = true)]
		public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong", SetLastError = true)]
		public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int uFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr UpdateWindowStyle(IntPtr hWnd);

		public WindowHandler(string title)
		{
			_title = title;
		}

		public Resolution? GetDesktopResolution()
		{
			if (GetWindowRect(Desktop, out var rect))
			{
				Resolution value = default(Resolution);
				value.width = rect.Right - rect.Left;
				value.height = rect.Bottom - rect.Top;
				return value;
			}
			return null;
		}

		public Position? GetWindowPosition()
		{
			if (!GetWindowRect(Window, out var rect))
			{
				return null;
			}
			return new Position(rect.Left, rect.Top);
		}

		public Position? GetCenteredPosition(Resolution resolution, DisplayModes displayMode)
		{
			if (!GetWindowRect(Desktop, out var rect))
			{
				return null;
			}
			int num = rect.Right - rect.Left;
			int num2 = rect.Bottom - rect.Top;
			int x;
			int y;
			if (displayMode == DisplayModes.Windowed)
			{
				x = (num - (resolution.width + _borderWidth * 2)) / 2;
				y = (num2 - (resolution.height + _borderWidth * 2 + _captionHeight)) / 2;
			}
			else
			{
				x = (num - resolution.width) / 2;
				y = (num2 - resolution.height) / 2;
			}
			return new Position(x, y);
		}

		public bool TrySetDisplay(DisplayModes targetDisplayMode, Resolution targetResolution, bool setPosition, Position? position)
		{
			int mask = (int)GetWindowLongPtr(Window, -16);
			switch (targetDisplayMode)
			{
			case DisplayModes.Fullscreen:
				return true;
			case DisplayModes.Borderless:
				Flags.Unset(ref mask, 12582912);
				SetWindowLongPtr(Window, -16, mask);
				if (!setPosition || !position.HasValue)
				{
					position = GetCenteredPosition(targetResolution, targetDisplayMode);
				}
				UpdateWindowRect(Window, position.Value.X, position.Value.Y, targetResolution.width, targetResolution.height);
				SetWindowLongPtr(Window, -16, mask);
				SetWindowLongPtr(Window, -16, mask);
				return true;
			case DisplayModes.Windowed:
			{
				Flags.Set(ref mask, 12582912);
				SetWindowLongPtr(Window, -16, mask);
				UpdateDecorationSize(Window);
				if (!setPosition || !position.HasValue)
				{
					position = GetCenteredPosition(targetResolution, targetDisplayMode);
				}
				int width = targetResolution.width + _borderWidth * 2;
				int height = targetResolution.height + _captionHeight + _borderWidth * 2;
				UpdateWindowRect(Window, position.Value.X, position.Value.Y, width, height);
				return true;
			}
			default:
				return false;
			}
		}

		private bool UpdateDecorationSize(IntPtr window)
		{
			if (!GetWindowRect(Window, out var rect))
			{
				return false;
			}
			if (!GetClientRect(Window, out var rect2))
			{
				return false;
			}
			int num = rect.Right - rect.Left - (rect2.Right - rect2.Left);
			int num2 = rect.Bottom - rect.Top - (rect2.Bottom - rect2.Top);
			_borderWidth = num / 2;
			_captionHeight = num2 - _borderWidth * 2;
			return true;
		}

		private void UpdateWindowRect(IntPtr window, int x, int y, int width, int height)
		{
			SetWindowPos(window, -2, x, y, width, height, 32);
		}

		private bool TestForErrors(IntPtr result)
		{
			if (result == IntPtr.Zero)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 0)
				{
					Debug.LogError("Error " + lastWin32Error + " occured. SetDisplayMode failed.");
					return true;
				}
			}
			return false;
		}
	}

	public const string UNKNOWN_DESKTOP_RESOLUTION_WARNING = "Desktop resolution was not detected. In Windowed\r\n\t\tand Borderless modes, resolutions which are too\r\n\t\tlarge for the monitor's native resolution may lead\r\n\t\tto Legacy GUI handling mouse input incorrectly.";

	[SerializeField]
	private Position _defaultOverridePosition = new Position(0, 0);

	[SerializeField]
	private bool _newResolutionResetsPosition = true;

	[SerializeField]
	private bool _guessBestDefaultResolution = true;

	private Resolution[] _sortedResolutions;

	private int _largestResolution;

	private IEnumerator _displayModeRoutine;

	private IEnumerator _resolutionRoutine;

	private IEnumerator _resolutionAndModeRoutine;

	private bool _initialized;

	private static ResolutionController s_instance = null;

	private DisplayModes _currentDisplayMode;

	private int _currentResolutionIndex;

	private Position _currentOverridePosition;

	private Position? _currentWindowPosition;

	private bool _useOverridePosition;

	private DisplayModes _previousDisplayMode = DisplayModes.Windowed;

	private static bool _applicationIsFocused = true;

	private bool _debugLogEnabled;

	private WindowHandler _windowsHandler;

	private const string USE_OVERRIDE_POSITION_KEY = "_useOverridePosition";

	private const string CURRENT_OVERRIDE_POSITION_X_KEY = "_currentOverridePosition.Value.X";

	private const string CURRENT_OVERRIDE_POSITION_Y_KEY = "_currentOverridePosition.Value.Y";

	private const string CURRENT_WINDOW_POSITION_X_KEY = "_currentWindowPosition.Value.X";

	private const string CURRENT_WINDOW_POSITION_Y_KEY = "_currentWindowPosition.Value.Y";

	private const string CURRENT_DISPLAY_MODE_KEY = "_currentDisplayMode";

	private const string PREVIOUS_DISPLAY_MODE_KEY = "_previousDisplayMode";

	private const string CURRENT_RESOLUTION_INDEX_KEY = "_currentResolutionIndex";

	public static ResolutionController Instance => s_instance;

	public DisplayModes CurrentDisplayMode => _currentDisplayMode;

	public Resolution CurrentResolution => SortedResolutions[_currentResolutionIndex];

	public string CurrentResolutionString => GenerateFullResolutionString(CurrentResolution);

	public Position CurrentOverridePosition
	{
		get
		{
			return _currentOverridePosition;
		}
		set
		{
			_currentOverridePosition = value;
		}
	}

	public bool UseOverridePosition
	{
		get
		{
			return _useOverridePosition;
		}
		set
		{
			if (_useOverridePosition != value)
			{
				ChangeUseOverridePosition(value);
			}
		}
	}

	public bool IsExclusiveFullscreen => Screen.fullScreen;

	public bool IsChangingDisplay
	{
		get
		{
			if (_displayModeRoutine == null && _resolutionRoutine == null)
			{
				return _resolutionAndModeRoutine != null;
			}
			return true;
		}
	}

	public bool NeedDesktopResolutionWarning
	{
		get
		{
			bool flag = CurrentDisplayMode == DisplayModes.Windowed || CurrentDisplayMode == DisplayModes.Borderless;
			if (!flag)
			{
				return true;
			}
			if (flag)
			{
				return GetDesktopResolution().HasValue;
			}
			return false;
		}
	}

	public Resolution[] SortedResolutions
	{
		get
		{
			if (_sortedResolutions == null)
			{
				_sortedResolutions = Sort(Screen.resolutions);
			}
			return _sortedResolutions;
		}
	}

	public bool IsCurrentResolution(Resolution resolution)
	{
		bool result = resolution.width == CurrentResolution.width && resolution.height == CurrentResolution.height;
		DebugLog("--->IsCurrentResolution(" + resolution.width + "x" + resolution.height + ") returning " + result.ToString());
		return result;
	}

	public bool IsAllowedResolution(Resolution resolution)
	{
		bool result = true;
		if (CurrentDisplayMode == DisplayModes.Windowed || CurrentDisplayMode == DisplayModes.Borderless)
		{
			Resolution? desktopResolution = GetDesktopResolution();
			if (desktopResolution.HasValue)
			{
				bool num = resolution.width <= desktopResolution.Value.width;
				bool flag = resolution.height <= desktopResolution.Value.height;
				result = num && flag;
			}
		}
		DebugLog("--->IsAllowedResolution( " + resolution.width + "x" + resolution.height + " ) returning " + result.ToString());
		return result;
	}

	public Resolution? GetDesktopResolution()
	{
		Resolution? resolution = null;
		resolution = _windowsHandler.GetDesktopResolution();
		if (resolution.HasValue)
		{
			DebugLog("--->GetDesktopResolution() returning " + resolution.Value.width + "x" + resolution.Value.height);
		}
		else
		{
			DebugLog("--->GetDesktopResolution() returning null");
		}
		return resolution;
	}

	public string GenerateFullResolutionString(Resolution resolution)
	{
		return $"{GenerateAspectRatioString(resolution)}: {GenerateResolutionString(resolution)}";
	}

	public string GenerateResolutionString(Resolution resolution)
	{
		return $"{resolution.width}x{resolution.height}";
	}

	public string GenerateAspectRatioString(Resolution resolution)
	{
		float f = (float)resolution.width / (float)resolution.height;
		if (Approximately(f, 1.33333337f, 0.1f))
		{
			return "4:3";
		}
		if (Approximately(f, 1.77777779f, 0.1f))
		{
			return "16:9";
		}
		if (Approximately(f, 1.6f, 0.1f))
		{
			return "16:10";
		}
		return "Unknown";
	}

	public DisplayModes GetCorrectWindowedDisplayMode(Resolution desiredResolution)
	{
		DisplayModes displayModes = DisplayModes.Windowed;
		Resolution? desktopResolution = GetDesktopResolution();
		if (desktopResolution.HasValue && desiredResolution.width == desktopResolution.Value.width && desiredResolution.height == desktopResolution.Value.height)
		{
			displayModes = DisplayModes.Borderless;
		}
		DebugLog("--->GetCorrectWindowedDisplayMode() returning " + displayModes);
		return displayModes;
	}

	public int MatchResolutionToIndex(Resolution resolution)
	{
		int num = -1;
		int num2 = 0;
		for (int i = 0; i < SortedResolutions.Length; i++)
		{
			bool num3 = SortedResolutions[i].width == resolution.width;
			bool flag = SortedResolutions[i].height == resolution.height;
			if (num3 && flag)
			{
				if (SortedResolutions[i].refreshRate == resolution.refreshRate)
				{
					num = i;
					break;
				}
				if (SortedResolutions[i].refreshRate > num2)
				{
					num2 = SortedResolutions[i].refreshRate;
					num = i;
				}
			}
		}
		if (num == -1)
		{
			num = 0;
			for (int j = 0; j < SortedResolutions.Length; j++)
			{
				int num4 = Math.Abs(SortedResolutions[j].width - resolution.width);
				if (num4 < Math.Abs(SortedResolutions[num].width - resolution.width))
				{
					num = j;
				}
				else if (num4 == Math.Abs(SortedResolutions[num].width - resolution.width) && Math.Abs(SortedResolutions[j].height - resolution.height) < Math.Abs(SortedResolutions[num].height - resolution.height))
				{
					num = j;
				}
			}
		}
		DebugLog("--->MatchResolutionToIndex(" + resolution.width + "x" + resolution.height + ") returning " + num);
		return num;
	}

	public bool TryChangeResolution(Resolution desiredResolution)
	{
		return TryChangeResolution(MatchResolutionToIndex(desiredResolution));
	}

	public bool TryChangeResolution(int desiredResolutionIndex)
	{
		DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ")...");
		if (!_initialized)
		{
			DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") returning false.  Not initialized.");
			return false;
		}
		if (IsChangingDisplay)
		{
			DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") returning false.  Already changing.");
			return false;
		}
		if (desiredResolutionIndex < 0 || desiredResolutionIndex >= SortedResolutions.Length)
		{
			DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") returning false.  Invalid index.");
			return false;
		}
		if (_currentResolutionIndex == desiredResolutionIndex)
		{
			DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") returning false.  Already at desired resolution.");
			return false;
		}
		if (_currentDisplayMode == DisplayModes.Windowed || _currentDisplayMode == DisplayModes.Borderless)
		{
			DisplayModes correctWindowedDisplayMode = GetCorrectWindowedDisplayMode(SortedResolutions[desiredResolutionIndex]);
			if (_currentDisplayMode != correctWindowedDisplayMode)
			{
				DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") need to change display mode an resolution, not just resolution.");
				return TryChangeResolutionAndDisplayMode(desiredResolutionIndex, correctWindowedDisplayMode);
			}
		}
		_resolutionRoutine = ChangeResolutionRoutine(desiredResolutionIndex);
		StartCoroutine(_resolutionRoutine);
		DebugLog("--->TryChangeResolution(" + desiredResolutionIndex + ") returning true.");
		return true;
	}

	public bool TryChangeDisplayMode(DisplayModes desiredDisplayMode)
	{
		DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ")..."));
		DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ") _currentDisplayMode is ", _currentDisplayMode));
		if (!_initialized)
		{
			DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ") returning false.  Not initialized."));
			return false;
		}
		if (IsChangingDisplay)
		{
			DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ") returning false.  Already changing."));
			return false;
		}
		if (desiredDisplayMode != DisplayModes.Fullscreen)
		{
			desiredDisplayMode = GetCorrectWindowedDisplayMode(SortedResolutions[_currentResolutionIndex]);
		}
		if (_currentDisplayMode == desiredDisplayMode)
		{
			DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ") _currentDisplayMode already correct - nothing to do."));
			return false;
		}
		_displayModeRoutine = ChangeDisplayModeRoutine(desiredDisplayMode);
		StartCoroutine(_displayModeRoutine);
		DebugLog(string.Concat("--->TryChangeDisplayMode(", desiredDisplayMode, ") returning true."));
		return true;
	}

	public bool TryChangeResolutionAndDisplayMode(Resolution desiredResolution, DisplayModes desiredDisplayMode, bool force = false)
	{
		return TryChangeResolutionAndDisplayMode(MatchResolutionToIndex(desiredResolution), desiredDisplayMode, force);
	}

	public bool TryChangeResolutionAndDisplayMode(int desiredResolutionIndex, DisplayModes desiredDisplayMode, bool force = false)
	{
		DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ")..."));
		DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") _currentResolutionIndex is ", _currentResolutionIndex, ", _currentDisplayMode is ", _currentDisplayMode));
		if (!_initialized)
		{
			DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") returning false.  Not initialized."));
			return false;
		}
		if (IsChangingDisplay)
		{
			DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") returning false.  Already changing."));
			return false;
		}
		if (desiredResolutionIndex < 0 || desiredResolutionIndex >= SortedResolutions.Length)
		{
			DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") returning false.  Invalid index."));
			return false;
		}
		if (!force)
		{
			if (_currentResolutionIndex == desiredResolutionIndex)
			{
				DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") already at desired resolution, only changing display mode."));
				return TryChangeDisplayMode(desiredDisplayMode);
			}
			if (_currentDisplayMode == desiredDisplayMode)
			{
				DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") already at desired display mode, only changing resolution."));
				return TryChangeResolution(desiredResolutionIndex);
			}
		}
		_resolutionAndModeRoutine = ChangeResolutionAndDisplayModeRoutine(desiredResolutionIndex, desiredDisplayMode);
		StartCoroutine(_resolutionAndModeRoutine);
		DebugLog(string.Concat("--->TryChangeResolutionAndDisplayMode(", desiredResolutionIndex, ", ", desiredDisplayMode, ") returning true."));
		return true;
	}

	public bool TryChangeOverridePosition(Position desiredOverridePosition)
	{
		DebugLog(string.Concat("--->TryChangeOverridePosition(", desiredOverridePosition, ")..."));
		if (!_initialized)
		{
			DebugLog(string.Concat("--->TryChangeOverridePosition(", desiredOverridePosition, ") returning false.  Not initialized."));
			return false;
		}
		if (_currentOverridePosition.X == desiredOverridePosition.X && _currentOverridePosition.Y == desiredOverridePosition.Y)
		{
			return false;
		}
		ChangeOverridePosition(desiredOverridePosition);
		DebugLog(string.Concat("--->TryChangeOverridePosition(", desiredOverridePosition, ") returning true."));
		return true;
	}

	public void ToggleFullscreen()
	{
		if (!_initialized)
		{
			DebugLog("--->ToggleFullscreen() doing nothing - not initialized");
		}
		else if (_currentDisplayMode != DisplayModes.Fullscreen)
		{
			TryChangeDisplayMode(DisplayModes.Fullscreen);
		}
		else
		{
			TryChangeDisplayMode(DisplayModes.Windowed);
		}
	}

	public void ClearSaveData()
	{
		if (!_initialized)
		{
			DebugLog("--->ClearSaveData() doing nothing - not initialized");
			return;
		}
		DebugLog("--->ClearSaveData() deleting entries from PlayerPrefs");
		PlayerPrefs.DeleteKey("_useOverridePosition");
		PlayerPrefs.DeleteKey("_currentOverridePosition.Value.X");
		PlayerPrefs.DeleteKey("_currentOverridePosition.Value.Y");
		PlayerPrefs.DeleteKey("_currentDisplayMode");
		PlayerPrefs.DeleteKey("_previousDisplayMode");
		PlayerPrefs.DeleteKey("_currentResolutionIndex");
	}

	public void Save()
	{
		if (!_initialized)
		{
			DebugLog("--->Save() doing nothing - not initialized");
			return;
		}
		DebugLog("--->Save() saving data to PlayerPrefs");
		SaveUseOverridePosition();
		SaveCurrentPositionOverride();
		SaveCurrentDisplayMode();
		SaveCurrentWindowPosition();
		SaveCurrentResolutionIndex();
	}

	private void DebugLog(object message)
	{
		if (_debugLogEnabled)
		{
			Debug.Log(message);
		}
	}

	private void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
			_windowsHandler = new WindowHandler(Game.Name);
			InitializeDisplay();
		}
		else
		{
			GameUtilities.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
	}

	private void InitializeDisplay()
	{
		DebugLog("--->InitializeDisplay()...");
		_useOverridePosition = LoadUseOverridePosition();
		_previousDisplayMode = LoadPreviousDisplayMode();
		_currentWindowPosition = TryLoadWindowPosition();
		int? targetResolutionIndex = TryLoadResolution();
		DisplayModes? targetDisplayMode = TryLoadDisplayMode();
		Position? targetOverridePosition = TryLoadOverridePosition();
		string text = "Available display resolutions:\n";
		_largestResolution = 0;
		for (int i = 0; i < SortedResolutions.Length; i++)
		{
			if (SortedResolutions[i].width >= SortedResolutions[_largestResolution].width && SortedResolutions[i].height >= SortedResolutions[_largestResolution].height)
			{
				_largestResolution = i;
			}
			text = text + "\t[ " + i + " ] " + SortedResolutions[i].width + "x" + SortedResolutions[i].height + "\n";
		}
		text = text + "\tLargest resolution is [ " + _largestResolution + " ] " + SortedResolutions[_largestResolution].width + "x" + SortedResolutions[_largestResolution].height;
		DebugLog(text);
		text = null;
		StartCoroutine(InitializeRoutine(targetResolutionIndex, targetDisplayMode, targetOverridePosition));
	}

	private IEnumerator InitializeRoutine(int? targetResolutionIndex, DisplayModes? targetDisplayMode, Position? targetOverridePosition)
	{
		DebugLog(string.Concat("--->InitializeRoutine(", targetResolutionIndex, ", ", targetDisplayMode, ", ", targetOverridePosition, ")..."));
		ChangeOverridePosition(targetOverridePosition.HasValue ? targetOverridePosition.Value : _defaultOverridePosition, updateWindow: false);
		int targetResolutionIndex2 = GetDefaultResolutionIndex();
		if (targetResolutionIndex.HasValue && targetResolutionIndex.Value >= 0 && targetResolutionIndex.Value < _sortedResolutions.Length)
		{
			targetResolutionIndex2 = targetResolutionIndex.Value;
		}
		_resolutionRoutine = ChangeResolutionRoutine(targetResolutionIndex2, updateWindow: false);
		yield return StartCoroutine(_resolutionRoutine);
		_displayModeRoutine = ChangeDisplayModeRoutine(targetDisplayMode.HasValue ? targetDisplayMode.Value : GetDefaultDisplayMode());
		yield return StartCoroutine(_displayModeRoutine);
		_initialized = true;
	}

	private int GetDefaultResolutionIndex()
	{
		if (_guessBestDefaultResolution)
		{
			Resolution? desktopResolution = GetDesktopResolution();
			if (desktopResolution.HasValue)
			{
				for (int i = 0; i < SortedResolutions.Length; i++)
				{
					bool num = SortedResolutions[i].width == desktopResolution.Value.width;
					bool flag = SortedResolutions[i].height == desktopResolution.Value.height;
					if (num && flag)
					{
						return i;
					}
				}
			}
		}
		return 0;
	}

	private DisplayModes GetDefaultDisplayMode()
	{
		return DisplayModes.Fullscreen;
	}

	private void OnApplicationFocus(bool status)
	{
		if (_initialized)
		{
			_applicationIsFocused = status;
		}
	}

	private void Update()
	{
		if (_initialized && _applicationIsFocused && !IsChangingDisplay)
		{
			TryRecordWindowPosition();
			QuerySomethingChanged();
		}
	}

	private bool TryRecordWindowPosition()
	{
		if (IsChangingDisplay)
		{
			return false;
		}
		if (_currentDisplayMode != DisplayModes.Windowed)
		{
			return false;
		}
		if (_useOverridePosition)
		{
			return false;
		}
		if (_windowsHandler.GetCenteredPosition(CurrentResolution, CurrentDisplayMode) == _windowsHandler.GetWindowPosition())
		{
			return false;
		}
		RecordWindowPosition();
		return true;
	}

	private void RecordWindowPosition()
	{
		_currentWindowPosition = _windowsHandler.GetWindowPosition();
		SaveCurrentWindowPosition();
	}

	private void QuerySomethingChanged()
	{
		bool flag = false;
		Resolution resolution = default(Resolution);
		resolution.width = Screen.width;
		resolution.height = Screen.height;
		resolution.refreshRate = 0;
		if (resolution.width != SortedResolutions[_currentResolutionIndex].width || resolution.height != SortedResolutions[_currentResolutionIndex].height)
		{
			flag = true;
		}
		DisplayModes displayModes = DisplayModes.Unknown;
		if (Screen.fullScreen)
		{
			displayModes = DisplayModes.Fullscreen;
		}
		else
		{
			_debugLogEnabled = false;
			displayModes = GetCorrectWindowedDisplayMode(resolution);
		}
		if (_currentDisplayMode != displayModes)
		{
			flag = true;
		}
		if (flag)
		{
			DebugLog("QuerySomethingChanged() Something changed");
			TryChangeResolutionAndDisplayMode(MatchResolutionToIndex(resolution), displayModes);
		}
	}

	private void UpdateWindow()
	{
		bool flag = _currentDisplayMode == DisplayModes.Borderless || _currentDisplayMode == DisplayModes.Windowed;
		bool setPosition = UseOverridePosition || (flag && _currentWindowPosition.HasValue);
		Position? position = (UseOverridePosition ? new Position?(CurrentOverridePosition) : _currentWindowPosition);
		_windowsHandler.TrySetDisplay(_currentDisplayMode, CurrentResolution, setPosition, position);
	}

	private IEnumerator ChangeDisplayModeRoutine(DisplayModes targetDisplayMode, bool updateWindow = true)
	{
		DebugLog(string.Concat("--->ChangeDisplayModeRoutine( ", targetDisplayMode, ", ", updateWindow.ToString(), " )..."));
		DebugLog(string.Concat("--->ChangeDisplayModeRoutine( ", targetDisplayMode, ", ", updateWindow.ToString(), " ) _currentDisplayMode is ", _currentDisplayMode));
		_previousDisplayMode = _currentDisplayMode;
		_currentDisplayMode = targetDisplayMode;
		SavePreviousDisplayMode();
		SaveCurrentDisplayMode();
		bool flag;
		switch (targetDisplayMode)
		{
		case DisplayModes.Fullscreen:
			flag = !Screen.fullScreen;
			DebugLog(string.Concat("--->ChangeDisplayModeRoutine( ", targetDisplayMode, ", ", updateWindow.ToString(), " ) calling Screen.fullscreen(true)."));
			Screen.fullScreen = true;
			break;
		default:
			flag = Screen.fullScreen;
			DebugLog(string.Concat("--->ChangeDisplayModeRoutine( ", targetDisplayMode, ", ", updateWindow.ToString(), " ) calling Screen.fullscreen(false)."));
			Screen.fullScreen = false;
			break;
		}
		if (flag)
		{
			yield return null;
		}
		if (updateWindow)
		{
			UpdateWindow();
		}
		WinCursor.Clip(state: true);
		_displayModeRoutine = null;
	}

	private IEnumerator ChangeResolutionRoutine(int targetResolutionIndex, bool updateWindow = true)
	{
		DebugLog("--->ChangeResolutionRoutine( " + targetResolutionIndex + ", " + updateWindow.ToString() + " )...");
		DebugLog("--->ChangeResolutionRoutine( " + targetResolutionIndex + ", " + updateWindow.ToString() + " ) _currentResolutionIndex is " + _currentResolutionIndex);
		_currentResolutionIndex = targetResolutionIndex;
		SaveCurrentResolutionIndex();
		bool flag = true;
		DebugLog("--->ChangeResolutionRoutine( " + targetResolutionIndex + ", " + updateWindow.ToString() + " ) calling Screen.SetResolution(" + CurrentResolution.width + ", " + CurrentResolution.height + ", " + IsExclusiveFullscreen.ToString() + ").");
		Screen.SetResolution(CurrentResolution.width, CurrentResolution.height, IsExclusiveFullscreen);
		if (flag)
		{
			yield return null;
		}
		if (_newResolutionResetsPosition)
		{
			_currentWindowPosition = null;
			updateWindow = updateWindow;
		}
		if (updateWindow)
		{
			UpdateWindow();
		}
		WinCursor.Clip(state: true);
		_resolutionRoutine = null;
	}

	private IEnumerator ChangeResolutionAndDisplayModeRoutine(int targetResolutionIndex, DisplayModes targetDisplayMode, bool updateWindow = true)
	{
		DebugLog(string.Concat("--->ChangeResolutionAndDisplayModeRoutine( ", targetResolutionIndex, ", ", targetDisplayMode, ", ", updateWindow.ToString(), " )..."));
		DebugLog(string.Concat("--->ChangeResolutionAndDisplayModeRoutine( ", targetResolutionIndex, ", ", targetDisplayMode, ", ", updateWindow.ToString(), " ) _currentResolutionIndex is ", _currentResolutionIndex, "_currentResolutionIndex is ", _currentResolutionIndex));
		_previousDisplayMode = _currentDisplayMode;
		_currentDisplayMode = targetDisplayMode;
		SavePreviousDisplayMode();
		SaveCurrentDisplayMode();
		_currentResolutionIndex = targetResolutionIndex;
		SaveCurrentResolutionIndex();
		bool flag = true;
		DebugLog(string.Concat("--->ChangeResolutionAndDisplayModeRoutine( ", targetResolutionIndex, ", ", targetDisplayMode, ", ", updateWindow.ToString(), " ) calling Screen.SetResolution(", CurrentResolution.width, ", ", CurrentResolution.height, ", ", (targetDisplayMode == DisplayModes.Fullscreen).ToString(), ")."));
		Screen.SetResolution(CurrentResolution.width, CurrentResolution.height, targetDisplayMode == DisplayModes.Fullscreen);
		if (flag)
		{
			yield return null;
		}
		if (_newResolutionResetsPosition)
		{
			_currentWindowPosition = null;
			updateWindow = updateWindow;
		}
		if (targetDisplayMode != DisplayModes.Fullscreen)
		{
			_currentDisplayMode = GetCorrectWindowedDisplayMode(CurrentResolution);
			SaveCurrentDisplayMode();
			updateWindow = true;
		}
		if (updateWindow)
		{
			UpdateWindow();
		}
		WinCursor.Clip(state: true);
		_resolutionAndModeRoutine = null;
	}

	private void ChangeUseOverridePosition(bool targetUseOverridePosition, bool updateWindow = true)
	{
		_useOverridePosition = targetUseOverridePosition;
		if (updateWindow)
		{
			UpdateWindow();
		}
	}

	private void ChangeOverridePosition(Position targetOverridePosition, bool updateWindow = true)
	{
		_currentOverridePosition.X = targetOverridePosition.X;
		_currentOverridePosition.Y = targetOverridePosition.Y;
		if (updateWindow)
		{
			UpdateWindow();
		}
	}

	private bool Approximately(float f1, float f2, float diff)
	{
		return Mathf.Abs(f2 - f1) < diff;
	}

	private float Round(float f, float digits)
	{
		float num = Mathf.Pow(10f, digits);
		return Mathf.Round(f * num) / num;
	}

	private bool LoadUseOverridePosition()
	{
		return PlayerPrefs.GetInt("_useOverridePosition", 0) == 1;
	}

	private void SaveUseOverridePosition()
	{
		PlayerPrefs.SetInt("_useOverridePosition", _useOverridePosition ? 1 : 0);
		PlayerPrefs.Save();
	}

	private Position? TryLoadOverridePosition()
	{
		if (PlayerPrefs.HasKey("_currentOverridePosition.Value.X") && PlayerPrefs.HasKey("_currentOverridePosition.Value.Y"))
		{
			return new Position(PlayerPrefs.GetInt("_currentOverridePosition.Value.X"), PlayerPrefs.GetInt("_currentOverridePosition.Value.Y"));
		}
		return null;
	}

	private void SaveCurrentPositionOverride()
	{
		PlayerPrefs.SetInt("_currentOverridePosition.Value.X", _currentOverridePosition.X);
		PlayerPrefs.SetInt("_currentOverridePosition.Value.Y", _currentOverridePosition.Y);
		PlayerPrefs.Save();
	}

	private Position? TryLoadWindowPosition()
	{
		if (PlayerPrefs.HasKey("_currentWindowPosition.Value.X") && PlayerPrefs.HasKey("_currentWindowPosition.Value.Y"))
		{
			return new Position(PlayerPrefs.GetInt("_currentWindowPosition.Value.X"), PlayerPrefs.GetInt("_currentWindowPosition.Value.Y"));
		}
		return null;
	}

	private void SaveCurrentWindowPosition()
	{
		if (_currentWindowPosition.HasValue)
		{
			PlayerPrefs.SetInt("_currentWindowPosition.Value.X", _currentWindowPosition.Value.X);
			PlayerPrefs.SetInt("_currentWindowPosition.Value.Y", _currentWindowPosition.Value.Y);
		}
		else
		{
			PlayerPrefs.DeleteKey("_currentWindowPosition.Value.X");
			PlayerPrefs.DeleteKey("_currentWindowPosition.Value.Y");
		}
		PlayerPrefs.Save();
	}

	private DisplayModes? TryLoadDisplayMode()
	{
		if (PlayerPrefs.HasKey("_currentDisplayMode"))
		{
			return (DisplayModes)PlayerPrefs.GetInt("_currentDisplayMode");
		}
		return null;
	}

	private void SaveCurrentDisplayMode()
	{
		PlayerPrefs.SetInt("_currentDisplayMode", (int)_currentDisplayMode);
		PlayerPrefs.Save();
	}

	private DisplayModes LoadPreviousDisplayMode()
	{
		if (PlayerPrefs.HasKey("_previousDisplayMode"))
		{
			return (DisplayModes)PlayerPrefs.GetInt("_previousDisplayMode");
		}
		return DisplayModes.Windowed;
	}

	private void SavePreviousDisplayMode()
	{
		PlayerPrefs.SetInt("_previousDisplayMode", (int)_previousDisplayMode);
		PlayerPrefs.Save();
	}

	private int? TryLoadResolution()
	{
		if (PlayerPrefs.HasKey("_currentResolutionIndex"))
		{
			return PlayerPrefs.GetInt("_currentResolutionIndex");
		}
		return null;
	}

	private void SaveCurrentResolutionIndex()
	{
		PlayerPrefs.SetInt("_currentResolutionIndex", _currentResolutionIndex);
		PlayerPrefs.Save();
	}

	private Resolution[] Sort(Resolution[] resolutions)
	{
		return (from r in resolutions
			orderby Round((float)r.width / (float)r.height, 1f) descending, r.width * r.height descending
			select r).ToArray();
	}
}

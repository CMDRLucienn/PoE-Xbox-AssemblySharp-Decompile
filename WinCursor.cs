using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WinCursor
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public struct POINT
	{
		public int X;

		public int Y;
	}

	private static RECT clipTarget;

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClipCursor(ref RECT lpRect);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClipCursor(IntPtr lpRect);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

	private static void CalculateClip()
	{
		RECT lpRect = default(RECT);
		POINT lpPoint = default(POINT);
		IntPtr foregroundWindow = GetForegroundWindow();
		if (!GetClientRect(foregroundWindow, ref lpRect))
		{
			Debug.LogError("WinCursor: Failed to GetClientRect.");
		}
		if (!GetWindowRect(foregroundWindow, ref clipTarget))
		{
			Debug.LogError("WinCursor: Failed to GetWindowRect.");
		}
		if (!ClientToScreen(foregroundWindow, ref lpPoint))
		{
			Debug.LogError("WinCursor: Failed to ClientToScreen.");
		}
		Debug.Log("Client Size: L " + lpRect.Left + ", R " + lpRect.Right + ", T " + lpRect.Top + ", B " + lpRect.Bottom);
		Debug.Log("Window Size: L " + clipTarget.Left + ", R " + clipTarget.Right + ", T " + clipTarget.Top + ", B " + clipTarget.Bottom);
		Debug.Log("Client Screen Origin: X " + lpPoint.X + ", Y " + lpPoint.Y);
		clipTarget.Left = lpRect.Left + lpPoint.X;
		clipTarget.Top = lpRect.Top + lpPoint.Y;
		clipTarget.Right = lpRect.Right + lpPoint.X;
		clipTarget.Bottom = lpRect.Bottom + lpPoint.Y;
		Debug.Log("Clip On. Range: L " + clipTarget.Left + ", R " + clipTarget.Right + ", T " + clipTarget.Top + ", B " + clipTarget.Bottom);
	}

	public static void Clip(bool state)
	{
		if (state && GameState.Mode.Option.GetOption(GameOption.BoolOption.CLIP_CURSOR))
		{
			CalculateClip();
			ClipCursor(ref clipTarget);
		}
		else
		{
			ClipCursor(IntPtr.Zero);
		}
	}
}

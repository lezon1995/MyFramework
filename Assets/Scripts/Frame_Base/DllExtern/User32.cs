using System;
using System.Runtime.InteropServices;

public class User32
{
	public const string USER32_DLL = "user32.dll";
	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern long SetWindowLong(IntPtr hwnd, int _nIndex, long dwNewLong);
	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern long GetWindowLong(IntPtr hwnd, int _nIndex);
	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern IntPtr GetForegroundWindow();
}
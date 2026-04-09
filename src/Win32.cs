using System;
using System.Runtime.InteropServices;

namespace Kwerty.DviZe.Win.Hotkeys;

internal static class Win32
{
    //
    // WinUser.h
    //

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int UnregisterHotKey(IntPtr hWnd, int id);

    public const uint WM_HOTKEY = 0x0312;

    //
    // winerror.h
    //

    public const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;
}

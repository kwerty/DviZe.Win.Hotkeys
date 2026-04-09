using System;

namespace Kwerty.DviZe.Win.Hotkeys;

[Flags]
public enum HotkeyModifiers 
{
    // Values defined in WinUser.h.
    None,
    Alt = 0x0001, // MOD_ALT
    Control = 0x0002, // MOD_CONTROL
    Shift = 0x0004, // MOD_SHIFT
    //Win = 0x0008, // MOD_WIN - Reserved by the OS.
    //NoRepeat = 0x4000, // MOD_NOREPEAT - Fails silently in testing.
}

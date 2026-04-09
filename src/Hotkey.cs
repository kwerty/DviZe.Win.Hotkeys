namespace Kwerty.DviZe.Win.Hotkeys;

public sealed record Hotkey
{
    public Hotkey()
    {
    }

    public Hotkey(HotkeyModifiers modifiers, Key key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public HotkeyModifiers Modifiers { get; init; }

    public Key Key { get; init; }
}
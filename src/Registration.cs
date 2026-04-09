using System;

namespace Kwerty.DviZe.Win.Hotkeys;

internal sealed class Registration(int id)
{
    public readonly int id = id;
    public Action<Hotkey> callbacks;
}

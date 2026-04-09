using Kwerty.DviZe.Workers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kwerty.DviZe.Win.Hotkeys;

internal sealed class HotkeyListenerSession(HiddenWindow hiddenWindow) : Worker
{
    readonly internal Dictionary<Hotkey, Registration> registrations = [];
    IDisposable hiddenWindowSubscription;

    protected override async Task OnStartingAsync(WorkerStartingContext startingContext)
    {
        hiddenWindowSubscription = await hiddenWindow.SubscribeAsync(Win32.WM_HOTKEY, HandleHiddenWindowEvent, startingContext.CancellationToken).ConfigureAwait(false);
    }

    protected override Task OnStoppingAsync()
    {
        hiddenWindowSubscription.Dispose();
        return Task.CompletedTask;
    }

    void HandleHiddenWindowEvent(HiddenWindowEvent evt)
    {
        if (Context.StoppingToken.IsCancellationRequested)
        {
            return;
        }

        var hotkey = new Hotkey
        {
            Modifiers = (HotkeyModifiers)(evt.LParam & 0xFFFF),
            Key = (Key)((evt.LParam >> 16) & 0xFFFF),
        };

        Action<Hotkey> callbacks = null;
        lock (registrations)
        {
            if (registrations.TryGetValue(hotkey, out var registration))
            {
                callbacks = registration.callbacks;
            }
        }

        callbacks?.Invoke(hotkey);
    }
}

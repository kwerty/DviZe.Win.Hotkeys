using Kwerty.DviZe.Win;
using Kwerty.DviZe.Win.Hotkeys;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApp1;

public class HotkeyExample(HotkeyListener hotkeyListener, ILogger<HotkeyExample> logger) : IHostedService
{
    IDisposable subscription1;
    IDisposable subscription2;
    IDisposable subscription3;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        subscription1 = await hotkeyListener.SubscribeAsync(HotkeyModifiers.Control | HotkeyModifiers.Alt, Key.K, HandleHotkeyEvent, cancellationToken);
        subscription2 = await hotkeyListener.SubscribeAsync(HotkeyModifiers.Shift, Key.H, HandleHotkeyEvent, cancellationToken);
        subscription3 = await hotkeyListener.SubscribeAsync(Key.Enter, HandleHotkeyEvent, cancellationToken);

        logger.LogInformation("Hotkeys registered: Ctrl+Alt+K, Shift+H, Enter (on its own).");
    }

    void HandleHotkeyEvent(Hotkey hotkey)
    {
        logger.LogInformation("Hotkey event! Modifier: {modifier}, Key: {key}", hotkey.Modifiers, hotkey.Key);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        subscription1.Dispose();
        subscription2.Dispose();
        subscription3.Dispose();
        return Task.CompletedTask;
    }
}

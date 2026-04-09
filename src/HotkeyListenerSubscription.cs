using Kwerty.DviZe.Workers;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Kwerty.DviZe.Win.Hotkeys;

internal sealed class HotkeyListenerSubscription(Hotkey hotkey, Action<Hotkey> callback, IWorkerProvider<HotkeyListenerSession> sessionProvider, HiddenWindow hiddenWindow, IThreadAccessor threadAccessor, ILoggerFactory loggerFactory)
    : Worker, IDisposable
{
    static int nextHotkeyId;
    readonly ILogger logger = loggerFactory.CreateLogger<HotkeyListenerSubscription>();
    HotkeyListenerSession session;
    IDisposable sessionReleaser;
    Registration registration;

    protected override async Task OnStartingAsync(WorkerStartingContext startingContext)
    {
        (session, sessionReleaser) = await sessionProvider.LeaseAsync(startingContext.CancellationToken).ConfigureAwait(false);

        try
        {
            await threadAccessor.UIThread;

            lock (session.registrations)
            {
                if (!session.registrations.TryGetValue(hotkey, out registration))
                {
                    registration = new Registration(nextHotkeyId++);
                    session.registrations.Add(hotkey, registration);

                    logger.LogDebug("Registering Hotkey Id: {id}, Modifiers: {modifier}, Key: {key}.", registration.id, hotkey.Modifiers, hotkey.Key);

                    if (Win32.RegisterHotKey(hiddenWindow.Hwnd.Value, registration.id, (uint)hotkey.Modifiers, (uint)hotkey.Key) == 0)
                    {
                        if (Marshal.GetLastWin32Error() == Win32.ERROR_HOTKEY_ALREADY_REGISTERED)
                        {
                            throw new InvalidOperationException("This hotkey has been registered by another process.");
                        }

                        throw Win32Exception.FromLastError(nameof(Win32.RegisterHotKey));
                    }
                }

                registration.callbacks += callback;
            }
        }
        catch
        {
            sessionReleaser.Dispose();
            throw;
        }
    }

    protected override async Task OnStoppingAsync()
    {
        await threadAccessor.UIThread;

        lock (session.registrations)
        {
            registration.callbacks -= callback;

            if (registration.callbacks == null)
            {
                session.registrations.Remove(hotkey);

                if (Win32.UnregisterHotKey(hiddenWindow.Hwnd.Value, registration.id) == 0)
                {
                    var exception = Win32Exception.FromLastError(nameof(Win32.UnregisterHotKey));
                    logger.LogCritical(exception, "Error unregistering hotkey.");
                }
            }
        }

        sessionReleaser.Dispose();
    }

    public void Dispose() => Context.TryStop();
}

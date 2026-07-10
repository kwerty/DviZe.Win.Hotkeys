using Kwerty.DviZe.Workers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kwerty.DviZe.Win.Hotkeys;

public sealed class HotkeyListener : IAsyncDisposable
{
    readonly HiddenWindow hiddenWindow;
    readonly IThreadAccessor threadAccessor;
    readonly ILoggerFactory loggerFactory;
    readonly OnDemand<HotkeyListenerSession> sessionOnDemand;
    readonly Runner<HotkeyListenerSubscription> subscriptionRunner;

    public HotkeyListener(HiddenWindow hiddenWindow, IThreadAccessor threadAccessor, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(hiddenWindow, nameof(hiddenWindow));
        ArgumentNullException.ThrowIfNull(threadAccessor, nameof(threadAccessor));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));

        this.hiddenWindow = hiddenWindow;
        this.threadAccessor = threadAccessor;
        this.loggerFactory = loggerFactory;
        sessionOnDemand = new OnDemand<HotkeyListenerSession>(new OnDemandOptions { RetryPolicy = OnDemandRetryPolicy.RetryAfterWorkerFailedToStart }, () => new(hiddenWindow), loggerFactory);
        subscriptionRunner = new Runner<HotkeyListenerSubscription>(loggerFactory);
    }

    /// <summary>
    /// Subscribes to hotkey events.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> which should be disposed to unsubscribe.</returns>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ObjectDisposedException" />
    public Task<IDisposable> SubscribeAsync(Key key, Action<Hotkey> callback, CancellationToken cancellationToken = default)
        => SubscribeAsyncCore(new Hotkey(HotkeyModifiers.None, key), callback, cancellationToken);

    /// <summary>
    /// Subscribes to hotkey events.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> which should be disposed to unsubscribe.</returns>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ObjectDisposedException" />
    public Task<IDisposable> SubscribeAsync(HotkeyModifiers modifiers, Key key, Action<Hotkey> callback, CancellationToken cancellationToken = default)
        => SubscribeAsyncCore(new Hotkey(modifiers, key), callback, cancellationToken);

    async Task<IDisposable> SubscribeAsyncCore(Hotkey hotkey, Action<Hotkey> callback, CancellationToken cancellationToken)
    {
        var subscription = new HotkeyListenerSubscription(hotkey, callback, sessionOnDemand, hiddenWindow, threadAccessor, loggerFactory);
        await subscriptionRunner.StartWorkerAsync(subscription, cancellationToken).ConfigureAwait(false);
        return subscription;
    }

    public async ValueTask DisposeAsync()
    {
        await subscriptionRunner.DisposeAsync().ConfigureAwait(false);
        await sessionOnDemand.DisposeAsync().ConfigureAwait(false);
    }
}

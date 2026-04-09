# DviZe.Win.Hotkeys

A .NET 10 library for registering global hotkeys in console and non-UI applications.

Designed to work alongside `MessagePump` and `HiddenWindow` from [DviZe.Win.Common](https://github.com/kwerty/DviZe.Win.Common), which provide the Win32 message loop and window handle needed to receive hotkey events.

```csharp
using Kwerty.DviZe.Win;
using Kwerty.DviZe.Win.Hotkeys;

await using var hotkeyListener = new HotkeyListener(hiddenWindow, messagePump.ThreadAccessor, loggerFactory);

var subscription = await hotkeyListener.SubscribeAsync(
    HotkeyModifiers.Control | HotkeyModifiers.Alt,
    Key.K,
    evt => Console.WriteLine($"Hotkey pressed: {evt.Modifiers}+{evt.Key}"), 
    cancellationToken);

subscription.Dispose(); // Unsubscribe.
```

See [ExampleApp1](examples/ExampleApp1/) for a more structured example with dependency injection.

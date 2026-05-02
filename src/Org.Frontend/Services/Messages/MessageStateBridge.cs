namespace Org.Frontend.Services.Messages;

public sealed class MessageStateBridge
{
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}

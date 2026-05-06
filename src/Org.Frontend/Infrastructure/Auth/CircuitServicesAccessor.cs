using System.Threading;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Org.Frontend.Infrastructure.Auth;

/// <summary>
/// Bridges circuit-scoped services to places (e.g. HttpClient handlers)
/// that may otherwise execute in a different DI scope.
/// </summary>
public sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<Holder> Current = new();

    public IServiceProvider? Services
    {
        get => Current.Value?.Services;
        set
        {
            var holder = Current.Value;
            if (holder is not null)
            {
                holder.Services = null;
            }

            if (value is not null)
            {
                Current.Value = new Holder { Services = value };
            }
        }
    }

    private sealed class Holder
    {
        public IServiceProvider? Services;
    }
}

public sealed class CircuitServicesAccessorHandler(
    IServiceProvider services,
    CircuitServicesAccessor accessor) : CircuitHandler
{
    private readonly IServiceProvider _services = services;
    private readonly CircuitServicesAccessor _accessor = accessor;

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            _accessor.Services = _services;
            try
            {
                await next(context);
            }
            finally
            {
                _accessor.Services = null;
            }
        };
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _accessor.Services = null;
        return Task.CompletedTask;
    }
}

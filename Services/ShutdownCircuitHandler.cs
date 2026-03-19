using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CursilloWeb.Services;

public class ShutdownCircuitHandler : CircuitHandler
{
    private readonly IHostApplicationLifetime _lifetime;
    private static int _circuits = 0;
    private static bool _appHasStarted = false;

    public ShutdownCircuitHandler(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _appHasStarted = true;
        Interlocked.Increment(ref _circuits);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (Interlocked.Decrement(ref _circuits) == 0 && _appHasStarted)
        {
            // Adding a small delay handle page refreshes where old circuit closes before the new one opens
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (_circuits == 0)
                {
                    _lifetime.StopApplication();
                }
            });
        }
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

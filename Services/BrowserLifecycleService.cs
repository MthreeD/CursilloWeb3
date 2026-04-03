using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CursilloWeb.Services;

public class BrowserLifecycleService
{
    private int _circuits = 0;
    private readonly IHostApplicationLifetime _lifetime;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    public BrowserLifecycleService(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public void CircuitOpened()
    {
        lock (_lock)
        {
            _circuits++;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
        }
    }

    public void CircuitClosed()
    {
        lock (_lock)
        {
            _circuits--;
            if (_circuits <= 0)
            {
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                // Wait 3 seconds for potential reconnect or new tab validation
                Task.Delay(3000, token).ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        _lifetime.StopApplication();
                    }
                });
            }
        }
    }
}

public class ShutdownCircuitHandler :CircuitHandler
{
    private readonly BrowserLifecycleService _lifecycleService;

    public ShutdownCircuitHandler(BrowserLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _lifecycleService.CircuitOpened();
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _lifecycleService.CircuitClosed();
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

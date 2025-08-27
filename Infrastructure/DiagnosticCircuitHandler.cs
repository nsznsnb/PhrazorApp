using Microsoft.AspNetCore.Components.Server.Circuits;

namespace PhrazorApp.Infrastructure
{
    public sealed class DiagnosticCircuitHandler : CircuitHandler
    {
        private readonly ILogger<DiagnosticCircuitHandler> _logger;
        public DiagnosticCircuitHandler(ILogger<DiagnosticCircuitHandler> logger) => _logger = logger;

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken ct)
        {
            _logger.LogWarning("CIRCUIT OPENED {CircuitId}", circuit.Id);
            return Task.CompletedTask;
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken ct)
        {
            _logger.LogWarning("CONNECTION UP {CircuitId}", circuit.Id);
            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken ct)
        {
            _logger.LogError("CONNECTION DOWN {CircuitId}", circuit.Id);
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken ct)
        {
            _logger.LogError("CIRCUIT CLOSED {CircuitId}", circuit.Id);
            return Task.CompletedTask;
        }
    }
}

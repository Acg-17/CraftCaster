using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using Xunit.Abstractions;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Tests.Manual;

public sealed class ManualOperationTests
{
    private readonly ITestOutputHelper _output;

    public ManualOperationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Mazo_Compartir_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:Mazo:compartir:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "compartir";
        var operationType = "Custom";
        var executeManualInvocation = false;
        MazoService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:Mazo:compartir:invoke) ENABLED START*/
        await ((dynamic)service!).CompartirAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:Mazo:compartir:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task Carta_Compartir_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:Carta:compartir:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "compartir";
        var operationType = "Custom";
        var executeManualInvocation = false;
        CartaService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:Carta:compartir:invoke) ENABLED START*/
        await ((dynamic)service!).CompartirAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:Carta:compartir:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task Estado_Activar_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:Estado:activar:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "activar";
        var operationType = "Custom";
        var executeManualInvocation = false;
        EstadoService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:Estado:activar:invoke) ENABLED START*/
        await ((dynamic)service!).ActivarAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:Estado:activar:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task Habilidad_Activar_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:Habilidad:activar:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "activar";
        var operationType = "Custom";
        var executeManualInvocation = false;
        HabilidadService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:Habilidad:activar:invoke) ENABLED START*/
        await ((dynamic)service!).ActivarAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:Habilidad:activar:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task Partida_CambiarEstado_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:Partida:cambiarEstado:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "cambiarEstado";
        var operationType = "Custom";
        var executeManualInvocation = false;
        PartidaService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:Partida:cambiarEstado:invoke) ENABLED START*/
        await ((dynamic)service!).CambiarEstadoAsync(default(int), EstadoPartida.Espera);
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:Partida:cambiarEstado:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task JugadorPartida_RobarCarta_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:robarCarta:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "robarCarta";
        var operationType = "Custom";
        var executeManualInvocation = false;
        JugadorPartidaService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:robarCarta:invoke) ENABLED START*/
        await ((dynamic)service!).RobarCartaAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:JugadorPartida:robarCarta:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task JugadorPartida_PasarTurno_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:pasarTurno:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "pasarTurno";
        var operationType = "Custom";
        var executeManualInvocation = false;
        JugadorPartidaService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:pasarTurno:invoke) ENABLED START*/
        await ((dynamic)service!).PasarTurnoAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:JugadorPartida:pasarTurno:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    [Fact]
    public async Task JugadorPartida_LanzarCarta_Custom_Manual_Contract()
    {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:lanzarCarta:arrange) ENABLED START*/
        // Create a test unit of work and assign service before enabling the generated invocation.
        /*PROTECTED REGION END*/

        var operationName = "lanzarCarta";
        var operationType = "Custom";
        var executeManualInvocation = false;
        JugadorPartidaService? service = null;
        object? invocationResult = null;

        if (executeManualInvocation)
        {
        /*PROTECTED REGION ID(test:custom:JugadorPartida:lanzarCarta:invoke) ENABLED START*/
        await ((dynamic)service!).LanzarCartaAsync(default(int));
        /*PROTECTED REGION END*/
        }
        _ = invocationResult;

        /*PROTECTED REGION ID(test:custom:JugadorPartida:lanzarCarta:assert) ENABLED START*/
        EnsureContractOutput(operationName, operationType);
        /*PROTECTED REGION END*/
    }

    private const string SuccessColor = "\u001b[32m";
    private const string ErrorColor = "\u001b[31m";
    private const string ResetColor = "\u001b[0m";

    private void EnsureContractOutput(string operationName, string operationType)
    {
        Ensure(!string.IsNullOrWhiteSpace(operationName), $"{operationName} declares the manual operation name.");
        Ensure(!string.IsNullOrWhiteSpace(operationType), $"{operationName} declares the manual operation type.");
        WriteTestMessage(true, $"{operationName} contract uses operation type {operationType}.");
    }

    private void Ensure(bool condition, string message)
    {
        WriteTestMessage(condition, message);
        Assert.True(condition, message);
    }

    private void WriteTestMessage(bool condition, string message)
    {
        var color = condition ? SuccessColor : ErrorColor;
        var status = condition ? "[OK] " : "[FAIL] ";
        _output.WriteLine($"{color}{status}{message}{ResetColor}");
    }

}

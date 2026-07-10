using CalculadoraEVTC.Application.UseCases;
using CalculadoraEVTC.Domain.Exceptions;
using CalculadoraEVTC.Domain.Interfaces;
using CalculadoraEVTC.Infrastructure.Data;
using CalculadoraEVTC.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/calculadora-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Calculadora EVTC API", Version = "v1" });
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    builder.Services.AddScoped<ICotacaoRepository, CotacaoRepository>();
    builder.Services.AddScoped<CalcularInvestimentoUseCase>();
    builder.Services.AddSingleton<DatabaseInitializer>();

    var app = builder.Build();

    // Inicializar banco
    using (var scope = app.Services.CreateScope())
    {
        var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await dbInit.InicializarAsync();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
    app.UseSerilogRequestLogging();

    // Endpoint principal
    app.MapPost("/api/calcular", async (
        CalcularInvestimentoUseCase useCase,
        CalculadoraEVTC.Application.DTOs.CalculoRequestDto request) =>
    {
        try
        {
            var resultado = await useCase.ExecutarAsync(request);
            return Results.Ok(resultado);
        }
        catch (PeriodoInvalidoException ex)
        {
            return Results.BadRequest(new { erro = ex.Message });
        }
        catch (CotacaoNaoEncontradaException ex)
        {
            return Results.NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro inesperado ao calcular investimento");
            return Results.Problem("Erro interno ao processar o cálculo.");
        }
    })
    .WithName("CalcularInvestimento")
    .WithSummary("Calcula o valor atualizado de um investimento pós-fixado")
    .WithDescription("Recebe valor, data inicial e data final e retorna o fator acumulado e valor atualizado.")
    .Produces<CalculadoraEVTC.Application.DTOs.CalculoResponseDto>()
    .ProducesProblem(400)
    .ProducesProblem(404);

    app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
        .WithName("Health");

    Log.Information("Calculadora EVTC API iniciada.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha ao iniciar a aplicação.");
}
finally
{
    Log.CloseAndFlush();
}

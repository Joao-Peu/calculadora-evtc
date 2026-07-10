using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CalculadoraEVTC.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=cotacoes.db";
        _logger = logger;
    }

    public async Task InicializarAsync()
    {
        _logger.LogInformation("Inicializando banco de dados...");

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Cotacao (
                id INT PRIMARY KEY,
                data DATE NOT NULL,
                indexador varchar(30) NOT NULL,
                valor DECIMAL(10,2) NOT NULL
            )");

        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Cotacao");
        if (count > 0)
        {
            _logger.LogInformation("Banco já populado com {Count} cotações.", count);
            return;
        }

        _logger.LogInformation("Inserindo cotações iniciais...");

        var inserts = @"
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (1, '2025-01-01', 'SQI', 10.50);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (2, '2025-01-02', 'SQI', 10.50);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (3, '2025-01-03', 'SQI', 10.50);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (6, '2025-01-06', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (7, '2025-01-07', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (8, '2025-01-08', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (9, '2025-01-09', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (10, '2025-01-10', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (13, '2025-01-13', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (14, '2025-01-14', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (15, '2025-01-15', 'SQI', 12.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (16, '2025-01-16', 'SQI', 9.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (17, '2025-01-17', 'SQI', 9.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (20, '2025-01-20', 'SQI', 9.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (21, '2025-01-21', 'SQI', 7.75);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (22, '2025-01-22', 'SQI', 7.75);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (23, '2025-01-23', 'SQI', 7.75);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (24, '2025-01-24', 'SQI', 7.75);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (27, '2025-01-27', 'SQI', 8.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (28, '2025-01-28', 'SQI', 8.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (29, '2025-01-29', 'SQI', 8.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (30, '2025-01-30', 'SQI', 8.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (31, '2025-01-31', 'SQI', 8.25);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (32, '2025-03-13', 'SQI', 12.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (33, '2025-03-14', 'SQI', 12.50);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (34, '2025-03-17', 'SQI', 11.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (35, '2025-03-18', 'SQI', 12.20);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (36, '2025-03-19', 'SQI', 13.00);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (37, '2025-03-20', 'SQI', 12.40);
            INSERT INTO Cotacao (id, data, indexador, valor) VALUES (38, '2025-03-21', 'SQI', 12.70);";

        foreach (var insert in inserts.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (!string.IsNullOrEmpty(insert))
                await conn.ExecuteAsync(insert);

        _logger.LogInformation("Banco inicializado com sucesso.");
    }
}

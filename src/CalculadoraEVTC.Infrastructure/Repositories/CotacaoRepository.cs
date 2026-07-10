using CalculadoraEVTC.Domain.Entities;
using CalculadoraEVTC.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace CalculadoraEVTC.Infrastructure.Repositories;

public class CotacaoRepository : ICotacaoRepository
{
    private readonly string _connectionString;

    public CotacaoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=cotacoes.db";
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<Cotacao?> ObterPorDataAsync(DateTime data)
    {
        using var conn = CreateConnection();
        var sql = "SELECT id, data, indexador, valor FROM Cotacao WHERE data = @Data";
        var result = await conn.QueryFirstOrDefaultAsync<CotacaoDb>(sql, new { Data = data.ToString("yyyy-MM-dd") });
        return result is null ? null : new Cotacao((int)result.id, DateTime.Parse(result.data), result.indexador, (decimal)result.valor);
    }

    public async Task<IEnumerable<Cotacao>> ObterPorPeriodoAsync(DateTime dataInicial, DateTime dataFinal)
    {
        using var conn = CreateConnection();
        var sql = @"SELECT id, data, indexador, valor FROM Cotacao 
                    WHERE data >= @DataInicial AND data <= @DataFinal 
                    ORDER BY data";
        var results = await conn.QueryAsync<CotacaoDb>(sql, new
        {
            DataInicial = dataInicial.ToString("yyyy-MM-dd"),
            DataFinal = dataFinal.ToString("yyyy-MM-dd")
        });
        return results.Select(r => new Cotacao((int)r.id, DateTime.Parse(r.data), r.indexador, (decimal)r.valor));
    }

    public async Task<DateTime?> ObterUltimoDiaUtilAnteriorAsync(DateTime data)
    {
        using var conn = CreateConnection();
        var sql = @"SELECT MAX(data) FROM Cotacao WHERE data < @Data";
        var result = await conn.ExecuteScalarAsync<string?>(sql, new { Data = data.ToString("yyyy-MM-dd") });
        return result is null ? null : DateTime.Parse(result);
    }

    private class CotacaoDb
    {
        public long id { get; set; }
        public string data { get; set; } = "";
        public string indexador { get; set; } = "";
        public double valor { get; set; }
    }
}
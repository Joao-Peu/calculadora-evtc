using CalculadoraEVTC.Domain.Entities;

namespace CalculadoraEVTC.Domain.Interfaces;

public interface ICotacaoRepository
{
    Task<Cotacao?> ObterPorDataAsync(DateTime data);
    Task<IEnumerable<Cotacao>> ObterPorPeriodoAsync(DateTime dataInicial, DateTime dataFinal);
    Task<DateTime?> ObterUltimoDiaUtilAnteriorAsync(DateTime data);
}

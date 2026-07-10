using CalculadoraEVTC.Domain.Entities;
using CalculadoraEVTC.Domain.ValueObjects;

namespace CalculadoraEVTC.Domain.Services;

public static class CalculadoraJurosService
{
    private const int DiasUteisAno = 252;
    private const int CasasDecimaisFatorDiario = 8;
    private const int CasasDecimaisFatorAcumulado = 16;
    private const int CasasDecimaisValorFinal = 8;

    /// <summary>
    /// Calcula o fator diário a partir da taxa anual.
    /// fator = (1 + taxa/100)^(1/252), arredondado na 8ª casa decimal.
    /// </summary>
    public static decimal CalcularFatorDiario(decimal taxaAnual)
    {
        double taxa = (double)taxaAnual;
        double fator = Math.Pow(1 + taxa / 100.0, 1.0 / DiasUteisAno);
        return Math.Round((decimal)fator, CasasDecimaisFatorDiario, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calcula o fator acumulado como produtório dos fatores diários.
    /// Truncado na 16ª casa decimal.
    /// </summary>
    public static decimal CalcularFatorAcumulado(IEnumerable<decimal> fatoresDiarios)
    {
        decimal fatorAcumulado = 1m;
        foreach (var fator in fatoresDiarios)
            fatorAcumulado *= fator;

        return Truncar(fatorAcumulado, CasasDecimaisFatorAcumulado);
    }

    /// <summary>
    /// Calcula o valor atualizado = truncar(valorAplicado * fatorAcumulado, 8).
    /// </summary>
    public static decimal CalcularValorAtualizado(decimal valorAplicado, decimal fatorAcumulado)
    {
        return Truncar(valorAplicado * fatorAcumulado, CasasDecimaisValorFinal);
    }

    /// <summary>
    /// Trunca um valor decimal na quantidade de casas decimais especificada (sem arredondamento).
    /// </summary>
    public static decimal Truncar(decimal valor, int casasDecimais)
    {
        decimal fator = (decimal)Math.Pow(10, casasDecimais);
        return Math.Truncate(valor * fator) / fator;
    }

    /// <summary>
    /// Verifica se uma data é dia útil (não é sábado nem domingo).
    /// </summary>
    public static bool EhDiaUtil(DateTime data)
        => data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday;

    /// <summary>
    /// Retorna todos os dias úteis entre dataInicial (inclusive) e dataFinal (exclusive).
    /// </summary>
    public static IEnumerable<DateTime> ObterDiasUteis(DateTime dataInicial, DateTime dataFinal)
    {
        var dias = new List<DateTime>();
        var dataAtual = dataInicial;
        while (dataAtual < dataFinal)
        {
            if (EhDiaUtil(dataAtual))
                dias.Add(dataAtual);
            dataAtual = dataAtual.AddDays(1);
        }
        return dias;
    }
}

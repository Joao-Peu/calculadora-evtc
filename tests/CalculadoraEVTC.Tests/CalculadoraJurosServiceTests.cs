using CalculadoraEVTC.Domain.Services;
using FluentAssertions;

namespace CalculadoraEVTC.Tests;

public class CalculadoraJurosServiceTests
{
    [Fact]
    public void CalcularFatorDiario_TaxaAnual12_DeveRetornarValorCorreto()
    {
        // Arrange
        var taxaAnual = 12m;
        var esperado = 1.00044982m;

        // Act
        var resultado = CalculadoraJurosService.CalcularFatorDiario(taxaAnual);

        // Assert
        resultado.Should().Be(esperado,
            "fator diário de 12% a.a. deve ser 1,00044982 conforme especificação");
    }

    [Theory]
    [InlineData(12.50, 1.00044982)] // 14/03/2025 conforme exemplo
    [InlineData(11.00, 1.00041421)] // aproximado
    [InlineData(13.00, 1.00048511)] // aproximado
    public void CalcularFatorDiario_DiversasTaxas_DeveArredondarNa8aCasaDecimal(
        double taxa, double esperado)
    {
        var resultado = CalculadoraJurosService.CalcularFatorDiario((decimal)taxa);
        resultado.Should().BeApproximately((decimal)esperado, 0.00000001m);
    }

    [Fact]
    public void CalcularFatorAcumulado_FatoresDoExemplo_DeveRetornarValorCorreto()
    {
        // Arrange - fatores do exemplo do teste
        var fatores = new[]
        {
            1.00044982m, // 14/03
            1.00041421m, // 17/03 (aproximado, usando 11%)
            1.00041421m, // 18/03
            1.00045690m, // 19/03
            1.00048511m, // 20/03
            1.00046397m, // 21/03
        };

        // Act
        var resultado = CalculadoraJurosService.CalcularFatorAcumulado(fatores);

        // Assert
        resultado.Should().BeGreaterThan(1m, "fator acumulado deve ser maior que 1");
        resultado.Should().BeLessThan(1.01m, "fator acumulado não deve ultrapassar 1% para este período");
    }

    [Fact]
    public void CalcularValorAtualizado_ExemploEsperado_DeveRetornar10027_40()
    {
        // Arrange - fator acumulado final do exemplo
        var valorAplicado = 10000m;
        var fatorAcumulado = 1.0027406329m;

        // Act
        var resultado = CalculadoraJurosService.CalcularValorAtualizado(valorAplicado, fatorAcumulado);

        // Assert
        resultado.Should().Be(10027.40m,
            "valor atualizado de R$10.000 com fator 1,00274063 deve ser R$10.027,40");
    }

    [Fact]
    public void Truncar_ValorComMaisDecimais_DeveTruncarSemArredondar()
    {
        // Arrange
        var valor = 1.123456789m;

        // Act
        var resultado = CalculadoraJurosService.Truncar(valor, 8);

        // Assert
        resultado.Should().Be(1.12345678m, "truncamento não deve arredondar");
    }

    [Theory]
    [InlineData(2025, 3, 13, true)]  // quinta
    [InlineData(2025, 3, 14, true)]  // sexta
    [InlineData(2025, 3, 15, false)] // sábado
    [InlineData(2025, 3, 16, false)] // domingo
    [InlineData(2025, 3, 17, true)]  // segunda
    public void EhDiaUtil_DeveIdentificarCorretamente(int ano, int mes, int dia, bool esperado)
    {
        var data = new DateTime(ano, mes, dia);
        CalculadoraJurosService.EhDiaUtil(data).Should().Be(esperado);
    }

    [Fact]
    public void ObterDiasUteis_PeriodoComFimDeSemana_DeveExcluirFinaisDeSemana()
    {
        // 13/03/2025 (qui) a 21/03/2025 (sex) = 7 dias úteis: 14,17,18,19,20,21
        var inicio = new DateTime(2025, 3, 14);
        var fim = new DateTime(2025, 3, 21);

        var dias = CalculadoraJurosService.ObterDiasUteis(inicio, fim.AddDays(1)).ToList();

        dias.Should().HaveCount(6);
        dias.Should().NotContain(d => d.DayOfWeek == DayOfWeek.Saturday);
        dias.Should().NotContain(d => d.DayOfWeek == DayOfWeek.Sunday);
    }

    [Fact]
    public void CalcularFatorAcumulado_SemFatores_DeveRetornarUm()
    {
        var resultado = CalculadoraJurosService.CalcularFatorAcumulado(Array.Empty<decimal>());
        resultado.Should().Be(1m);
    }
}

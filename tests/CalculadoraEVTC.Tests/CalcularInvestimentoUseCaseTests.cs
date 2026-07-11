using CalculadoraEVTC.Application.DTOs;
using CalculadoraEVTC.Application.UseCases;
using CalculadoraEVTC.Domain.Entities;
using CalculadoraEVTC.Domain.Exceptions;
using CalculadoraEVTC.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CalculadoraEVTC.Tests;

public class CalcularInvestimentoUseCaseTests
{
    private readonly Mock<ICotacaoRepository> _repositoryMock;
    private readonly Mock<ILogger<CalcularInvestimentoUseCase>> _loggerMock;
    private readonly CalcularInvestimentoUseCase _useCase;

    public CalcularInvestimentoUseCaseTests()
    {
        _repositoryMock = new Mock<ICotacaoRepository>();
        _loggerMock = new Mock<ILogger<CalcularInvestimentoUseCase>>();
        _useCase = new CalcularInvestimentoUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecutarAsync_ExemploDoTeste_DeveRetornarValorCorreto()
    {
        // Arrange - exemplo do PDF: R$10.000 de 13/03 a 21/03/2025
        var request = new CalculoRequestDto(10000m, new DateTime(2025, 3, 13), new DateTime(2025, 3, 21));

        ConfigurarCotacoes(new Dictionary<DateTime, decimal>
        {
            { new DateTime(2025, 3, 13), 12.00m },
            { new DateTime(2025, 3, 14), 12.50m },
            { new DateTime(2025, 3, 17), 11.00m },
            { new DateTime(2025, 3, 18), 12.20m },
            { new DateTime(2025, 3, 19), 13.00m },
            { new DateTime(2025, 3, 20), 12.40m },
        });

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        resultado.ValorAtualizado.Should().Be(10027.40m,
            "valor atualizado deve ser R$10.027,40 conforme exemplo do teste");
        resultado.DiasUteis.Should().Be(6);
        resultado.FatorAcumulado.Should().BeApproximately(1.0027406329m, 0.0000001m);
    }

    [Fact]
    public async Task ExecutarAsync_ValorNegativo_DeveLancarPeriodoInvalidoException()
    {
        var request = new CalculoRequestDto(-1000m, DateTime.Today, DateTime.Today.AddDays(5));

        await _useCase.Invoking(u => u.ExecutarAsync(request))
            .Should().ThrowAsync<PeriodoInvalidoException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task ExecutarAsync_DataFinalAnteriorInicial_DeveLancarPeriodoInvalidoException()
    {
        var request = new CalculoRequestDto(1000m, DateTime.Today, DateTime.Today.AddDays(-1));

        await _useCase.Invoking(u => u.ExecutarAsync(request))
            .Should().ThrowAsync<PeriodoInvalidoException>()
            .WithMessage("*Data final deve ser posterior*");
    }

    [Fact]
    public async Task ExecutarAsync_CotacaoNaoEncontrada_DeveLancarCotacaoNaoEncontradaException()
    {
        var request = new CalculoRequestDto(1000m, new DateTime(2025, 3, 13), new DateTime(2025, 3, 14));

        _repositoryMock.Setup(r => r.ObterPorDataAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((Cotacao?)null);

        await _useCase.Invoking(u => u.ExecutarAsync(request))
            .Should().ThrowAsync<CotacaoNaoEncontradaException>();
    }

    [Fact]
    public async Task ExecutarAsync_PeriodoSemDiasUteis_DeveRetornarFatorUm()
    {
        // Sábado a domingo = sem dias úteis
        var request = new CalculoRequestDto(1000m, new DateTime(2025, 3, 13), new DateTime(2025, 3, 14));

        ConfigurarCotacoes(new Dictionary<DateTime, decimal>
        {
            { new DateTime(2025, 3, 13), 12.00m }
        });

        var resultado = await _useCase.ExecutarAsync(request);

        resultado.FatorAcumulado.Should().Be(1m);
        resultado.ValorAtualizado.Should().Be(1000m);
    }

    private void ConfigurarCotacoes(Dictionary<DateTime, decimal> cotacoes)
    {
        foreach (var kvp in cotacoes)
        {
            var data = kvp.Key;
            var cotacao = new Cotacao(0, data, "SQI", kvp.Value);
            _repositoryMock.Setup(r => r.ObterPorDataAsync(data)).ReturnsAsync(cotacao);
        }
    }
}

using CalculadoraEVTC.Application.DTOs;
using CalculadoraEVTC.Domain.Exceptions;
using CalculadoraEVTC.Domain.Interfaces;
using CalculadoraEVTC.Domain.Services;
using Microsoft.Extensions.Logging;

namespace CalculadoraEVTC.Application.UseCases;

public class CalcularInvestimentoUseCase
{
    private readonly ICotacaoRepository _cotacaoRepository;
    private readonly ILogger<CalcularInvestimentoUseCase> _logger;

    public CalcularInvestimentoUseCase(
        ICotacaoRepository cotacaoRepository,
        ILogger<CalcularInvestimentoUseCase> logger)
    {
        _cotacaoRepository = cotacaoRepository;
        _logger = logger;
    }

    public async Task<CalculoResponseDto> ExecutarAsync(CalculoRequestDto request)
    {
        _logger.LogInformation(
            "Iniciando cálculo. ValorAplicado: {Valor}, DataInicial: {Inicio}, DataFinal: {Fim}",
            request.ValorAplicado, request.DataInicial, request.DataFinal);

        ValidarRequest(request);

        var primeiroDiaRentavel = request.DataInicial.AddDays(1);
        var diasUteis = CalculadoraJurosService
            .ObterDiasUteis(primeiroDiaRentavel, request.DataFinal.AddDays(1))
            .ToList();

        _logger.LogInformation("Total de dias úteis no período: {DiasUteis}", diasUteis.Count);

        var detalhes = new List<DetalhesDiarioDto>();
        var fatoresDiarios = new List<decimal>();

        foreach (var dia in diasUteis)
        {
            var diaAnterior = await ObterDiaUtilAnteriorAsync(dia);

            var cotacao = await _cotacaoRepository.ObterPorDataAsync(diaAnterior)
                ?? throw new CotacaoNaoEncontradaException(diaAnterior);

            var fatorDiario = CalculadoraJurosService.CalcularFatorDiario(cotacao.Valor);
            fatoresDiarios.Add(fatorDiario);

            var fatorAcumuladoTruncado = CalculadoraJurosService.CalcularFatorAcumulado(fatoresDiarios);
            var valorAtualizado = CalculadoraJurosService.CalcularValorAtualizado(
                request.ValorAplicado, fatorAcumuladoTruncado);

            _logger.LogDebug(
                "Dia: {Data} | Taxa: {Taxa}% | FatorDiario: {FD} | FatorAcumulado: {FA} | ValorAtualizado: {VA}",
                dia, cotacao.Valor, fatorDiario, fatorAcumuladoTruncado, valorAtualizado);

            detalhes.Add(new DetalhesDiarioDto(
                dia, cotacao.Valor, fatorDiario, fatorAcumuladoTruncado, valorAtualizado));
        }

        var fatorFinal = CalculadoraJurosService.CalcularFatorAcumulado(fatoresDiarios);
        var valorFinal = CalculadoraJurosService.CalcularValorAtualizado(
            request.ValorAplicado, fatorFinal);

        _logger.LogInformation(
            "Cálculo concluído. FatorAcumulado: {FA}, ValorAtualizado: {VA}",
            fatorFinal, valorFinal);

        return new CalculoResponseDto(
            request.ValorAplicado,
            request.DataInicial,
            request.DataFinal,
            fatorFinal,
            valorFinal,
            diasUteis.Count,
            detalhes);
    }

    private void ValidarRequest(CalculoRequestDto request)
    {
        if (request.ValorAplicado <= 0)
            throw new PeriodoInvalidoException("Valor aplicado deve ser maior que zero.");

        if (request.DataFinal <= request.DataInicial)
            throw new PeriodoInvalidoException("Data final deve ser posterior à data inicial.");

        if (!CalculadoraJurosService.EhDiaUtil(request.DataInicial))
            throw new PeriodoInvalidoException("Data inicial deve ser um dia útil.");
    }

    private Task<DateTime> ObterDiaUtilAnteriorAsync(DateTime data)
    {
        var candidato = data.AddDays(-1);
        while (!CalculadoraJurosService.EhDiaUtil(candidato))
            candidato = candidato.AddDays(-1);
        return Task.FromResult(candidato);
    }
}
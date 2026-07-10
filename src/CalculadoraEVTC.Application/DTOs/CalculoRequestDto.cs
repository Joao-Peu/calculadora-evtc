namespace CalculadoraEVTC.Application.DTOs;

public record CalculoRequestDto(
    decimal ValorAplicado,
    DateTime DataInicial,
    DateTime DataFinal
);

public record CalculoResponseDto(
    decimal ValorAplicado,
    DateTime DataInicial,
    DateTime DataFinal,
    decimal FatorAcumulado,
    decimal ValorAtualizado,
    int DiasUteis,
    IEnumerable<DetalhesDiarioDto> Detalhes
);

public record DetalhesDiarioDto(
    DateTime DataReferencia,
    decimal TaxaAnual,
    decimal FatorDiario,
    decimal FatorAcumulado,
    decimal ValorAtualizado
);

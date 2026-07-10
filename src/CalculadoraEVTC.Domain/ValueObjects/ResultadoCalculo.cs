namespace CalculadoraEVTC.Domain.ValueObjects;

public record ResultadoCalculo(
    decimal ValorAplicado,
    DateTime DataInicial,
    DateTime DataFinal,
    decimal FatorAcumulado,
    decimal ValorAtualizado,
    int DiasUteis
);

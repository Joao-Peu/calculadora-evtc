namespace CalculadoraEVTC.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class CotacaoNaoEncontradaException : DomainException
{
    public CotacaoNaoEncontradaException(DateTime data)
        : base($"Cotação não encontrada para a data {data:dd/MM/yyyy}.") { }
}

public class PeriodoInvalidoException : DomainException
{
    public PeriodoInvalidoException(string message) : base(message) { }
}

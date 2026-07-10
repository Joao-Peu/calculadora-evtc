namespace CalculadoraEVTC.Domain.Entities;

public class Cotacao
{
    public int Id { get; private set; }
    public DateTime Data { get; private set; }
    public string Indexador { get; private set; }
    public decimal Valor { get; private set; }

    public Cotacao(int id, DateTime data, string indexador, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(indexador))
            throw new ArgumentException("Indexador não pode ser vazio.", nameof(indexador));
        if (valor < 0)
            throw new ArgumentException("Valor da taxa não pode ser negativo.", nameof(valor));

        Id = id;
        Data = data;
        Indexador = indexador;
        Valor = valor;
    }
}

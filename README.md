# Calculadora EVTC — Investimentos Pós-Fixados

API REST em .NET 8 para cálculo de saldo atualizado de aplicações com indexador pós-fixado, seguindo o padrão de 252 dias úteis (juros compostos).

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 8 Minimal API |
| Arquitetura | Clean Architecture + DDD |
| Banco de dados | SQLite + Dapper |
| Logs | Serilog (console + arquivo) |
| Testes | xUnit + FluentAssertions + Moq |
| Frontend | React 18 |
| Container | Docker + docker-compose |

## Arquitetura

```
src/
├── CalculadoraEVTC.Domain/          # Entidades, ValueObjects, Interfaces, Serviços de domínio
├── CalculadoraEVTC.Application/     # Use Cases, DTOs
├── CalculadoraEVTC.Infrastructure/  # Repositórios, Dapper, SQLite, Inicialização de BD
└── CalculadoraEVTC.API/             # Minimal API, Program.cs, Middleware
tests/
└── CalculadoraEVTC.Tests/           # Testes unitários (xUnit + FluentAssertions + Moq)
frontend/                            # React SPA
```

## Regras de Negócio

### Fator Diário
```
fator_diario = (1 + taxa_anual / 100) ^ (1/252)
arredondado na 8ª casa decimal
```

### Fator Acumulado
```
fator_acumulado = ∏ fator_diario_i  (produtório)
truncado na 16ª casa decimal
```

### Valor Atualizado
```
valor_atualizado = truncar(valor_aplicado × fator_acumulado, 8)
```

### Regras de datas
- O primeiro dia **não rende** — a rentabilidade começa no dia posterior
- A taxa do dia D usa a cotação do **dia útil anterior (D-1)**
- São considerados apenas **dias úteis** (sem finais de semana)

## Como rodar

### Com Docker (recomendado)
```bash
docker-compose up --build
```
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Frontend: http://localhost:3000

### Sem Docker

**Backend:**
```bash
cd src/CalculadoraEVTC.API
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

**Testes:**
```bash
cd tests/CalculadoraEVTC.Tests
dotnet test --verbosity normal
```

## Endpoint

### POST /api/calcular

**Request:**
```json
{
  "valorAplicado": 10000.00,
  "dataInicial": "2025-03-13T00:00:00",
  "dataFinal": "2025-03-21T00:00:00"
}
```

**Response:**
```json
{
  "valorAplicado": 10000.00,
  "dataInicial": "2025-03-13",
  "dataFinal": "2025-03-21",
  "fatorAcumulado": 1.0027406329672200,
  "valorAtualizado": 10027.40,
  "diasUteis": 6,
  "detalhes": [
    {
      "dataReferencia": "2025-03-14",
      "taxaAnual": 12.50,
      "fatorDiario": 1.00044982,
      "fatorAcumulado": 1.0004498200000000,
      "valorAtualizado": 10004.49
    }
  ]
}
```

## Validações

- Valor aplicado deve ser maior que zero
- Data final deve ser posterior à data inicial
- Data inicial deve ser dia útil
- Cotação deve existir para todos os dias do período

## Logs

Logs estruturados via Serilog:
- Console em desenvolvimento
- Arquivo rotativo em `logs/calculadora-YYYYMMDD.log`

## Exemplo validado

Aplicação de R$10.000,00 de 13/03/2025 a 21/03/2025:

| Data | Taxa Anual | Fator Diário | Fator Acumulado | Valor Atualizado |
|------|-----------|-------------|----------------|-----------------|
| 14/03 | 12,50% | 1,00044982 | 1,00044982 | R$ 10.004,49 |
| 17/03 | 11,00% | 1,00041421 | 1,00091753 | R$ 10.009,17 |
| 18/03 | 12,20% | 1,00041421 | 1,00133212 | R$ 10.013,32 |
| 19/03 | 13,00% | 1,00045690 | 1,00178962 | R$ 10.017,89 |
| 20/03 | 12,40% | 1,00048511 | 1,00227560 | R$ 10.022,75 |
| 21/03 | 12,70% | 1,00046397 | 1,00274063 | R$ 10.027,40 |

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
| Frontend | Angular 17 |
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
frontend/                            # Angular 17 SPA
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
valor_atualizado = truncar(valor_aplicado × fator_acumulado, 2)
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
- API: http://localhost:5001
- Swagger: http://localhost:5001/swagger
- Frontend: http://localhost:3000

### Sem Docker

**Backend:**
```bash
cd src/CalculadoraEVTC.API
dotnet run
```

> O banco SQLite é criado e populado automaticamente na primeira execução.

**Frontend:**
```bash
cd frontend
npm install
ng serve
```

> **Atenção:** ao rodar sem Docker, altere a URL da API no arquivo `frontend/src/app/app.ts`:
> ```typescript
> // Troque 5001 por 5000
> this.http.post<CalculoResponse>('http://localhost:5000/api/calcular', request)
> ```

Acesse: http://localhost:4200

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
  "dataInicial": "2025-03-13T00:00:00",
  "dataFinal": "2025-03-21T00:00:00",
  "fatorAcumulado": 1.0027406329672246,
  "valorAtualizado": 10027.40,
  "diasUteis": 6,
  "detalhes": [
    {
      "dataReferencia": "2025-03-14T00:00:00",
      "taxaAnual": 12.00,
      "fatorDiario": 1.00044982,
      "fatorAcumulado": 1.0004498200000000,
      "valorAtualizado": 10004.50
    }
  ]
}
```

## Validações

- Valor aplicado deve ser maior que zero
- Data final deve ser posterior à data inicial
- Data inicial deve ser dia útil
- Cotação deve existir para todos os dias do período

## Cotações disponíveis no banco

O seed segue o script oficial fornecido no teste:
- **Janeiro/2025:** 01/01 a 31/01
- **Março/2025:** 13/03 a 21/03

## Logs

Logs estruturados via Serilog:
- Console em tempo real
- Arquivo rotativo em `logs/calculadora-YYYYMMDD.log`

## Exemplo validado

Aplicação de R$10.000,00 de 13/03/2025 a 21/03/2025:

| Data | Fator Diário | Fator Acumulado | Valor Atualizado |
|------|-------------|----------------|-----------------|
| 14/03 | 1,00044982 | 1,00044982000000 | R$ 10.004,50 |
| 17/03 | 1,00046750 | 1,00091753029085 | R$ 10.009,18 |
| 18/03 | 1,00041421 | 1,00133212034107 | R$ 10.013,32 |
| 19/03 | 1,00045690 | 1,00178962898685 | R$ 10.017,90 |
| 20/03 | 1,00048511 | 1,00227560715377 | R$ 10.022,76 |
| 21/03 | 1,00046397 | 1,00274063296722 | R$ 10.027,40 |

> **Nota:** A taxa exibida em cada linha representa a cotação do dia útil anterior (D-1), conforme especificado no enunciado. O valor final de R$10.027,40 valida o cálculo conforme o exemplo do PDF.
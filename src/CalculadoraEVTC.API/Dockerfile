FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/CalculadoraEVTC.API/CalculadoraEVTC.API.csproj", "src/CalculadoraEVTC.API/"]
COPY ["src/CalculadoraEVTC.Application/CalculadoraEVTC.Application.csproj", "src/CalculadoraEVTC.Application/"]
COPY ["src/CalculadoraEVTC.Domain/CalculadoraEVTC.Domain.csproj", "src/CalculadoraEVTC.Domain/"]
COPY ["src/CalculadoraEVTC.Infrastructure/CalculadoraEVTC.Infrastructure.csproj", "src/CalculadoraEVTC.Infrastructure/"]

RUN dotnet restore "src/CalculadoraEVTC.API/CalculadoraEVTC.API.csproj"

COPY . .

RUN dotnet build "src/CalculadoraEVTC.API/CalculadoraEVTC.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/CalculadoraEVTC.API/CalculadoraEVTC.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/data /app/logs

ENTRYPOINT ["dotnet", "CalculadoraEVTC.API.dll"]
export interface CalculoRequest {
  valorAplicado: number;
  dataInicial: string;
  dataFinal: string;
}

export interface DetalhesDiario {
  dataReferencia: string;
  taxaAnual: number;
  fatorDiario: number;
  fatorAcumulado: number;
  valorAtualizado: number;
}

export interface CalculoResponse {
  valorAplicado: number;
  dataInicial: string;
  dataFinal: string;
  fatorAcumulado: number;
  valorAtualizado: number;
  diasUteis: number;
  detalhes: DetalhesDiario[];
}
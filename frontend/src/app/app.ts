import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { CalculoRequest, CalculoResponse } from './models/calculo.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  valorAplicado = 10000;
  dataInicial = '2025-03-13';
  dataFinal = '2025-03-21';

  resultado: CalculoResponse | null = null;
  erro: string | null = null;
  loading = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  get rendimento(): number {
    if (!this.resultado) return 0;
    return this.resultado.valorAtualizado - this.resultado.valorAplicado;
  }

  formatarData(dataStr: string): string {
    const data = new Date(dataStr);
    const corrigida = new Date(data.getTime() + data.getTimezoneOffset() * 60000);
    return corrigida.toLocaleDateString('pt-BR');
  }

  formatarFator(valor: number): string {
    return valor.toFixed(16);
  }

  formatarMoeda(valor: number): string {
    return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  calcular(): void {
    this.erro = null;
    this.resultado = null;
    this.loading = true;

    const request: CalculoRequest = {
      valorAplicado: this.valorAplicado,
      dataInicial: this.dataInicial + 'T00:00:00',
      dataFinal: this.dataFinal + 'T00:00:00',
    };

    this.http.post<CalculoResponse>('http://localhost:5001/api/calcular', request).subscribe({
      next: (res) => {
        console.log('RESPOSTA:', res);
        this.resultado = res;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.log('ERRO:', err);
        this.erro = err.error?.erro || 'Erro ao conectar com a API.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
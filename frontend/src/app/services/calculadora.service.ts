import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CalculoRequest, CalculoResponse } from '../models/calculo.model';

@Injectable({
  providedIn: 'root'
})
export class CalculadoraService {
  private readonly apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  calcular(request: CalculoRequest): Observable<CalculoResponse> {
    return this.http.post<CalculoResponse>(`${this.apiUrl}/calcular`, request);
  }
}
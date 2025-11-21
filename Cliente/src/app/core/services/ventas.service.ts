import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { VentaCreateDto, VentaDto } from '../models/dtos';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class VentasService {
  private base = `${environment.apiUrl}/ventas`;

  constructor(private http: HttpClient) {}

  crearVenta(dto: VentaCreateDto): Observable<VentaDto> {
    return this.http.post<VentaDto>(this.base, dto);
  }

  getMisVentas(): Observable<VentaDto[]> {
    return this.http.get<VentaDto[]>(this.base);
  }

  getVenta(id: number): Observable<VentaDto> {
    return this.http.get<VentaDto>(`${this.base}/${id}`);
  }
}

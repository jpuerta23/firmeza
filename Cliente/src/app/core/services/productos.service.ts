import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Producto } from '../models/dtos';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProductosService {
  private base = `${environment.apiUrl}/productos`;

  constructor(private http: HttpClient) {}

  getProductos(): Observable<Producto[]> {
    return this.http.get<Producto[]>(this.base);
  }

  getProducto(id: number) {
    return this.http.get<Producto>(`${this.base}/${id}`);
  }
}

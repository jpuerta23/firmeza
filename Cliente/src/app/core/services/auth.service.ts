import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = `${environment.apiUrl}/auth`; // ajustar si tu auth usa otra ruta

  constructor(private http: HttpClient) { }

  login(credentials: { email: string; password: string }): Observable<void> {
    return this.http.post<{ token: string }>(`${this.api}/login`, credentials)
      .pipe(map(res => {
        if (res?.token) this.saveToken(res.token);
      }));
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.api}/register`, data);
  }

  logout() {
    localStorage.removeItem('token');
  }

  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLogged(): boolean {
    return !!this.getToken();
  }

  // parse payload safely
  private getPayload(): any | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const part = token.split('.')[1];
      return JSON.parse(atob(part));
    } catch {
      return null;
    }
  }

  getUserId(): string | null {
    const payload = this.getPayload();
    if (!payload) return null;
    // common claim names (NameIdentifier)
    return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
      || payload['nameid']
      || payload['sub']
      || null;
  }

  getRole(): string | null {
    const payload = this.getPayload();
    if (!payload) return null;
    return payload['role']
      || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      || null;
  }
}

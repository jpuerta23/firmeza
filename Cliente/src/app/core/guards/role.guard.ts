import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const expected = route.data['expectedRole'] as string;
    const role = this.auth.getRole();
    if (!role) {
      this.router.navigate(['/auth/login']);
      return false;
    }
    if (role === expected || (Array.isArray(expected) && expected.includes(role))) return true;
    this.router.navigate(['/']); // o alguna página "no autorizado"
    return false;
  }
}

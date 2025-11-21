import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterModule]
})
export class NavbarComponent {
    constructor(public authService: AuthService, private router: Router) { }

    logout() {
        this.authService.logout();
        this.router.navigate(['/auth/login']);
    }
}

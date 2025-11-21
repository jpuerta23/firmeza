import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
    registerForm: FormGroup;
    error: string = '';
    loading: boolean = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router
    ) {
        this.registerForm = this.fb.group({
            nombre: ['', Validators.required],
            documento: ['', Validators.required],
            telefono: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]]
        });
    }

    onSubmit() {
        if (this.registerForm.invalid) return;

        this.loading = true;
        this.error = '';

        this.authService.register(this.registerForm.value).subscribe({
            next: () => {
                this.loading = false;
                this.router.navigate(['/auth/login']);
            },
            error: (err) => {
                this.loading = false;
                this.error = err.error?.Error || 'Error al registrarse';
            }
        });
    }
}

import { CommonModule } from "@angular/common";
import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { FormsModule, NgForm } from "@angular/forms";
import { AuthService } from "../../services/auth";
import { Router } from "@angular/router";
import { AuthStorageService } from "../../services/auth-storage.service";
import { collectIdentityErrors } from "../../services/identity-errors";

@Component({
    selector: 'app-register',
    templateUrl: './register.html',
    styleUrl: './register.css',
    imports: [CommonModule, FormsModule],
    standalone: true
})

export class Register implements OnInit {
    Name = '';
    Surname = '';
    Email = '';
    Password = '';
    ConfirmPassword = '';
    backendErrors: string[] = [];

    constructor(private authService: AuthService, private router: Router, private authStorage: AuthStorageService, private cdr: ChangeDetectorRef) { }

    ngOnInit(): void {
        const token = this.authStorage.getToken();
        const role = this.authStorage.getRole();
        if (token && role) {
            this.authStorage.redirectUserByRole(role);
        }
    }

    register(form: NgForm) {
        if (form.invalid) {
            return;
        }
        if (this.Password !== this.ConfirmPassword) {
            this.backendErrors = ['Le password non coincidono'];
            return;
        }
        this.backendErrors = [];
        this.authService.register(
            this.Name,
            this.Surname,
            this.Email,
            this.Password,
            this.ConfirmPassword
        ).subscribe({
            next: () => {
                this.backendErrors = [];
                this.authService.login(this.Email, this.Password).subscribe({
                    next: () => {
                        const role = this.authStorage.getRole();
                        if (role)
                            this.authStorage.redirectUserByRole(role);
                        else {
                            alert('Errore nella login, riprova');
                            this.router.navigate(['/login']);
                        }
                    },
                    error: () => {
                        this.router.navigate(['/login']);
                    }
                });
            },
            error: (err) => {
                this.backendErrors = collectIdentityErrors(
                    err,
                    'Si è verificato un errore durante la registrazione'
                );
                this.backendErrors = [...this.backendErrors];
                this.cdr.detectChanges();
            }
        }
        );
    }
    navigateToLogin() {
        this.router.navigate(['/login']);
    }

}
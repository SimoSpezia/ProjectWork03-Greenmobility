import { CommonModule } from "@angular/common";
import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { FormsModule, NgForm } from "@angular/forms";
import { AuthService } from "../../services/auth";
import { Router } from "@angular/router";
import { AuthStorageService } from "../../services/auth-storage.service";
import { finalize } from "rxjs";

@Component({
    selector: 'app-login',
    templateUrl: './login.html',
    imports: [CommonModule, FormsModule],
    standalone: true
})
export class Login implements OnInit {

    email = '';
    password = '';
    backendErrors: string[] = [];
    isLoading = false;

    constructor(private authService: AuthService, private router: Router, private authStorage: AuthStorageService, private cdr: ChangeDetectorRef) { }

    ngOnInit(): void {
        const token = this.authStorage.getToken();
        const role = this.authStorage.getRole();
        if (token && role) {
            this.authStorage.redirectUserByRole(role);
        } else if (token || role) {
            this.authStorage.clearSession();
        }
    }

    login(form: NgForm) {
        if (form.invalid) {
            return;
        }
        this.backendErrors = [];
        this.isLoading = true;

        this.authService.login(this.email, this.password)
            .pipe(finalize(() => {
                this.isLoading = false;
            }))
            .subscribe({
                next: () => {
                    const role =
                        this.authStorage.getRole();

                    if (role)
                        this.authStorage.redirectUserByRole(role);
                    else {
                        this.authStorage.clearSession();
                        alert('Errore nella login, riprova');
                        window.location.reload();
                    }
                },

                error: (err) => {
                    let message = "Credenziali non valide";
                    if (err && typeof (err as any).error === 'string') {
                        message = (err as any).error;
                    }
                    this.isLoading = false;
                    this.backendErrors = [message];
                    this.cdr.detectChanges();
                    setTimeout(() => {
                        this.scrollToTop();
                    }, 0);

                }
            });
    }

    navigateToRegister() {
        this.router.navigate(['/register']);
    }

    private scrollToTop(): void {
        if (typeof window !== 'undefined') {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

}
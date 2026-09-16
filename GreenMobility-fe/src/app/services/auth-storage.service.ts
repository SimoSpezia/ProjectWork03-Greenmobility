import { Injectable } from "@angular/core";
import { Router } from "@angular/router";

@Injectable({
    providedIn: "root"
})
export class AuthStorageService {

    private readonly token = "token";
    private readonly email = "email";
    private readonly role = "role";
    private readonly expiration = 'expiration';

    constructor(private router: Router) { }

    saveSession(token: string, email: string, role: string, expiration: string): void {
        localStorage.setItem(this.token, token);
        localStorage.setItem(this.email, email);
        localStorage.setItem(this.role, role);
        localStorage.setItem(this.expiration, expiration);
    }

    clearSession(): void {
        localStorage.removeItem(this.token);
        localStorage.removeItem(this.email);
        localStorage.removeItem(this.role);
        localStorage.removeItem(this.expiration);
    }

    getToken(): string | null {
        return localStorage.getItem(this.token);
    }

    getEmail(): string | null {
        return localStorage.getItem(this.email);
    }

    getRole(): string | null {
        return localStorage.getItem(this.role);
    }

    getExpiration(): string | null {
        return localStorage.getItem(this.expiration);
    }

    public redirectUserByRole(role: string): void {

        switch (role) {
            case 'Admin':
                this.router.navigate(['/admin/hubs']);
                break;

            case 'Operator':
                this.router.navigate(['/operator/maintenance']);
                break;

            case 'Customer':
                this.router.navigate(['/hubs']);
                break;

            default:
                this.router.navigate(['/login']);
                break;
        }
    }
}
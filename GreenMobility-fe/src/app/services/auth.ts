import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { Login } from '../models/login';
import { AuthStorageService } from './auth-storage.service';
import { Register } from '../models/register';
@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly baseUrl =
        'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/Auth';

    constructor(private http: HttpClient, private authStorage: AuthStorageService) { }

    login(email: string, password: string): Observable<Login> {

        return this.http.post<Login>(this.baseUrl + "/login",
            {
                email,
                password
            }
        ).pipe(tap(auth => {
            const role = auth.roles?.[0];
            if (!auth?.token || !role) {
                this.authStorage.clearSession();
                return;
            }
            this.authStorage.saveSession(
                auth.token,
                email,
                role,
                auth.expiration
            );
        }));
    }

    register(Name: string, Surname: string, Email: string, Password: string, ConfirmPassword: string): Observable<Register> {

        return this.http.post<Register>(this.baseUrl + "/register", {
            Name: Name,
            Surname: Surname,
            Email: Email,
            Password: Password,
            ConfirmPassword: ConfirmPassword
        });

    }

    logout(): void {
        this.authStorage.clearSession();
    }

    isLoggedIn(): boolean {
        return !!this.authStorage.getToken();
    }
}
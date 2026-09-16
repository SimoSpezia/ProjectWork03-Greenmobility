import {
    HttpInterceptorFn,
    HttpErrorResponse
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStorageService } from '../auth-storage.service';

export const authInterceptor:
    HttpInterceptorFn =
    (req, next) => {

        const authStorage = inject(AuthStorageService);
        const router = inject(Router);
        const token = authStorage.getToken();

        if (token) {
            req = req.clone({
                setHeaders: {
                    Authorization:
                        `Bearer ${token}`
                }
            });
        }

        return next(req).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401 && !req.url.includes('/auth/') && !router.url.includes('/vehicledevice') && !router.url.includes('/apikey')) {
                    authStorage.clearSession();
                    router.navigate(['/login']);
                }
                return throwError(() => error);
            })
        );
    };
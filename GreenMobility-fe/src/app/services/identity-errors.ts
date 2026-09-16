export function mapIdentityError(code: string, description: string): string {
    switch (code) {
        case 'PasswordRequiresDigit':
            return 'La password deve contenere almeno un numero';

        case 'PasswordRequiresUpper':
            return 'La password deve contenere almeno una lettera maiuscola';

        case 'PasswordRequiresLower':
            return 'La password deve contenere almeno una lettera minuscola';

        case 'PasswordRequiresNonAlphanumeric':
            return 'La password deve contenere almeno un carattere speciale';

        case 'DuplicateEmail':
            return 'Esiste gia un account con questa email';

        case 'Utente gia esistente!':
            return 'Esiste gia un account con questa email';

        default:
            return description;
    }
}

export function collectIdentityErrors(err: any, fallback: string): string[] {
    const errors: string[] = [];

    if (Array.isArray(err?.error)) {
        for (const e of err.error) {
            if (e.code && e.description) {
                errors.push(mapIdentityError(e.code, e.description));
            }
        }
    } else if (typeof err?.error === 'string') {
        errors.push(mapIdentityError(err.error, err.error));
    } else if (typeof err?.error?.Message === 'string') {
        errors.push(err.error.Message);
    } else {
        errors.push(fallback);
    }

    return errors;
}

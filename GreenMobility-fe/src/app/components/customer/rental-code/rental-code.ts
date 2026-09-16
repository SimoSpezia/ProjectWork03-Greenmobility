import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RentalService } from '../../../services/rental';
import { RentalResponse } from '../../../models/rental';

@Component({
    selector: 'app-rental-code',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './rental-code.html',})
export class RentalCodeComponent implements OnInit {
    rentalResponse: RentalResponse | null = null;
    showConfirmModal = false;

    constructor(
        private rentalService: RentalService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.rentalResponse = this.rentalService.currentRental;

        if (!this.rentalResponse) {
            const stored = sessionStorage.getItem('currentRental');
            this.rentalResponse = stored ? (JSON.parse(stored) as RentalResponse) : null;
        }

        if (this.rentalResponse) {
            sessionStorage.setItem('currentRental', JSON.stringify(this.rentalResponse));
        }
    }

    confirmGoToHubs(): void {
        this.showConfirmModal = true;
    }

    hideConfirmDialog(): void {
        this.showConfirmModal = false;
    }

    goToHubs(): void {
        sessionStorage.removeItem('currentRental');
        this.router.navigate(['/hubs']);
    }
}
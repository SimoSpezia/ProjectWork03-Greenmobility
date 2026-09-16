import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HubService } from '../../../services/hub';
import { Hub } from '../../../models/hub';
import { Vehicle } from '../../../models/vehicle';
import { BatteryIndicatorComponent } from '../../battery-indicator/battery-indicator';
import { RentalService } from '../../../services/rental';
import { HeaderComponent } from '../../header/header';

@Component({
    selector: 'app-select-vehicle',
    standalone: true,
    imports: [CommonModule, BatteryIndicatorComponent, HeaderComponent],
    templateUrl: './select-vehicle.html',
    styleUrl: './select-vehicle.css'
})
export class SelectVehicleComponent implements OnInit {
    hub: Hub | null = null;
    vehicles: Vehicle[] = [];
    selectedVehicleType: number | null = null;
    isLoading = false;
    errorMessage = '';
    showConfirmModal = false;
    selectedVehicle: Vehicle | null = null;

    constructor(
        private hubService: HubService,
        private route: ActivatedRoute,
        private router: Router,
        private rentalService: RentalService,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        const idParam = this.route.snapshot.paramMap.get('id');
        const hubId = idParam ? Number(idParam) : NaN;

        if (!idParam || Number.isNaN(hubId)) {
            this.errorMessage = 'Hub non valido.';
            return;
        }

        this.loadHub(hubId);
    }

    loadHub(hubId: number): void {
        this.isLoading = true;
        this.hubService.getHubById(hubId).subscribe({
            next: (hub) => {
                this.hub = hub;
                const rawVehicles = (hub.vehicles ?? []) as Vehicle[];
                
                this.vehicles = rawVehicles.filter(v => 
                    v.vehicleStatusId === 1 || 
                    v.vehicleStatusName?.toLowerCase() === 'disponibile'
                );
                this.cdr.markForCheck();
                this.isLoading = false;
            },
            error: () => {
                this.errorMessage = 'Errore nel caricamento dei veicoli.';
                this.isLoading = false;
            }
        });
    }

    confirmRental(vehicle: Vehicle): void {
        this.selectedVehicle = vehicle;
        this.showConfirmModal = true;
    }

    hideConfirmDialog(): void {
        this.showConfirmModal = false;
        this.selectedVehicle = null;
    }

    confirmAndStartRental(): void {
        if (this.selectedVehicle) {
            this.startRental(this.selectedVehicle);
            this.hideConfirmDialog();
        }
    }

    startRental(vehicle: Vehicle): void {
        this.rentalService.reserveVehicle(vehicle.vehicleId).subscribe({
            next: (response) => {
                this.rentalService.currentRental = { ...response, uic: vehicle.uic };
                this.router.navigate(['/customer/rental-code']);
            },
            error: (err) => {
                this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.Message || err.error?.message || 'Errore nella prenotazione');
                this.cdr.markForCheck();
            }
        });
    }

    goToHubs(): void {
        this.router.navigate(['/hubs']);
    }

    trackByVehicleId(_: number, vehicle: Vehicle): number {
        return vehicle.vehicleId;
    }

    selectVehicleType(typeId: number | null): void {
        this.selectedVehicleType = this.selectedVehicleType === typeId ? null : typeId;
    }

    get filteredVehicles(): Vehicle[] {
        if (!this.selectedVehicleType) return this.vehicles;
        return this.vehicles.filter(v => v.vehicleTypeId === this.selectedVehicleType);
    }

    getVehicleIcon(vehicleTypeId: number): string {
        return vehicleTypeId === 1 ? 'bi-bicycle' : 'bi-scooter';
    }

    getButtonClass(typeId: number): string {
        return this.selectedVehicleType === typeId ? 'btn btn-primary' : 'btn btn-outline-secondary';
    }
}
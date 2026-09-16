import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BatteryIndicatorComponent } from '../battery-indicator/battery-indicator';
import { Vehicle, VehicleStatus } from '../../models/vehicle';
import { VehicleService } from '../../services/vehicle';
import { AuthStorageService } from '../../services/auth-storage.service';
import { HeaderComponent } from '../header/header';

interface MaintenanceStatusOption {
    id: number;
    label: string;
}

@Component({
    selector: 'app-operator',
    imports: [CommonModule, FormsModule, BatteryIndicatorComponent, HeaderComponent],
    templateUrl: './operator.html',})
export class OperatorComponent implements OnInit {
    vehicles: Vehicle[] = [];
    errorMessage = '';
    successMessage = '';

    editingVehicleId: number | null = null;
    editingVehicle: Vehicle | null = null;
    submitted = false;

    editBatteryLevel: number | null = null;
    editVehicleStatusId: number | null = null;

    vehicleStatuses: MaintenanceStatusOption[] = [];

    constructor(
        private vehicleService: VehicleService,
        private cdr: ChangeDetectorRef,
        private authStorage: AuthStorageService,
        private router: Router
    ) {}

    get isFormOpen(): boolean {
        return this.editingVehicleId !== null;
    }

    ngOnInit(): void {
        if (this.redirectCustomer()) return;
        this.loadStatuses();
        this.loadMaintenanceList();
    }
    private redirectCustomer(): boolean {
        if (this.authStorage.getRole() === 'Customer') {
            this.router.navigate(['/hubs']);
            return true;
        }
        return false;
    }

    loadMaintenanceList(): void {
        this.vehicleService.getMaintenanceList().subscribe({
            next: (data) => {
                this.vehicles = data;
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.Message || 'Errore nel caricamento della lista manutenzione.';
                console.error(err);
            }
        });
    }

    loadStatuses(): void {
        this.vehicleService.getVehicleStatuses().subscribe({
            next: (statuses: VehicleStatus[]) => {
                this.vehicleStatuses = statuses.map((status) => ({
                    id: status.vehicleStatusId,
                    label: status.status
                }));
                this.cdr.markForCheck();
            },
            error: (err) => {
                console.error('Errore nel caricamento degli stati:', err);
            }
        });
    }

    ButtonOnEdit(vehicle: Vehicle): void {
        this.editingVehicleId = vehicle.vehicleId;
        this.editingVehicle = vehicle;
        this.editVehicleStatusId = vehicle.vehicleStatusId;
        this.editBatteryLevel = vehicle.batteryLevel;
        this.submitted = false;
        this.clearMessages();
    }

    onConfirmEdit(): void {
        if (this.editingVehicleId === null) return;

        this.submitted = true;
        this.clearMessages();

        if (!this.isValid()) return;

        this.vehicleService.maintainVehicle(
            this.editingVehicleId,
            this.editBatteryLevel ?? undefined,
            this.editVehicleStatusId ?? undefined
        ).subscribe({
            next: () => {
                this.successMessage = 'Veicolo aggiornato correttamente.';
                this.loadMaintenanceList();
                this.closeForm();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.Message || 'Errore nell\'aggiornamento del veicolo.';
                this.cdr.markForCheck();
            }
        });
    }

    onCancel(): void {
        this.closeForm();
    }

    getTypeLabel(typeId: number): string {
        if (typeId === 1) return 'E-Bike';
        if (typeId === 2) return 'Monopattino';
        return 'Sconosciuto';
    }

    getStatusLabel(statusId: number): string {
        return this.vehicleStatuses.find((status) => status.id === statusId)?.label ?? 'Sconosciuto';
    }

    private clearMessages(): void {
        this.errorMessage = '';
        this.successMessage = '';
    }

    private closeForm(): void {
        this.editingVehicleId = null;
        this.editingVehicle = null;
        this.submitted = false;
    }

    private isValid(): boolean {
        if (this.editVehicleStatusId === null) return false;
        if (this.editBatteryLevel === null) return false;
        if (this.editBatteryLevel < 0 || this.editBatteryLevel > 100) return false;
        return true;
    }
}
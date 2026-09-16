import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { VehicleService } from '../../../services/vehicle';
import { Vehicle, VehicleCreateDto, VehicleStatus, VehicleType, VehicleUpdateDto } from '../../../models/vehicle';
import { HubService } from '../../../services/hub';
import { Hub } from '../../../models/hub';
import { AuthStorageService } from '../../../services/auth-storage.service';
import { BatteryIndicatorComponent } from '../../battery-indicator/battery-indicator';
import { HeaderComponent } from '../../header/header';

@Component({
    selector: 'app-admin-vehicles',
    imports: [CommonModule, FormsModule, BatteryIndicatorComponent, HeaderComponent],
    templateUrl: './vehicles.html',
})
export class AdminVehiclesComponent implements OnInit {
    vehicles: Vehicle[] = [];
    errorMessage = '';
    successMessage = '';
    createdApiKey = '';

    showConfirmModal = false;
    vehicleIdToDelete: number | null = null;
    vehicleUicToDelete: string | null = null;
    searchTerm = '';
    isLoading = true;

    
    isCreating = false;
    editingVehicleId: number | null = null;
    submitted = false;

    
    createVehicleTypeId = 1;
    createHubId = 1;

    
    editVehicleTypeId: number | null = null;
    editHubId: number | null = null;
    editVehicleStatusId: number | null = null;
    editBatteryLevel: number | null = null;

    
    vehicleTypes: VehicleType[] = [];
    vehicleStatuses: VehicleStatus[] = [];
    hubs: Hub[] = [];
    isLoadingHubs = false;
    hubError = '';
    typesError = '';
    statusesError = '';

    private loadedListsCount = 0;

    get isFormOpen(): boolean {
        return this.isCreating || this.editingVehicleId !== null;
    }

    constructor(
        private vehicleService: VehicleService,
        private hubService: HubService,
        private cdr: ChangeDetectorRef,
        private authStorage: AuthStorageService,
        private router: Router
    ) { }

    ngOnInit(): void {
        if (this.redirectNonAdmin()) return;
        this.loadVehicles();
        this.loadVehicleTypes();
        this.loadVehicleStatuses();
        this.loadHubs();
    }

    private redirectNonAdmin(): boolean {
        const role = this.authStorage.getRole();
        if (role === 'Customer') {
            this.router.navigate(['/hubs']);
            return true;
        }

        if (role === 'Operator') {
            this.router.navigate(['/operator/maintenance']);
            return true;
        }

        return false;
    }

    loadVehicles(): void {
        this.vehicleService.getVehicles().subscribe({
            next: (vehicles) => {
                this.hubService.getHubs().subscribe({
                    next: (hubs) => {
                        const activeHubIds = new Set(hubs.map(h => h.id));
                        this.vehicles = vehicles.map(v =>
                            activeHubIds.has(v.hubId) ? v : { ...v, hubName: undefined }
                        );
                        this.isLoading = false;
                        this.cdr.markForCheck();
                    },
                    error: (err) => {
                        console.error('Errore nel caricamento degli hub:', err);
                        this.vehicles = vehicles;
                        this.isLoading = false;
                        this.cdr.markForCheck();
                    }
                });
            },
            error: (err) => {
                this.isLoading = false;
                this.errorMessage = 'Errore nel caricamento dei veicoli.';
                console.error(err);
                this.cdr.markForCheck();
            }
        });
    }

    loadVehicleTypes(): void {
        this.vehicleService.getVehicleTypes().subscribe({
            next: (data) => {
                this.vehicleTypes = data;
                this.loadedListsCount++;
                this.cdr.markForCheck();
            },
            error: () => {
                this.typesError = 'Errore nel caricamento dei tipi di veicolo.';
                this.loadedListsCount++;
            }
        });
    }

    loadVehicleStatuses(): void {
        this.vehicleService.getVehicleStatuses().subscribe({
            next: (data) => {
                this.vehicleStatuses = data;
                this.loadedListsCount++;
                this.cdr.markForCheck();
            },
            error: () => {
                this.statusesError = 'Errore nel caricamento degli stati di veicolo.';
                this.loadedListsCount++;
            }
        });
    }

    loadHubs(): void {
        this.isLoadingHubs = true;
        this.hubService.getHubs().subscribe({
            next: (data) => {
                this.hubs = data;
                if (this.hubs.length > 0) {
                    this.createHubId = this.hubs[0].id;
                }
                this.isLoadingHubs = false;
                this.loadedListsCount++;
                this.cdr.markForCheck();
            },
            error: () => {
                this.hubError = 'Errore nel caricamento degli hub.';
                this.isLoadingHubs = false;
                this.loadedListsCount++;
            }
        });
    }

    get filteredVehicles(): Vehicle[] {
        const term = this.searchTerm.trim().toLowerCase();
        if (!term) return this.vehicles;

        return this.vehicles.filter((v) =>
            (v.uic || '').toLowerCase().includes(term) ||
            (v.vehicleStatusName || '').toLowerCase().includes(term) ||
            (v.hubName || 'Nessun hub').toLowerCase().includes(term)
        );
    }

    clearSearch(): void {
        this.searchTerm = '';
    }

    
    getAssignedVehicleCount(hub: Hub): number {
        return (hub.vehicles ?? []).filter(v => !v.isDeleted).length;
    }

    isHubFull(hub: Hub): boolean {
        return this.getAssignedVehicleCount(hub) >= hub.maximumCapacity;
    }

    isSelectedCreateHubFull(): boolean {
        const hub = this.hubs.find(h => h.id === this.createHubId);
        return hub ? this.isHubFull(hub) : false;
    }

    
    ButtonOnAdd(): void {
        this.createVehicleTypeId = this.vehicleTypes.length > 0 ? this.vehicleTypes[0].vehicleTypeId : 1;
        this.createHubId = this.hubs.length > 0 ? this.hubs[0].id : 1;
        this.isCreating = true;
        this.editingVehicleId = null;
        this.submitted = false;
        this.clearMessages();
    }

    onConfirmAdd(): void {
        this.submitted = true;
        this.clearMessages();

        if (!this.isValidCreate()) return;

        const createDto: VehicleCreateDto = {
            vehicleTypeId: this.createVehicleTypeId,
            hubId: this.createHubId
        };

        this.vehicleService.createVehicle(createDto).subscribe({
            next: (res) => {
                this.createdApiKey = res.apiKey;
                this.successMessage = 'Veicolo creato con successo!';
                this.loadVehicles();
                this.loadHubs();
                this.closeForm();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.Message || 'Errore nella creazione del veicolo.';
                this.cdr.markForCheck();
            }
        });
    }

    
    ButtonOnEdit(vehicle: Vehicle): void {
        this.editingVehicleId = vehicle.vehicleId;
        this.editVehicleTypeId = vehicle.vehicleTypeId;
        this.editHubId = vehicle.hubId;
        this.editVehicleStatusId = vehicle.vehicleStatusId;
        this.editBatteryLevel = vehicle.batteryLevel;
        this.isCreating = false;
        this.submitted = false;
        this.clearMessages();
    }

    onConfirmEdit(): void {
        if (this.editingVehicleId === null) return;
        this.submitted = true;
        this.clearMessages();

        if (!this.isValidEdit()) return;

        const dto: VehicleUpdateDto = {
            hubId: this.editHubId ?? undefined,
            vehicleTypeId: this.editVehicleTypeId ?? undefined,
            vehicleStatusId: this.editVehicleStatusId ?? undefined,
            batteryLevel: this.editBatteryLevel ?? undefined
        };

        this.vehicleService.updateVehicle(this.editingVehicleId, dto).subscribe({
            next: () => {
                this.successMessage = 'Veicolo aggiornato con successo!';
                this.loadVehicles();
                this.loadHubs();
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

    private closeForm(): void {
        this.isCreating = false;
        this.editingVehicleId = null;
        this.submitted = false;
        this.errorMessage = '';
    }

    private isValidCreate(): boolean {
        if (this.createVehicleTypeId < 1) return false;
        if (this.createHubId < 1) return false;
        if (this.isSelectedCreateHubFull()) return false;
        return true;
    }

    private isValidEdit(): boolean {
        if (this.editHubId === null || this.editHubId < 1) return false;
        if (this.editVehicleStatusId === null) return false;
        if (
            this.editBatteryLevel !== null &&
            (this.editBatteryLevel < 0 || this.editBatteryLevel > 100)
        ) return false;
        return true;
    }

    
    softDelete(id: number): void {
        const vehicle = this.vehicles.find(v => v.vehicleId === id);
        if (vehicle) {
            this.vehicleIdToDelete = id;
            this.vehicleUicToDelete = vehicle.uic;
            this.showConfirmModal = true;
        }
    }

    hideConfirmDialog(): void {
        this.showConfirmModal = false;
        this.vehicleIdToDelete = null;
        this.vehicleUicToDelete = null;
    }

    confirmAndDelete(): void {
        if (this.vehicleIdToDelete !== null) {
            const idToDelete = this.vehicleIdToDelete;
            this.hideConfirmDialog();
            this.clearMessages();

            this.vehicleService.softDeleteVehicle(idToDelete).subscribe({
                next: () => {
                    this.successMessage = 'Veicolo sospeso con successo.';
                    this.loadVehicles();
                },
                error: (err) => {
                    this.errorMessage = err.error?.Message || 'Errore nella sospensione del veicolo.';
                }
            });
        }
    }

    private clearMessages(): void {
        this.errorMessage = '';
        this.successMessage = '';
        this.createdApiKey = '';
    }
}
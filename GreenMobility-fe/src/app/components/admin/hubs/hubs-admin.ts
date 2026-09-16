import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HubService } from '../../../services/hub';
import { Hub } from '../../../models/hub';
import { VehicleService } from '../../../services/vehicle';
import { AuthStorageService } from '../../../services/auth-storage.service';
import { HeaderComponent } from '../../header/header';
@Component({
  selector: 'app-hubs-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, HeaderComponent],
  templateUrl: './hubs-admin.html',
})
export class HubsAdmin implements OnInit {
  hubs: Hub[] = [];
  editingId: number | null = null;
  isCreating = false;
  formHub: Omit<Hub, 'id'> = { name: '', address: '', city: '', maximumCapacity: 0 };
  submitted = false;
  showConfirmModal = false;
  hubIdToDelete: number | null = null;
  hubNameToDelete: string | null = null;
  showBatteryModal = false;
  batteryMessage = '';
  errorMessage = '';
  searchTerm = '';
  isLoading = true;
  constructor(
    private hubService: HubService,
    private cdr: ChangeDetectorRef,
    private vehicleService: VehicleService,
    private authStorage: AuthStorageService,
    private router: Router
  ) { }

  get isFormOpen(): boolean {
    return this.isCreating || this.editingId !== null;
  }
  ngOnInit(): void {
    if (this.redirectNonAdmin()) return;
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
  loadHubs(): void {
    this.hubService.getHubs().subscribe({
      next: (data) => {
        this.hubs = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  ButtonOnAdd(): void {
    this.formHub = { name: '', address: '', city: '', maximumCapacity: 0 };
    this.isCreating = true;
    this.editingId = null;
    this.submitted = false;
    this.errorMessage = '';
  }
  onConfirmAdd(): void {
    this.submitted = true;
    if (!this.isValid()) return;
    this.errorMessage = '';
    this.hubService.createHub({ ...this.formHub }).subscribe({
      next: () => {
        this.loadHubs();
        this.closeForm();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.message || err.error?.Message || 'Errore nella creazione dell\'hub.');
        this.cdr.markForCheck();
      }
    });
  }

  ButtonOnEdit(hub: Hub): void {
    const { id, name, address, city, maximumCapacity } = hub;
    this.formHub = { name, address, city, maximumCapacity };
    this.editingId = id;
    this.isCreating = false;
    this.submitted = false;
    this.errorMessage = '';
  }
  onConfirmEdit(): void {
    if (this.editingId === null) return;
    this.submitted = true;
    if (!this.isValid()) return;
    this.errorMessage = '';

    this.vehicleService.getVehicles().subscribe({
      next: (vehicles) => {
        const hubVehiclesCount = vehicles.filter(v => v.hubId === this.editingId).length;
        if (this.formHub.maximumCapacity < hubVehiclesCount) {
          this.errorMessage = `La capacità massima non può essere inferiore al numero di veicoli attivi nell'hub (${hubVehiclesCount} veicoli).`;
          this.cdr.markForCheck();
          return;
        }

        this.hubService.updateHub(this.editingId!, { ...this.formHub }).subscribe({
          next: () => {
            this.loadHubs();
            this.closeForm();
            this.cdr.markForCheck();
          },
          error: (err) => {
            this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.message || err.error?.Message || 'Errore durante la modifica dell\'hub.');
            this.cdr.markForCheck();
          }
        });
      },
      error: (err) => {
        this.errorMessage = 'Errore durante il recupero dei veicoli.';
        this.cdr.markForCheck();
      }
    });
  }

  onCancel(): void {
    this.closeForm();
  }
  private isValid(): boolean {
    const { name, address, city, maximumCapacity } = this.formHub;
    return (
      name.trim().length > 0 &&
      address.trim().length > 0 &&
      city.trim().length > 0 &&
      maximumCapacity >= 1 &&
      maximumCapacity >= 5 &&
      maximumCapacity <= 75
    );
  }
  private closeForm(): void {
    this.isCreating = false;
    this.editingId = null;
    this.submitted = false;
    this.errorMessage = '';
  }

  ButtonOnDelete(hub: Hub): void {
    this.hubIdToDelete = hub.id;
    this.hubNameToDelete = hub.name;
    this.showConfirmModal = true;
  }

  hideConfirmDialog(): void {
    this.showConfirmModal = false;
    this.hubIdToDelete = null;
    this.hubNameToDelete = null;
  }

  confirmAndDelete(): void {
    if (this.hubIdToDelete !== null) {
      const idToDelete = this.hubIdToDelete;
      this.hideConfirmDialog();
      this.errorMessage = '';
      this.hubService.deleteHub(idToDelete).subscribe({
        next: () => {
          this.loadHubs();
        },
        error: (err) => {
          this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.message || err.error?.Message || 'Errore durante la cancellazione dell\'hub.');
          this.cdr.markForCheck();
        }
      });
    }
  }
  ButtonOnBattery(hub: Hub): void {
    this.errorMessage = '';
    this.hubService.setMaintenanceBattery(hub.id).subscribe({
      next: (res) => {
        this.batteryMessage = res.message;
        this.showBatteryModal = true;
        this.loadHubs();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.message || err.error?.Message || 'Errore durante l\'impostazione dei veicoli in manutenzione.');
        this.cdr.markForCheck();
      }
    });
  }

  closeBatteryModal(): void {
    this.showBatteryModal = false;
    this.batteryMessage = '';
  }

  get filteredHubs(): Hub[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.hubs;

    return this.hubs.filter((h) =>
      (h.name || '').toLowerCase().includes(term) ||
      (h.city || '').toLowerCase().includes(term)
    );
  }
  clearSearch(): void {
    this.searchTerm = '';
  }
}
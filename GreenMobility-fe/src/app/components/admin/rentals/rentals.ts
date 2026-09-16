import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RentalService, RentalAdminDto } from '../../../services/rental';
import { UserService } from '../../../services/user';
import { VehicleService } from '../../../services/vehicle';
import { AuthStorageService } from '../../../services/auth-storage.service';
import { forkJoin } from 'rxjs';
import { HeaderComponent } from '../../header/header';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, HeaderComponent],
  templateUrl: './rentals.html',})
export class RentalsAdmin implements OnInit {
  rentals: RentalAdminDto[] = [];
  searchTerm = '';
  isLoading = true;

  sortColumn: keyof RentalAdminDto | '' = 'endDate';
  sortDirection: 'asc' | 'desc' = 'desc';

  constructor(
    private rentalService: RentalService,
    private userService: UserService,
    private vehicleService: VehicleService,
    private cdr: ChangeDetectorRef,
    private authStorage: AuthStorageService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (this.redirectNonAdmin()) return;
    this.loadRentals();
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

  loadRentals() {
    this.isLoading = true;
    forkJoin({
      rentals: this.rentalService.getAllRentals(),
      users: this.userService.getUsers(),
      vehicles: this.vehicleService.getVehicles()
    }).subscribe({
      next: ({ rentals, users, vehicles }) => {
        this.rentals = rentals.map(rental => {
          const user = users.find(u => u.Id === rental.userId);
          const vehicle = vehicles.find(v => v.vehicleId === rental.vehicleId);

          const formattedStart = rental.startDate ? this.formatDateUtc(rental.startDate) : '';
          const formattedEnd = rental.endDate ? this.formatDateUtc(rental.endDate) : '';

          return {
            ...rental,
            nome: user ? user.Name : 'N/D',
            cognome: user ? user.Surname : 'N/D',
            uic: vehicle ? vehicle.uic : 'N/D',
            startDateFormatted: formattedStart || 'N/D',
            endDateFormatted: formattedEnd || null
          };
        });

        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Error fetching rentals data:', err);
        this.cdr.markForCheck();
      }
    });
  }

  get filteredRentals(): RentalAdminDto[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.rentals;

    return this.rentals.filter((r) =>
      (r.nome || '').toLowerCase().includes(term) ||
      (r.cognome || '').toLowerCase().includes(term) ||
      (r.uic || '').toLowerCase().includes(term) ||
      (r.startDateFormatted || '').toLowerCase().includes(term) ||
      (r.endDateFormatted || '').toLowerCase().includes(term)
    );
  }

  clearSearch(): void {
    this.searchTerm = '';
  }


  private formatDateUtc(dateStr: string): string {
    if (!dateStr || dateStr.startsWith('0001-01-01')) return '';
    try {
      const date = new Date(dateStr);
      if (isNaN(date.getTime())) return '';
      
      const adjustedDate = new Date(date.getTime() + 2 * 60 * 60 * 1000);
      const day = String(adjustedDate.getUTCDate()).padStart(2, '0');
      const month = String(adjustedDate.getUTCMonth() + 1).padStart(2, '0');
      const year = adjustedDate.getUTCFullYear();
      const hours = String(adjustedDate.getUTCHours()).padStart(2, '0');
      const minutes = String(adjustedDate.getUTCMinutes()).padStart(2, '0');
      return `${day}/${month}/${year} ${hours}:${minutes}`;
    } catch {
      return '';
    }
  }
}

import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HubService } from '../../../services/hub';
import { Hub } from '../../../models/hub';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../header/header';


@Component({
    selector: 'app-hubs-customer',
    standalone: true,
    imports: [CommonModule, FormsModule, HeaderComponent],
    templateUrl: './hubs-customer.html',
    styleUrl: './hubs-customer.css'
})
export class HubsCustomer implements OnInit {
    availableHubs: Hub[] = [];
    searchTerm = "";
    isLoading = true;

    constructor(private hubService: HubService, private cdr: ChangeDetectorRef, private router: Router) { }

    ngOnInit(): void {
        this.hubService.getHubs().subscribe({
            next: (data) => {
                this.availableHubs = data;
                this.isLoading = false;
                this.cdr.markForCheck();
            },
            error: () => {
                this.isLoading = false;
                this.cdr.markForCheck();
            }
        });
    }

    navigateToHubVehicles(hub: Hub): void {
        this.router.navigate(['/hubs', hub.id, 'vehicles']);
    }

    get filteredHubs(): Hub[] {
        const term = this.searchTerm.trim().toLowerCase();
        if (!term) return this.availableHubs;

        return this.availableHubs.filter((h) =>
            (h.name || '').toLowerCase().includes(term) ||
            (h.city || '').toLowerCase().includes(term)
        );
    }
	clearSearch(): void {
		this.searchTerm = '';
	}
}

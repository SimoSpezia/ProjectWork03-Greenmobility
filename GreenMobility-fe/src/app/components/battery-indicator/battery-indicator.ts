import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-battery-indicator',
    imports: [CommonModule],
    templateUrl: './battery-indicator.html',
    styleUrl: './battery-indicator.css'
})
export class BatteryIndicatorComponent {
    @Input() level: number = 100;

    getIconClass(): string {
        if (this.level > 80) return 'bi-battery-full text-success';
        if (this.level > 50) return 'bi-battery-half text-success';
        if (this.level > 20) return 'bi-battery-half text-warning';
        return 'bi-battery text-danger';
    }

    getBadgeClass(): string {
        if (this.level > 50) return 'battery-badge-success';
        if (this.level > 20) return 'battery-badge-warning';
        return 'battery-badge-danger';
    }
}
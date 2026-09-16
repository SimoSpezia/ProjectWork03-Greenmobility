import { CommonModule } from "@angular/common";
import { Component, Input } from "@angular/core";
import { AuthService } from "../../services/auth";
import { AdminNavbarComponent } from "../admin/navbar/navbar";

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule, AdminNavbarComponent],
    templateUrl: './header.html',
})
export class HeaderComponent {  
    @Input() showAdminNav = false;

    constructor(private authService: AuthService) { }
    onLogout(): void {
        this.authService.logout();
    }
}
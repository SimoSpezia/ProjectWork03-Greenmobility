import { Component, signal } from '@angular/core';
import { NavigationStart, Router, RouterOutlet } from '@angular/router';
import { AuthStorageService } from '../../services/auth-storage.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',})
export class App {
  protected readonly title = signal('GreenMobility-fe');
  private readonly publicRoutes = new Set<string>(['/login', '/register']);

  constructor(
    private router: Router,
    private authStorage: AuthStorageService
  ) {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.redirectIfUnauthenticated(event.url);
      }
    });
  }

  private redirectIfUnauthenticated(url: string): void {
    const path = url.split('?')[0];
    if (this.publicRoutes.has(path) || path.startsWith('/vehicledevice') || path.startsWith('/apikey')) {
      return;
    }

    if (!this.authStorage.getToken()) {
      this.router.navigate(['/login']);
    }
  }
}
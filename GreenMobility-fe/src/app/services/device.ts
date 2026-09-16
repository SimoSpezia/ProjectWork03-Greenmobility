import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DeviceService {
  private http = inject(HttpClient);
  private apiUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/noleggi'; 

  constructor() { }

  verifyCode(apiKey: string, code: string): Observable<any> {
    const headers = new HttpHeaders().set('ApiKey', apiKey);
    return this.http.post<any>(`${this.apiUrl}/unlock-vehicle`, { rentalCode: code }, { headers });
  }

  endRental(apiKey: string, batteryLevel: number): Observable<any> {
    const headers = new HttpHeaders().set('ApiKey', apiKey);
    return this.http.patch<any>(`${this.apiUrl}/end-rental`, { batteryLevel }, { headers });
  }
}

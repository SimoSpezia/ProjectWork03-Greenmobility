import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, of } from 'rxjs';
import { Hub } from '../models/hub';

@Injectable({
  providedIn: 'root'
})
export class HubService {
  private readonly apiUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/hubs';

  constructor(private http: HttpClient) { }

  
  getHubs(): Observable<Hub[]> {
    return this.http.get<Hub[]>(this.apiUrl).pipe(
      map(hubs => hubs.sort((a, b) => {
        const cityCompare = a.city.localeCompare(b.city);
        if (cityCompare !== 0) return cityCompare;
        return a.name.localeCompare(b.name);
      }))
    );
  }

  
  getHubById(id: number): Observable<Hub> {
    return this.http.get<Hub>(`${this.apiUrl}/${id}`).pipe(
      map(hub => ({
        ...hub,
        vehicles: (hub.vehicles ?? []).sort((a, b) => a.uic.localeCompare(b.uic))
      }))
    );
  }

  
  createHub(hub: Omit<Hub, 'id'>): Observable<Hub> {
    return this.http.post<Hub>(this.apiUrl, hub);
  }

  
  updateHub(id: number, hubUpdate: Partial<Omit<Hub, 'id'>>): Observable<Hub> {
    return this.http.patch<Hub>(`${this.apiUrl}/${id}`, hubUpdate);
  }

  
  deleteHub(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  
  setMaintenanceBattery(id: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/${id}/maintenance-battery`, {});
  }
}

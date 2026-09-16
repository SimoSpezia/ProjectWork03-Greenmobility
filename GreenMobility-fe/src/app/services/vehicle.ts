import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { VehicleCreateDto, Vehicle, VehicleUpdateDto, VehicleStatus, VehicleType } from '../models/vehicle';
import { map, Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class VehicleService {
    private readonly baseUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/vehicles';
    private readonly statusTypeUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/status_type';

    constructor(private http: HttpClient) { }

    createVehicle(dto: VehicleCreateDto): Observable<{ vehicle: Vehicle; apiKey: string }> {
        return this.http.post<{ vehicle: Vehicle; apiKey: string }>(this.baseUrl, dto);
    }

    getVehicles(): Observable<Vehicle[]> {
        return this.http.get<Vehicle[]>(this.baseUrl).pipe(
            map(vehicles => vehicles.sort((a, b) => a.uic.localeCompare(b.uic, undefined, { numeric: true }))));
    }

    updateVehicle(id: number, dto: VehicleUpdateDto): Observable<void> {
        return this.http.patch<void>(`${this.baseUrl}/${id}`, dto);
    }

    softDeleteVehicle(id: number): Observable<void> {
        return this.http.patch<void>(`${this.baseUrl}/${id}/soft-delete`, {});
    }


    getMaintenanceList(): Observable<Vehicle[]> {
        return this.http.get<Vehicle[]>(`${this.baseUrl}/maintenance-list`).pipe(
            map(vehicles => vehicles.sort((a, b) => a.uic.localeCompare(b.uic, undefined, { numeric: true }))));
    }

    maintainVehicle(id: number, batteryLevel?: number, statusId?: number): Observable<void> {
        let params = new HttpParams();
        if (batteryLevel !== undefined && batteryLevel !== null) {
            params = params.set('batteryLevel', batteryLevel.toString());
        }
        if (statusId !== undefined && statusId !== null) {
            params = params.set('statusId', statusId.toString());
        }
        return this.http.patch<void>(`${this.baseUrl}/${id}/maintain-vehicle`, {}, { params });
    }

    getVehicleStatuses(): Observable<VehicleStatus[]> {
        return this.http.get<VehicleStatus[]>(`${this.statusTypeUrl}/GetAllVehicleStatuses`);
    }

    getVehicleTypes(): Observable<VehicleType[]> {
        return this.http.get<VehicleType[]>(`${this.statusTypeUrl}/GetAllVehicleTypes`);
    }

    getVehicleById(id: number): Observable<Vehicle> {
        return this.http.get<Vehicle>(`${this.baseUrl}/${id}`);
    }
}
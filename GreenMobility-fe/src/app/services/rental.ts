import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RentalResponse } from '../models/rental'; 


export interface RentalAdminDto {
  id: number;
  userId: string;
  vehicleId: number;
  startDate: string | null;
  endDate: string | null;
  totalCost: number | null;
  rentalCode: string | null;
  nome?: string;
  cognome?: string;
  uic?: string;
  startDateFormatted?: string;
  endDateFormatted?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class RentalService {

  private readonly apiUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/noleggi';

  currentRental: RentalResponse | null = null;

  constructor(private http: HttpClient) { }

  getAllRentals(): Observable<RentalAdminDto[]> {
    return this.http.get<RentalAdminDto[]>(this.apiUrl);
  }

  reserveVehicle(vehicleId: number): Observable<RentalResponse> {
    return this.http.post<RentalResponse>(
      `${this.apiUrl}/reserve-vehicle`,
      { vehicleId }
    );
  }
}
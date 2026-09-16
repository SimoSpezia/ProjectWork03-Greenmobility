import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { CreateUser, User } from '../models/user';

@Injectable({
	providedIn: 'root'
})
export class UserService {
	private readonly baseUrl = 'https://wa-be-greenmobility-demo-babyewe8hfevf2hc.germanywestcentral-01.azurewebsites.net/api/users';

	constructor(private http: HttpClient) { }

	getUsers(): Observable<User[]> {
		return this.http.get<any[]>(this.baseUrl).pipe(
			map((users) => users.map((u) => ({
				Id: u.id ??'',
				Name: u.name ?? '',
				Surname: u.surname ?? '',
				Email: u.email ??'',
				Role: u.role ?? (Array.isArray(u.roles) ? u.roles[0] : '')
			})))
		);
	}

    getUserById(id: string): Observable<User> {
		return this.http.get<any>(`${this.baseUrl}/${id}`).pipe(
			map((u) => ({
				Id: u.id ?? '',
				Name: u.name ?? '',
				Surname: u.surname ?? '',
				Email: u.email ?? '',
				Role: u.role ?? (Array.isArray(u.roles) ? u.roles[0] : '')
			}))
		);
    }

	softDeleteUser(id: string): Observable<void> {
		return this.http.patch<void>(`${this.baseUrl}/${id}/suspend`, {});
	}

	createUser(user: CreateUser): Observable<User> {
		return this.http.post<User>(this.baseUrl, user);
	}

	updateUser(id: string, user: Partial<Omit<User, 'Id'>>): Observable<User> {
		return this.http.patch<User>(`${this.baseUrl}/${id}`, user);
	}
}
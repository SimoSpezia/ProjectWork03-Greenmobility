import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user';
import { CreateUser, User } from '../../../models/user';
import { collectIdentityErrors } from '../../../services/identity-errors';
import { AuthStorageService } from '../../../services/auth-storage.service';
import { HeaderComponent } from '../../header/header';

@Component({
	selector: 'app-admin-users',
	imports: [CommonModule, FormsModule, HeaderComponent],
	templateUrl: './users.html',})
export class AdminUsersComponent implements OnInit {
	users: User[] = [];
	searchTerm = '';
	errorMessage = '';
	successMessage = '';
	isLoading = true;

	showConfirmModal = false;
	userIdToDelete: string | null = null;
	userNameToDelete: string | null = null;

	
	isCreating = false;
	editingId: string | null = null;
	submitted = false;

	formUser: Omit<User, 'Id'> = { Name: '', Surname: '', Email: '', Role: '' };
	originalUser: Omit<User, 'Id'> | null = null;

	password = '';
	confirmPassword = '';
	backendErrors: string[] = [];

	get isFormOpen(): boolean {
		return this.isCreating || this.editingId !== null;
	}

	constructor(
		private userService: UserService,
		private cdr: ChangeDetectorRef,
		private authStorage: AuthStorageService,
		private router: Router
	) { }

	ngOnInit(): void {
		if (this.redirectNonAdmin()) return;
		this.loadUsers();
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

	loadUsers(): void {
		this.userService.getUsers().subscribe({
			next: (data) => {
				this.users = data;
				this.isLoading = false;
				this.cdr.markForCheck();
			},
			error: (err) => {
				this.isLoading = false;
				this.errorMessage = 'Errore nel caricamento degli utenti.';
				console.error(err);
			}
		});
	}

	get filteredUsers(): User[] {
		const term = this.searchTerm.trim().toLowerCase();
		if (!term) return this.users;

		return this.users.filter((u) =>
			(u.Email || '').toLowerCase().includes(term) ||
			(u.Role || '').toLowerCase().includes(term)
		);
	}

	clearSearch(): void {
		this.searchTerm = '';
	}

	
	ButtonOnAdd(): void {
		this.formUser = { Name: '', Surname: '', Email: '', Role: '' };
		this.password = '';
		this.confirmPassword = '';
		this.originalUser = null;
		this.isCreating = true;
		this.editingId = null;
		this.submitted = false;
		this.clearMessages();
	}

	onConfirmAdd(): void {
		this.submitted = true;
		this.clearMessages();

		if (!this.isValidCreate()) return;

		const createPayload: CreateUser = {
			...this.formUser,
			Password: this.password,
			ConfirmPassword: this.confirmPassword
		};

		this.userService.createUser(createPayload).subscribe({
			next: () => {
				this.loadUsers();
				this.closeForm();
				this.cdr.markForCheck();
			},
			error: (err) => {
				this.backendErrors = collectIdentityErrors(
					err,
					'Errore nella creazione dell\'utente.'
				);
				this.backendErrors = [...this.backendErrors];
				this.cdr.detectChanges();
			}
		});
	}

	
	ButtonOnEdit(user: User): void {
		const loaded = {
			Name: user.Name ?? '',
			Surname: user.Surname ?? '',
			Email: user.Email ?? '',
			Role: user.Role ?? ''
		};
		this.formUser = { ...loaded };
		this.originalUser = { ...loaded };
		this.editingId = user.Id;
		this.isCreating = false;
		this.submitted = false;
		this.password = '';
		this.confirmPassword = '';
		this.clearMessages();
	}

	onConfirmEdit(): void {
		if (this.editingId === null) return;
		this.submitted = true;
		this.clearMessages();

		if (!this.isValidEdit()) return;

		const payload = this.buildUpdatePayload();
		if (!payload) {
			this.closeForm();
			return;
		}

		this.userService.updateUser(this.editingId, payload).subscribe({
			next: () => {
				this.loadUsers();
				this.closeForm();
				this.cdr.markForCheck();
			},
			error: (err) => {
				this.backendErrors = collectIdentityErrors(
					err,
					'Errore nell\'aggiornamento dell\'utente.'
				);
				this.backendErrors = [...this.backendErrors];
				this.cdr.detectChanges();
			}
		});
	}

	
	onCancel(): void {
		this.closeForm();
	}

	private closeForm(): void {
		this.isCreating = false;
		this.editingId = null;
		this.submitted = false;
		this.password = '';
		this.confirmPassword = '';
		this.backendErrors = [];
		this.errorMessage = '';
	}

	private isValidCreate(): boolean {
		const { Name, Surname, Email, Role } = this.formUser;
		if (!Name.trim() || Name.trim().length < 2 || Name.trim().length > 24) return false;
		if (!Surname.trim() || Surname.trim().length < 2 || Surname.trim().length > 24) return false;
		if (!Email.trim()) return false;
		if (!Role) return false;
		if (!this.password || this.password.length < 8 || this.password.length > 24) return false;
		if (this.password !== this.confirmPassword) return false;
		return true;
	}

	private isValidEdit(): boolean {
		const { Name, Surname, Email } = this.formUser;
		if (!Name.trim() || Name.trim().length < 2 || Name.trim().length > 24) return false;
		if (!Surname.trim() || Surname.trim().length < 2 || Surname.trim().length > 24) return false;
		if (!Email.trim()) return false;
		return true;
	}

	private buildUpdatePayload(): Partial<Omit<User, 'Id'>> | null {
		if (!this.originalUser) return null;

		const p: Partial<Omit<User, 'Id'>> = {};

		if (this.formUser.Name && this.formUser.Name.trim() !== this.originalUser.Name) p.Name = this.formUser.Name.trim();
		if (this.formUser.Surname && this.formUser.Surname.trim() !== this.originalUser.Surname) p.Surname = this.formUser.Surname.trim();
		if (this.formUser.Email && this.formUser.Email.trim() !== this.originalUser.Email) p.Email = this.formUser.Email.trim();
		if (this.formUser.Role && this.formUser.Role.trim() !== this.originalUser.Role) p.Role = this.formUser.Role.trim();

		return Object.keys(p).length ? p : null;
	}

	
	ButtonOnDelete(user: User): void {
		this.userIdToDelete = user.Id;
		this.userNameToDelete = `${user.Name || ''} ${user.Surname || ''}`.trim() || user.Email;
		this.showConfirmModal = true;
	}

	hideConfirmDialog(): void {
		this.showConfirmModal = false;
		this.userIdToDelete = null;
		this.userNameToDelete = null;
	}

	confirmAndDelete(): void {
		if (this.userIdToDelete !== null) {
			const idToDelete = this.userIdToDelete;
			this.hideConfirmDialog();
			this.clearMessages();

			this.userService.softDeleteUser(idToDelete).subscribe({
				next: () => {
					this.successMessage = 'Utente eliminato con successo.';
					this.loadUsers();
				},
				error: (err) => {
					this.errorMessage = typeof err.error === 'string' ? err.error : (err.error?.Message || err.error?.message || 'Errore nell\'eliminazione dell\'utente.');
				}
			});
		}
	}

	private clearMessages(): void {
		this.errorMessage = '';
		this.successMessage = '';
		this.backendErrors = [];
	}
}

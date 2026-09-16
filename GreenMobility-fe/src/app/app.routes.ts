import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { AdminVehiclesComponent } from './components/admin/vehicles/vehicles';
import { AdminUsersComponent } from './components/admin/users/users';
import { OperatorComponent } from './components/operator/operator';
import { SelectVehicleComponent } from './components/customer/select-vehicle/select-vehicle';
import { RentalCodeComponent } from './components/customer/rental-code/rental-code';
import { Device } from './components/device/device';
import { RentalsAdmin } from './components/admin/rentals/rentals';


export const routes: Routes = [

    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full'
    },
    {
        path: 'login',
        component: Login
    },
    {
        path: 'register',
        component: Register
    },
    {
        path: 'hubs',
        loadComponent: () =>
            import('./components/customer/hubs/hubs-customer').then(m => m.HubsCustomer),
    },
    {
        path: 'admin/hubs',
        loadComponent: () =>
            import('./components/admin/hubs/hubs-admin').then(m => m.HubsAdmin),
    },
    {
        path: 'admin/vehicles',
        component: AdminVehiclesComponent
    },
    {
        path: 'admin/users',
        component: AdminUsersComponent
    },
    {
        path: 'operator/maintenance',
        component: OperatorComponent
    },
    {
        path: 'hubs/:id/vehicles',
        component: SelectVehicleComponent
    },
    {
        path: 'customer/rental-code',
        component: RentalCodeComponent
    },
    { path: 'vehicledevice/:apikey', component: Device },
    { path: 'vehicledevice', component: Device },
    { path: 'apikey', component: Device },
    { path: 'admin/rentals', component: RentalsAdmin }
];

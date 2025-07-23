import { Routes } from '@angular/router';

import { BuildingDetailsComponent } from './building-details/building-details.component';
import { BuildingListComponent } from './building-list/building-list.component';
import { CreateUserComponent } from './create-user/create-user.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    { path: '', redirectTo: '/buildings', pathMatch: 'full' },
    { path: 'buildings', component: BuildingListComponent },
    { path: 'buildings/:id', component: BuildingDetailsComponent },
    { path: 'register', component: CreateUserComponent },
    { path: 'login', component: LoginComponent },
  // ... other routes
  ];

import { Routes } from '@angular/router';
import { LoginFormComponent } from './core/auth/login-form/login-form.component';
import { AuthGuard } from './core/auth/services/auth.guard';
import { PropertySelectionComponent } from './property/property-selection/property-selection.component';

export const routes: Routes = [
  { path: 'login', component: LoginFormComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  { 
    path: 'dashboard', 
    loadChildren: () => import('./core/dashboards/dashboard-default/dashboard-default.module')
      .then(m => m.DashboardDefaultModule) 
  },
  { 
    path: 'create-user',
    loadComponent: () => import('./core/auth/create-user/create-user.component')
      .then(m => m.CreateUserComponent)
  },
  {
    path: 'propertyLanding', 
    loadComponent: () => import('./property/property-landing/property-landing.component')
      .then(m => m.PropertyLandingComponent),
    canActivate: [AuthGuard]
  },
  { path: 'propertySelector', component: PropertySelectionComponent },
  {
    path: 'bookingsReport',
    loadChildren: () => import('./Menu/reports/reports.module')
      .then(m => m.ReportsModule)
  }
];
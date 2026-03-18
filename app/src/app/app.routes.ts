//app.routes.ts as the “top-level map” and each feature has its own *.routes.ts. for example property-landing.routes.ts (feature owns its child routes)
import { Routes } from '@angular/router';
import { LoginFormComponent } from '././core/auth/login-form/login-form';
import { PropertySelectionComponent } from './property/property-selection/property-selection.component';
import { ReportsComponent } from './menu/reports/reports.component';
import { CreateUserComponent } from './core/auth/create-user/create-user';
import { ActivateAccountComponent } from './core/auth/activate-account/activate-account.component';
import { authGuard } from './core/auth/guards/auth.guard';
import { propertySelectionGuard } from './core/auth/guards/property-selection.guard';

export const routes: Routes = [
  { path: '', component: LoginFormComponent, pathMatch : 'full' }, // boot page /
  { path: 'propertySelector', component: PropertySelectionComponent, canActivate: [authGuard] },
  { path: 'create-user', component: CreateUserComponent },
  { path: 'activate', component: ActivateAccountComponent }, // Email activation page
  { path: 'bookings', component: ReportsComponent, canActivate: [authGuard, propertySelectionGuard] },
  {
    path: 'propertyLanding',
    canActivate: [authGuard, propertySelectionGuard], // Protect entire property landing and all child routes - require property selection
    loadChildren: () =>
      import('././property/property-landing/property-landing.routes')
        .then(m => m.PROPERTY_LANDING_ROUTES),
  },
];

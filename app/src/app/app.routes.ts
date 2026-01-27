//app.routes.ts as the “top-level map” and each feature has its own *.routes.ts. for example property-landing.routes.ts (feature owns its child routes)
import { Routes } from '@angular/router';
import { LoginFormComponent } from '././core/auth/login-form/login-form';
import { PropertySelectionComponent } from './property/property-selection/property-selection.component';
import { ReportsComponent } from './menu/reports/reports.component';

export const routes: Routes = [
  { path: '', component: LoginFormComponent }, // boot page /
  { path: 'propertySelector', component: PropertySelectionComponent },
  { path: 'bookings', component: ReportsComponent },
  {
    path: 'propertyLanding',
    loadChildren: () =>
      import('././property/property-landing/property-landing.routes')
        .then(m => m.PROPERTY_LANDING_ROUTES),
  },
];
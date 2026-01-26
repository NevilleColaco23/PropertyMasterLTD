import { Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';
import { ReportsComponent } from '../../menu/reports/reports.component';

export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      { path: 'bookingsReport', component: ReportsComponent },
    ],
  },
];
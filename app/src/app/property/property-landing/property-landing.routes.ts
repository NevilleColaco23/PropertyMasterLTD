import { Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';
import { ReportsComponent } from '../../menu/reports/reports.component';
import { MenuAccessMap } from '../../core/Settings/MenuAccessMap/menu-access-map/menu-access-map';
import { PropertyMasterComponent } from '../property-master/property-master.component';
import { Dashboard1Component } from '../../dashboard/dashboard1/dashboard1.component';

export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      // Default route - redirect to dashboard1
      { path: '', redirectTo: 'dashboard1', pathMatch: 'full' },

      // Dashboard - Home page
      { path: 'dashboard1', component: Dashboard1Component },

      // Report screen shown when user clicks the "Bookings report" menu item
      { path: 'bookingsReport', component: ReportsComponent },

      // Menu access matrix (Task Management-style UI).
      // This route is expected to be linked from the dynamic main menu
      // via a path like /propertyLanding/menu-access-map returned by the
      // /menu/GetinitialData endpoint.
      { path: 'MenuAccessmapping', component: MenuAccessMap },

      // Property Master Management
      { path: 'property-master', component: PropertyMasterComponent },
    ],
  },
];

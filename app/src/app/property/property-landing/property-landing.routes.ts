import { Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';
import { ReportsComponent } from '../../menu/reports/reports.component';
import { MenuAccessMap } from '../../core/Settings/MenuAccessMap/menu-access-map/menu-access-map';
import { PropertyMasterComponent } from '../property-master/property-master.component';
import { Dashboard1Component } from '../../dashboard/dashboard1/dashboard1.component';
import { noGuestGuard } from '../../core/auth/guards/no-guest.guard';

export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      // Default route - redirect to dashboard1
      { path: '', redirectTo: 'dashboard1', pathMatch: 'full' },

      // Dashboard - Home page (guests can view)
      { path: 'dashboard1', component: Dashboard1Component },

      // Report screen shown when user clicks the "Bookings report" menu item (guests can view)
      { path: 'bookingsReport', component: ReportsComponent },

      // Menu access matrix — admin only, block guests
      { path: 'MenuAccessmapping', component: MenuAccessMap, canActivate: [noGuestGuard] },

      // Property Master Management — write operations, block guests
      { path: 'property-master', component: PropertyMasterComponent, canActivate: [noGuestGuard] },
    ],
  },
];

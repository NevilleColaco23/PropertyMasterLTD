import { Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';
import { ReportsComponent } from '../../menu/reports/reports.component';
import { MenuAccessMap } from '../../core/Settings/MenuAccessMap/menu-access-map/menu-access-map';

export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      // Report screen shown when user clicks the "Bookings report" menu item
      { path: 'bookingsReport', component: ReportsComponent },

      // Menu access matrix (Task Management-style UI).
      // This route is expected to be linked from the dynamic main menu
      // via a path like /propertyLanding/menu-access-map returned by the
      // /menu/GetinitialData endpoint.
      { path: 'MenuAccessmapping', component: MenuAccessMap },
    ],
  },
];
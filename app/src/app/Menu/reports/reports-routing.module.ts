// src/app/reports/reports-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ReportsComponent } from '../reports/reports.component';

const routes: Routes = [
  // The path here is relative to the parent lazy-loaded path ('/reports')
  // So, '/reports/bookings' will load BookingsReportComponent
  { path: 'bookings', component: ReportsComponent },
  // Add a default route for '/reports' if needed
  // { path: '', component: ReportsDashboardComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)], // Use forChild for feature modules
  exports: [RouterModule]
})
export class ReportsRoutingModule { }
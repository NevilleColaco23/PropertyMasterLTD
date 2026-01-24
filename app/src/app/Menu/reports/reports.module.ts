// src/app/reports/reports.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsRoutingModule } from './reports-routing.module';
// Angular Material Imports
import { MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card'; // For a nice container
import { MatInputModule } from '@angular/material/input'; // For search input
// For data fetching
import { HttpClientModule } from '@angular/common/http'; // Make sure this is imported if not already in app module
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReportsRoutingModule,
    MatSortModule,
    MatCardModule,
    MatInputModule,
    HttpClientModule,
  ],
  providers: [
    ErrorHandlingService
  ]
})
export class ReportsModule { }
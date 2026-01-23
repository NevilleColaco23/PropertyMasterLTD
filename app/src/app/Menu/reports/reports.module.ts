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
import { AppConfig } from '../../app.config'; // Assuming you have AppConfig for API path
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
    // Provide AppConfig and ErrorHandlingCommonServiceService here
    // if they are specifically used by components in this module and not globally provided.
    // If they are provided in root ('AppModule'), you don't need them here.
    AppConfig,
    ErrorHandlingService
  ]
})
export class ReportsModule { }
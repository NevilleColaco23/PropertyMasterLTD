// src/app/reports/reports.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsRoutingModule } from './reports-routing.module';
import { ReportsComponent } from '../reports/reports.component';
// Angular Material Imports
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card'; // For a nice container
import { MatProgressBarModule } from '@angular/material/progress-bar'; // For loading indicator
import { MatFormFieldModule } from '@angular/material/form-field'; // For search input
import { MatInputModule } from '@angular/material/input'; // For search input
import { MatIconModule } from '@angular/material/icon'; // For search icon
// For data fetching
import { HttpClientModule } from '@angular/common/http'; // Make sure this is imported if not already in app module
import { AppConfig } from '../../Appconfig'; // Assuming you have AppConfig for API path
import { ErrorHandlingCommonServiceService } from '../../Common/Services/error-handling-common-service.service';


@NgModule({
  declarations: [ ReportsComponent],
  imports: [
    CommonModule,
    ReportsRoutingModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    HttpClientModule
  ],
  providers: [
    // Provide AppConfig and ErrorHandlingCommonServiceService here
    // if they are specifically used by components in this module and not globally provided.
    // If they are provided in root ('AppModule'), you don't need them here.
    AppConfig,
    ErrorHandlingCommonServiceService
  ]
})
export class ReportsModule { }
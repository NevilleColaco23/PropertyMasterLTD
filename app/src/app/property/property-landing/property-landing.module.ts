import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PropertyLandingRoutingModule } from './property-landing-routing.module';
import { PropertyLandingComponent } from './property-landing.component';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    PropertyLandingComponent
  ],
  imports: [
    CommonModule,
    MatSelectModule,
    FormsModule,
    PropertyLandingRoutingModule
  ]
})
export class PropertyLandingModule { }

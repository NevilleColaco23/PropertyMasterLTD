import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PropertyLandingRoutingModule } from './property-landing-routing.module';
import { PropertyLandingComponent } from './property-landing.component';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatIcon } from '@angular/material/icon';


@NgModule({
  declarations: [
    PropertyLandingComponent
  ],
  imports: [
    CommonModule,
    MatSelectModule,
    FormsModule,
    MatToolbarModule,
    MatMenuModule,
    MatIcon,
    PropertyLandingRoutingModule
  ]
})
export class PropertyLandingModule { }

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';

const routes: Routes = [{ path: '', component: PropertyLandingComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PropertyLandingRoutingModule { }

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PropertyLandingComponent } from './property-landing.component';
import { ReportsComponent } from '../../Menu/reports/reports.component'; 

const routes: Routes = [
  {
    path: '', component: PropertyLandingComponent ,
  children: [
      { path: 'bookingsReport', component: ReportsComponent } // ReportsComponent is a "child" page that 
                      // should be rendered in the body of the "parent" PropertyLandingComponent's layout
                      //path should match with the subPath in DB
    ]}
  ];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PropertyLandingRoutingModule { }

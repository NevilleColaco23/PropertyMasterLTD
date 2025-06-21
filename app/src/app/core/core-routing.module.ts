import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {PropertyHomepageComponent} from '../property-homepage/property-homepage.component';

const routes: Routes = [
    { path: 'propertyHomepage/:value', component: PropertyHomepageComponent , data: { showHeader: true }}];
  
  @NgModule({
    imports: [RouterModule.forRoot(routes)],// 28.9.2024 1.20
    exports: [RouterModule]
  })
  export class CoreRoutingModule { }
  
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginFormComponent } from './core/auth/login-form/login-form.component';
import { AuthGuard } from './core/auth/services/auth.guard';
import { PropertySelectionComponent } from './property/property-selection/property-selection.component';


const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  { path: 'login', component: LoginFormComponent },

  { path: 'dashboard', loadChildren: () => import('./core/dashboards/dashboard-default/dashboard-default.module')
    .then(m => m.DashboardDefaultModule) },

    { path: 'create-user',loadChildren: () => import('./core/auth/create-user/create-user.component').then(m => m.CreateUserComponent)
  },
  {path: 'propertyLanding', loadChildren: () => import('./property/property-landing/property-landing.component').then(m => m.PropertyLandingComponent) 
    ,canActivate: [AuthGuard]},

  { path: 'propertySelector', component: PropertySelectionComponent },
    
  {path: 'bookingsReport',
    loadChildren: () => import('./Menu/reports/reports.module').then(m => m.ReportsModule)}
];


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

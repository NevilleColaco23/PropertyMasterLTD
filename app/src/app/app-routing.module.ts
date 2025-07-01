import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginFormComponent } from './core/auth/login-form/login-form.component';  // adjust path accordingly

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginFormComponent },
  { path: 'dashboard', loadChildren: () => import('./core/dashboards/dashboard-default/dashboard-default.module')
    .then(m => m.DashboardDefaultModule) },
    { path: 'create-user',loadChildren: () => import('./core/auth/create-user/create-user.module').then(m => m.CreateUserModule)
  }
];


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

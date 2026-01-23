import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateUserComponent } from './create-user.component'; // Assuming your component is named create-user.component

const routes: Routes = [
  {
    path: '', // This path is relative to the path defined in your AppRoutingModule (e.g., '/create-user')
    component: CreateUserComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CreateUserRoutingModule { }

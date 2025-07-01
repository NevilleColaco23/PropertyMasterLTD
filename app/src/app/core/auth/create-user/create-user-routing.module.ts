import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateUserComponent } from './create-user.component'; // Assuming your component is named create-user.component
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

const routes: Routes = [
  {
    path: '', // This path is relative to the path defined in your AppRoutingModule (e.g., '/create-user')
    component: CreateUserComponent
  }
];

@NgModule({
  imports: [MatFormFieldModule, MatButtonModule, MatCardModule, RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CreateUserRoutingModule { }

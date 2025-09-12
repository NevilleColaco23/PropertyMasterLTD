import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

//TODO : check if this is required
const routes: Routes = [
];

@NgModule({
  imports: [RouterModule.forChild(routes)], // Use forChild for feature modules
  exports: [RouterModule]
})

export class systemmessagesnackbarRoutingModule { }
import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-default',
  templateUrl: './dashboard-default.component.html',
  styleUrl: './dashboard-default.component.css'
})
export class DashboardDefaultComponent {

  constructor() { 
    // Initialization logic can go here if needed
    console.log('DashboardDefaultComponent initialized');
  }
}

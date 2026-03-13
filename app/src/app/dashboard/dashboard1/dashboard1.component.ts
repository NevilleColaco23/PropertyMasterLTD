import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import { Router } from '@angular/router';

export interface DashboardType {
  value: string;
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-dashboard1',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: ['./dashboard1.component.css']
})
export class Dashboard1Component implements OnInit {

  selectedDashboard: string = 'dashboard1';

  dashboardTypes: DashboardType[] = [
    {
      value: 'dashboard1',
      label: 'Overview Dashboard',
      icon: 'dashboard',
      route: '/propertyLanding/dashboard1'
    },
    {
      value: 'dashboard2',
      label: 'Analytics Dashboard',
      icon: 'analytics',
      route: '/propertyLanding/dashboard2'
    },
    {
      value: 'dashboard3',
      label: 'Reports Dashboard',
      icon: 'assessment',
      route: '/propertyLanding/dashboard3'
    }
  ];

  constructor(private router: Router) { }

  ngOnInit(): void {
    console.log('Dashboard1 component initialized');
  }

  onDashboardChange(event: MatSelectChange): void {
    const selectedDashboard = this.dashboardTypes.find(d => d.value === event.value);
    if (selectedDashboard) {
      console.log('Switching to dashboard:', selectedDashboard.label);
      // Navigate to the selected dashboard route
      // this.router.navigate([selectedDashboard.route]);

      // For now, just log it since other dashboards don't exist yet
      if (selectedDashboard.value !== 'dashboard1') {
        console.warn(`Dashboard ${selectedDashboard.label} is not yet implemented`);
        // You can show a snackbar message here
      }
    }
  }

}

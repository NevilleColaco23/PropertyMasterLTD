# Adding New Dashboard Types - Quick Guide

## Overview
The dashboard now includes a dropdown selector in the top-right corner that allows users to switch between different dashboard types.

## Current Dashboard Types

1. **Overview Dashboard** (dashboard1) - Current implementation
2. **Analytics Dashboard** (dashboard2) - Placeholder
3. **Reports Dashboard** (dashboard3) - Placeholder

## How to Add a New Dashboard

### Step 1: Create the New Dashboard Component

```bash
cd app/src/app/dashboard
# Create dashboard2 folder and files
```

Example: `dashboard2.component.ts`
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard2',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './dashboard2.component.html',
  styleUrls: ['./dashboard2.component.css']
})
export class Dashboard2Component implements OnInit {
  ngOnInit(): void {
    console.log('Dashboard2 component initialized');
  }
}
```

### Step 2: Add Route to property-landing.routes.ts

```typescript
import { Dashboard2Component } from '../../dashboard/dashboard2/dashboard2.component';

export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      { path: '', redirectTo: 'dashboard1', pathMatch: 'full' },
      { path: 'dashboard1', component: Dashboard1Component },
      { path: 'dashboard2', component: Dashboard2Component },  // ← Add this
      // ... other routes
    ],
  },
];
```

### Step 3: Update Dashboard Selector

In `dashboard1.component.ts` (and copy to dashboard2, dashboard3, etc.):

```typescript
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
  },
  // Add new dashboard here:
  {
    value: 'dashboard4',
    label: 'Your New Dashboard',
    icon: 'insert_chart',
    route: '/propertyLanding/dashboard4'
  }
];
```

### Step 4: Enable Navigation

In the `onDashboardChange` method, uncomment the navigation line once the dashboard is implemented:

```typescript
onDashboardChange(event: MatSelectChange): void {
  const selectedDashboard = this.dashboardTypes.find(d => d.value === event.value);
  if (selectedDashboard) {
    console.log('Switching to dashboard:', selectedDashboard.label);
    
    // Uncomment this line when dashboard is ready:
    this.router.navigate([selectedDashboard.route]);
    
    // Remove this warning block:
    // if (selectedDashboard.value !== 'dashboard1') {
    //   console.warn(`Dashboard ${selectedDashboard.label} is not yet implemented`);
    // }
  }
}
```

### Step 5: Copy the Selector to New Dashboard

Each dashboard component should have the same selector dropdown. Copy the following to each new dashboard:

**HTML** (add to the top of each dashboard template):
```html
<div class="dashboard-header">
  <div class="header-left">
    <h1>
      <mat-icon>analytics</mat-icon>  <!-- Change icon -->
      Analytics Dashboard  <!-- Change title -->
    </h1>
    <p class="dashboard-subtitle">View detailed analytics and insights</p>
  </div>
  
  <div class="header-right">
    <mat-form-field appearance="outline" class="dashboard-selector">
      <mat-label>Dashboard Type</mat-label>
      <mat-select [(value)]="selectedDashboard" (selectionChange)="onDashboardChange($event)">
        @for (dashboard of dashboardTypes; track dashboard.value) {
          <mat-option [value]="dashboard.value">
            <mat-icon>{{ dashboard.icon }}</mat-icon>
            {{ dashboard.label }}
          </mat-option>
        }
      </mat-select>
    </mat-form-field>
  </div>
</div>
```

**TypeScript** (add to component class):
```typescript
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import { Router } from '@angular/router';

// Add to imports array:
imports: [
  // ... other imports
  MatFormFieldModule,
  MatSelectModule
]

// Add to component class:
selectedDashboard: string = 'dashboard2';  // Set to current dashboard value

dashboardTypes: DashboardType[] = [
  // ... same array as dashboard1
];

constructor(private router: Router) { }

onDashboardChange(event: MatSelectChange): void {
  const selectedDashboard = this.dashboardTypes.find(d => d.value === event.value);
  if (selectedDashboard) {
    this.router.navigate([selectedDashboard.route]);
  }
}
```

**CSS** (copy from dashboard1.component.css):
```css
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
  gap: 24px;
}

.header-left {
  flex: 1;
}

.header-right {
  display: flex;
  align-items: center;
  min-width: 280px;
}

.dashboard-selector {
  width: 100%;
}

/* ... rest of the styles from dashboard1 */
```

## Recommended Dashboard Types

### Analytics Dashboard (dashboard2)
- Charts and graphs
- Trend analysis
- Performance metrics
- Revenue analytics

### Reports Dashboard (dashboard3)
- Booking reports
- Financial reports
- Occupancy reports
- Custom report builder

### Operational Dashboard (dashboard4)
- Real-time operations
- Task management
- Staff assignments
- Housekeeping status

### Financial Dashboard (dashboard5)
- Revenue overview
- Expenses tracking
- Profit margins
- Payment status

## Material Icons for Dashboards

Common icons you can use:
- `dashboard` - General dashboard
- `analytics` - Analytics/graphs
- `assessment` - Reports/assessment
- `insights` - Insights/trends
- `leaderboard` - Rankings/performance
- `pie_chart` - Pie charts
- `bar_chart` - Bar charts
- `timeline` - Timeline/history
- `account_balance` - Financial
- `trending_up` - Growth/trends

## Testing Checklist

When adding a new dashboard:

- [ ] Component created with proper imports
- [ ] Route added to property-landing.routes.ts
- [ ] Added to dashboardTypes array in all dashboard components
- [ ] Dropdown selector added to new dashboard template
- [ ] CSS styles copied and customized
- [ ] Navigation works correctly
- [ ] Responsive design tested (mobile/tablet/desktop)
- [ ] Unit tests added
- [ ] Console errors checked
- [ ] Build succeeds without errors

## Common Issues

### Issue: Dashboard selector not showing
**Solution**: Ensure MatFormFieldModule and MatSelectModule are imported

### Issue: Navigation not working
**Solution**: Check that the route is properly registered in property-landing.routes.ts

### Issue: Dropdown doesn't update when navigating
**Solution**: Set `selectedDashboard` value correctly in each component's constructor/ngOnInit

### Issue: Styling looks different
**Solution**: Copy all CSS from dashboard1, including the responsive styles

## File Structure Example

```
app/src/app/dashboard/
├── dashboard1/
│   ├── dashboard1.component.ts
│   ├── dashboard1.component.html
│   ├── dashboard1.component.css
│   └── dashboard1.component.spec.ts
├── dashboard2/
│   ├── dashboard2.component.ts
│   ├── dashboard2.component.html
│   ├── dashboard2.component.css
│   └── dashboard2.component.spec.ts
├── dashboard3/
│   ├── dashboard3.component.ts
│   ├── dashboard3.component.html
│   ├── dashboard3.component.css
│   └── dashboard3.component.spec.ts
└── shared/
    ├── dashboard-selector/  (optional - can create reusable component)
    └── models/
        └── dashboard-type.interface.ts
```

## Advanced: Create Reusable Dashboard Selector Component

To avoid code duplication, you can create a shared dashboard selector component:

```typescript
// app/src/app/dashboard/shared/dashboard-selector/dashboard-selector.component.ts
@Component({
  selector: 'app-dashboard-selector',
  standalone: true,
  template: `
    <mat-form-field appearance="outline" class="dashboard-selector">
      <mat-label>Dashboard Type</mat-label>
      <mat-select [value]="currentDashboard" (selectionChange)="onSelectionChange($event)">
        @for (dashboard of dashboardTypes; track dashboard.value) {
          <mat-option [value]="dashboard.value">
            <mat-icon>{{ dashboard.icon }}</mat-icon>
            {{ dashboard.label }}
          </mat-option>
        }
      </mat-select>
    </mat-form-field>
  `
})
export class DashboardSelectorComponent {
  @Input() currentDashboard!: string;
  @Input() dashboardTypes!: DashboardType[];
  @Output() dashboardChange = new EventEmitter<string>();
  
  onSelectionChange(event: MatSelectChange): void {
    this.dashboardChange.emit(event.value);
  }
}
```

Then use it in each dashboard:
```html
<app-dashboard-selector 
  [currentDashboard]="selectedDashboard"
  [dashboardTypes]="dashboardTypes"
  (dashboardChange)="handleDashboardChange($event)">
</app-dashboard-selector>
```

This makes maintenance easier when you have many dashboard types!

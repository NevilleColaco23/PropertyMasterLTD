import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule  } from '@angular/forms';
import { UserPropertyAccessService, PropertyModel } from '../services/user-property-access.service';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivityMessageService } from '../../services/activity-message.service';
import { APP_CONFIG, AppConfig } from '../../configuration/app.config.token';
import { ActivityMessages } from '../../constants/activity-messages';


@Component({
  selector: 'app-property-selection',
  standalone: true,
  imports: [MatSelectModule, MatFormFieldModule, ReactiveFormsModule, MatButtonModule, MatIconModule],
  templateUrl: './property-selection.component.html',
  styleUrls: ['./property-selection.component.css']
})
export class PropertySelectionComponent implements OnInit {

  toppings = new FormControl<PropertyModel[] | null>([]);
  toppingList: PropertyModel[] = [];
  private loaderService = inject(UserPropertyAccessService);
  private http = inject(HttpClient);
  private activityMessage = inject(ActivityMessageService);
  private config = inject(APP_CONFIG);

  constructor( private router: Router) {  }

  ngOnInit(): void {
  this.loaderService.getDropdownOptions().subscribe({
    next: (data) => {
      this.toppingList = data || [];
      console.log('Loaded properties:', this.toppingList);  // Debug: See what properties were loaded

      if (this.toppingList.length > 0 && (this.toppings.value?.length ?? 0) === 0) {
        this.toppings.setValue([this.toppingList[0]]);
      }
    },
    error: err => console.error(err)
  });

  this.toppings.valueChanges.subscribe(v => console.log('selection changed', v));
}

  get selectedLabel(): string {
    const vals = this.toppings.value ?? [];
    if (!Array.isArray(vals) || vals.length === 0) {
      return 'None';
    }
    return vals.map(p => p.name ?? '').filter(n => !!n).join(', ');
  }

  trackById(index: number, item: PropertyModel) {
    return item.id ?? index;
  }

  compareProperties = (a: PropertyModel | null, b: PropertyModel | null) => {
    if (!a || !b) return a === b;
    return a.id === b.id;
  };

   applySelection(): void {
    const selected = this.toppings.value ?? [];

    console.log('Raw selected properties:', selected);

    // Extract property IDs and save to localStorage
    const propertyIds = selected
      .map(p => p.id)
      .filter(id => id !== null && id !== undefined);

    console.log('Extracted property IDs:', propertyIds);

    localStorage.setItem('selectedPropertyIds', JSON.stringify(propertyIds));

    // Log property selection to backend using centralized message template
    const propertyNames = selected.map(p => p.name).join(', ');
    const message = selected.length === 1
      ? ActivityMessages.PROPERTY_SELECTED(propertyNames)
      : ActivityMessages.PROPERTIES_BULK_SELECTED(selected.length, propertyNames);

    console.log('🏠 About to log property selection:', message);
    const headers = this.activityMessage.createHeaders(message);

    // Make API call to log the selection
    const url = `${this.config.apiUrl}/property/selection/log`;
    console.log('🌐 Calling API:', url);

    this.http.post(url, {
      propertyIds: propertyIds,
      propertyNames: propertyNames
    }, { headers }).subscribe({
      next: () => {
        console.log('✅ Property selection logged successfully:', message);
        this.router.navigate(['/propertyLanding']);
      },
      error: (err) => {
        console.error('⚠️ Failed to log property selection, but continuing:', err);
        // Still navigate even if logging fails
        this.router.navigate(['/propertyLanding']);
      }
    });
  }

  clearSelection(): void {
    this.toppings.setValue([]);
  }

  signOut(): void {
    // Clear localStorage
    localStorage.removeItem('selectedPropertyIds');
    console.log('🧹 Cleared selected property context');
    // Navigate to login page
    this.router.navigate(['/']);
  }

  // Helper method to get selected property IDs from localStorage (can be used in other components)
  static getSelectedPropertyIds(): number[] {
    const stored = localStorage.getItem('selectedPropertyIds');
    if (!stored) return [];
    try {
      return JSON.parse(stored) as number[];
    } catch {
      return [];
    }
  }
}

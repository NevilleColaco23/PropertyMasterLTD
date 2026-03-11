import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule  } from '@angular/forms';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';


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
  private loaderService = inject(GetAllPropertiesServiceService);

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

    console.log('Raw selected properties:', selected);  // Debug: See raw data

    // Extract property IDs and save to localStorage
    // Note: Filter keeps IDs that are not null/undefined, including 0 and negative numbers
    const propertyIds = selected
      .map(p => p.id)
      .filter(id => id !== null && id !== undefined);

    console.log('Extracted property IDs:', propertyIds);  // Debug: See extracted IDs

    localStorage.setItem('selectedPropertyIds', JSON.stringify(propertyIds));
    console.log('Saved property IDs to localStorage:', propertyIds);

    this.router.navigate(['/propertyLanding']).then(navigated => {});
  }

  clearSelection(): void {
    this.toppings.setValue([]);
  }

  signOut(): void {
    // Clear localStorage
    localStorage.removeItem('selectedPropertyIds');
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

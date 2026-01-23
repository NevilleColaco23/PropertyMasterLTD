import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule  } from '@angular/forms';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';


@Component({
  selector: 'app-property-selection',
  imports: [MatSelectModule, MatFormFieldModule, ReactiveFormsModule],
  templateUrl: './property-selection.component.html',
  styleUrls: ['./property-selection.component.css']
})
export class PropertySelectionComponent implements OnInit {

  toppings = new FormControl<PropertyModel[] | null>([]);
  toppingList: PropertyModel[] = [];

  constructor(private propertyService: GetAllPropertiesServiceService, private router: Router) {}

  ngOnInit(): void {
    this.propertyService.getDropdownOptions().subscribe({
      next: (data) => this.toppingList = data || [],
      error: err => console.error(err)
    });

    this.toppings.valueChanges.subscribe(v => console.log('selection changed', v));
  }

  get selectedLabel(): string {
    const vals = this.toppings.value ?? [];
    if (!Array.isArray(vals) || vals.length === 0) {
      return 'None';
    }
    return vals.map(p => (p as any)?.name ?? '').filter(n => !!n).join(', ');
  }

  trackById(index: number, item: PropertyModel) {
    return (item as any)?.id ?? index;
  }

  compareProperties = (a: PropertyModel | null, b: PropertyModel | null) => {
    if (!a || !b) return a === b;
    return (a as any).id === (b as any).id;
  };

   applySelection(): void {
    const selected = this.toppings.value ?? [];
    console.log('Apply clicked. Selected items:', selected);

  this.router.navigate(['/propertyLanding']).then(navigated => {});

  }

  clearSelection(): void {
    this.toppings.setValue([]);
  }
}
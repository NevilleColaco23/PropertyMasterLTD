import { Component } from '@angular/core';
import { GetAllPropertiesService } from './services/get-all-properties.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-property-index',
  templateUrl: './property-index.component.html',
  styleUrl: './property-index.component.scss'
})
export class PropertyIndexComponent {
  dropdownOptions : any[] = [];

  constructor(private dropdownService: GetAllPropertiesService,private router: Router) {}

  ngOnInit(): void {
    this.fetchDropdownOptions();
  }

  fetchDropdownOptions(): void {
    this.dropdownService.getDropdownOptions().subscribe(
      (options: string[]) => {
        this.dropdownOptions = options;
      },
      (error: any) => {
        console.error('Error fetching dropdown options', error);
      }
    );
  }

  onSelect(event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target) {
     const value = target.value;
    console.log("value111");
    console.log(value);
    this.router.navigate(['/propertyHomepage', value]);
    
  }}}

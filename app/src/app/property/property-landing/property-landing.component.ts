import { Component } from '@angular/core';
import { GetAllPropertiesServiceService } from '../services/get-all-properties-service.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-property-landing',
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})
export class PropertyLandingComponent {
dropdownOptions : any[] = [];
selectedOption: string | undefined;

selectedCountry: string = ''; 

  constructor(private dropdownService: GetAllPropertiesServiceService,private router: Router) {}

  ngOnInit(): void {
    this.fetchDropdownOptions();
  }

  countries = [
    { code: 'US', name: 'United States' },
    { code: 'CA', name: 'Canada' },
    { code: 'IN', name: 'India' },
    { code: 'QA', name: 'Qatar' },
  ];

 fetchDropdownOptions(): void {
  console.log('Fetching dropdown options...');
    this.dropdownService.getDropdownOptions().subscribe(
      (options: string[]) => {
        console.log('Dropdown options fetched:', options);
        this.dropdownOptions = options;
      },
      (error: any) => {
        console.error('Error fetching dropdown options', error);
      }
    );
  }

}

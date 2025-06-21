import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router'
import { CalendarEvent } from 'angular-calendar';

@Component({
  selector: 'app-property-homepage',
  templateUrl: './property-homepage.component.html',
  styleUrl: './property-homepage.component.scss'
})
export class PropertyHomepageComponent {
value: string;
viewDate: Date = new Date();
events: CalendarEvent[] = [];

constructor(private route: ActivatedRoute) {}

ngOnInit(): void {
  this.value = this.route.snapshot.paramMap.get('value');
  console.log("Received value: " + this.value);
}}

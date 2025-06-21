import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalenderComponent } from '../calender/calender.component';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns'
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [CalenderComponent],
  imports: [
    CommonModule 
    ,CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory,
    }),NgbModule
  ],
  exports :[CalenderComponent]
})
export class CalendarComponentModule { }

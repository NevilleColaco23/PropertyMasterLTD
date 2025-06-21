import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-calender-details-popup',
  standalone: true,
  imports: [],
  templateUrl: './calender-details-popup.component.html',
  styleUrl: './calender-details-popup.component.scss'
})
export class CalenderDetailsPopupComponent {
  @Input() day: any;

  close() {
    // Logic to close the modal
  }
}

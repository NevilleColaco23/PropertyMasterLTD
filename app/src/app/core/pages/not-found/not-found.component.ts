import { Component, OnInit } from '@angular/core';
import { faSadTear } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss'
})
export class NotFoundComponent implements OnInit {

  faSadTear = faSadTear;

  constructor() { }

  ngOnInit() {
  }

}
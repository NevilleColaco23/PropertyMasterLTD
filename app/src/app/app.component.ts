import { Component, OnInit } from '@angular/core';
import { LoggingService } from './core/auth/services/logging.service';
import { Router } from '@angular/router';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

  //The service must be injected here to ensure immediate initialization.
  constructor(private loggingService: LoggingService, private router: Router) { }

  ngOnInit() {}
    
  title = 'testangularapi.client';
}

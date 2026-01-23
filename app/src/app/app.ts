import { Component, OnInit } from '@angular/core';
import { LoggingService } from './core/auth/services/logging.service';
import { RouterOutlet  } from '@angular/router';
import { LoaderComponent } from './core/loader/loader.component'; 

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LoaderComponent], 
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

  //The service must be injected here to ensure immediate initialization.
  constructor(private loggingService: LoggingService) { }

  ngOnInit() {}
    
  title = 'testangularapi.client';
}

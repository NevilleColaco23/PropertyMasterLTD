import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AsyncPipe } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoaderService } from './core/auth/services/loader-service.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AsyncPipe, MatProgressSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('propertyMaster');
  loaderService = inject(LoaderService);
}

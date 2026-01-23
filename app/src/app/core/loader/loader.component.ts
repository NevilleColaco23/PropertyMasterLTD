import { Component } from '@angular/core';
import { LoaderService } from '../../core/auth/services/loader-service.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'; 
import { AsyncPipe } from '@angular/common'; 


@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [MatProgressSpinnerModule, AsyncPipe],
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.css'
})
export class LoaderComponent {
constructor(public loaderService: LoaderService) { }
}

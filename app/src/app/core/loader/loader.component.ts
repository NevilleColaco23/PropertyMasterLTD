import { Component } from '@angular/core';
import { LoaderService } from '../../core/auth/services/loader-service.service';

@Component({
  selector: 'app-loader',
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.css'
})
export class LoaderComponent {
constructor(public loaderService: LoaderService) { }
}

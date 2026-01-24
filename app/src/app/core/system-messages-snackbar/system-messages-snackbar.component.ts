import { Component, signal, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { APP_CONFIG, AppConfig } from '../../app.config.token';
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

interface Booking {
  id: number;
  name: string;
}

@Component({
  selector: 'app-system-messages-snackbar',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  templateUrl: './system-messages-snackbar.component.html',
  styleUrl: './system-messages-snackbar.component.css'
})
export class SystemMessagesSnackbarComponent {
 isLoading = signal(false);
  dataSource: Booking[] = [];
  private pathAPI: string;

  // Inject the service directly into the constructor
  constructor(
    private http: HttpClient, private snackBar: MatSnackBar, private errorHandling: ErrorHandlingService, @Inject(APP_CONFIG) private appConfig: AppConfig
  ) { this.pathAPI = this.appConfig.apiUrl; }

  ngOnInit(): void { }

  getSystemMessage(): void {
    this.isLoading.set(true);
    const params = new HttpParams().set('_limit', 5);

    this.http.get<any>(this.pathAPI + 'v1/Messages/GetSystemMessages', { params: params })
      .pipe(
        map(response => {
          const results = response.map((item: any) => ({
            id: item.id,
            name: item.title,
          }));
          return results as Booking[];
        }),
        catchError(error => {
          this.isLoading.set(false);
          this.errorHandling.handleError(error);
          return of([]);
        })
      )
      .subscribe({
        next: (data: Booking[]) => {
          this.dataSource = data;
          this.isLoading.set(false);
          this.snackBar.open('Bookings loaded successfully!', 'Dismiss', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });
        },
        error: (err) => {
          console.error('Subscription error:', err);
        }
      });
  }
}

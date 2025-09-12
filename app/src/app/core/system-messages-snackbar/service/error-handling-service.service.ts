import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService {

  constructor(private snackBar: MatSnackBar) {}

  public handleError(error: any): Observable<any> {
    const errorMessage = 'An error occurred. Please try again later.' + (error.error?.title ? ` Details: ${error.error.title}` : '');
    console.error('Error details:', error);
    this.snackBar.open(errorMessage, 'Dismiss', {
      duration: 5000,
      panelClass: ['error-snackbar'],
    });
    // Return an empty observable to continue the stream without error
    return of(null);
  }

public handleSuccess(message: string): void {
    this.snackBar.open(message, 'Dismiss', {
      duration: 3000,
      panelClass: ['success-snackbar'],
    });
  }
}
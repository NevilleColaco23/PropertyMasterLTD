import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {  throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingCommonServiceService {

  constructor() { }

  handleError(error: HttpErrorResponse) {
    console.log('Error occurred:', error);
    // Handle the error appropriately
    let errorMessage = 'Unknown error!';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    return throwError(errorMessage);
  }
}

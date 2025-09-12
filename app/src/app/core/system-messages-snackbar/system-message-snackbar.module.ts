import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
//Component
import { SystemMessagesSnackbarComponent } from './system-messages-snackbar.component';
//utilities
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';


@NgModule({
  declarations: [SystemMessagesSnackbarComponent],
  imports: [
    CommonModule,
    MatProgressSpinnerModule
  ],
  exports: [
    SystemMessagesSnackbarComponent
  ],
  providers: [ErrorHandlingService]
})

export class systemmessagessnackbarModule { }
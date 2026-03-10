import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon'; // For search icon
import { MatDivider } from '@angular/material/divider'; // For search icon
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { Router } from '@angular/router';
import { timer } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../services/auth.service'; //Tip: this services is injected based on classic method - constructor injection
import { LoaderService } from '../../../core/auth/services/loader-service.service'; //Tip: this services is injected using inject() function (popular in standalone) (NEW)
import { LOG_LOGIN_SUCCESS } from '../../../common/Constants/Constants';
import { LoggingService } from '../../system/service/logging.service';
import { environment } from '../../../environments/environment';



enum LocalLoginState {
  None,
  Waiting,
  Success,
  ErrorWrongData,
  ErrorOther
}

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [MatCardModule, ReactiveFormsModule, MatFormFieldModule, MatIconModule, MatDivider, MatInputModule, MatButtonModule],
  templateUrl: './login-form.html',
  styleUrl: './login-form.css'
})

export class LoginFormComponent  {
  hidePassword = true;
  faSpinner = faSpinner;

  // Declare loginForm but initialize in constructor or after constructor
  loginForm;

  localLoginState = LocalLoginState.None;
  get localLoginStates() { return LocalLoginState; }
  private loaderService = inject(LoaderService);

  constructor(private as: AuthService, private fb: FormBuilder,private router: Router,private loggingService: LoggingService) {

    this.loginForm = this.fb.group({
      email: [environment.testData?.login.email || '', [Validators.required, Validators.email]],
      password: [environment.testData?.login.password || '', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
    this.loaderService.show();

    const loginData = this.loginForm.value;
      
    this.localLoginState = LocalLoginState.Waiting;
    this.loginForm.disable();
    const email = loginData.email ?? '';
    const password = loginData.password ?? '';
    
     this.as.authenticate(email, password).pipe(
          finalize(() => this.loaderService.hide()) // Use finalize to ensure hide() is called whether the request succeeds or fails
        ).subscribe(
      _ => {
        this.localLoginState = LocalLoginState.Success;
    
    this.loggingService.logPageNavigation(`loginSuccess`, LOG_LOGIN_SUCCESS, `User logged in successfully with email: ${email}`);
        
    this.router.navigate(['/propertySelector']).then(navigated => {});

        timer(5000).subscribe(() => this.localLoginState = LocalLoginState.None); // In case user logs out without navigating elsewhere; the 'success' would still be visible.
        this.loginForm.enable();
      },
      err => {
        this.loginForm.enable();

        if (err.status == 401) {
          // Check if it's an unactivated account error
          if (err.error?.message && err.error.message.includes('activate')) {
            alert('❌ Account Not Activated\n\n' + err.error.message);
          }
          this.localLoginState = LocalLoginState.ErrorWrongData;
        } else {
          this.localLoginState = LocalLoginState.ErrorOther;
        }
      });
    }
  }

  goToCreateUser() {
    this.router.navigate(['/create-user']);
  }
}

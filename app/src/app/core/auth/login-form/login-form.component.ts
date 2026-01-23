import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../services/auth.service';
import { timer } from 'rxjs';
import { Router } from '@angular/router';
import { LoaderService } from '../../../core/auth/services/loader-service.service';
import { finalize } from 'rxjs/operators';
import { LOG_LOGIN_SUCCESS } from '../../../Common/Constants/Constants';
import { LoggingService } from '../../../core/auth/services/logging.service';
import { MatCardModule } from '@angular/material/card';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon'; // For search icon
import { MatDivider } from '@angular/material/divider'; // For search icon


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
  imports: [MatCardModule, ReactiveFormsModule, MatFormFieldModule, MatIconModule, MatDivider],
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.css'
})
export class LoginFormComponent  {
  hidePassword = true;
  faSpinner = faSpinner;

  // Declare loginForm but initialize in constructor or after constructor
  loginForm;

  localLoginState = LocalLoginState.None;
  get localLoginStates() { return LocalLoginState; }

  constructor(private as: AuthService, private fb: FormBuilder,private router: Router,private loaderService: LoaderService,private loggingService: LoggingService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],password: ['', Validators.required]
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

        if (err.status == 401)
          this.localLoginState = LocalLoginState.ErrorWrongData;
        else
          this.localLoginState = LocalLoginState.ErrorOther;
      });
    }
  }

  goToCreateUser() {
    this.router.navigate(['/create-user']);
  }
}
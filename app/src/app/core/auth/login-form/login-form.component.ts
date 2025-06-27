import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { AuthService } from '../services/auth.service';
import { Subscription, timer } from 'rxjs';

enum LocalLoginState {
  None,
  Waiting,
  Success,
  ErrorWrongData,
  ErrorOther
}

@Component({
  selector: 'app-login-form',
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

  constructor(private as: AuthService, private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],password: ['', Validators.required]
    });
  }

  onSubmit() {
    console.log('Login form submitted');
    if (this.loginForm.valid) {
      const loginData = this.loginForm.value;
      
    this.localLoginState = LocalLoginState.Waiting;
    this.loginForm.disable();
    const email = loginData.email ?? '';
    const password = loginData.password ?? '';
    
     this.as.authenticate(email, password).subscribe(
      _ => {
        this.localLoginState = LocalLoginState.Success;
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
}
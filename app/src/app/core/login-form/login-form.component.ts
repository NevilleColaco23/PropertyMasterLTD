import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'app-login-form',
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.css'
})
export class LoginFormComponent  {
  hidePassword = true;

  // Declare loginForm but initialize in constructor or after constructor
  loginForm;

  constructor(private fb: FormBuilder) {
    // Initialize loginForm here inside the constructor
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const loginData = this.loginForm.value;
      console.log('Login data:', loginData);
      // TODO: Add authentication logic here
    }
  }
}
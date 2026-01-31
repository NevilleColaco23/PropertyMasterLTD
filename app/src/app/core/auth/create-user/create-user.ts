import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  if (!password || !confirm) return null;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-create-user',
  templateUrl: './create-user.html',
  styleUrls: ['./create-user.css'],
  standalone: true,
  imports: [MatCardModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, CommonModule,MatIconModule, MatButtonModule
    , MatDividerModule
  ],
})


export class CreateUserComponent {
private authService = inject(AuthService);
private router = inject(Router);

  hidePassword = true;
  hideConfirmPassword = true;
  private fb = inject(FormBuilder);


  form = this.fb.group(
    {
      username: ['nevillecolaco', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
      email: ['nevillecolaco19@gmail.com', [Validators.required, Validators.email]],
      password: ['12345678', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['12345678', [Validators.required]],
      phone: ['+1 (555) 123-4567', [Validators.required, Validators.pattern(/^\+?[0-9\s\-()]{7,20}$/)]],
    },
    { validators: passwordMatchValidator }
  );

  constructor() {}


  goToLogin() {
  console.log('Going to login...');
    this.router.navigateByUrl('/').then(ok => console.log('navigateByUrl ok?', ok));
 
  }

  get f() {
    return this.form.controls;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const signUpPayload = {
      username: this.f.username.value!,
      email: this.f.email.value!,
      password: this.f.password.value!,
      phone: this.f.phone.value!,
    };

    this.authService.signUp(signUpPayload).subscribe({
      next: res => {
        console.log('SignUp success:', res.body);

        // Example: after successful signup, go back to login
        this.router.navigateByUrl('/', { replaceUrl: true });
      },
      error: err => {
        console.error('SignUp failed:', err);
        // TODO: show a snackbar/toast with err.error, etc.
      },
    });
  }

 
}
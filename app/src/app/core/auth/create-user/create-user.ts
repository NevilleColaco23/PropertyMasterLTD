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
import { map } from 'rxjs/internal/operators/map';
import { Observable } from 'rxjs/internal/Observable';
import { catchError } from 'rxjs/internal/operators/catchError';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlingService } from '../../system/service/error-handling-service.service';

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
private errorHandling = inject(ErrorHandlingService);

  hidePassword = true;
  hideConfirmPassword = true;
  private fb = inject(FormBuilder);
  private pathAPI: string = environment.apiUrl;

  form = this.fb.group(
    {
      username: [environment.testData?.signup.username || '', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
      email: [environment.testData?.signup.email || '', [Validators.required, Validators.email]],
      password: [environment.testData?.signup.password || '', [Validators.required, Validators.minLength(8)]],
      confirmPassword: [environment.testData?.signup.confirmPassword || '', [Validators.required]],
      phone: [environment.testData?.signup.phone || '', [Validators.required, Validators.pattern(/^\+?[0-9\s\-()]{7,20}$/)]],
      propertyCode: [environment.testData?.signup.propertyCode || ''],
    }, 
    { validators: passwordMatchValidator }
  );

  constructor(private http: HttpClient) {}

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
      propertyCode: this.f.propertyCode.value || ''  // ✅ Include property code (empty string if not provided)
    };

    this.authService.signUp(signUpPayload).subscribe({
      next: res => {
        console.log('SignUp success:', res.body);

        // Show success popup with property-specific message
        const hasPropertyCode = this.f.propertyCode.value && this.f.propertyCode.value.trim() !== '';
        const message = hasPropertyCode 
          ? '✅ Signup successful!\n\nYou will be added to your property once your account is activated.\n\nPlease check your email inbox for the activation link.'
          : '✅ Signup successful!\n\nYou will have access to our demo hotel to explore all features.\n\nPlease check your email inbox for the activation link to activate your account.';

        alert(message);

        // Navigate to login page after user closes the alert
        this.router.navigateByUrl('/', { replaceUrl: true });
      },
      error: err => {
        console.error('SignUp failed:', err);

        // Show error message
        const errorMessage = err.error?.message || err.error || 'Signup failed. Please try again.';
        alert('❌ Signup failed:\n\n' + errorMessage);
      },
    });
  }

 
}

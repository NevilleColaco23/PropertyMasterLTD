import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../auth/services/auth.service';


@Component({
  selector: 'app-create-user',
  templateUrl: './create-user.component.html',
  styleUrl: './create-user.component.css'
})
export class CreateUserComponent {
userForm: FormGroup;

  constructor(private fb: FormBuilder, private as: AuthService) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [
        Validators.required,
        Validators.pattern(/^[0-9]{8,15}$/) // only digits, 8-15 characters
      ]],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required]
    }, { validator: this.passwordsMatchValidator });
  }

  ngOnInit(): void {
  this.userForm = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [
      Validators.required,
      Validators.pattern(/^[0-9]{8,15}$/)
    ]],
    password: ['', Validators.required],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordsMatchValidator });
}


  passwordsMatchValidator(form: AbstractControl): { [key: string]: boolean } | null {
      const password = form.get('password')?.value;
      const confirmPassword = form.get('confirmPassword')?.value;
      return password === confirmPassword ? null : { passwordMismatch: true };
    }
  
 get passwordMismatch() {
    return (
      this.userForm.hasError('passwordMismatch') &&
      this.userForm.get('confirmPassword')?.touched
    );
  }

  onSubmit() {
    if (this.userForm.valid) {
        this.as.signUp(this.userForm.value.name, this.userForm.value.email,this.userForm.value.password
          ,this.userForm.value.phone)
        .subscribe(
      response => {
        console.log('Sign-up successful:', response);
        alert('Sign-up successful!');
        this.userForm.reset();
        
      },
      err => {
        console.error('Sign-up error:', err);
        this.userForm.enable();
        alert('Sign-up failed. Please try again.');
      }
    );
    }
  }
}

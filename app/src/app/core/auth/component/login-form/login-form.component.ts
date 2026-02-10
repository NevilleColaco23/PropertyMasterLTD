import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { Subscription, timer } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { AuthenticationSuccessData } from '../../models/login-data/login-data';
import { Router } from '@angular/router';

enum LocalLoginState {
  None,
  Waiting,
  Success,
  ErrorWrongData,
  ErrorOther
}

enum ExternalLoginState {
  None,
  Waiting,
  Success,
  Error
}

@Component({
    selector: 'app-login-form',
    templateUrl: './login-form.component.html',
    standalone: false
})
export class LoginFormComponent implements OnInit, OnDestroy {

  faSpinner = faSpinner;

  isLoggedIn: boolean = false;
  loggedInUsername: string;
  validityDays!: number;

  form!: FormGroup;
  formSubmitAttempt!: boolean;

  localLoginState = LocalLoginState.None;
  get localLoginStates() { return LocalLoginState; }

  externalLoginState = ExternalLoginState.None;
  get externalLoginStates() { return ExternalLoginState; }

  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  private sub!: Subscription;

  constructor(private as: AuthService, private router: Router) { }

  ngOnInit() {
    this.sub = this.as.signInState.subscribe(userData => {
      this.isLoggedIn = userData != null;
      this.updateUserData(userData);

      if (!this.isLoggedIn && !this.form)
        this.createLoginForm();
    });
  }

  private createLoginForm() {
    this.form = new FormGroup({
      username: new FormControl('', [Validators.required]),
      password: new FormControl('', [Validators.required, Validators.minLength(6)]),
      grant_type: new FormControl('password'),
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }  

  onSubmit() {

    this.formSubmitAttempt = true;
    if (this.form.invalid) {
      return;
    }

    this.localLoginState = LocalLoginState.Waiting;
    this.form.disable();

    this.as.authenticate(this.form.value.username, this.form.value.password).pipe(
      finalize(() => {
        // Use finalize() to ensure form is re-enabled whether the request succeeds or fails
        // This is more reliable than duplicating the enable() call in both success and error handlers
        this.form.enable();
      })
    ).subscribe(
      _ => {
        this.localLoginState = LocalLoginState.Success;
        
        // Note: In a more complete implementation, you would navigate to another page here:
        // this.router.navigate(['/propertySelector']).then(navigated => {});
        // However, this app uses conditional rendering (*ngIf) based on auth state
        // instead of router navigation, so the PropertyIndexComponent will automatically
        // be shown when authentication succeeds and updates the signInState.
        
        // Clear success state after 5 seconds in case user logs out without navigating elsewhere
        timer(5000).subscribe(() => this.localLoginState = LocalLoginState.None);
      },
      err => {
        // Form is automatically re-enabled by finalize() above
        if (err.status == 401)
          this.localLoginState = LocalLoginState.ErrorWrongData;
        else
          this.localLoginState = LocalLoginState.ErrorOther;
      }
    );
  }

  signOut() {
    this.as.signOut();
  }

  updateUserData(userData: AuthenticationSuccessData) {
    if (this.isLoggedIn) {
      this.loggedInUsername = userData.username;
      this.validityDays = Math.round(this.as.getValidityDays());
    } else {
      this.loggedInUsername = '';
      this.validityDays = 0;
    }
  }

  onCreateGuestClick() {
    this.router.navigate(['/users']);
  }
}

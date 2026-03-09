import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-activate-account',
  templateUrl: './activate-account.html',
  styleUrls: ['./activate-account.css']
})
export class ActivateAccountComponent implements OnInit {
  loading = true;
  success = false;
  message = '';
  userId: string | null = null;
  token: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // Extract query parameters
    this.userId = this.route.snapshot.queryParamMap.get('userId');
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.userId || !this.token) {
      this.loading = false;
      this.success = false;
      this.message = 'Invalid activation link. Please check your email and try again.';
      return;
    }

    // Call backend API to activate account
    this.activateAccount();
  }

  private activateAccount(): void {
    const apiUrl = `https://theretreatapp.up.railway.app/api/v1/account/ConfirmEmail?userId=${this.userId}&token=${encodeURIComponent(this.token!)}`;

    this.http.post<any>(apiUrl, {}).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = true;
        this.message = response.message || 'Your account has been activated successfully!';
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.success = false;
        
        if (error.error && error.error.message) {
          this.message = error.error.message;
        } else if (error.status === 400) {
          this.message = 'Invalid or expired activation link.';
        } else if (error.status === 404) {
          this.message = 'Account not found.';
        } else {
          this.message = 'Activation failed. Please try again or contact support.';
        }
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goToSignup(): void {
    this.router.navigate(['/signup']);
  }

  resendActivation(): void {
    // TODO: Implement resend activation email
    alert('Resend activation feature coming soon!');
  }
}
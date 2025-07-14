// src/app/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    if (this.authService.isSignedIn()) {
      return true; // User is signed in and token is locally valid
    } else {
      // Token is missing or expired locally (isSignedIn() handles cleanup/redirection via signOutInternalAndRedirect)
      console.log('AuthGuard: User not signed in or token expired. Redirecting to login.');
      // The `isSignedIn()` call now internally triggers the logout and redirection.
      // We just need to return a UrlTree to ensure Angular's routing system completes
      // the navigation properly.
      return this.router.createUrlTree(['/login']);
    }
  }
}
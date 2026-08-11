import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AuthService } from '../core/auth/services/auth.service';
import { AuthenticationSuccessData } from '../core/auth/model/login-data';
import { PostsService } from './posts.service';
import { Post } from './post.model';
import { ProfileSettingsDialogComponent, ProfileSettingsData } from './profile-settings-dialog/profile-settings-dialog.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatTooltipModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user$!: Observable<AuthenticationSuccessData | null>;
  isGuest = false;
  posts$!: Observable<Post[]>;
  composerText = '';

  displayUsername: string | null = null;
  displayEmail: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router,
    private postsService: PostsService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.user$ = this.authService.signInState;
    this.isGuest = this.authService.isGuestUser();
    this.posts$ = this.postsService.posts$;
  }

  goBack(): void {
    this.router.navigate(['/propertyLanding']);
  }

  signOut(): void {
    this.authService.signOut().subscribe(() => {
      this.router.navigate(['/']);
    });
  }

  submitPost(authorName: string): void {
    if (!this.composerText.trim()) {
      return;
    }
    this.postsService.addPost(authorName, this.composerText);
    this.composerText = '';
  }

  deletePost(id: string): void {
    this.postsService.deletePost(id);
  }

  openSettings(user: AuthenticationSuccessData): void {
    const dialogRef = this.dialog.open(ProfileSettingsDialogComponent, {
      width: '420px',
      data: {
        username: this.displayUsername ?? user.username,
        email: this.displayEmail ?? user.email
      } as ProfileSettingsData
    });

    dialogRef.afterClosed().subscribe((result: ProfileSettingsData | undefined) => {
      if (result) {
        this.displayUsername = result.username;
        this.displayEmail = result.email;
      }
    });
  }
}

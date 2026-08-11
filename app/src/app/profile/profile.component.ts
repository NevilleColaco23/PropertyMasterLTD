import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule, MatChipInputEvent } from '@angular/material/chips';
import { ENTER, COMMA } from '@angular/cdk/keycodes';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AuthService } from '../core/auth/services/auth.service';
import { AuthenticationSuccessData } from '../core/auth/model/login-data';
import { PostsService } from './posts.service';
import { GroupsService } from './groups.service';
import { Attachment, Group, Post, PostType } from './post.model';
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
    MatTooltipModule,
    MatSelectModule,
    MatFormFieldModule,
    MatChipsModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user$!: Observable<AuthenticationSuccessData | null>;
  isGuest = false;
  posts$!: Observable<Post[]>;
  groups$!: Observable<Group[]>;
  composerText = '';
  composerPostType: PostType = PostType.General;
  composerTargetGroupId: number | null = null;
  composerAttachments: Attachment[] = [];

  editingGroupId: number | null = null;
  editGroupName = '';
  editGroupDescription = '';

  groupChipSeparatorKeys: number[] = [ENTER, COMMA];

  postTypes = [
    { value: PostType.General, label: 'General' },
    { value: PostType.Important, label: 'Important' },
    { value: PostType.Announcement, label: 'Announcement' },
    { value: PostType.Question, label: 'Question' }
  ];

  // Per-post open comment box text, keyed by post id.
  commentDrafts: { [postId: number]: string } = {};
  expandedComments: { [postId: number]: boolean } = {};

  displayUsername: string | null = null;
  displayEmail: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router,
    private postsService: PostsService,
    private groupsService: GroupsService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.user$ = this.authService.signInState;
    this.isGuest = this.authService.isGuestUser();
    this.posts$ = this.postsService.posts$;
    this.groups$ = this.groupsService.groups$;
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
    this.postsService.addPost(
      authorName,
      this.composerText,
      this.composerPostType,
      this.composerTargetGroupId ?? undefined,
      this.composerAttachments
    );
    this.composerText = '';
    this.composerPostType = PostType.General;
    this.composerTargetGroupId = null;
    this.composerAttachments = [];
  }

  onComposerFilesSelected(event: Event): void {
    this.readFilesAsAttachments(event, files => this.composerAttachments.push(...files));
  }

  selectComposerGroup(groupId: number): void {
    this.composerTargetGroupId = this.composerTargetGroupId === groupId ? null : groupId;
  }

  addGroupChip(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();

    if (value) {
      this.groupsService.addGroup(value).subscribe(id => {
        if (id) {
          this.composerTargetGroupId = id;
        }
      });
    }

    event.chipInput?.clear();
  }

  startEditGroup(group: Group): void {
    this.editingGroupId = group.id;
    this.editGroupName = group.name;
    this.editGroupDescription = group.description;
  }

  cancelEditGroup(): void {
    this.editingGroupId = null;
    this.editGroupName = '';
    this.editGroupDescription = '';
  }

  saveEditGroup(): void {
    if (this.editingGroupId == null || !this.editGroupName.trim()) {
      return;
    }

    this.groupsService.updateGroup(this.editingGroupId, this.editGroupName, this.editGroupDescription).subscribe(() => {
      this.cancelEditGroup();
    });
  }

  deleteGroup(group: Group): void {
    if (!confirm(`Delete group "${group.name}"?`)) {
      return;
    }

    this.groupsService.deleteGroup(group.id).subscribe(() => {
      if (this.composerTargetGroupId === group.id) {
        this.composerTargetGroupId = null;
      }
      if (this.editingGroupId === group.id) {
        this.cancelEditGroup();
      }
    });
  }

  removeComposerAttachment(index: number): void {
    this.composerAttachments.splice(index, 1);
  }

  deletePost(id: number): void {
    this.postsService.deletePost(id);
  }

  toggleComments(postId: number): void {
    this.expandedComments[postId] = !this.expandedComments[postId];
  }

  submitComment(postId: number, authorName: string): void {
    const text = this.commentDrafts[postId];
    if (!text || !text.trim()) {
      return;
    }
    this.postsService.addComment(postId, authorName, text);
    this.commentDrafts[postId] = '';
  }

  acknowledgePost(postId: number, userName: string): void {
    this.postsService.acknowledgePost(postId, userName);
  }

  acknowledgeComment(postId: number, commentId: number, userName: string): void {
    this.postsService.acknowledgeComment(postId, commentId, userName);
  }

  hasAcknowledged(acknowledgements: { userName: string }[], userName: string): boolean {
    return acknowledgements?.some(a => a.userName === userName) ?? false;
  }

  private readFilesAsAttachments(event: Event, onDone: (attachments: Attachment[]) => void): void {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    if (files.length === 0) {
      return;
    }

    const attachments: Attachment[] = [];
    let remaining = files.length;

    files.forEach(file => {
      const reader = new FileReader();
      reader.onload = () => {
        attachments.push({
          url: reader.result as string,
          fileName: file.name,
          contentType: file.type
        });
        remaining--;
        if (remaining === 0) {
          onDone(attachments);
        }
      };
      reader.readAsDataURL(file);
    });

    input.value = '';
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
